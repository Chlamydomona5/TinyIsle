using System;
using System.Collections.Generic;
using System.Linq;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : SerializedMonoBehaviour
{
    [ReadOnly, SerializeField] private SortType currentSortType = SortType.ByCost;
    [SerializeField] private int buttonsPerLine;

    [SerializeField] private UnitInfoPanel unitInfoPanel;
    [SerializeField] private List<List<ShopButton>> _shopButtons;
    [SerializeField] private List<RectTransform> buttonLineParents;
    
    [SerializeField] private string currentCategory;
    [SerializeField] private int currentPage;
    [SerializeField] private ShopButton selectedButton;
    
    [SerializeField] private Button sortButton;
    [SerializeField] private Button unfoldButton;
    
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private RectTransform sideButtonsParent;
    [SerializeField] private GameObject pageParent;
    
    [SerializeField] private Button nextButton;
    [SerializeField] private Image nextButtonImage;
    [SerializeField] private Sprite nextButtonAbledSprite;
    [SerializeField] private Sprite nextButtonDisabledSprite;
    
    [SerializeField] private Button prevButton;
    [SerializeField] private Image prevButtonImage;
    [SerializeField] private Sprite prevButtonAbledSprite;
    [SerializeField] private Sprite prevButtonDisabledSprite;
    
    [SerializeField] private TextMeshProUGUI pageText;
    public List<Unit_SO> InShopList
    {
        get
        {
            if(TestManager.Instance.allInShop) return Resources.LoadAll<Unit_SO>("Unit").ToList();
            
            var res = Resources.LoadAll<Unit_SO>("Unit").Where(x => x.inShop).ToList(); 
            var unlockList = UnlockWebManager.Instance.unlockedUnits;
            res.AddRange(unlockList);
            return res;
        }
    }

    private Dictionary<Unit_SO, DateTime> _historyList = new();
    private Sequence _transformTween;
    
    private void Start()
    {
        GenerateLine(0, currentSortType);
        nextButton.onClick.AddListener(() => NextPage(true));
        prevButton.onClick.AddListener(() => NextPage(false));
    }
    
    private void SwitchToCategory(string category)
    {
        currentPage = 0;
        currentCategory = category;
        
        GenerateLine(0, currentSortType, category);
    }

    private void GenerateLine(int lineCount, SortType sortType, string category = "", bool intoFirstLine = false)
    {
        //Debug.Log("Generate Page: " + page + "category: " + category);
        Clear();

        //Filter the list
        var list = InShopList;
        if (!string.IsNullOrEmpty(category))
        {
            list = list.Where(x => x.category == category).ToList();
        }

        switch (sortType)
        {
            case SortType.ByCost:
                list = list.OrderBy(x => x.cost).ToList();
                break;
            case SortType.ByHistory:
                list = list.OrderBy(x => _historyList.ContainsKey(x) ? _historyList[x] : DateTime.MinValue).ToList();
                list.Reverse();
                break;
        }

        //Generate buttons for the page
        for (int i = lineCount * buttonsPerLine; i < (lineCount + 1) * buttonsPerLine; i++)
        {
            UpdateButton(i >= list.Count ? null : list[i], intoFirstLine ? 0 : lineCount, i % buttonsPerLine);
        }
    }
    
    private void GenerateAll(SortType sortType, string category = "")
    {
        Clear();
        for (int i = 0; i < _shopButtons.Count; i++)
        {
            GenerateLine(i, sortType, category);
        }
    }

    private void UpdateButton(Unit_SO unit, int line, int count)
    {
        if(!unit)
        {
            _shopButtons[line][count].gameObject.SetActive(false);
            return;
        }
        _shopButtons[line][count].gameObject.SetActive(true);
        _shopButtons[line][count].Init(unit, ClickShopButton);
    }

    public void OpenButton()
    {
        var open = buttonLineParents[0].gameObject.activeSelf;
        
        if (open)
        {
            _transformTween?.Complete();
            _transformTween = DOTween.Sequence();
            _transformTween.Append(contentParent.DOAnchorPosX(-contentParent.rect.width, 0.2f));
            _transformTween.Join(sideButtonsParent.DOAnchorPosY(-sideButtonsParent.rect.height, 0.2f));
            _transformTween.onComplete += () =>
            {
                Close();
            };
        }
        else
        {
            buttonLineParents[0].gameObject.SetActive(true);
            unfoldButton.gameObject.SetActive(true);
            sortButton.gameObject.SetActive(true);
            pageParent.SetActive(true);
             
            GenerateLine(currentPage, currentSortType, "", true);
            RefreshPageUI();
            
            _transformTween?.Complete();
            _transformTween = DOTween.Sequence();
            _transformTween.Append(contentParent.DOAnchorPosX(0, 0.2f));
            _transformTween.Join(sideButtonsParent.DOAnchorPosY(0, 0.2f));
        }
    }

    public void SortButton()
    {
        var allTypes = Enum.GetValues(typeof(SortType)).Cast<SortType>().ToList();
        var index = allTypes.IndexOf(currentSortType);
        currentSortType = allTypes[(index + 1) % allTypes.Count];
        
        GenerateAll(currentSortType, currentCategory);
    }

    public void Close()
    {
        Clear();
        buttonLineParents.ForEach(x => x.gameObject.SetActive(false));
        unfoldButton.gameObject.SetActive(false);
        sortButton.gameObject.SetActive(false);
        pageParent.SetActive(false);
    }

    public void FoldButton()
    {
        var fold = !buttonLineParents[1].gameObject.activeSelf;
        if (fold)
        {
            buttonLineParents.ForEach(x => x.gameObject.SetActive(true));
            GenerateAll(currentSortType);
            pageParent.SetActive(false);
        }
        else
        {
            buttonLineParents.ForEach(x => x.gameObject.SetActive(false));
            buttonLineParents[0].gameObject.SetActive(true);
            pageParent.SetActive(true);
        }
    }

    public void Clear()
    {
        selectedButton = null;
        unitInfoPanel.gameObject.SetActive(false);
    }

    public void ClickShopButton(ShopButton button)
    {
        if (selectedButton == button)
        {
            if (button.OnBuyClick())
            {
                _historyList[button.unit] = DateTime.Now;
            }
            button.RefreshCost();
        }
        else
        {
            selectedButton = button;
        
            unitInfoPanel.gameObject.SetActive(true);
            unitInfoPanel.LoadInfoSO(button.unit);
        }
        
        AudioManager.instance.Play("ShopButton");
    }
    
    public void NextPage(bool next)
    {
        var list = InShopList;
        if (!string.IsNullOrEmpty(currentCategory))
        {
            list = list.Where(x => x.category == currentCategory).ToList();
        }

        var maxPage = list.Count / buttonsPerLine - 1;
        
        Debug.Log("Max Page: " + maxPage + "next: " + next + "current: " + currentPage);
        
        var move = next ? 1 : -1;
        if (currentPage + move > maxPage || currentPage + move < 0)
        {
            
        }
        else
        {
            currentPage += move;
            GenerateLine(currentPage, currentSortType, currentCategory, true);
        }
        
        RefreshPageUI();
    }

    private void RefreshPageUI()
    {
        //Update UI
        var maxPage = InShopList.Count / buttonsPerLine - 1;
        pageText.text = (currentPage + 1) + "/" + (maxPage + 1);
        nextButtonImage.sprite = currentPage == maxPage ? nextButtonDisabledSprite : nextButtonAbledSprite;
        prevButtonImage.sprite = currentPage == 0 ? prevButtonDisabledSprite : prevButtonAbledSprite;
    }

    public enum SortType
    {
        ByCost,
        ByHistory,
    }
}