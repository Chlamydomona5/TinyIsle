
using UnityEngine;

public class UnlockUniqueUnit : UnlockEffect
{
    public string UnitName;
    private Unit_SO Unit => Resources.Load<Unit_SO>($"Unit/{UnitName}");

    public override Sprite Icon => Resources.Load<Sprite>("NormalIcon/" + UnitName);

    public override string ForbiddenID => "NoSpace";

    public override bool IsAbleToEffect()
    {
        return GridManager.Instance.FindPlaceForUnit(Unit).x != -1000;
    }

    public override void Effect()
    {
        GridManager.Instance.BuildUnit(Unit, GridManager.Instance.FindPlaceForUnit(Unit));
    }
}
