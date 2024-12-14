using System.Collections.Generic;
using System.Linq;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpiritManager : Singleton<SpiritManager>
{
    public List<Spirit> spirits;

    public GrassSpirit GrassSpirit => spirits.Find(spirit => spirit is GrassSpirit) as GrassSpirit;
    public FleshinessSpirit FleshinessSpirit => spirits.Find(spirit => spirit is FleshinessSpirit) as FleshinessSpirit;

    [SerializeField, ReadOnly]
    private List<Vector2Int> AllocatedStackCoords => spirits.Select(spirit => spirit.Destination).ToList();

    [ShowInInspector, ReadOnly] private List<FurnitureUnitEntity> _furnitures = new List<FurnitureUnitEntity>();

    public bool TryGetAvailableStack(out Vector2Int coord)
    {
        coord = Vector2Int.zero;
        if(PropertyManager.Instance.GoldIsFull()) return false;
        
        var crystalSpots = GridManager.Instance.crystalController.FindAllCrystal(CrystalType.Gold)
            .FindAll(x => !GridManager.Instance.HasUnitEntityOrLocked(x.coord));
        var availableSpots = crystalSpots.FindAll(spot =>
            !AllocatedStackCoords.Exists(coord => coord == spot.coord));

        if (availableSpots.Count > 0)
        {
            coord = availableSpots[Random.Range(0, availableSpots.Count)].coord;
            return true;
        }

        coord = Vector2Int.zero;
        return false;
    }

    public bool IsCoordStackAvailable(Vector2Int coord)
    {
        return GridManager.Instance.crystalController.HasCrystal(coord, CrystalType.Gold);
    }

    public void AddSpirit(Spirit spirit)
    {
        spirits.Add(spirit);
        
        AchievementManager.Instance.AccumulateProgress(AchievementType.SpiritCount);

        foreach (var furniture in _furnitures)
        {
            spirit.buffCarrier.AddBuff(new Buff<PortageImpact>(furniture, furniture.furnitureUnitSo.impact));
        }
    }

    public void RegisterFurniture(FurnitureUnitEntity furniture)
    {
        _furnitures.Add(furniture);

        foreach (var spirit in spirits)
        {
            spirit.buffCarrier.AddBuff(new Buff<PortageImpact>(furniture, furniture.furnitureUnitSo.impact));
        }
        
        AchievementManager.Instance.AccumulateProgress(AchievementType.FurnitureCount);
    }

    public void UnregisterFurniture(FurnitureUnitEntity furniture)
    {
        _furnitures.Remove(furniture);

        foreach (var spirit in spirits)
        {
            spirit.buffCarrier.RemoveBuffFrom(furniture);
        }
    }
    
    public Spirit ClosestSpirit(Vector2Int coord)
    {
        return spirits.OrderBy(spirit => Vector2Int.Distance(spirit.Destination, coord)).FirstOrDefault();
    }
}