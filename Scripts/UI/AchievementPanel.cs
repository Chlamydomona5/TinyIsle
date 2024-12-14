using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private List<AchievementButton> achievementButtons;
    
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    
    [SerializeField] private PageButton pageButtonPrefab;
    [SerializeField] private Transform pageButtonParent;
    private List<PageButton> _pageButtons;
    private int _currentPage = 0;

    [SerializeField] GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI infoNameText;
    [SerializeField] private TextMeshProUGUI infoDescriptionText;
    [SerializeField] private TextMeshProUGUI infoProgressText;
    
    [ShowInInspector, ReadOnly] private List<Achievement_SO> _checkedAchievements = new List<Achievement_SO>();
    
    private bool _initialized = false;

    private void Start()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(pageButtonParent.GetComponent<RectTransform>());
    }

    private void Init()
    {
        if (_initialized) return;
        _initialized = true;
        
        _pageButtons = new List<PageButton>();
        for (var i = 0; i < AchievementManager.Instance.AllAchievements.Count / 8 + 1; i++)
        {
            var pageButton = Instantiate(pageButtonPrefab, pageButtonParent);
            pageButton.Init(i);
            pageButton.button.onClick.AddListener(() => ClickPage(pageButton));
            _pageButtons.Add(pageButton);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(pageButtonParent.GetComponent<RectTransform>());
        
        LoadPage(0);
        
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
    }

    public void LoadPage(int page)
    {
        Debug.Log("Load page " + page);
        _currentPage = page;
        
        // Update achievement buttons
        var list = AchievementManager.Instance.AllAchievements.GetRange(page * 8, Math.Min(8, AchievementManager.Instance.AllAchievements.Count - page * 8));
        for (var i = 0; i < list.Count; i++)
        {
            achievementButtons[i].gameObject.SetActive(true);
            achievementButtons[i].SetAchievement(list[i], this, _checkedAchievements.Contains(list[i]));
        }

        for (var i = list.Count; i < achievementButtons.Count; i++)
        {
            achievementButtons[i].gameObject.SetActive(false);
        }
        
        // Update page buttons
        for (var i = 0; i < _pageButtons.Count; i++)
        {
            _pageButtons[i].BeSelected(i == page);
        }
    }
    
    public void NextPage()
    {
        if (_currentPage < AchievementManager.Instance.AllAchievements.Count / 8)
        {
            LoadPage((_currentPage + 1) % (_pageButtons.Count));
        }
    } 
    
    public void PreviousPage()
    {
        if (_currentPage > 0)
        {
            LoadPage((_currentPage - 1) % (_pageButtons.Count));
        }
    }
    
    public void ClickPage(PageButton page)
    {
        LoadPage(_pageButtons.IndexOf(page));
    }

    public void ShowAchievement(Achievement_SO achievementSO)
    {
        infoPanel.SetActive(true);
        infoNameText.text = achievementSO.LocalizeName;
        infoDescriptionText.text = achievementSO.Description;
        infoProgressText.text = $"{AchievementManager.Instance.GetProgress(achievementSO.type)} / {achievementSO.amount}";

        if (AchievementManager.Instance.IsAchievementFinished(achievementSO) && !_checkedAchievements.Contains(achievementSO))
        {
            _checkedAchievements.Add(achievementSO);
            LoadPage(_currentPage);
        }
    }

    private void OnEnable()
    {
        Init();
        
        infoPanel.SetActive(false);
        LoadPage(0);
    }
}