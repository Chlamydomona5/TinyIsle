using System;
using System.Collections;
using System.Collections.Generic;
using AtmosphericHeightFog;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class TimeManager : Singleton<TimeManager>
{
    [SerializeField] private float naturalDayRainPossibility = 0.1f;
    [SerializeField] private float naturalNightRainPossibility = 0.3f;
    [SerializeField] private float naturalSunnyPossibility = 0.1f;
    
    public TimeStage CurrentTime
    {
        get
        {
            var minute = _dayTimer;
            
            if (minute >= 0 && minute < 5) return TimeStage.EarlyDay;
            if (minute >= 5 && minute < 10) return TimeStage.LateDay;
            if (minute >= 10 && minute < 15) return TimeStage.EarlyNight;
            if (minute >= 15 && minute < 20) return TimeStage.LateNight;
            
            return TimeStage.None;
        }
    }
    public UnityEvent<TimeStage> onTimeChanged = new();

    [SerializeField] private Light directionalLight;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private HeightFogGlobal fogGlobal;
    
    private TimeStage _lastTime = TimeStage.LateNight;
    private int _dayTimer;

    public int DayTimer
    {
        get => _dayTimer;
        set => _dayTimer = value % 20;
    }
    
    [SerializeField,ReadOnly] public WeatherFishTank weatherFishTank;
    
    [HideInInspector] public UnityEvent<WeatherType> onWeatherChanged = new();
    
    public WeatherEvent CurrentWeather; 
    public WeatherEvent NextWeather;
    
    public WeatherVisual visual;
    
    [OdinSerialize] private Dictionary<TimeStage, TimeStageVisual> _timeStages = new();
    [OdinSerialize] private Dictionary<TimeStage, List<GameObject>> _timeStagesEffect = new();

    private bool _firstUpdateWeather;
    
    private void Start()
    {
        StartCoroutine(UpdateTime());
    }

    private IEnumerator UpdateTime()
    {
        while (true)
        {
            DayTimer += 1;
            TryUpdateStage();
            
            //If there is a weather event running
            if (CurrentWeather != null)
            {
                CurrentWeather.Execute();
            }
            
            yield return new WaitForSeconds(60f);
        }
    }

    private void TryUpdateStage()
    {
        if (CurrentTime != _lastTime)
        {
            onTimeChanged.Invoke(CurrentTime);

            directionalLight.transform.DORotate(_timeStages[CurrentTime].lightRotation, 1f);
            directionalLight.DOColor(_timeStages[CurrentTime].lightColor, 1f);
            directionalLight.DOIntensity(_timeStages[CurrentTime].lightIntensity, 1f);

            waterMaterial.DOColor(_timeStages[CurrentTime].deepWaterColor, "__2", 1f);
            waterMaterial.DOColor(_timeStages[CurrentTime].shallowWaterColor, "__3", 1f);
            waterMaterial.DOColor(_timeStages[CurrentTime].bubbleColor, "__5", 1f);
            waterMaterial.DOColor(_timeStages[CurrentTime].toxicColor, "__11", 1f);
            waterMaterial.DOFloat(_timeStages[CurrentTime].starLightIntensity, "__28", 1f);
            waterMaterial.DOFloat(_timeStages[CurrentTime].floatIntensity, "__27", 1f);
            waterMaterial.DOFloat(_timeStages[CurrentTime].scatteringIntensity, "__17", 1f);
            
            globalVolume.profile.TryGet<Bloom>(out var bloom);
            DOTween.To(() => bloom.threshold.value, x => bloom.threshold.value = x, _timeStages[CurrentTime].globalVolumeBloomThreshold, 1f);
            DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, _timeStages[CurrentTime].globalVolumeBloomIntensity, 1f);
            
            DOTween.To(() => fogGlobal.fogIntensity, x => fogGlobal.fogIntensity = x, _timeStages[CurrentTime].fogIntensity, 1f);
            DOTween.To(() => fogGlobal.fogHeightEnd, x => fogGlobal.fogHeightEnd = x, _timeStages[CurrentTime].fogHeightEnd, 1f);
            DOTween.To(() => fogGlobal.skyboxFogIntensity, x => fogGlobal.skyboxFogIntensity = x, _timeStages[CurrentTime].skyboxFogIntensity, 1f);
            
            foreach (var effect in _timeStagesEffect[_lastTime])
            {
                effect.SetActive(false);
            }
            
            foreach (var effect in _timeStagesEffect[CurrentTime])
            {
                effect.SetActive(true);
            }
            
            _lastTime = CurrentTime;
            
            if (CurrentWeather != null)
            {
                CurrentWeather.End();
            }
            
            if(!_firstUpdateWeather)
            {
                StartNewWeather(true);
                _firstUpdateWeather = true;
            }
            else
            {
                StartNewWeather(false);
            }
        }
    }

    public void SetTimeStageToNext()
    {
        DayTimer += 5;
        TryUpdateStage();
        onTimeChanged.Invoke(CurrentTime);
    }
    
    public void ArrangeNext(WeatherEvent weather, bool forceEnd = false)
    {
        NextWeather = weather;
        if (forceEnd)
        {
            CurrentWeather?.End();
            StartNewWeather();
        }
    }

    private void StartNewWeather(bool normal = false)
    {
        //If there is a weather event waiting
        if (NextWeather != null)
        {
            CurrentWeather = NextWeather;
            NextWeather = null;
        }
        else
        {
            //Natural Weather
            if(CurrentTime == TimeStage.EarlyDay || CurrentTime == TimeStage.LateDay)
            {
                if (Random.value < naturalDayRainPossibility) CurrentWeather = GetEvent(WeatherType.Rain);
                else if (Random.value < naturalSunnyPossibility) CurrentWeather = GetEvent(WeatherType.Sunny);
                else CurrentWeather = GetEvent(WeatherType.Normal);
            }
            else if(CurrentTime == TimeStage.EarlyNight || CurrentTime == TimeStage.LateNight)
            {
                if (Random.value < naturalNightRainPossibility) CurrentWeather = GetEvent(WeatherType.Rain);
                else CurrentWeather = GetEvent(WeatherType.Normal);
            }
            else
            {
                CurrentWeather = GetEvent(WeatherType.Normal);
            }
            
            if (normal)
            {
                CurrentWeather = GetEvent(WeatherType.Normal);
            }
        }

        CurrentWeather?.Start();
        onWeatherChanged.Invoke(CurrentWeather.WeatherType);
    }
    
    public void RegisterWeatherFishTank(WeatherFishTank tank)
    {
        weatherFishTank = tank;
    }
    
    
    public WeatherEvent GetEvent(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Rain:
                return new RainEvent();
            case WeatherType.Sunny:
                return new SunnyEvent();
            case WeatherType.Normal:
                return new NormalEvent();
            default:
                return null;
        }
    }
}

[Flags]
public enum TimeStage
{
    None = 0,
    EarlyDay = 1,
    LateDay = 2,
    EarlyNight = 4,
    LateNight = 8,
    All = EarlyDay | LateDay | EarlyNight | LateNight
}