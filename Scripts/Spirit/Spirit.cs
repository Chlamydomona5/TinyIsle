using System;
using System.Collections;
using Core;
using DG.Tweening;
using UnityEngine;

public class Spirit : PortageCreature
{
    public int currentHeart;
    public float heartProgress;

    private bool _isFull;
    public bool IsFull => _isFull;
    
    private SkinnedMeshRenderer _renderer;
    private float _fullTimer;
    
    protected virtual void Start()
    {
        SpiritManager.Instance.AddSpirit(this);
        
        buffCarrier = new BuffCarrier<PortageImpact>();
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Crash");
        animator.SetTrigger("Crash");
    }

    public void OnSpiritBackHome(bool back)
    {
        if(back)
        {
            belongUnit.model.onSpiritBack.Invoke();
            Collider.enabled = false;
            transform.SetParent(belongUnit.transform);
        }
        else
        {
            belongUnit.model.onSpiritOut.Invoke();
            Collider.enabled = true;
            transform.SetParent(null);
        }
    }

    public void AddHeartProgress(float process)
    {
        heartProgress += process;
        if (heartProgress >= 1)
        {
            currentHeart += (int)heartProgress;
            heartProgress -= (int)heartProgress;
            VFXManager.Instance.PermanentVFX("HeartUpgrade", transform, Vector3.zero);
        }
        else
        {
            VFXManager.Instance.PermanentVFX("HeartUp", transform, Vector3.zero);
        }
        
        Debug.Log($"Spirit {name} has {currentHeart} hearts, heart progress: {heartProgress}, {process} added");
    }

    public void BeFull()
    {
        _fullTimer = 5;
        
        if (!_isFull)
        {
            _isFull = true;
            StartCoroutine(FullTimer());
            _renderer.sharedMaterial.DOFloat(4, "_volume", .5f);
        }
    }
    
    private IEnumerator FullTimer()
    {
        while (_fullTimer > 0)
        {
            _fullTimer -= Time.deltaTime;
            yield return null;
        }

        _renderer.sharedMaterial.DOFloat(1, "_volume", .5f);
        _isFull = false;
    }
}