using AMS.UI.SoftMask;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewItemPanel : MonoBehaviour,IPointerClickHandler
{
    public UISoftMask mask;
    public Image background;
    public Image icon;
    public TextMeshProUGUI text;
    
    private UnityEvent _onClose = new UnityEvent();
    private Sequence _sequence;

    public UnityEvent Open(string itemName, string itemDesc, Sprite itemIcon)
    {
        UIManager.Instance.CloseUI();
        
        gameObject.SetActive(true);
        
        background.transform.localScale = Vector3.zero;
        mask.opacity = 0;
        
        _sequence = DOTween.Sequence();
        _sequence.Append(DOTween.To(() => mask.opacity, x => mask.opacity = x, 1f, 0.5f));
        _sequence.AppendInterval(0.5f);
        _sequence.AppendCallback(() => AudioManager.instance.Play("NewItemPanel"));
        _sequence.Append(background.transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.OutBack));
        _sequence.AppendInterval(.75f);
        icon.sprite = itemIcon;
        text.text = $"<size=32><color=#FFFFA0>{itemName}: </color></size>{itemDesc}";
        
        return _onClose;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_sequence.IsPlaying())
        {
            gameObject.SetActive(false);
            _onClose.Invoke();
            _onClose.RemoveAllListeners();
        }
    }
}