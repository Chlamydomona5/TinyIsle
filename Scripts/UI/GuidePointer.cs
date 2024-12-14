using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GuidePointer : WorldUI
{
    [SerializeField] private Image press, release;
    
    private Sequence _currentTween;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void DragMode(Vector3 start, Vector3 end)
    {
        var sequence = StartPointer();

        _rectTransform.position = start;
        sequence.AppendCallback((() =>
        {
            SetPress(false);
            _rectTransform.position = start;
        }));
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback((() =>
        {
            SetPress(true);
        }));
        sequence.AppendInterval(0.25f);
        sequence.Append(_rectTransform.DOMove(end, 1.25f).SetEase(Ease.OutCubic));
        sequence.AppendInterval(0.5f);
        sequence.Append(_rectTransform.DOMove(start, .75f));
        sequence.AppendCallback(() => SetPress(false));
        sequence.AppendCallback(() => DragMode(start, end));
    }

    public void TapMode(Vector3 pos)
    {
        var sequence = StartPointer();
        
        _rectTransform.position = pos;
        sequence.AppendCallback((() =>
        {
            SetPress(true);
            _rectTransform.position = pos;
        }));
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() => SetPress(false));
        sequence.Append(_rectTransform.DOMoveY(_rectTransform.position.y - .1f, 0.5f));
        sequence.AppendCallback(() =>
        {
            _rectTransform.position = pos;
        });
        sequence.AppendCallback(() => TapMode(pos));

    }

    private Sequence StartPointer()
    {
        Sequence sequence = DOTween.Sequence();
        _currentTween?.Kill();
        _currentTween = sequence;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            _rectTransform.localScale = Vector3.zero;
            sequence.Append(_rectTransform.DOScale(Vector3.one, 0.75f));
        }
        
        return sequence;
    }

    public void LongPressMode(Vector3 pos)
    {
        var sequence = StartPointer();
        
        _rectTransform.position = (pos);
        sequence.AppendCallback((() =>
        {
            SetPress(true);
            _rectTransform.position = (pos);
        }));
        sequence.AppendInterval(1.25f);
        sequence.AppendCallback(() => SetPress(false));
        sequence.Append(_rectTransform.DOMoveY(_rectTransform.position.y - .1f, 0.75f));
        sequence.AppendCallback(() => LongPressMode(pos));
    }
    
    public void TurnOff()
    {
        _currentTween?.Kill();
        gameObject.SetActive(false);
    }

    private void SetPress(bool on)
    {
        press.gameObject.SetActive(on);
        release.gameObject.SetActive(!on);
    }
}