using Concept.Helpers;
using UnityEditor;
using UnityEngine;

namespace Concept.Editor
{
    [CustomPropertyDrawer(typeof(Vector2Range))]
    public class Vector2RangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty x = property.FindPropertyRelative("x");
            SerializedProperty y = property.FindPropertyRelative("y");

            float width = position.width / 2;
            EditorGUI.Slider(new Rect(position.x, position.y, width - 2, position.height), x, 0f, 1f, new GUIContent("X"));
            EditorGUI.Slider(new Rect(position.x + width + 2, position.y, width - 2, position.height), y, 0f, 1f, new GUIContent("Y"));

            EditorGUI.EndProperty();
        }
    }
}