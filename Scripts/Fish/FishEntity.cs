using System;
using DG.Tweening;
using UnityEngine;

public class FishEntity : Box
{
    public static float YOffsetInWater = -0.55f;
    
    public Fish_SO data;
    
    public bool InWater
    {
        get => _inWater;
        set
        {
            _inWater = value;
            SetSign("NotInWater", !value);
            if(_animation)
                if (InWater)
                {
                    if(_animation.GetClip("InWater"))
                        _animation.Play("InWater");
                }
                else
                {
                    if(_animation.GetClip("GroundJump"))
                        _animation.Play("GroundJump");
                }
        }
    }

    private bool _inWater;
    
    private float _groundTimer;
    private bool _dead;
    
    private Animation _animation;
    
    public void Init(Vector2Int coord, Fish_SO fishSO)
    {
        base.Init(coord, fishSO.coveredCoords, fishSO.modelPrefab, fishSO.Rewards);
        data = fishSO;
        _animation = GetComponentInChildren<Animation>();
    }
    
    public override void OnClick()
    {
        if (InWater)
        {
            Open();
        }
        else
        {
            var water = true;
            var place = GridManager.Instance.FindPlaceForEntity(coveredCoordsRaw, GroundType.Low);
            if (place.x == -1000)
            {
                water = false;
                place = coordinate;
            }
            
            transform.FlipToCoordAnim(transform.position, place, 0.5f, 0, false).onComplete += () =>
            {
                InWater = water;
                MoveTo(place, InWater ? YOffsetInWater : 0);
            };
        }
    }

    private void FixedUpdate() {
        if (!InWater && !_dead)
        {
            _groundTimer += Time.fixedDeltaTime;
            if (_groundTimer >= 15)
            {
                _groundTimer = 0;
                _dead = true;
                JumpIntoWater();
            }
        }
    }
    
    
}