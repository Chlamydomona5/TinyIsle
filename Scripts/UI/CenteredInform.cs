using DG.Tweening;
using TMPro;
using UnityEngine;

public class CenteredInform : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private Sequence _sequence;

    public void Open(string messageID)
    {
        if(_sequence != null && _sequence.active) _sequence.Kill();
        
        gameObject.SetActive(true);
        text.text = Methods.GetLocalText(messageID);
        
        transform.localScale = Vector3.zero;
        
        _sequence = DOTween.Sequence();
        _sequence.Append(transform.DOScale(Vector3.one * 1f, 0.5f).SetEase(Ease.OutBack));
        _sequence.AppendInterval(1f);
        _sequence.Append(transform.DOScale(Vector3.zero, 0.2f));
        _sequence.AppendCallback(() => gameObject.SetActive(false));
    }

}