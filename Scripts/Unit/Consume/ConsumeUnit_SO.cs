using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unit.Consume;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumeUnit_SO", menuName = "SO/ConsumeUnit_SO")]
public class ConsumeUnit_SO : Unit_SO
{
    public CrystalType typeMask;
    
    public bool hasThreshold;
    [ShowIf("hasThreshold")]
    public float triggerThreshold;
    
    public override string prefix => "UnitConsume";
    
    public override List<(string iconId, string text, int lineNumber)> GetIconValuePair()
    {
        return new List<(string iconId, string text, int lineNumber)>();
    }
}