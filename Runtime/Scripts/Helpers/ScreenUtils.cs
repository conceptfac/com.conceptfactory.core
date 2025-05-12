using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Concept.Helpers
{

public static class ScreenUtils
{

        #region Delegates
        public delegate void onResolutionChanged(int width, int height);
        public static onResolutionChanged OnResolutionChanged;
        #endregion

        public static string GetAspectLabel(Vector2Int resolution)
        {
            // Convertendo para inteiros
            int width = Mathf.RoundToInt(resolution.x);
            int height = Mathf.RoundToInt(resolution.y);

            // Verificação de validade
            if (width <= 0 || height <= 0)
                return "Invalid";

            // Reduz a fração (ex: 2400x1080 → 20:9)
            int gcd = GCD(width, height);
            int aspectW = width / gcd;
            int aspectH = height / gcd;

            return $"{aspectW}:{aspectH}";
        }

        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return Mathf.Abs(a);
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