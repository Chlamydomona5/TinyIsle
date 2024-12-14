using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class PropertyManager : Singleton<PropertyManager> , ISaveable
{
    [SerializeField] private Dictionary<PropertyType, float> _resources = new();
    
    [SerializeField] public float currentExpandCost = 1000;
    [SerializeField] private float currentExpandCostAddition = 1000;
    [SerializeField] private float currentExpandCostMultiplier = 2f;
    
    [SerializeField] private float dequeInterval = 5f;
    [SerializeField] private int startExpandCrystal = 3;
    
    [SerializeField] private float costMultiplier = 1.1f;
    
    private Dictionary<Unit_SO, float> _costDict = new();
    
    [SerializeField] private PropertyUI propertyUI;
    [ShowInInspector, ReadOnly] private int _currentGoldLimit = 1;
    public int CurrentGoldLimitIndex => _currentGoldLimit;
    
    private List<IncomeRecord> _incomeRecords = new();

    public float Gold => _resources[PropertyType.Gold];
    public bool CanExpand => Gold >= currentExpandCost;

    private IEnumerator Start()
    {
        //Refresh UI
        AddProperty(0, "Start");
        StartCoroutine(GoldRecordDeque());

        yield return new WaitForSeconds(1f);
        
        if(TestManager.Instance.onTest)
            for (int i = 0; i < startExpandCrystal; i++)
            {
                GetExpandCrystal(false);
            }
    }

    public void AddProperty(float delta, string source = "unknown", bool isIncome = true, PropertyType type = PropertyType.Gold)
    {
        _resources[type] += delta;

        if (type == PropertyType.Gold)
        {
            delta -= Mathf.Max(0, _resources[type] - CurrentGoldLimit());
            _resources[type] = Mathf.Min(_resources[type], CurrentGoldLimit());
        }

        if (isIncome)
            _incomeRecords.Add(new IncomeRecord()
            {
                Type = type,
                Amount = delta,
                Source = source
            });
        
        GridManager.Instance.core.UpdateFlowerCount(GetCurrentCoreFlowerNumber());
    }

    public float GetExpandCostAfter(int time)
    {
        var cost = currentExpandCost;
        for (int i = 0; i < time; i++)
        {
            cost += currentExpandCostAddition;
            cost *= currentExpandCostMultiplier;
        }

        return cost;
    }

    public int GetCurrentCoreFlowerNumber()
    {
        float i = 0;
        int number = -1;
        while (i <= Gold)
        {
            i += GetExpandCostAfter(number + 1);
            number++;
        }
        
        return number;
    }
    
    public float CurrentGoldLimit()
    {
        var limit = 0f;
        for (int i = 0; i < _currentGoldLimit; i++)
        {
            limit += GetExpandCostAfter(i);
        }

        return limit;
    }
    
    public Vector2Int GetExpandCrystal(bool cost = true)
    {
        var all = GridManager.Instance.crystalController.FindAllCrystalable(CrystalType.Expand);
        
        //If there is place and enough gold / cost is not required
        if (all.Count > 0 && Gold >= currentExpandCost || !cost)
        {
            if (cost)
            {
                AddProperty(-currentExpandCost);
                currentExpandCost += currentExpandCostAddition;
                currentExpandCost *= currentExpandCostMultiplier;
                //Refresh UI
                AddProperty(0);
            }
            
            //Generate a new expand crystal
            var des = all[Random.Range(0, all.Count)];
            GridManager.Instance.crystalController.GenerateExpandCrystal(Vector3.zero, des, 2);
            return des;
        }
        return new Vector2Int(-1000, -1000);
        
    }

    public float AvrIncome(PropertyType type = PropertyType.Gold)
    {
        float total = 0;
        foreach (var record in _incomeRecords.Where(r => r.Type == type))
        {
            total += record.Amount > 0 ? record.Amount : 0;
        }

        // Debug.Log("Avr = " + total / dequeInterval);
        
        return total / dequeInterval;
    }
    
    private IEnumerator GoldRecordDeque()
    {
        while (true)
        {
            if (_incomeRecords.Count > 0)
            {
                //Remove records older than 5 seconds
                _incomeRecords.RemoveAll(r => (DateTime.Now - r.Time).TotalSeconds > dequeInterval);
            }
            
            AchievementManager.Instance.AssignProgress(AchievementType.AvrProduce, (int)AvrIncome());
            
            yield return new WaitForSeconds(1);
        }
    }
    
    public void AssignCost(Unit_SO unit)
    {
        if(!_costDict.ContainsKey(unit)) _costDict[unit] = unit.cost;
        
        _costDict[unit] *= costMultiplier;
    }
    
    public float GetCost(Unit_SO unit)
    {
        return _costDict.ContainsKey(unit) ? _costDict[unit] : unit.cost;
    }
    
    public void RedoCost(Unit_SO unit)
    {
        if(!_costDict.ContainsKey(unit)) _costDict[unit] = unit.cost;
        
        _costDict[unit] /= costMultiplier;
    }
    
    public class IncomeRecord
    {
        public PropertyType Type;
        public float Amount;
        public string Source;
        public DateTime Time = DateTime.Now;
    }
    
    public enum PropertyType
    {
        Gold, 
    }

    public string GetPropertyText(PropertyType type)
    {
        var current = _resources[type];
        var limit = currentExpandCost;
        return current.ToString("N0") + " / " + limit.ToString("N0") + 
               (_currentGoldLimit > 1 ? $"({CurrentGoldLimit().ToString("N0")})" : "");
    }

    public void AddCurrentGoldLimit(int count)
    {
        _currentGoldLimit = Mathf.Min(5, _currentGoldLimit + count);
        propertyUI.AddLimit(count);
    }

    public void AddToTestLimit()
    {
        currentExpandCost = 2000;
    }

    public bool GoldIsFull()
    {
        return (Gold + .1f) >= CurrentGoldLimit();
    }

    public string TimeToNextLimit()
    {
        var limit = currentExpandCost;
        var goldLeft = Gold;
        int level = 0;
        
        while (goldLeft >= limit)
        {
            goldLeft -= limit;
            level++;
            limit = GetExpandCostAfter(level);
        }

        if (AvrIncome() < 0.1f) return "~";
        
        var secs = (limit - goldLeft) / AvrIncome();
        //Time form
        return TimeSpan.FromSeconds(secs).ToString(@"hh\:mm\:ss");
    }

    public void Save()
    {
        // Save All fields
        ES3.Save("PropertyManager.resources", _resources);
        ES3.Save("PropertyManager.currentExpandCost", currentExpandCost);
        ES3.Save("PropertyManager.currentExpandCostAddition", currentExpandCostAddition);
        ES3.Save("PropertyManager.currentExpandCostMultiplier", currentExpandCostMultiplier);
        ES3.Save("PropertyManager.costMultiplier", costMultiplier);
        ES3.Save("PropertyManager._costDict", _costDict);
        ES3.Save("PropertyManager._currentGoldLimit", _currentGoldLimit);
    }

    public void Load()
    {
        // Load All fields
        _resources = ES3.Load("PropertyManager.resources", _resources);
        currentExpandCost = ES3.Load("PropertyManager.currentExpandCost", currentExpandCost);
        currentExpandCostAddition = ES3.Load("PropertyManager.currentExpandCostAddition", currentExpandCostAddition);
        currentExpandCostMultiplier = ES3.Load("PropertyManager.currentExpandCostMultiplier", currentExpandCostMultiplier);
        costMultiplier = ES3.Load("PropertyManager.costMultiplier", costMultiplier);
        _costDict = ES3.Load("PropertyManager._costDict", _costDict);
        _currentGoldLimit = ES3.Load("PropertyManager._currentGoldLimit", _currentGoldLimit);
    }
}