using System;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class AchievementManager : Singleton<AchievementManager>, ISaveable
{
    private List<Achievement_SO> _allAchievements = new();
    public List<Achievement_SO> AllAchievements => _allAchievements;
    public int TotalPoint => _totalPoint;
    
    [ShowInInspector, ReadOnly] private List<Achievement_SO> _finishedAchievements;
    [ShowInInspector, ReadOnly] private Dictionary<AchievementType, int> _achievementProgress = new();
    [ShowInInspector, ReadOnly] private int _totalPoint;
    
    public override void Awake()
    {
        base.Awake();
        _allAchievements = new List<Achievement_SO>(Resources.LoadAll<Achievement_SO>("Achievements"));
        _allAchievements = _allAchievements.OrderBy(x => x.type).ThenBy(x => x.amount).ToList();
        _finishedAchievements = new List<Achievement_SO>();
    }
    
    /// <summary>
    /// Assign the max value to an achievement
    /// </summary>
    public void AssignProgress(AchievementType type, int amount)
    {
        if (!_achievementProgress.ContainsKey(type))
        {
            _achievementProgress.Add(type, amount);
        }
        _achievementProgress[type] = Mathf.Max(_achievementProgress[type], amount);
        
        CheckAchievements();
    }
    
    /// <summary>
    /// Accumulate the progress of an achievement
    /// </summary>
    public void AccumulateProgress(AchievementType type, int amount = 1)
    {
        if (!_achievementProgress.ContainsKey(type))
        {
            _achievementProgress.Add(type, 0);
        }

        _achievementProgress[type] += amount;
        
        CheckAchievements();
    }

    private void CheckAchievements()
    {
        foreach (var achievement in _allAchievements)
        {
            if (_finishedAchievements.Contains(achievement)) continue;
            if (_achievementProgress.ContainsKey(achievement.type) && _achievementProgress[achievement.type] >= achievement.amount)
            {
                _finishedAchievements.Add(achievement);
                AchievementFinishedEffect(achievement);
                _totalPoint += achievement.worthPoint;
            }
        }
    }
    
    private void AchievementFinishedEffect(Achievement_SO achievement)
    {
        Debug.Log($"Achievement Unlocked: {achievement.name}");
        TutorialManager.Instance.EnableAchievementButton();

        var possiblePlaces = GridManager.Instance.crystalController.FindAllCrystalable(CrystalType.Unlock);
        if (possiblePlaces.Count == 0) return;
        //Sort by distance to the center
        possiblePlaces.RemoveAll(x => GridManager.Instance.core.GetComponent<UnitEntity>().RealCoveredCoords.Contains(x));
        possiblePlaces.Sort((x, y) =>
            Vector2Int.Distance(x, Vector2Int.zero).CompareTo(Vector2Int.Distance(y, Vector2Int.zero)));
        var place = possiblePlaces[0];
        
        GridManager.Instance.crystalController.GenerateUnlockCrystal(Vector3.one, place, 1);
        
        UIManager.Instance.TipStraight(Methods.TransferVariable(Methods.GetLocalText("AchievementUnlocked"), "AchievementName", achievement.LocalizeName));
        AudioManager.instance.Play("Achievement");
    }
    
    public bool IsAchievementFinished(Achievement_SO achievement)
    {
        return _finishedAchievements.Contains(achievement);
    }
    
    public int GetProgress(AchievementType type)
    {
        if (!_achievementProgress.ContainsKey(type))
        {
            return 0;
        }
        return _achievementProgress[type];
    }

    public void Save()
    {
    }

    public void Load()
    {
    }
}