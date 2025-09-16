using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Vector2RangeAttribute))]
public class Vector2RangeDrawer : PropertyDrawer
{
    const float kSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            EditorGUI.LabelField(position, label.text, "Use Vector2 with Vector2Range");
            return;
        }

        Vector2RangeAttribute range = (Vector2RangeAttribute)attribute;
        Vector2 value = property.vector2Value;

        EditorGUI.BeginProperty(position, label, property);

        float line = EditorGUIUtility.singleLineHeight;
        float labelWidth = EditorGUIUtility.labelWidth;
        float contentWidth = position.width - labelWidth;
        float x = position.x;
        float y = position.y;

        // --- LABEL (nome da propriedade) ---
        Rect labelRect = new Rect(x, y + line + kSpacing, labelWidth, line); // empurra pro meio
        GUIStyle middleLeft = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft
        };
        EditorGUI.LabelField(labelRect, label, middleLeft);

        // --- VALORES (em cima do slider) ---
        Rect topValuesRect = new Rect(x + labelWidth, y, contentWidth, line);
        GUIStyle miniLeft = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
        GUIStyle miniRight = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

        EditorGUI.LabelField(
            new Rect(topValuesRect.x, topValuesRect.y, topValuesRect.width * 0.5f, topValuesRect.height),
            value.x.ToString("0.00"), miniLeft);
        EditorGUI.LabelField(
            new Rect(topValuesRect.x + topValuesRect.width * 0.5f, topValuesRect.y,
                     topValuesRect.width * 0.5f, topValuesRect.height),
            value.y.ToString("0.00"), miniRight);

        // --- SLIDER ---
        Rect sliderRect = new Rect(x + labelWidth, y + line + kSpacing, contentWidth, line);
        EditorGUI.MinMaxSlider(sliderRect, ref value.x, ref value.y, range.min, range.max);
        value.x = Mathf.Clamp(value.x, range.min, value.y);
        value.y = Mathf.Clamp(value.y, value.x, range.max);

        // --- LABELS FIXOS Min/Max (embaixo) ---
        Rect bottomLabelsRect = new Rect(x + labelWidth, y + line * 2 + kSpacing * 2, contentWidth, line);
        EditorGUI.LabelField(
            new Rect(bottomLabelsRect.x, bottomLabelsRect.y, bottomLabelsRect.width * 0.5f, bottomLabelsRect.height),
            "Min", miniLeft);
        EditorGUI.LabelField(
            new Rect(bottomLabelsRect.x + bottomLabelsRect.width * 0.5f, bottomLabelsRect.y,
                     bottomLabelsRect.width * 0.5f, bottomLabelsRect.height),
            "Max", miniRight);

        property.vector2Value = value;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // altura total
        return EditorGUIUtility.singleLineHeight * 3 + kSpacing * 3;
    }
}
