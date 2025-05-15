#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using Concept.Helpers;
using Concept.Core;

namespace Concept.Editor
{


[InitializeOnLoad]
public static class GameViewResolutionMonitor
{
    private static Vector2 lastResolution = Vector2.zero;

    static GameViewResolutionMonitor()
    {
        EditorApplication.update += CheckResolution;
    }

    private static void CheckResolution()
    {
        Vector2Int currentResolution = GetMainGameViewResolution();

        if (lastResolution != currentResolution && currentResolution.x > 0 && currentResolution.y > 0)
        {
            lastResolution = currentResolution;
            bool isPortrait = currentResolution.y > currentResolution.x;
                ScreenMonitor.OnResolutionChanged?.Invoke(currentResolution.x,currentResolution.y);
                Debug.Log($"[GameViewResolutionMonitor] Resolution changed: {currentResolution.x}x{currentResolution.y} (Aspect Ratio: {ScreenUtils.GetAspectLabel(currentResolution)})");
        }
    }

    public static Vector2Int GetMainGameViewResolution()
    {
        Type T = Type.GetType("UnityEditor.GameView,UnityEditor");
        MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);

        if (GetSizeOfMainGameView != null)
        {
            object resolution = GetSizeOfMainGameView.Invoke(null, null);

                if (resolution is Vector2 resolutionVec2)
                {
                    // Converter para Vector2Int
                    return new Vector2Int((int)resolutionVec2.x, (int)resolutionVec2.y);
                }

        }

        return Vector2Int.zero;
    }

   




}
#endif
}