using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using RectInt = Unit.RectInt;

public abstract class RangeEffect : FeatureEffect
{
    [Title("是否影响自己")] public bool AffectSelf;

    [Title("Range Parameters")] [Title("范围是否为周围一圈")]
    public bool Around = true;

    [HideIf("Around"), Title("以中心扩张的范围")] public RectInt rectRange = new(0, 0, 0, 0);
    public List<string> tagsFilter = new();

    public List<T> GetTargetUnits<T>(UnitEntity unitEntity) where T : UnitEntity
    {
        if (Around)
        {
            return GridManager.Instance.FindUnitsAround<T>(unitEntity, tagsFilter, AffectSelf);
        }
        else
        {
            return GridManager.Instance.FindUnitsInEffect<T>(unitEntity, rectRange, tagsFilter, AffectSelf);
        }
    }

    public List<CrystalStack> GetTargetCrystals(UnitEntity unitEntity, CrystalType mask)
    {
        if (Around)
        {
            return GridManager.Instance.crystalController.FindCrystalsAround(unitEntity, mask);
        }
        else
        {
            return GridManager.Instance.crystalController.FindCrystalAt(unitEntity.coordinate, rectRange, mask);
        }
    }
    
    public List<Vector2Int> GetTargetCoords(Unit_SO so)
    {
        if (Around)
        {
            return GridManager.Instance.GetAround8Dir(so.coveredCoords);
        }
        else
        {
            return GridManager.Instance.GetRectCoords(Vector2Int.zero, rectRange);
        }
    }
}