using UnityEngine;

namespace Concept.Helpers
{
    [System.Serializable]
    public struct Vector2Range
    {
        public float x; // min
        public float y; // max

        public Vector2Range(float min, float max)
        {
            x = min;
            y = max;
        }
    }
}