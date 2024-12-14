using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid
{
    [Serializable]
    public class Adjacent
    {
        public bool[] directions = new bool[4]; 
        public bool Up { get => directions[0]; set => directions[0] = value; }
        public bool Right { get => directions[1]; set => directions[1] = value; }
        public bool Down { get => directions[2]; set => directions[2] = value; }
        public bool Left { get => directions[3]; set => directions[3] = value; }
        
        public Adjacent(bool up, bool right, bool down, bool left)
        {
            directions[0] = up;
            directions[1] = right;
            directions[2] = down;
            directions[3] = left;
        }

        public Adjacent()
        {
            directions[0] = false;
            directions[1] = false;
            directions[2] = false;
            directions[3] = false;
        }
        
        public static bool operator ==(Adjacent a, Adjacent b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            
            for (int i = 0; i < 4; i++)
            {
                if (a.directions[i] != b.directions[i]) return false;
            }

            return true;
        }

        public static bool operator !=(Adjacent a, Adjacent b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Adjacent adjacent)
            {
                return this == adjacent;
            }

            return false;
        }
        
        public override int GetHashCode()
        {
            var code = 12132213;
            for (int i = 0; i < 4; i++)
            {
                code = code * 23 + directions[i].GetHashCode();
            }

            return code;
        }

        public Adjacent Rotate(int rotation)
        {
            var newDirections = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                newDirections[(i + rotation) % 4] = directions[i];
            }

            return new Adjacent(newDirections[0], newDirections[1], newDirections[2], newDirections[3]);
        }
    }
}