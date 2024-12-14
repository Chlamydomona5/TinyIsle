using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PortageUnit_SO", menuName = "SO/PortageUnit_SO")]
public class PortageUnit_SO : Unit_SO
{
    public override string prefix => "UnitPortage";

    [Title("Creature")] 
    public bool isSpirit = true;
    [HideIf("isSpirit")]
    public GameObject creatureModel;
    [ShowIf("isSpirit")] 
    public Spirit spiritPrefab;
    public Sprite SpiritIcon => Resources.Load<Sprite>("SpiritIcon/" + ID);

    public int stamina;
    public int maxLoad;
    public float speed;

    public override List<(string iconId, string text, int lineNumber)> GetIconValuePair()
    {
        return new List<(string iconId, string text, int lineNumber)>()
        {
            ("stamina", $"{stamina}", 1),
            ("carryLimit", $"{maxLoad}", 1),
            ("speed", $"{speed.ToString("F1")}m/s", 1),
        };
    }

    public override List<(string iconId, string text)> GetIconDescPair()
    {
        var list = new List<(string, string)>(base.GetIconDescPair());
        list.Add(("stamina", $"体力值为{stamina}"));
        list.Add(("carryLimit", $"最多能够搬运{maxLoad}个水晶"));
        list.Add(("speed", $"移动速度为{speed}秒"));

        return list;
    }
}