using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Reward;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Box : EntityBase
{
    public Dictionary<BoxReward, float> Rewards;

    public abstract void OnClick();
    
    public virtual void Init(Vector2Int coord, List<Vector2Int> covered, GameObject model, Dictionary<BoxReward, float> rewards)
    {
        Init(coord, covered);
        if(model)
        {
            Instantiate(model, transform);
            GetHeight(model);
        }
        Rewards = rewards;
    }
    

    public virtual void Open()
    {
        if (Methods.GetRandomValueInDict(Rewards).Reward(this))
        {
            VFXManager.Instance.PlayVFX("UseBox", transform.position + Vector3.up * 0.5f);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GridManager.Instance.boxController.RemoveBox(this);
    }
    
    public void JumpIntoWater()
    {
        transform.DOKill();
        transform.FlipToCoordAnim(transform.position, GridManager.Instance.RandomEmptyCoordInRange(1), 0.5f, 0, false).onComplete += () =>
        {
            AchievementManager.Instance.AccumulateProgress(AchievementType.FishLetGoCount);
            Destroy(gameObject);
        };
    }
}