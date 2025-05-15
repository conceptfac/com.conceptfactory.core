using System;
using Concept.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Concept.Core
{

[Serializable] public class OnLandscapeOrientationEvent : UnityEvent { }
[Serializable] public class OnPortraitOrientationEvent : UnityEvent { }


public class ScreenMonitor : TSingleton<ScreenMonitor>
{
    private ScreenOrientation _lastScreenOrientation;

    #region Delegates
    public delegate void onResolutionChanged(int width, int height);
    public static onResolutionChanged OnResolutionChanged;
        #endregion

        private void OnEnable()
        {
            DontDestroyOnLoad(this);
        }

        protected override void Update()
        {
            base.Update();
        if (_lastScreenOrientation != Screen.orientation)
        {
            _lastScreenOrientation = Screen.orientation;
            OnResolutionChanged?.Invoke(Screen.width, Screen.height);
        }

    }
}


}