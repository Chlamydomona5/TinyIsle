using System;
using DG.Tweening;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private float startSize;
    [SerializeField] private Vector2 scaleRange;
    [SerializeField] private float moveRange;
    [SerializeField] private float height = 3f;
    
    [SerializeField] private Camera uiCamera;
    public Camera cam;
    private bool _touchForbidden;
    private Transform _followTran;
    private float _followTimer;
    private Tween _moveTween;
    private Tween _rotateTween;
    
    public override void Awake()
    {
        base.Awake();
        cam = GetComponentInChildren<Camera>();
        
        SetSize(startSize);
    }
    
    public void DragMove(Vector3 delta)
    {
        if (_touchForbidden) return;
        var pos = transform.position + Methods.YtoZero(delta);
        transform.position = new Vector3(Mathf.Clamp(pos.x, -moveRange, moveRange), pos.y, Mathf.Clamp(pos.z, -moveRange, moveRange));
    }

    public void MoveTo(Vector3 pos)
    {
        transform.position = Methods.YtoZero(pos) + Vector3.up * height;
    }

    public void MoveTo(Vector3 pos, float time)
    {
        if(_moveTween != null && _moveTween.IsPlaying()) _moveTween.Complete();
        
        _touchForbidden = true;
        _moveTween = transform.DOMove(Methods.YtoZero(pos) + Vector3.up * height, time).SetEase(Ease.OutCubic).OnComplete(() => _touchForbidden = false);
    }
    
    public void Zoom(float delta)
    {
        if (_touchForbidden) return;
        
        var orthographicSize = Mathf.Clamp(cam.orthographicSize * delta, scaleRange.x, scaleRange.y);
        SetSize(orthographicSize);
    }

    public void SetSize(float size)
    {
        cam.orthographicSize = size;
        uiCamera.orthographicSize = size;
    }

    public void ForbidTouch(bool active)
    {
        _touchForbidden = active;
    }

    public void Follow(Transform trans, float duration)
    {
        ForbidTouch(true);
        _followTran = trans;
        _followTimer = duration;
    }
    
    public void Unfollow()
    {
        ForbidTouch(false);
        _followTran = null;
    }

    private void Update()
    {
        if (_touchForbidden && _followTran)
        {
            transform.position = _followTran.position;
            _followTimer -= Time.deltaTime;
            if (_followTimer <= 0) Unfollow();
        }
    }
    
    public void ResetCamera()
    {
        Unfollow();
        transform.position = Methods.YtoZero(Vector3.zero) + Vector3.up * height;
        cam.orthographicSize = 10;
        uiCamera.orthographicSize = 10;
    }

    public void Rotate(bool forward)
    {
        if(_rotateTween != null && _rotateTween.IsPlaying()) _rotateTween.Complete();
        
        _rotateTween = transform.DORotate(transform.rotation.eulerAngles + new Vector3(0, forward ? 90 : -90, 0), .25f)
            .SetEase(Ease.OutCubic);
    }

    public void FoucsTo(UnitEntity unitEntity)
    {
        SetSize(5);
        MoveTo(unitEntity.transform.position - Vector3.right * 1 + Vector3.forward * 1, 0.25f);
    }
    
    public void ScaleWindow(int size)
    {
        var vector = new Vector2(16, 9) * size;
        Screen.SetResolution((int)vector.x, (int)vector.y, true);
    }
}