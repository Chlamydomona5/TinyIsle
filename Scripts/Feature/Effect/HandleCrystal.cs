using System.Linq;
using UnityEngine;

public class HandleCrystal : RangeEffect
{
    public HandleCrystal()
    {
        paramsDictFloat.Add(EffectStatFloat.Possibliity, 0);
        paramsDictInt.Add(EffectStatInt.MaxEffectAmount, 1);
        paramsDictInt.Add(EffectStatInt.BlockCount, 1);
        paramsDictInt.Add(EffectStatInt.MaxAscendLevel, 1);
        paramsDictInt.Add(EffectStatInt.AscendLevelStep, 1);
    }
    
    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        if(Random.value > GetFloat(EffectStatFloat.Possibliity)) return;
        
        var crystals = GetTargetCrystals(self, CrystalType.Gold);
        
        // Move
        if(GetInt(EffectStatInt.BlockCount) == 0) return;
        var maxAmount = GetInt(EffectStatInt.MaxEffectAmount);
        foreach (var stack in crystals)
        {
            var number = Mathf.Min(maxAmount, stack.Count);
            for (var i = 0; i < number; i++)
            {
                var place = GridManager.Instance.crystalController.FindBestCrystalableByDistance(stack.Coordinate,
                    GetInt(EffectStatInt.BlockCount));
                if(place.x == -1000) return;
                
                var crystal = stack.Pop();

                var ascendCount = GetInt(EffectStatInt.AscendLevelStep);
                while (crystal.goldAscendLevel < GetInt(EffectStatInt.MaxAscendLevel) && ascendCount > 0)
                {
                    crystal.Ascend();
                    ascendCount--;
                }

                GridManager.Instance.crystalController.FlipOnePopedCrystal(crystal, place);
            }
        }
    }

    public override void BeforeMove(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override void AfterMove(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override void BeforeDestroy(UnitFeature_SO feature, UnitEntity self)
    {
    }

    public override bool SpecialCondition(UnitFeature_SO feature, UnitEntity self)
    {
        return GetTargetCrystals(self, CrystalType.Gold).Count > 0;
    }
}
