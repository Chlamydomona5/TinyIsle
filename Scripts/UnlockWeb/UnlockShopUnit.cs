
using System.Linq;
using UnityEngine;

public class UnlockShopUnit : UnlockEffect
{
    public string UnitName;
    private Unit_SO Unit => Resources.Load<Unit_SO>($"Unit/{UnitName}");

    public override int MaxLevel =>
        Unit.features?.Count > 0 ? Unit.features.Max(feature => feature.unlockEvolveLevel) : 1;
    public override Sprite Icon => Resources.Load<Sprite>("NormalIcon/" + UnitName);

    public override string ForbiddenID => "NoSpaceForUnit";


    public override string ExtraDescription(int currentLevel)
    {
        if (currentLevel == MaxLevel) return "";
        if (!Unit.features.Find(x => x.unlockEvolveLevel == currentLevel + 1)) return "";
        return Unit.features?.Count > 0 ? Unit.features.Find(x => x.unlockEvolveLevel == currentLevel + 1).Description : "";
    }

    public override bool IsAbleToEffect()
    {
        return GridManager.Instance.FindPlaceForUnit(Unit).x != -1000;
    }

    public override void Effect()
    {
        UnlockWebManager.Instance.unlockedUnits.Add(Unit);
        UnlockWebManager.Instance.UnlockedEvolves.Add(Unit, 1);
        
        if(GridManager.Instance.FindPlaceForUnit(Unit).x != -1000)
            GridManager.Instance.BuildUnit(Unit, GridManager.Instance.FindPlaceForUnit(Unit));
    }

    public override void Upgrade()
    {
        UnlockWebManager.Instance.UnlockedEvolves[Unit] += 1;
    }
}
