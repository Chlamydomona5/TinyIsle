using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class WorldUI : MonoBehaviour
{
    private Camera _camera;
    
    private void Awake()
    {
        _camera = CameraManager.Instance.cam;
    }

    private void FixedUpdate()
    {
        // Face the camera
        if (_camera) transform.forward = _camera.transform.forward;
    }
    
    [Button]
    private void RotateToCamera()
    {
        transform.forward = Camera.main.transform.forward;
    }
}