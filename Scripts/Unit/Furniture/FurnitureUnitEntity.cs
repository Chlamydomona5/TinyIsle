using Sirenix.OdinInspector;
using UnityEngine;

public class FurnitureUnitEntity : UnitEntity
{
    public FurnitureUnit_SO furnitureUnitSo;
    public FurnitureUnit_SO furnitureUnitSoRef;

    public override void Init(Unit_SO info, Vector2Int coord)
    {
        base.Init(info, coord);

        furnitureUnitSoRef = (FurnitureUnit_SO)unitSoRef;
        furnitureUnitSo = (FurnitureUnit_SO)unitSo;

        SpiritManager.Instance.RegisterFurniture(this);
    }

    protected override void OnDestroy()
    {
        SpiritManager.Instance.UnregisterFurniture(this);
        base.OnDestroy();
    }
}