using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Concept.Helpers
{

    public static class ScreenUtils
    {

        public static string GetAspectLabel(int width, int height)
        {
            return GetAspectLabel(new Vector2Int(width, height));
        }
        public static string GetAspectLabel(Vector2Int resolution)
        {
            int width = Mathf.RoundToInt(resolution.x);
            int height = Mathf.RoundToInt(resolution.y);

            if (width <= 0 || height <= 0)
                return "Invalid";

            float aspect = (float)width / height;
            float aspectInverted = (float)height / width;

            var knownRatios = new Dictionary<string, float>
    {
        { "1:1", 1f },
        { "4:3", 4f / 3f },
        { "3:2", 3f / 2f },
        { "5:4", 5f / 4f },
        { "16:10", 16f / 10f },
        { "16:9", 16f / 9f },
        { "18:9", 18f / 9f },
        { "19.5:9", 19.5f / 9f },
        { "20:9", 20f / 9f },
        { "21:9", 21f / 9f },
        { "32:9", 32f / 9f },
        { "2.35:1", 2.35f },
        { "2.39:1", 2.39f },
    };

            const float tolerance = 0.015f;

            // Tenta encontrar proporção padrão
            foreach (var pair in knownRatios)
            {
                if (Mathf.Abs(aspect - pair.Value) < tolerance)
                    return pair.Key;

                if (Mathf.Abs(aspectInverted - pair.Value) < tolerance)
                {
                    // Inverte o rótulo da proporção também (ex: "4:3" vira "3:4")
                    string[] parts = pair.Key.Split(':');
                    return $"{parts[1]}:{parts[0]}";
                }
            }

            // Se não for conhecida, retorna a forma reduzida
            int gcd = GCD(width, height);
            return $"{width / gcd}:{height / gcd}";
        }

        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
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
}