using System.Collections.Generic;
using System.Linq;
using Core;
using DG.Tweening;
using UnityEngine;

public static class Methods
{
    public static bool OnTest = true;

    public static Vector3 YtoZero(Vector3 vec)
    {
        return new Vector3(vec.x, 0, vec.z);
    }

    public static string GetLocalText(string id)
    {
        var localText = I2.Loc.LocalizationManager.GetTranslation(id);
        
        if (localText == null)
        {
            Debug.LogWarning($"Local text {id} not found");
            return "N_" + id;
        }
        return localText.Trim('\"', '\r', '\n');
    }
    
    public static bool HasLocalText(string id, out string text)
    {
        return I2.Loc.LocalizationManager.TryGetTranslation(id, out text);
    }

    public static float UpgradeFormula_Interval(int level, float baseValue, float growthConstant)
    {
        return Mathf.Max(baseValue * (1 - growthConstant), 0.1f);
    }

    public static int IncreaseShopCost(int currentValue, float growthConstant)
    {
        return currentValue * (1 + (int)growthConstant);
    }

    public static int Rarity2Int(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return 1;
            case Rarity.Rare:
                return 2;
            case Rarity.Epic:
                return 3;
            case Rarity.Legend:
                return 4;
            default:
                return 0;
        }
    }

    public static Vector2 World2Canvas(Vector3 worldPos, Canvas canvas, Camera camera)
    {
        var viewportPos = camera.WorldToViewportPoint(worldPos);
        var canvasRect = canvas.GetComponent<RectTransform>();
        var canvasPos = new Vector2(viewportPos.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
            viewportPos.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);
        return canvasPos;
    }

    public static Dictionary<Rarity, float> Rarity2Weight = new()
    {
        { Rarity.Common, 0.55f },
        { Rarity.Rare, 0.25f },
        { Rarity.Epic, 0.15f },
        { Rarity.Legend, 0.05f },
    };

    public static Color Rarity2Color(Rarity rarity)
    {
        return Rarity2ColorDict[rarity];
    }

    public static Dictionary<Rarity, Color> Rarity2ColorDict = new()
    {
        { Rarity.Common, new Color(0.8f, 0.8f, 0.8f) },
        { Rarity.Rare, new Color(0.2f, 0.8f, 0.2f) },
        { Rarity.Epic, new Color(0.8f, 0.3f, 0.8f) },
        { Rarity.Legend, new Color(1f, 0.5f, 0.2f) },
    };

    public static Dictionary<Rarity, int> RarityToMultiplier = new()
    {
        { Rarity.Common, 1 },
        { Rarity.Rare, 2 },
        { Rarity.Epic, 5 },
        { Rarity.Legend, 10 },
    };

    public static T GetRandomValueInDict<T>(Dictionary<T, float> dict)
    {
        var totalWeight = dict.Values.ToArray().Sum();
        var randWeight = Random.Range(0, totalWeight);

        if (dict.Count == 0)
        {
            Debug.LogAssertion("Dict may be empty");
            return default;
        }

        //For some reason, the dict can't be detected when its empty, so I add a check here to prevent it from crashing
        int calCount = 0;
        while (calCount < 100)
        {
            foreach (var pair in dict)
            {
                if (randWeight < pair.Value) return pair.Key;

                randWeight -= pair.Value;
            }

            calCount++;
        }

        return dict.Keys.ToArray()[0];
    }

    public static string GenerateFeatureDescription(UnitFeature_SO featureSo)
    {
        //Effect
        var effectDesc = featureSo.Description;
        effectDesc = TransferFeatureVaribles(effectDesc, featureSo);

        return effectDesc;
    }

    public static string TransferFeatureVaribles(string origin, UnitFeature_SO featureSo)
    {
        var result = origin;
        //turn string like "{xxx}" to variables and makes it italic and bold
        foreach (var pair in featureSo.effect.paramsDictFloat)
        {
            if (result == null) continue;

            result = result.Replace("{" + pair.Key + "}", $"{ParamToString(pair.Key.ToString(), pair.Value)}");
        }
        
        foreach (var pair in featureSo.effect.paramsDictInt)
        {
            if (result == null) continue;

            result = result.Replace("{" + pair.Key + "}", $"{ParamToString(pair.Key.ToString(), pair.Value)}");
        }

        return result;
    }
    
    public static string TransferVariable(string origin, string key, string value)
    {
        return origin.Replace("{" + key + "}", value);
    }

    public static string ParamToString(string paramName, int value)
    {
        return (value >= 0 ? "+" : "") + value;
    }
    
    public static string ParamToString(string paramName, float value)
    {
        if (paramName.Contains("Ratio"))
        {
            return (value >= 0 ? "+" : "") + value.ToString("P1");
        }

        return (value >= 0 ? "+" : "") + value.ToString("F1");
    }
    
    // Extend Method for transform
    public static Tween FlipToCoordAnim(this Transform transform, Vector3 from, Vector2Int des, float time, int index,
        bool rotate = true)
    {
        //Perform the crystal animation
        transform.position = from;
        //Rotate Y axis to face the destination
        transform.rotation = Quaternion.LookRotation(GridManager.Instance.Coord2Pos(des) - from);
                  
        var unit = GridManager.Instance.FindUnitAt(des);
        var height = index * .25f;
        //If the crystal is going to be consumed by a unit
        if (unit) 
        {
            if (unit.unitSo.ID == "Core")
            {
                height = 0f;
            }                                                                                                                                                                                          
            else if (!unit.unitSo.crystalCoexistable)
            {
                height = unit.modelHeight;
            }
        }

        var distance = Methods.YtoZero(transform.position - GridManager.Instance.Coord2Pos(des)).magnitude;
        
        var tween = transform.DOJump(GridManager.Instance.Coord2Pos(des) + height * Vector3.up, 1f + Random.value * 1.5f + distance / 3f, 1, time);
        if (rotate)
            transform.DORotate(Vector3.right * 360f, time, RotateMode.FastBeyond360).onComplete += () => transform.rotation = Quaternion.identity;
        else
            transform.DOShakeRotation(time, Vector3.one * 40f).onComplete += () => transform.rotation = Quaternion.identity;
        return tween;
    }
    
    public static Tween FlipToWorldPosAnim(this Transform transform, Vector3 from, Vector3 des, float time, int index,
        bool rotate = true)
    {
        transform.position = from;
        //Rotate Y axis to face the destination
        transform.rotation = Quaternion.LookRotation(des - from);

        var tween = transform.DOJump(des, 2f, 1, time);
        if (rotate)
            transform.DORotate(Vector3.right * 360f, time, RotateMode.FastBeyond360).onComplete += () => transform.rotation = Quaternion.identity;
        else
            transform.DOShakeRotation(time, Vector3.one * 40f).onComplete += () => transform.rotation = Quaternion.identity;
        return tween;
    }

    public enum OperatorType
    {
        Add,
        Multiply,
    }
}