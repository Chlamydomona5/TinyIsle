using System;
using Reward;
using Sirenix.OdinInspector;
using UnityEngine;

public class WeatherFishTank : MonoBehaviour
{ 
    [SerializeField, ReadOnly] public WeatherType currentType;
    [SerializeField, ReadOnly] private Transform fishModel;
    private Vector3 _fishOffset = new Vector3(-0.6f, 1.2f, 0.6f);
    private ParticleSystem _effectParticle;
    
    private void Start()
    {
        _effectParticle = GetComponentInChildren<ParticleSystem>();
        TimeManager.Instance.RegisterWeatherFishTank(this);
        
        TimeManager.Instance.onWeatherChanged.AddListener((_ =>
        {
            ClearFish();
            _effectParticle.Stop();
        }));
    }

    public bool RegisterFish(WeatherFishReward reward)
    {
        if(currentType != WeatherType.Normal) return false;
        
        currentType = reward.type;
        TimeManager.Instance.ArrangeNext(TimeManager.Instance.GetEvent(currentType), true);
        _effectParticle.Play();
        
        return true;
    }

    public void UpdateModel(Box box)
    {
        if(fishModel) fishModel.FlipToCoordAnim(fishModel.position, 
            GridManager.Instance.RandomEmptyCoordInRange(2), 0.5f, 0, false).onComplete += () => Destroy(fishModel.gameObject);
        fishModel = box.transform;
        box.transform.SetParent(transform, false);
        box.transform.localPosition = _fishOffset;
        Destroy(box);
    }
    
    public void ClearFish()
    {
        if(fishModel) fishModel.FlipToCoordAnim(fishModel.position, 
            GridManager.Instance.RandomEmptyCoordInRange(2), 0.5f, 0, false).onComplete += () => Destroy(fishModel.gameObject);
        currentType = WeatherType.Normal;
    }
}