using Sirenix.OdinInspector;
using UnityEngine;

public class ConsumeUnitEntity : UnitEntity
{
    public ConsumeUnit_SO consumeUnitSo;
    public ConsumeUnit_SO consumeUnitSoRef;

    [SerializeField, ReadOnly] private float currentAmount;
    
    public override void Init(Unit_SO info, Vector2Int coord)
    {
        base.Init(info, coord);

        consumeUnitSoRef = (ConsumeUnit_SO)unitSoRef;
        consumeUnitSo = (ConsumeUnit_SO)unitSo;
    }

    public void ConsumeCrystal(Crystal crystal)
    {
        currentAmount += crystal.Value;
        crystal.gameObject.SetActive(false);
        model.onConsume.Invoke();

        if (consumeUnitSo.hasThreshold)
        {
            while (currentAmount >= consumeUnitSo.triggerThreshold)
            {
                foreach (var feature in activitatedFeatures)
                {
                    feature.OnConsume(this, currentAmount, crystal.type);
                }

                currentAmount -= consumeUnitSo.triggerThreshold;
            }
        }
        else
        {
            foreach (var feature in activitatedFeatures)
            {
                feature.OnConsume(this, currentAmount, crystal.type);
            }

            currentAmount = 0;
        }

    }

    public override string UniqueAttributeInfo => "";
}