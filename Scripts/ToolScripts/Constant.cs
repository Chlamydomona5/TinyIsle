
using System.Collections.Generic;
using UnityEngine;

public static class Constant
{
    public static int MaxNormalLevel = 5;
    public static float BasicSellConstant = 0.5f;
    
    public static Dictionary<CrystalType, Color> CrystalType2Color = new()
    {
        { CrystalType.Gold, new Color(195 / 255f, 183 / 255f, 89 / 255f) },
    };

    public static Dictionary<CrystalType, Color> CrystalType2CritColor = new()
    {
        { CrystalType.Gold, new Color(217 / 255f, 140 / 255f, 56 / 255f) },
    };

    public static Dictionary<string, int> SignMaterials = new()
    {
        { "Disable", 1 },
        { "Tired", 2 },
        { "NotInWater", 3 },
    };
}
