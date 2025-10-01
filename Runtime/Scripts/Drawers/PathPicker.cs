#if UNITY_EDITOR

using UnityEngine;

namespace Concept.UI
{
    public class PathPickerAttribute : PropertyAttribute
    {
        public string defaultPath { get; }

        public PathPickerAttribute(string defaultPath = "")
        {
            this.defaultPath = defaultPath;
        }
    }

}

#endif