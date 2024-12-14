using DG.Tweening;
using UnityEngine;

public class MaskBar : MonoBehaviour
{
    [SerializeField] private RectTransform content;

    private bool _on;
    
    public void OnClick()
    {
        _on = !_on;
        content.DOAnchorPos(_on ? new Vector2(0, 0) : new Vector2(content.sizeDelta.x, 0), 0.125f).SetEase(Ease.OutCubic);
    }

    public Sequence HoldFor(float seconds)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(content.DOAnchorPos(new Vector2(0, 0), 0.5f));
        sequence.AppendInterval(seconds);
        sequence.Append(content.DOAnchorPos(new Vector2(content.sizeDelta.x, 0), 0.5f));
        return sequence;
    }
}