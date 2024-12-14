using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Unit_SO unit;

    [SerializeField] private Image unitImage;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI testName;
    
    [SerializeField] private Image background;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite unselectedSprite;

    private Tween _scaleTween;
    
    public void Init(Unit_SO _, UnityAction<ShopButton> onClick)
    {
        unit = _;
        RefreshCost();
        
        if(unit.Icon)
        {
            testName.text = "";
            unitImage.sprite = unit.Icon;
        }
        else
        {
            unitImage.sprite = null;
            testName.text = unit.name;
        }
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick(this));
    }

    public bool OnBuyClick()
    {
        if (PropertyManager.Instance.Gold < PropertyManager.Instance.GetCost(unit)) return false;
        
        if (GridManager.Instance.TrySpawnUnit(unit, out var entity))
        {
            PropertyManager.Instance.AddProperty(-PropertyManager.Instance.GetCost(unit));
            PropertyManager.Instance.AssignCost(unit);
            return true;
        }
        else UIManager.Instance.CenteredInform("NoSpace");

        return false;
    }

    public void RefreshCost()
    {
        costText.text = PropertyManager.Instance.GetCost(unit).ToString("N0");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.sprite = selectedSprite;
        _scaleTween?.Complete();
        _scaleTween = transform.DOScale(1.15f, 0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.sprite = unselectedSprite;
        _scaleTween?.Complete();
        _scaleTween = transform.DOScale(1f, 0.1f);
    }
}