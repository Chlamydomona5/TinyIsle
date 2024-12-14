using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToClose : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
    }
}