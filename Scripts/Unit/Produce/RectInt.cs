using System.Collections.Generic;
using UnityEngine;

namespace Unit
{
    public class RectInt
    {
        public int up;
        public int right;
        public int down;
        public int left;
        
        public RectInt(int up, int right, int down, int left)
        {
            this.up = up;
            this.right = right;
            this.down = down;
            this.left = left;
        }
        
        //Generate a square
        public Square GenerateSquare()
        {
            return new Square(new Vector2((right - left) / 2f, (up - down) / 2f), right + left + 1, up + down + 1);
        }

        public bool IsZero()
        {
            return up == 0 && right == 0 && down == 0 && left == 0;
        }
    }

    public class Square
    {
        public Vector2 pivot;
        public int width;
        public int length;
        
        public Square(Vector2 pivot, int width, int length)
        {
            this.pivot = pivot;
            this.width = width;
            this.length = length;
        }
    }
}