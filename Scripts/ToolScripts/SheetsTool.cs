using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using FlexFramework.Excel;
using UnityEditor;
using UnityEngine;
using RectInt = Unit.RectInt;

#if UNITY_EDITOR

public static class SheetsTool
{
    [MenuItem("SheetTools/Import Sheets")]
    public static void Import()
    {
        #region Delete

        List<string> assets2Delete = new();
        assets2Delete.AddRange(AssetDatabase.FindAssets("", new[] { "Assets/Data/Resources/Unit" }));
        foreach (var asset in assets2Delete)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(asset));
        }

        #endregion

        var workBook = new WorkBook("Assets/Data/UnitInfo.xlsx");
        var generalSheet = workBook[0];
        var produceSheet = workBook[1];
        var portageSheet = workBook[2];
        var consumeSheet = workBook[3];
        var furnitureSheet = workBook[4];

        //Start from 3, since 1 is the title, 2 is the default value
        foreach (var row in generalSheet.Rows.Skip(2))
        {
            if (row[GetColumn("LOAD", generalSheet)].Convert<string>() != "*") continue;

            //Type
            var type = row[GetColumn("Type", generalSheet)].Convert<string>();
    
            switch (type)
            {
                case "Produce":
                    var produce = ScriptableObject.CreateInstance<ProduceUnit_SO>();
                    ReadGeneralInfo(produce, row);
                    ReadAsProduce(produce);
                    produce.features = ReadFeature(produce).ToList();
                    AssetDatabase.CreateAsset(produce, $"Assets/Data/Resources/Unit/{produce.ID}.asset");
                    break;
                case "Portage":
                    var portage = ScriptableObject.CreateInstance<PortageUnit_SO>();
                    ReadGeneralInfo(portage, row);
                    ReadAsPortage(portage);
                    portage.features = ReadFeature(portage).ToList();
                    AssetDatabase.CreateAsset(portage, $"Assets/Data/Resources/Unit/{portage.ID}.asset");
                    break;
                case "Consume":
                    var consume = ScriptableObject.CreateInstance<ConsumeUnit_SO>();
                    ReadGeneralInfo(consume, row);
                    ReadAsConsume(consume);
                    consume.features = ReadFeature(consume).ToList();
                    AssetDatabase.CreateAsset(consume, $"Assets/Data/Resources/Unit/{consume.ID}.asset");
                    break;
                case "Furniture":
                    var furniture = ScriptableObject.CreateInstance<FurnitureUnit_SO>();
                    ReadGeneralInfo(furniture, row);
                    ReadAsFurniture(furniture);
                    furniture.features = ReadFeature(furniture).ToList();
                    AssetDatabase.CreateAsset(furniture, $"Assets/Data/Resources/Unit/{furniture.ID}.asset");
                    break;
                default:
                    var defaultUnit = ScriptableObject.CreateInstance<Unit_SO>();
                    ReadGeneralInfo(defaultUnit, row);
                    defaultUnit.features = ReadFeature(defaultUnit).ToList();
                    AssetDatabase.CreateAsset(defaultUnit, $"Assets/Data/Resources/Unit/{defaultUnit.ID}.asset");
                    break;
            }
        }

        //Re-open the xlsx
        Application.OpenURL(Application.dataPath + "/Data/UnitInfo.xlsx");

        #region Read Methods

        void ReadAsConsume(ConsumeUnit_SO so)
        {
            var row = consumeSheet.FirstOrDefault(x => x[0].Convert<string>() == so.ID);
            if (row == default) Debug.LogError("Name is empty in consume: " + so.ID);
            
            Read(consumeSheet, row, GetColumn("ConsumeType", consumeSheet), out string consumeType);
            so.typeMask = (CrystalType)Enum.Parse(typeof(CrystalType), consumeType);

            Read(consumeSheet, row, GetColumn("HasThreshold", consumeSheet), out so.hasThreshold);
            Read(consumeSheet, row, GetColumn("TriggerThreshold", consumeSheet), out so.triggerThreshold);
        }
        
        void ReadAsProduce(ProduceUnit_SO so)
        {
            var row = produceSheet.FirstOrDefault(x => x[0].Convert<string>() == so.ID);
            if (row == default) Debug.LogError("Name is empty in produce: " + so.ID);

            //Parameters
            Read(produceSheet, row, GetColumn("CrystalType", produceSheet), out string type);
            so.crystalType = (CrystalType)Enum.Parse(typeof(CrystalType), type);
            Read(produceSheet, row, GetColumn("ProduceType", produceSheet), out string produceType);
            so.produceType = (ProduceType)Enum.Parse(typeof(ProduceType), produceType);
            
            Read(produceSheet, row, GetColumn("Produce", produceSheet), out so.produce);
            Read(produceSheet, row, GetColumn("ProduceCount", produceSheet), out so.produceCount);
            Read(produceSheet, row, GetColumn("CrystalSize", produceSheet), out so.crystalSize);
            Read(produceSheet, row, GetColumn("AscendLevel", produceSheet), out so.ascendLevel);
            Read(produceSheet, row, GetColumn("Distance", produceSheet), out so.distance);
            Read(produceSheet, row, GetColumn("ProduceInterval", produceSheet), out so.produceInterval);
            Read(produceSheet, row, GetColumn("IntervalGrowth", produceSheet), out so.produceIntervalGrowthConstant);
            Read(produceSheet, row, GetColumn("CritPossibility", produceSheet), out so.critPossibility);
            Read(produceSheet, row, GetColumn("CritMultiplier", produceSheet), out so.critMultiplier);
            Read(produceSheet, row, GetColumn("WillTired", produceSheet), out so.willTired);
            Read(produceSheet, row, GetColumn("TiredProduceTimes", produceSheet), out so.tiredProduceTimes);
            Read(produceSheet, row, GetColumn("TiredDuration", produceSheet), out so.tiredDuration);

        }

        void ReadAsPortage(PortageUnit_SO so)
        {
            var row = portageSheet.FirstOrDefault(x => x[0].Convert<string>() == so.ID);
            if (row == default) Debug.LogError("Name is empty in portage: " + so.ID);
            
            //Parameters
            Read(portageSheet, row, GetColumn("IsSpirit", portageSheet), out so.isSpirit);
            Read(portageSheet, row, GetColumn("Stamina", portageSheet), out so.stamina);
            Read(portageSheet, row, GetColumn("MaxLoad", portageSheet), out so.maxLoad);
            Read(portageSheet, row, GetColumn("Speed", portageSheet), out so.speed);

            //Creature Model
            if(so.isSpirit)
                so.spiritPrefab = Resources.Load<Spirit>("SpiritPrefabs/" + so.ID);
            else
                so.creatureModel = Resources.Load<GameObject>("CreatureModels/" + so.ID);

        }
        
        void ReadAsFurniture(FurnitureUnit_SO so)
        {
            var row = furnitureSheet.FirstOrDefault(x => x[0].Convert<string>() == so.ID);
            if (row == default) Debug.LogError("Name is empty in furniture: " + so.ID);
            
            //Parameters
            var impact = new PortageImpact();
            
            Read(furnitureSheet, row, GetColumn("Stamina", furnitureSheet), out impact.stamina);
            Read(furnitureSheet, row, GetColumn("MaxLoad", furnitureSheet), out impact.maxLoad);
            Read(furnitureSheet, row, GetColumn("Speed", furnitureSheet), out impact.speed);

            so.impact = impact;
        }

        int GetColumn(string name, WorkSheet sheet)
        {
            for (int i = 0; i < sheet.Rows[0].Count; i++)
            {
                if (sheet.Rows[0][i].Convert<string>() == name)
                {
                    return i;
                }
            }

            return -1;
        }

        void Read<T>(WorkSheet sheet, Row row, int index, out T value)
        {
            //Debug.Log(row.Count + " " + index);
            if (row.Count <= index || row[index].Convert<string>() == "")
            {
                value = sheet[1][index].Convert<T>();
            }
            else
            {
                value = row[index].Convert<T>();
            }
        }

        void ReadGeneralInfo(Unit_SO so, Row row)
        {
            //ID and Description
            Read(generalSheet, row, GetColumn("ID", generalSheet), out string id);
            so.ID = id;
            Read(generalSheet, row, GetColumn("Shop", generalSheet), out so.category);
            Read(generalSheet, row, GetColumn("Cost", generalSheet), out so.cost);
            Read(generalSheet, row, GetColumn("Only", generalSheet), out so.only);
            Read(generalSheet, row, GetColumn("InShop", generalSheet), out so.inShop);
            Read(generalSheet, row, GetColumn("Evolvable", generalSheet), out so.evolvable);
            Read(generalSheet, row, GetColumn("CrystalCoexistable", generalSheet), out so.crystalCoexistable);
            Read(generalSheet, row, GetColumn("SellPriceConstant", generalSheet), out so.sellPriceConstant);
            Read(generalSheet, row, GetColumn("RandomActivate", generalSheet), out so.randomActivate);
            Read(generalSheet, row, GetColumn("RandomEvolveLimit", generalSheet), out so.randomEvolveLimit);
            
            //Ground Type
            Read(generalSheet, row, GetColumn("GroundType", generalSheet), out string groundType);
            so.groundType = (GroundType)Enum.Parse(typeof(GroundType), groundType);

            //Add 3 fixed tags
            for (int j = 0; j < 3; j++)
            {
                var index = GetColumn("Tag" + (j + 1), generalSheet);
                if (row.Count > index && row[index].Convert<string>() != "")
                {
                    so.tags.Add(row[index].Convert<string>());
                }
            }

            #region Size

            //Size is a string like "0x0\n0c0\n0x0", in which "c" means center, "c"'s coordinate is (0,0), "x" means covered, "0" means empty
            string size;
            Read(generalSheet, row, GetColumn("Size", generalSheet), out size);
            //Reverse the string
            var array = size.ToCharArray();
            System.Array.Reverse(array);
            size = new string(array);
            //Turn into rows
            var lines = size.Split('\n');
            //Find "c" and set it as pivot
            var pivot = new Vector2Int();
            for (int j = 0; j < lines.Length; j++)
            {
                var line = lines[j];
                if (line.Contains("C"))
                {
                    pivot = new Vector2Int(line.IndexOf('C'), j);
                    break;
                }
            }

            //Convert "c" and "x" to covered coords
            for (int j = 0; j < lines.Length; j++)
            {
                var line = lines[j];
                for (int k = 0; k < line.Length; k++)
                {
                    if (line[k] == 'C' || line[k] == 'X')
                    {
                        so.coveredCoords.Add(new Vector2Int(k - pivot.x, j - pivot.y));
                    }
                }
            }

            #endregion

            //Model is named as so.ID_1/2/3
            so.model = Resources.Load<UnitModel>("UnitModels/" + so.ID);
        }

        List<UnitFeature_SO> ReadFeature(Unit_SO unit)
        {
            var list = new List<UnitFeature_SO>();
            foreach (var feature in Resources.LoadAll<UnitFeature_SO>("UnitFeature"))
            {
                if (feature.ID.Contains(unit.ID))
                {
                    list.Add(feature);
                }
            }

            return list;
        }

        #endregion
    }


    [MenuItem("SheetTools/ExportID")]
    public static void Export()
    {
        List<string> alreadyExists = new List<string>();
        StreamReader sr = new StreamReader(Application.dataPath + "\\Data\\IDs.csv");

        string line;
        while ((line = sr.ReadLine()) != null)
        {
            Debug.Log(line);
            alreadyExists.Add(line.Split('\n')[0]);
        }

        sr.Close();
        sr.Dispose();

        StreamWriter sw = new StreamWriter(Application.dataPath + "\\Data\\IDs.csv", true);
        sw.Write("\n");
        foreach (var idBase in Resources.LoadAll<IDBase_SO>(""))
        {
            if (alreadyExists.Exists(x => x.Contains(idBase.ID))) continue;
            sw.Write(idBase.prefix + "_" + idBase.ID + "," + "\n");
            sw.Write(idBase.prefix + "_" + idBase.ID + "_Desc" + "," + "\n");
        }

        sw.Flush();
        sw.Close();
    }
}

#endif


