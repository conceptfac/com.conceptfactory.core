using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Concept.UI;

namespace Concept.Editor
{


    [CustomPropertyDrawer(typeof(VectorRangeAttribute))]
    public class MultiHandleVectorDrawer : PropertyDrawer
    {
        const float kHandleWidth = 10f;
        const float kLineHeight = 18f;
        const float kSpacing = 2f;
        const float kMargin = 3f;
        const float kHandleRadius = 3f;

        private static Dictionary<string, int> activeHandles = new Dictionary<string, int>();

        private string GetPropertyKey(SerializedProperty property)
            => property.serializedObject.targetObject.GetInstanceID() + "_" + property.propertyPath;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var range = (VectorRangeAttribute)attribute;

            int dimension = property.propertyType switch
            {
                SerializedPropertyType.Vector2 => 2,
                SerializedPropertyType.Vector3 => 3,
                SerializedPropertyType.Vector4 => 4,
                _ => 0
            };

            if (dimension == 0)
            {
                EditorGUI.LabelField(position, label.text, "Use Vector2, Vector3 ou Vector4");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // --- Desenha borda em volta de todo o elemento ---
            Color prevColor = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.6f, 0.6f, 0.6f);
            GUI.DrawTexture(new Rect(position.x, position.y, position.width, position.height), Texture2D.whiteTexture);
            GUI.color = prevColor;

            // --- Label principal centralizado verticalmente ---
            float totalHeight = GetPropertyHeight(property, label);
            float labelHeight = EditorGUIUtility.singleLineHeight;
            float labelY = position.y + (totalHeight - labelHeight) / 2f; // centraliza verticalmente
            Rect labelRect = new Rect(position.x + 8f, labelY, EditorGUIUtility.labelWidth, labelHeight);
            GUIStyle middleLeft = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
            EditorGUI.LabelField(labelRect, label, middleLeft);

            // --- Conteúdo (fields + slider) ---
            Rect contentRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth - 8f, totalHeight);

            // --- Valores atuais ---
            Vector4 value = Vector4.zero;
            if (dimension == 2) value = property.vector2Value;
            else if (dimension == 3) value = property.vector3Value;
            else value = property.vector4Value;

            // --- Campos editáveis em cima ---
            float fieldsY = contentRect.y + kLineHeight + kSpacing;
            float fieldWidth = contentRect.width / dimension;
            for (int i = 0; i < dimension; i++)
            {
                Rect r = new Rect(contentRect.x + i * fieldWidth, fieldsY, fieldWidth - 2, kLineHeight);
                value[i] = EditorGUI.FloatField(r, Mathf.Clamp(value[i], range.min, range.max));
            }

            // --- Barra do slider ---
            float barY = fieldsY + kLineHeight + kMargin;
            Rect barRect = new Rect(contentRect.x, barY, contentRect.width, kLineHeight);
            EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f));
            Handles.color = new Color(0.3f, 0.3f, 0.3f);
            Handles.DrawAAPolyLine(2f, new Vector3[]
            {
            new Vector3(barRect.x, barRect.y),
            new Vector3(barRect.x + barRect.width, barRect.y),
            new Vector3(barRect.x + barRect.width, barRect.y + barRect.height),
            new Vector3(barRect.x, barRect.y + barRect.height),
            new Vector3(barRect.x, barRect.y)
            });

            // --- Normalização ---
            float[] normalized = new float[dimension];
            for (int i = 0; i < dimension; i++)
                normalized[i] = Mathf.InverseLerp(range.min, range.max, value[i]);

            Event e = Event.current;
            string key = GetPropertyKey(property);

            // --- Handles ---
            for (int i = 0; i < dimension; i++)
            {
                float minNorm = (i == 0) ? 0f : normalized[i - 1] + kHandleWidth / barRect.width;
                float maxNorm = (i == dimension - 1) ? 1f : normalized[i + 1] - kHandleWidth / barRect.width;

                if (activeHandles.ContainsKey(key) && activeHandles[key] == i && e.type == EventType.MouseDrag)
                {
                    float mouseNorm = Mathf.InverseLerp(barRect.x, barRect.x + barRect.width - kHandleWidth, e.mousePosition.x);
                    normalized[i] = Mathf.Clamp(mouseNorm, minNorm, maxNorm);
                    value[i] = Mathf.Lerp(range.min, range.max, normalized[i]);
                    property.serializedObject.ApplyModifiedProperties();
                    e.Use();
                }

                float handleX = Mathf.Lerp(barRect.x, barRect.x + barRect.width - kHandleWidth, normalized[i]);
                Rect handleRect = new Rect(handleX, barRect.y - 2, kHandleWidth, barRect.height + 4);

                prevColor = GUI.color;
                GUI.color = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.8f, 0.8f, 0.8f);
                EditorGUI.DrawRect(handleRect, GUI.color);
                GUI.color = prevColor;

                if (e.type == EventType.MouseDown && handleRect.Contains(e.mousePosition))
                {
                    activeHandles[key] = i;
                    GUI.FocusControl(null);
                    e.Use();
                }
                if (e.type == EventType.MouseUp && activeHandles.ContainsKey(key) && activeHandles[key] == i)
                {
                    activeHandles.Remove(key);
                    e.Use();
                }
            }

            for (int i = 1; i < dimension; i++) value[i] = Mathf.Max(value[i], value[i - 1]);

            // --- Labels embaixo ---
            float yLabel = barRect.y + kLineHeight + kSpacing;
            GUIStyle miniStyle = new GUIStyle(EditorStyles.miniLabel);

            string[] names = range.names != null && range.names.Length == dimension ? range.names :
                             dimension == 2 ? new string[] { "Min", "Max" } :
                             dimension == 3 ? new string[] { "Min", "Med", "Max" } :
                             new string[] { "Min", "MedA", "MedB", "Max" };

            for (int i = 0; i < dimension; i++)
            {
                Rect labelPos;
                float labelWidth = 40f;

                if (i == 0)
                {
                    miniStyle.alignment = TextAnchor.MiddleLeft;
                    labelPos = new Rect(barRect.x, yLabel, labelWidth, kLineHeight);
                }
                else if (i == dimension - 1)
                {
                    miniStyle.alignment = TextAnchor.MiddleRight;
                    labelPos = new Rect(barRect.x + barRect.width - labelWidth, yLabel, labelWidth, kLineHeight);
                }
                else
                {
                    miniStyle.alignment = TextAnchor.MiddleCenter;
                    float handleX = Mathf.Lerp(barRect.x, barRect.x + barRect.width - kHandleWidth, normalized[i]);
                    labelPos = new Rect(handleX + kHandleWidth / 2 - labelWidth / 2, yLabel, labelWidth, kLineHeight);
                }

                EditorGUI.LabelField(labelPos, names[i], miniStyle);
            }

            // --- Salva valores ---
            if (dimension == 2) property.vector2Value = new Vector2(value.x, value.y);
            else if (dimension == 3) property.vector3Value = new Vector3(value.x, value.y, value.z);
            else property.vector4Value = value;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return kLineHeight * 3 + kSpacing * 3 + kMargin + kLineHeight;
        }
    }

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

            // --- LABEL PRINCIPAL (nome da propriedade) ---
            Rect labelRect = new Rect(x, y + line + kSpacing, labelWidth, line);
            GUIStyle middleLeft = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
            EditorGUI.LabelField(labelRect, label, middleLeft);

            // --- CAMPOS EDITÁVEIS ---
            float fieldWidth = 50f;
            Rect minFieldRect = new Rect(x + labelWidth, y, fieldWidth, line);
            Rect maxFieldRect = new Rect(x + labelWidth + contentWidth - fieldWidth, y, fieldWidth, line);

            value.x = Mathf.Clamp(EditorGUI.FloatField(minFieldRect, value.x), range.min, range.max);
            value.y = Mathf.Clamp(EditorGUI.FloatField(maxFieldRect, value.y), range.min, range.max);

            // --- SLIDER ---
            Rect sliderRect = new Rect(x + labelWidth, y + line + kSpacing, contentWidth, line);
            EditorGUI.MinMaxSlider(sliderRect, ref value.x, ref value.y, range.min, range.max);

            // Garante consistência (min ≤ max e dentro do range)
            value.x = Mathf.Clamp(value.x, range.min, value.y);
            value.y = Mathf.Clamp(value.y, value.x, range.max);

            // --- LABELS DE MIN/MAX ---
            Rect bottomLabelsRect = new Rect(x + labelWidth, y + line * 2 + kSpacing * 2, contentWidth, line);
            GUIStyle miniLeft = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            GUIStyle miniRight = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

            string minName = (range.names != null && range.names.Length > 0) ? range.names[0] : "Min";
            string maxName = (range.names != null && range.names.Length > 1) ? range.names[1] : "Max";

            EditorGUI.LabelField(
                new Rect(bottomLabelsRect.x, bottomLabelsRect.y, bottomLabelsRect.width * 0.5f, bottomLabelsRect.height),
                minName, miniLeft);
            EditorGUI.LabelField(
                new Rect(bottomLabelsRect.x + bottomLabelsRect.width * 0.5f, bottomLabelsRect.y,
                         bottomLabelsRect.width * 0.5f, bottomLabelsRect.height),
                maxName, miniRight);

            property.vector2Value = value;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + kSpacing * 3;
        }
    }

}