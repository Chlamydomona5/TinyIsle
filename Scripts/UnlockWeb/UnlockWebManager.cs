using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class UnlockWebManager : Singleton<UnlockWebManager>, ISaveable
{
    public List<int> layerSplit = new() {2, 2, 2, 2, 2};
    
    public List<UnlockItem> allUnlockItems;
    private Dictionary<UnlockItem, int> _currentUnlocked = new();

    public List<Unit_SO> unlockedUnits = new();
    public Dictionary<Unit_SO, int> UnlockedEvolves = new();

    /// <summary>
    /// For inform UI only
    /// </summary>
    [SerializeField] private List<UnlockItem> ableItem;
    [SerializeField] private UnlockInformUI unlockInformUI;

    private List<UnlockItem> _unlocksSaved = new();
    
    public override void Awake()
    {
        base.Awake();
        
        allUnlockItems = Resources.LoadAll<UnlockItem>("UnlockItems").ToList();
        _currentUnlocked = allUnlockItems.ToDictionary(item => item, _ => 0);
        ableItem = new List<UnlockItem>();
    }

    public void AddToUnlock(Vector2Int coord)
    {
        var item = allUnlockItems.Find(i => i.coord == coord);

        if(_currentUnlocked[item] >= item.Effect.MaxLevel) return;
        
        _unlocksSaved.Add(item);
        //Unlock the item
        _currentUnlocked[item]++;
    }

    public void ReleaseUnlock(bool willCost = true, UnlockItem specficItem = null)
    {
        var list = _unlocksSaved;
        if (specficItem) list = new List<UnlockItem> {specficItem};
        
        // Make sure each item only unlock once
        var onlyList = list.Distinct().ToList();
        
        foreach (var item in onlyList)
        {
            for (int i = list.Count(x => x == item); i > 0; i--)
            {
                if (willCost)
                {
                    //Suck the cost
                    if (!TestManager.Instance.freeUnlock)
                    {
                        var cost = item.CostExpandCrystal;
                        var crystals =
                            GridManager.Instance.crystalController.FindAllCrystal(CrystalType.Unlock);
                        crystals.GetRange(0, cost).ForEach(crystal =>
                        {
                            var instance = GridManager.Instance.crystalController.PopCrystalStack(crystal.coord).Pop();
                            instance.FlipToAnimation(instance.transform.position, new Vector2Int(1, 1), 0.25f, 0, false).onComplete += () =>
                            {
                                instance.gameObject.SetActive(false);
                            };
                        });
                    }
                }
            
                if (_currentUnlocked[item] - i == 0)
                    item.UnlockEffect();
                else item.UpgradeEffect();
            
                AchievementManager.Instance.AccumulateProgress(AchievementType.Unlock);
            }
        }
        
        _unlocksSaved.Clear();
    }
    
    public int GetCurrentReleaseCost()
    {
        return _unlocksSaved.Sum(item => item.CostExpandCrystal);
    }

    public bool IsUnlocked(UnlockItem item)
    {
        return _currentUnlocked[item] > 0;
    }
    
    public bool IsMaxLevel(UnlockItem item)
    {
        return _currentUnlocked[item] >= item.Effect.MaxLevel;
    }
    
    public int GetUnlockLevel(UnlockItem item)
    {
        return _currentUnlocked[item];
    }
    
    public void DetectUnlockInform()
    {
        if (!unlockInformUI) return;
        
        foreach (var item in allUnlockItems)
        {
            if (_currentUnlocked[item] == 0 && item.preItems.All(IsUnlocked))
            {
                var able = GridManager.Instance.crystalController.FindAllCrystal(CrystalType.Expand).Count >= item.CostExpandCrystal;
                if(able)
                {
                    //Inform the new item that can be unlocked
                    if (!ableItem.Contains(item))
                    {
                        ableItem.Add(item);
                        unlockInformUI.TryToInform(item);
                    }
                }
                //If the item is unable again, remove it for next inform
                else if(ableItem.Contains(item)) ableItem.Remove(item);
            }
        }
    }

    public bool UnlockVisible(UnlockItem target)
    {
        var maxY = _currentUnlocked.Min(item => item.Key.coord.y);
        if(_currentUnlocked.Where(x => x.Value != 0).Any())
            maxY = _currentUnlocked.Where(x => x.Value != 0).Max(item => item.Key.coord.y);
        var maxLayer = 0;
        while ((maxY + 1) >= layerSplit.Take(maxLayer).Sum())
        {
            maxLayer++;
        }

        var targetY = target.coord.y;
        var targetLayer = 0;
        while ((targetY + 1) >= layerSplit.Take(targetLayer).Sum())
        {
            targetLayer++;
        }
        
        return targetLayer <= maxLayer;
    }


    public void Save()
    {
        ES3.Save("UnlockWebManager.CurrentUnlocked", _currentUnlocked);
        ES3.Save("UnlockWebManager.UnlockedUnits", unlockedUnits);
        ES3.Save("UnlockWebManager.UnlockedEvolves", UnlockedEvolves);
    }

    public void Load()
    {
        _currentUnlocked = ES3.Load("UnlockWebManager.CurrentUnlocked", _currentUnlocked);
        unlockedUnits = ES3.Load("UnlockWebManager.UnlockedUnits", unlockedUnits);
        UnlockedEvolves = ES3.Load("UnlockWebManager.UnlockedEvolves", UnlockedEvolves);
    }
}