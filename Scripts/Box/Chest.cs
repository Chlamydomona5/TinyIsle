using System;
using System.Collections;
using System.Collections.Generic;
using CarterGames.Assets.AudioManager;
using MoreMountains.Feedbacks;
using Reward;
using Sirenix.OdinInspector;
using UnityEngine;

public class Chest : Box
{
    [SerializeField] private Dictionary<BoxReward, float> _rewards = new();

    [SerializeField] private List<MMF_Player> shakers = new();

    [SerializeField, ReadOnly] private int touchCount;
    private float _tapResetTime = 1f;
    private Coroutine _tapCountCoroutine;

    public override void OnClick()
    {
        Debug.Log("Chest clicked");
        
        if (shakers.Count > touchCount)
        {
            if (touchCount - 1 >= 0)
                shakers[touchCount - 1].StopFeedbacks();
            shakers[touchCount].PlayFeedbacks();
        }

        touchCount++;

        if (_tapCountCoroutine != null) StopCoroutine(_tapCountCoroutine);
        _tapCountCoroutine = StartCoroutine(TapCount());

        AudioManager.instance.Play("ClickChest");
    }

    public override void Init(Vector2Int coord, List<Vector2Int> covered, GameObject model,
        Dictionary<BoxReward, float> rewards)
    {
        base.Init(coord, covered, model, rewards);
        shakers[shakers.Count - 1].Events.OnComplete.AddListener(() => Open());
    }

    public override void Open()
    {
        base.Open();
        AudioManager.instance.Play("OpenChest");
    }

    private IEnumerator TapCount()
    {
        yield return new WaitForSeconds(_tapResetTime);
        touchCount = 0;
    }
}