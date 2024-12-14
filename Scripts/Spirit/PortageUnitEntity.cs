using System.Collections.Generic;
using Unit;
using UnityEngine;

public class PortageUnitEntity : UnitEntity
{
    public PortageCreature creature;
    
    public PortageUnit_SO portageUnitSo;
    public PortageUnit_SO portageUnitSoRef;

    public override string UniqueAttributeInfo =>
        $"Stamina: {creature.CurrentParam.stamina}\nCarry Limit: {creature.CurrentParam.maxLoad}\nSpeed: {creature.CurrentParam.speed}m/s";

    public override void Init(Unit_SO info, Vector2Int coord)
    {
        base.Init(info, coord);
        //Create SO and Feature Instances
        portageUnitSo = (PortageUnit_SO)unitSo;
        portageUnitSoRef = (PortageUnit_SO)unitSoRef;
        var list = new List<UnitFeature_SO>();
        foreach (var feature in portageUnitSoRef.features)
        {
            list.Add(Instantiate(feature));
        }
        portageUnitSo.features = list;

        if (!portageUnitSo.isSpirit)
        {
            creature = new GameObject("Creature").AddComponent<PortageCreature>();
            creature.model = Instantiate(portageUnitSo.creatureModel, creature.transform);
        }
        else
        {
            creature = Instantiate(portageUnitSo.spiritPrefab);
        }
        creature.Init(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(creature.gameObject)
            Destroy(creature.gameObject);
    }
}