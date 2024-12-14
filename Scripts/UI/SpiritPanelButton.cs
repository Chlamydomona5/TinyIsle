using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SpiritPanelButton : MonoBehaviour
{
    [SerializeField] private List<SpiritButton> buttons;
    [SerializeField] private RectTransform buttonsParent;
    [SerializeField] private SpiritButton buttonPrefab;

    private bool _open;
    
    public void OnClick()
    {
        _open = !_open;
        buttonsParent.DOAnchorPosX(_open ? 0 : -buttonsParent.sizeDelta.x, 0.25f);
    }
    
    public void AddButton(Spirit spirit)
    {
        var button = Instantiate(buttonPrefab, buttonsParent);
        buttons.Add(button);

        button.spirit = spirit;
        button.image.sprite = spirit.belongUnit.portageUnitSo.SpiritIcon;
        button.button.onClick.AddListener(() =>
        {
            CameraManager.Instance.MoveTo(spirit.transform.position, 0.25f); 
            UIManager.Instance.DisplaySpiritInfo(spirit);
        });

        _open = false;
        //Magic Number for hide the new button
        buttonsParent.anchoredPosition = new Vector2(-(buttonsParent.sizeDelta.x + 200f), buttonsParent.anchoredPosition.y);
    }
}