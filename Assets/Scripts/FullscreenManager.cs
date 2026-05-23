using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenManager : MonoBehaviourSingletonPersistent<FullscreenManager>
{

    void Start()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow; // Or ExclusiveFullScreen
        Screen.SetResolution(1920, 1080, true); // Set resolution and fullscreen
    }

}
