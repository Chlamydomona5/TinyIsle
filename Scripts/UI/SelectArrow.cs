using System;
using DG.Tweening;
using UnityEngine;

public class SelectArrow : MonoBehaviour
{
    [SerializeField] private Transform arrow;
    
    private void Start()
    {
        //Look at camera
        arrow.LookAt(Camera.main.transform);
        
        arrow.DOLocalMoveY(arrow.localPosition.y + 0.25f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        arrow.DORotate(new Vector3(0, 180f, 90f), 4f).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
    }
    
}