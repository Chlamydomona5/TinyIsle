/*
using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class PropertyFloat
{
    public float Gold;
    public float Faith;

    public PropertyFloat()
    {
    }
    
    public PropertyFloat(PropertyFloat origin)
    {
        Gold = origin.Gold;
        Faith = origin.Faith;
    }
    
    public PropertyFloat(float gold, float faith)
    {
        Gold = gold;
        Faith = faith;
    }
    
    //Override operator +, -, *, /
    public static PropertyFloat operator +(PropertyFloat a, PropertyFloat b)
    {
        PropertyFloat c = new PropertyFloat();
        c.Gold = a.Gold + b.Gold;
        c.Faith = a.Faith + b.Faith;
        return c;
    }
    
    public static PropertyFloat operator -(PropertyFloat a, PropertyFloat b)
    {
        PropertyFloat c = new PropertyFloat();
        c.Gold = a.Gold - b.Gold;
        c.Faith = a.Faith - b.Faith;
        return c;
    }
    
    public static PropertyFloat operator *(PropertyFloat a, PropertyFloat b)
    {
        PropertyFloat c = new PropertyFloat();
        c.Gold = a.Gold * b.Gold;
        c.Faith = a.Faith * b.Faith;
        return c;
    }
    
    public static PropertyFloat operator *(PropertyFloat a, float b)
    {
        PropertyFloat c = new PropertyFloat();
        c.Gold = a.Gold * b;
        c.Faith = a.Faith * b;
        return c;
    }
    
    public static PropertyFloat operator /(PropertyFloat a, PropertyFloat b)
    {
        PropertyFloat c = new PropertyFloat();
        c.Gold = a.Gold / b.Gold;
        c.Faith = a.Faith / b.Faith;
        return c;
    }

    public override string ToString()
    {
        //Return the one that its abs is above 0, if both are 0, return 0
        if (Math.Abs(Gold) > 0)
        {
            return Gold.ToString("F2");
        }
        else if (Math.Abs(Faith) > 0)
        {
            return Faith.ToString("F2");
        }
        else if(Math.Abs(Gold) > 0 && Math.Abs(Faith) > 0)
        {
            return Gold.ToString("F2") + "," + Faith.ToString("F2");
        }
        else
        {
            return "0";
        }
    }
}
*/
