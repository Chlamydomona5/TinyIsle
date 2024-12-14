using System;
using System.Collections.Generic;
using Core;
using Sirenix.OdinInspector;
using UI;
using Unit;
using UnityEngine;
using RectInt = Unit.RectInt;

public class Unit_SO : IDBase_SO, IInfoDisplay
{
    public override string prefix => "Unit";

    public bool only;
    public bool crystalCoexistable;
    public bool evolvable;
    
    public string category;
    public float cost;
    public float sellPriceConstant;
    
    [Title("Type")] 
    public List<string> tags = new();
    public bool inShop;
    public GroundType groundType;
    
    [Title("Model")]
    public UnitModel model;
    public List<Vector2Int> coveredCoords = new();
    
    [Title("Move")]
    public bool groundFixed;
    
    [Title("Feature")]
    [Title("每次进化时随机解锁一个特性")]
    public bool randomActivate;
    public int randomEvolveLimit = 0;
    
    public List<UnitFeature_SO> features = new();

    public virtual List<(string iconId, string text, int lineNumber)> GetIconValuePair()
    {
        var list = new List<(string iconId, string text, int lineNumber)>();
        if(crystalCoexistable) list.Add(("crystalCoexistable", "", 2));

        if (features.Exists(x => x.effect is HandleCrystal))
        {
            var handleCrystal = features.Find(x => x.effect is HandleCrystal);
            list.Add(("ascendLevel", handleCrystal.effect.GetInt(FeatureEffect.EffectStatInt.MaxAscendLevel).ToString(), 1));
        }
        
        return list;
    }
    
    public virtual List<(string iconId, string text)> GetIconDescPair()
    {
        var list = new List<(string iconId, string text)>();
        if (only) list.Add(("only", "该单位只能存在一个"));
        if (crystalCoexistable) list.Add(("crystalCoexistable", "该单位可以和水晶共存"));
        
        if (features.Exists(x => x.effect is HandleCrystal))
        {
            var handleCrystal = features.Find(x => x.effect is HandleCrystal);
            list.Add(("ascendLevel", "升华等级上限：" + handleCrystal.effect.GetInt(FeatureEffect.EffectStatInt.MaxAscendLevel)));
        }
        return list;
    }
}