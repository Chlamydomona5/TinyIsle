using System;
using System.Collections.Generic;
using CarterGames.Assets.AudioManager;
using Core;
using Reward;
using UnityEngine;

public class FishSpotEntity : MonoBehaviour
{
    [SerializeField] private FishspotAttributes visual;
    [SerializeField] private BoxCollider boxCollider;

    public ParticleSystem castParticle;
    public ParticleSystem lightParticle;
    public ParticleSystem heavyParticle;

    private FishController _controller;

    private FishSpot_SO _spotSo;

    private bool _isFishing;
    private float destroyTimer;

    public void Init(FishSpot_SO spotSo, FishController controller)
    {
        _spotSo = spotSo;
        _controller = controller;

        visual.Init(spotSo.mainColor, spotSo.minColor, spotSo.scale);
        boxCollider.transform.localScale = new Vector3(spotSo.scale, 1, spotSo.scale);

        destroyTimer = FishController.fishSpotDisappearTime;
    }

    public void OnClick()
    {
        _controller.StartFishAt(this);
        visual.disappear();
        AudioManager.instance.Play("ClickFishSpot");
        _isFishing = true;
    }

    private void FixedUpdate()
    {
        destroyTimer -= Time.fixedDeltaTime;
        if (destroyTimer <= 0 && !_isFishing)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        _controller.RemoveSpot(this);
    }

    public Fish_SO GetFish()
    {
        var fish = Methods.GetRandomValueInDict(_spotSo.Fishes);
        return fish;
    }
}