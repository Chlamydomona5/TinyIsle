using System.Collections.Generic;
using UnityEngine;

public class CrystalStack
{
    public Transform Bottom;
    public Vector2Int Coordinate;
    
    public Stack<Crystal> Crystals = new Stack<Crystal>();
    
    public int Count => Crystals.Count;
    public bool IsEmpty => Crystals.Count == 0;
    public CrystalType CurrentType => Crystals.Peek().type;

    public CrystalStack(Vector2Int coord, Transform bottom)
    {
        Bottom = bottom;
        Coordinate = coord;
        
        ReAdjustPos();
    }
    
    public CrystalStack(CrystalStack stack)
    {
        Bottom = stack.Bottom;
        Coordinate = stack.Coordinate;
        Crystals = new Stack<Crystal>(stack.Crystals);
    }

    public void ReAdjustPos()
    {
        Bottom.position = GridManager.Instance.Coord2Pos(Coordinate) +
                          (GridManager.Instance.FindUnitAt(Coordinate) ? GridManager.Instance.FindUnitAt(Coordinate).model.CurrentHeight : 0) * Vector3.up;
    }

    public void ClearAll()
    {
        Crystals.Clear();
    }
    
    public Crystal Pop()
    {
        return Crystals.Pop();
    }

    public void MoveAllTo(CrystalStack destination)
    {
        while (Crystals.Count > 0)
        {
            var crystal = Crystals.Pop();
            destination.Push(crystal);
        }
    }
    
    public void Push(Crystal crystal)
    {
        crystal.stackIndex = Count;

        crystal.transform.SetParent(Bottom, false);
        crystal.transform.localPosition = Vector3.up * (0.25f * crystal.stackIndex);
        crystal.transform.rotation = Quaternion.identity;
        
        Crystals.Push(crystal);
        
        ReAdjustPos();
    }
}
