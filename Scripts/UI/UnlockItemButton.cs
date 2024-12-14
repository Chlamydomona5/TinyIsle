using System;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockItemButton : SerializedMonoBehaviour
{
    public UnlockItem item;
    public State state;
    
    private UnlockWebPanel _unlockWebPanel;
    
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField] private Image canUnlockIcon;
    
    [OdinSerialize] private Dictionary<UnlockType, Sprite> _unknownSprites;
    [OdinSerialize] private Dictionary<UnlockType, Sprite> _lockedSprites;
    [OdinSerialize] private Dictionary<UnlockType, Sprite> _unlockedSprites;
    [OdinSerialize] private Dictionary<UnlockType, Sprite> _fullyUnlockedSprites;
    
    [SerializeField] private TextMeshProUGUI achievementPointText;
    
    [SerializeField] private Transform levelIconParent;
    [OdinSerialize] private List<(Image on, Image off)> _levelIcons;
    
    public void Init(UnlockWebPanel unlockWebPanel, UnlockItem _)
    {
        item = _;
        _unlockWebPanel = unlockWebPanel;
        state = State.Unknown;
        
        if(item.unlockType != UnlockType.MiniumAchievementPoint) icon.sprite = _.Icon;
        
        button.onClick.AddListener(() =>
        {
            _unlockWebPanel.currentItem = this;
            _unlockWebPanel.Select(item);
        });
    }

    public void OnBuy()
    {
        if (!TestManager.Instance.freeUnlock)
        {
            if (state.Equals(State.Unknown) || state.Equals(State.UnlockedMax)) return;
            if (!item.CanUnlock(_unlockWebPanel.currentResource)) return;
            if(!_unlockWebPanel.CheckDependency(item)) return;

            if (UnlockWebManager.Instance.IsMaxLevel(item)) return;
        }
        
        if(item.Effect != null)
        {
            if(!item.Effect.IsAbleToEffect())
            {
                UIManager.Instance.CenteredInform(item.Effect.ForbiddenID);
                return;
            }
        }

        AudioManager.instance.Play("ConsumeExpandCrystal");
        
        UnlockWebManager.Instance.AddToUnlock(item.coord);
        
        _unlockWebPanel.RefreshButton();
        _unlockWebPanel.Select(item);
        
        TutorialManager.Instance?.endCurrentEvent.Invoke("UnlockWebUnlock");
    }

    private void RefreshSelf()
    {
        if (item.unlockType == UnlockType.MiniumAchievementPoint)
        {
            achievementPointText.gameObject.SetActive(state == State.Locked);
            achievementPointText.text = AchievementManager.Instance.TotalPoint.ToString() + "/" + item.minPoint;
        }
        
        switch (state)
        {
            case State.Unknown:
                if(item.unlockType == UnlockType.CostExpandCrystal) icon.gameObject.SetActive(false);
                background.sprite = _unknownSprites[item.unlockType];
                TryCanUnlockIcon(false);
                levelIconParent.gameObject.SetActive(false);
                break;
            case State.Locked:
                if(item.unlockType == UnlockType.CostExpandCrystal) icon.gameObject.SetActive(true);
                icon.color = new Color(1, 1, 1, 0.4f);
                background.sprite = _lockedSprites[item.unlockType];
                TryCanUnlockIcon(true);
                levelIconParent.gameObject.SetActive(false);
                break;
            case State.UnlockedBase:
                if(item.unlockType == UnlockType.CostExpandCrystal) icon.gameObject.SetActive(true);
                icon.color = Color.white;   
                background.sprite = _unlockedSprites[item.unlockType];
                TryCanUnlockIcon(true);
                AdjustLevel();
                break;
            case State.UnlockedMax:
                if(item.unlockType == UnlockType.CostExpandCrystal) icon.gameObject.SetActive(true);
                icon.color = Color.white;
                background.sprite = _fullyUnlockedSprites[item.unlockType];
                TryCanUnlockIcon(false);
                AdjustLevel();
                break;
        }
    }

    private void AdjustLevel()
    {
        var max = item.Effect.MaxLevel;
        levelIconParent.gameObject.SetActive(true);
        if (item.Effect == null) return;
        var level = UnlockWebManager.Instance.GetUnlockLevel(item);
        for (int i = 0; i < _levelIcons.Count; i++)
        {
            var index = i;
            _levelIcons[i].on.gameObject.SetActive(index < level && index < max);
            _levelIcons[i].off.gameObject.SetActive(index >= level && index < max);
        }
    }

    public void TryCanUnlockIcon(bool on)
    {
        if (on && item.CanUnlock(_unlockWebPanel.currentResource)) canUnlockIcon.gameObject.SetActive(true);
        else canUnlockIcon.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        RefreshSelf();
    }

    public enum State
    {
        Unknown,
        Locked,
        UnlockedBase,
        UnlockedMax
    }
}