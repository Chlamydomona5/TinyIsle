using System;
using CarterGames.Assets.AudioManager;
using DG.Tweening;
using UnityEngine;

[Serializable]
public abstract class WeatherEvent
{
    public abstract WeatherType WeatherType { get; }
    
    public abstract void Start();
    public abstract void Execute();
    public abstract void End();
    
    protected void BGMFadeIn(string bgmName)
    {
        MusicPlayer.instance.PlayTrack(Resources.Load<AudioClip>(bgmName), TransitionType.Fade);
    }
    
    protected void BGMFadeOut()
    {
        MusicPlayer.instance.StopTrack();
    }
}

public class NormalEvent : WeatherEvent
{
    public override WeatherType WeatherType => WeatherType.Normal;

    public override void Start()
    {
    }

    public override void Execute()
    {
    }

    public override void End()
    {
    }
    
}

public class RainEvent : WeatherEvent
{
    public override WeatherType WeatherType => WeatherType.Rain;

    public override void Start()
    {
        TimeManager.Instance.visual.startRain();
        BGMFadeIn("Weather_Rain");
    }

    public override void Execute()
    {
    }

    public override void End()
    {
        TimeManager.Instance.visual.stopRain();
        BGMFadeOut();
    }
}

public class SunnyEvent : WeatherEvent
{
    public override WeatherType WeatherType => WeatherType.Sunny;

    public override void Start()
    {
        TimeManager.Instance.visual.startSun();
        BGMFadeIn("Weather_Sunny");
    }

    public override void Execute()
    {
    }

    public override void End()
    {
        TimeManager.Instance.visual.stopSun();
        BGMFadeOut();
        
    }
}

public enum WeatherType
{
    Normal,
    Rain,
    Sunny,
}
