using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WorldTextUI : WorldUI
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Vector3 scale;

    private Tween _tween;
    
    private void Start()
    {
        scale = transform.localScale;
        gameObject.SetActive(false);
    }

    public void Open(string str, Vector3 pos)
    {
        if (_tween != null && _tween.IsActive()) _tween.Complete();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            _tween = transform.DOScale(scale, 0.5f).SetEase(Ease.OutBack);
        }
     
        transform.position = pos;
        text.text = str;
    }
    
    public void Close()
    {
        if (_tween != null && _tween.IsActive()) _tween.Complete();
        
        if (gameObject.activeSelf)
        {
            _tween = transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                gameObject.SetActive(false));
        }
    }
}