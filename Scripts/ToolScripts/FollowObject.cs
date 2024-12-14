using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;


public class FollowObject : MonoBehaviour
{
    public Transform followTran;
    public Transform lookAtTran;
    public Vector3 offset;

    public bool isTweening;
    private bool _needUpdate = true;

    private Transform _originTran;
    private Vector3 _originOffset;
    private Vector3 _originRotation;

    private Tween _nowTween;
    public UnityEvent OnTweenEnd;

    private void Awake()
    {
        _originTran = followTran;
        _originOffset = offset;
        _originRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (_needUpdate)
        {
            if (followTran)
                transform.position = followTran.position + offset;
            if (lookAtTran) transform.LookAt(lookAtTran, Vector3.up);
        }
    }

    public void ResetFollow(float time = 1f)
    {
        //End now Tween
        if (_nowTween != null && _nowTween.active) _nowTween.Complete();

        if (followTran != _originTran)
            SetNewOffsetWithoutAngle(_originOffset, _originTran, time);
        else SetNewOffsetWithoutAngle(_originOffset, null, time);
    }

    public Tween SetNewOffsetWithAngle(Vector3 newOffset, Vector3 rotation, Transform follow = null,
        float time = 2f)
    {
        isTweening = true;
        //Try to make sure rotate is faster than move
        transform.DORotate(rotation, time);
        if (follow)
        {
            _needUpdate = false;
            followTran = follow;
            offset = newOffset;
            return _nowTween = transform.DOMove(followTran.position + newOffset, time).OnComplete(delegate
            {
                isTweening = false;
                OnTweenEnd.Invoke();
                OnTweenEnd.RemoveAllListeners();
                _needUpdate = true;
            });
        }
        else
        {
            return _nowTween = DOTween.To(() => offset, x => offset = x, newOffset, time).OnComplete(delegate
            {
                isTweening = false;
                OnTweenEnd.Invoke();
                OnTweenEnd.RemoveAllListeners();
            });
        }
    }

    public Tween SetNewOffsetWithoutAngle(Vector3 newOffset, Transform follow = null, float time = 2f)
    {
        return _nowTween = SetNewOffsetWithAngle(newOffset, _originRotation, follow, time);
    }
}
