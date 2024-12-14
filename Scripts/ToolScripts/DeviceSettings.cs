using System;
using UnityEngine;

public class DeviceSettings : MonoBehaviour
{
    private void Start()
    {
        if(TestManager.Instance.onTest) return;
        //Set the target frame rate to the current refresh rate
        Application.targetFrameRate = 60;
        //Lock the screen orientation to landscape
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}