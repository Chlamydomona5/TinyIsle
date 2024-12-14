using System;
using UnityEngine;

public class ProducePreview : MonoBehaviour
{
    public void AdjustTo(ProduceUnitEntity unit, Vector2Int coord)
    {
        gameObject.SetActive(true);
        
        Vector3 pos;
        if (unit.produceUnitSo.distance == 0)
            pos = GridManager.Instance.Coord2Pos(
                GridManager.Instance.crystalController.FindBestCrystalableAround(unit, coord));
        else
            pos = GridManager.Instance.Coord2Pos(
                GridManager.Instance.crystalController.FindBestCrystalableByDistance(coord,
                    unit.produceUnitSo.distance));
        
        transform.position = new Vector3(pos.x, 0.01f, pos.z);
    }
}