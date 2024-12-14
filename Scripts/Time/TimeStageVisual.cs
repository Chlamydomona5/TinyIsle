using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimeStageVisual", menuName = "Time/TimeStageVisual")]
public class TimeStageVisual : ScriptableObject
{
    public Vector3 lightRotation;
    public Color lightColor;
    public float lightIntensity;
    
    [ColorUsage(true, true)]
    public Color deepWaterColor;
    [ColorUsage(true, true)]
    public Color shallowWaterColor;
    [ColorUsage(true, true)]
    public Color bubbleColor;
    [ColorUsage(true, true)]
    public Color toxicColor;
    
    public float starLightIntensity;
    public float floatIntensity;
    public float scatteringIntensity;
    
    public float globalVolumeBloomThreshold;
    public float globalVolumeBloomIntensity;
    
    public float fogIntensity;
    public float fogHeightEnd;
    public float skyboxFogIntensity;
}
