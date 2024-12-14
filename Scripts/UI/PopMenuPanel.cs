using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class PopMenuPanel : SerializedMonoBehaviour
{
    public BuildHandler buildHandler;
    
    [OdinSerialize] private Dictionary<DisplayMode, Transform> modeParents;
    
    [SerializeField, ReadOnly] private DisplayMode currentDisplayMode;
    
    private bool _appearing = false;
    private Tween _popTween;

    public void Pop(bool appear)
    {
        if (appear) UIManager.Instance.CloseUI();
        
        if (_appearing != appear)
        {
            var rectTransform = GetComponent<RectTransform>();
            _appearing = appear;
            if (_popTween != null) _popTween.Kill();
            _popTween = rectTransform.DOAnchorPosX(appear ? 0 : rectTransform.rect.width, 0.25f);
        }
        
        if (!appear)
        {
            modeParents[currentDisplayMode].gameObject.SetActive(false);
        }
    }

    public void SwitchDisplayMode(DisplayMode mode)
    {
        currentDisplayMode = mode;
        
        foreach (var pair in modeParents)
        {
            pair.Value.gameObject.SetActive(pair.Key == mode);
        }
    }
    
    public void DisplayAchievements()
    {
        SwitchDisplayMode(DisplayMode.Achievements);
    }
}

public enum DisplayMode
{
    Achievements
}