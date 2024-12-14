using System.Collections.Generic;
using UnityEngine;


public class CenteredMap<T>
{
    private T[,] blockMap;
    private int maxSize;

    public CenteredMap(int size)
    {
        maxSize = size;
        blockMap = new T[size, size];
    }

    public T[,] Map => blockMap;

    public T this[int x, int y]
    {
        get => blockMap[x + maxSize / 2, y + maxSize / 2];
        set => blockMap[x + maxSize / 2, y + maxSize / 2] = value;
    }

    public T this[Vector2Int vec]
    {
        get => blockMap[vec.x + maxSize / 2, vec.y + maxSize / 2];
        set => blockMap[vec.x + maxSize / 2, vec.y + maxSize / 2] = value;
    }
    
    public bool IsInRange(Vector2Int vec)
    {
        if(maxSize % 2 == 0)
            return vec.x >= -maxSize / 2 && vec.x < maxSize / 2 && vec.y >= -maxSize / 2 && vec.y < maxSize / 2;
        else
            return vec.x >= -maxSize / 2 && vec.x <= maxSize / 2 && vec.y >= -maxSize / 2 && vec.y <= maxSize / 2;
    }
    
    public void Assign(T[,] map)
    {
        blockMap = map;
    }
    
    public List<T> AllValuesNotDefault()
    {
        List<T> values = new List<T>();
        for (int x = 0; x < maxSize; x++)
        {
            for (int y = 0; y < maxSize; y++)
            {
                if (blockMap[x,y] != null && !blockMap[x, y].Equals(default(T)))
                    values.Add(blockMap[x, y]);
            }
        }

        return values;
    }

}