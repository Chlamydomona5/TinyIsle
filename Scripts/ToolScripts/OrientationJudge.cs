using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class OrientationJudge : MonoBehaviour
{
    public UnityEvent<bool> onOrientationChange;
    [SerializeField] private Canvas canvas;
    
    private bool _lastPortrait;

    private void Start()
    {
        onOrientationChange.AddListener(CanvasActive);
    }

    private void FixedUpdate()
    {
        bool portrait = Screen.width < Screen.height;
        if (portrait != _lastPortrait)
        {
            _lastPortrait = portrait;
            onOrientationChange.Invoke(portrait);
        }
    }

    private void CanvasActive(bool portrait)
    {
        canvas.enabled = portrait;
    }
}