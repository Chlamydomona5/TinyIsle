using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class SuckCrystal : RangeEffect
{
    public RespondType Type = RespondType.ProduceNew;
    
    public SuckCrystal()
    {
        paramsDictInt.Add(EffectStatInt.ConvertCount, 3);
        paramsDictInt.Add(EffectStatInt.BlockCount, 1);
        paramsDictInt.Add(EffectStatInt.MaxAscendLevel, 1);
        paramsDictInt.Add(EffectStatInt.AscendLevelStep, 1);
    }

    public override void Effect(UnitFeature_SO feature, UnitEntity self)
    {
        // Absorbtion
        var targets = GetTargetCrystals(self, CrystalType.Gold);
        if(targets == null || targets.Count == 0) return;
        
        var selected = new List<Crystal>();
        int count = 0;
        while (count < GetInt(EffectStatInt.ConvertCount))
        {
            foreach (var stack in targets)
            {
                if (stack.Count == 0)
                {
                    // If all stacks are empty, break
                    if (targets.TrueForAll(x => x.Count == 0))
                    {
                        foreach (var sCrystal in selected)
                        {
                            GridManager.Instance.crystalController.PushCrystalTo(sCrystal.coordinate, sCrystal);
                        }
                        return;
                    }
                    else continue;
                }
                selected.Add(stack.Pop());
                count++;
                if (count >= GetInt(EffectStatInt.ConvertCount)) break;
            }
        }
        
        if(selected.Count == 0) return;
        
        Tween moveTween = null;
        foreach (var crystal in selected)
        {
            moveTween = crystal.FlipToAnimation(crystal.transform.position, self.coordinate);
            moveTween.onComplete += () => crystal.gameObject.SetActive(false);
        }

        //Produce
        if (Type == RespondType.ProduceNew)
        {
            var crystalController = GridManager.Instance.crystalController;
        
            var highestValueCrystal = selected.OrderByDescending(x => x.Value).First();
            int size = highestValueCrystal.size;
            int ascendLevel = Mathf.Min(highestValueCrystal.goldAscendLevel + GetInt(EffectStatInt.AscendLevelStep), GetInt(EffectStatInt.MaxAscendLevel));

            var des = crystalController.FindBestCrystalableByDistance(self.coordinate, GetInt(EffectStatInt.BlockCount));

            moveTween.onComplete += () =>
            {
                crystalController.GenerateGoldCrystalStandard(self.transform.position, des, size, ascendLevel);
            };
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

    public enum RespondType
    {
        ProduceNew,
    }
}