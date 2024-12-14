using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TipUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float time = 5f;
    private float _timer;

    public void Init(string str)
    {
        text.text = str;
        _timer = time;

        background.anchoredPosition = new Vector2(-1000f, 0);
        background.DOAnchorPosX(0, 0.5f);
    }

    private void FixedUpdate()
    {
        _timer -= Time.fixedDeltaTime;
        if (_timer <= 0)
        {
            Close();
        }
    }

    public void Close()
    {
        background.DOAnchorPosX(-1000f, 0.5f).onComplete = () => Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Close();
    }
}