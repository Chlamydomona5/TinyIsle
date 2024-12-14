using System;
using DG.Tweening;
using UnityEngine;

public class TheSeed : Box
{
    private Sequence _seq;

    public override void OnClick()
    {
        if(_seq != null) return;
        _seq = DOTween.Sequence();
        _seq.Append(transform.DOScale(Vector3.one * 1.5f, 0.5f));
        _seq.Join(transform.DOMoveY(transform.position.y + 2f, 0.5f));
        _seq.AppendInterval(0.25f);
        _seq.Append(transform.FlipToCoordAnim(transform.position, Vector2Int.one, 0.5f, 0, false));
        _seq.Append(transform.DOScale(Vector3.zero, 0.2f));
        _seq.AppendCallback((() =>
        {
            PropertyManager.Instance.AddCurrentGoldLimit(1);
            Destroy(gameObject);
        }));
    }
}