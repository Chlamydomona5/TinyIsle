
using System;

public enum GroundType
{
    Empty,
    Low,
    Normal,
    High,
}

//Extension of GroundType
public static class GroundTypeExtension
{
    public static float Height(this GroundType type)
    {
        return type switch
        {
            GroundType.Empty => -1f,
            GroundType.Low => -0.35f,
            GroundType.Normal => 0,
            GroundType.High => .3f,
            _ => 0
        };
    }
    
    public static bool CanFall(this GroundType type, GroundType other)
    {
        //Turn GroundType to Array
        var typeArray = new[] {GroundType.Empty, GroundType.Low, GroundType.Normal, GroundType.High};
        var typeIndex = Array.IndexOf(typeArray, type);
        var otherIndex = Array.IndexOf(typeArray, other);
        //Return true if typeIndex is bigger than otherIndex
        return typeIndex >= otherIndex;
    }
}
