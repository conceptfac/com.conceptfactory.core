using Concept.Core;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Concept.Helpers
{

public static class ScreenUtils
{

        #region Delegates
        public delegate void onResolutionChanged(float width, float height);
        public static onResolutionChanged OnResolutionChanged;
        #endregion

        public static string GetAspectLabel(Vector2 resolution)
    {
        // Convertendo para inteiros
        int width = Mathf.RoundToInt(resolution.x);
        int height = Mathf.RoundToInt(resolution.y);

        float epsilon = 0.01f;
        float aspect = (float)width / height;
        bool isPortrait = height > width;

        // Corrigir proporção para portrait
        if (isPortrait)
            aspect = 1f / aspect;

        // Comparando com as proporções mais comuns
        if (Mathf.Abs(aspect - (16f / 9f)) < epsilon) return isPortrait ? "9:16" : "16:9";
        if (Mathf.Abs(aspect - (16f / 10f)) < epsilon) return isPortrait ? "10:16" : "16:10";
        if (Mathf.Abs(aspect - (4f / 3f)) < epsilon) return isPortrait ? "3:4" : "4:3";
        if (Mathf.Abs(aspect - (21f / 9f)) < epsilon) return isPortrait ? "9:21" : "21:9";
        if (Mathf.Abs(aspect - (18f / 9f)) < epsilon) return isPortrait ? "9:18" : "18:9";

        return aspect.ToString("0.00");
    }

        public static void CloneRectTransform(RectTransform source, RectTransform target)
        {
            if (source == null || target == null) return;

            // Anchors e Pivot
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;

            // Se anchors são fixos (não esticados), usa anchoredPosition e sizeDelta
            if (source.anchorMin == source.anchorMax)
            {
                target.anchoredPosition = source.anchoredPosition;
                target.sizeDelta = source.sizeDelta;
            }
            else
            {
                // Se estiver esticado, usa offsetMin e offsetMax
                target.offsetMin = source.offsetMin;
                target.offsetMax = source.offsetMax;
            }

            // Transformações locais
            target.localScale = source.localScale;
            target.localRotation = source.localRotation;
        }


    }

    [Serializable] public class OnLandscapeOrientationEvent : UnityEvent { }
    [Serializable] public class OnPortraitOrientationEvent : UnityEvent { }


}