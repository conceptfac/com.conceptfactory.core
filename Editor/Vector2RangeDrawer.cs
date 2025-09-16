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

        // --- LABEL (nome da propriedade), centralizado ---
        Rect labelRect = new Rect(x, y + line + kSpacing, labelWidth, line);
        GUIStyle middleLeft = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
        EditorGUI.LabelField(labelRect, label, middleLeft);

        // --- CAMPOS EDITÁVEIS (em cima do slider) ---
        float fieldWidth = 50f;
        Rect minFieldRect = new Rect(x + labelWidth, y, fieldWidth, line);
        Rect maxFieldRect = new Rect(x + labelWidth + contentWidth - fieldWidth, y, fieldWidth, line);

        value.x = EditorGUI.FloatField(minFieldRect, value.x);
        value.y = EditorGUI.FloatField(maxFieldRect, value.y);

        // --- SLIDER ---
        Rect sliderRect = new Rect(x + labelWidth, y + line + kSpacing, contentWidth, line);
        EditorGUI.MinMaxSlider(sliderRect, ref value.x, ref value.y, range.min, range.max);

        // Garante consistência (min ≤ max e dentro do range)
        value.x = Mathf.Clamp(value.x, range.min, value.y);
        value.y = Mathf.Clamp(value.y, value.x, range.max);

        // --- LABELS FIXOS Min/Max (embaixo) ---
        Rect bottomLabelsRect = new Rect(x + labelWidth, y + line * 2 + kSpacing * 2, contentWidth, line);
        GUIStyle miniLeft = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
        GUIStyle miniRight = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
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
        return EditorGUIUtility.singleLineHeight * 3 + kSpacing * 3;
    }
}
