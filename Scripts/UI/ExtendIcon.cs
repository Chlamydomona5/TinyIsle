using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ExtendIcon : MonoBehaviour
{
    [SerializeField] private Image extendLine;
    [SerializeField] private Image icon;

    public void OnEnable()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(extendLine.rectTransform.DOSizeDelta(new Vector2(150f, 150f), 1f));
        sequence.Join(extendLine.DOFade(0, 1f));
        sequence.AppendInterval(1f);
        sequence.SetLoops(-1, LoopType.Restart);
    }
}