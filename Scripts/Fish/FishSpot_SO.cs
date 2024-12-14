
using System.Collections.Generic;
using System.Linq;
using Core;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "FishSpot", menuName = "SO/FishSpot")]
public class FishSpot_SO : SerializedScriptableObject
{
    public float scale;
    public Color mainColor;
    public Color minColor;
    public Dictionary<Fish_SO, float> Fishes = new();
}

public class FishSpotSet
{
    public Dictionary<FishSpot_SO, float> SpotsDay = new();
    public Dictionary<FishSpot_SO, float> SpotsNight = new();
    
    public Dictionary<FishSpot_SO, float> SpotsDayRain = new();
    public Dictionary<FishSpot_SO, float> SpotsNightRain = new();
    
    public Dictionary<FishSpot_SO, float> SpotsDaySunny = new();
    
    public FishSpot_SO GetSpot()
    {
        var weather = TimeManager.Instance.CurrentWeather.WeatherType;
        var time = TimeManager.Instance.CurrentTime;
        
        // Get the right dictionary
        Dictionary<FishSpot_SO, float> dict = null;
        if(time == TimeStage.EarlyDay || time == TimeStage.LateDay)
        {
            if(weather == WeatherType.Rain)
                dict = SpotsDayRain;
            else if(weather == WeatherType.Sunny)
                dict = SpotsDaySunny;
            else
                dict = SpotsDay;
        }
        else
        {
            if(weather == WeatherType.Rain)
                dict = SpotsNightRain;
            else
                dict = SpotsNight;
        }

        return Methods.GetRandomValueInDict(dict);
    }
}
