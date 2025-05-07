#if UNITY_EDITOR
using Concept.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Concept.Editor
{
    /// <summary>
    /// A drawer to handle drawing fields with a [HideIf] attribute. When fields have this attribute they will be hidden
    /// in the inspector conditionally based on the evaluation of a field or property.
    /// </summary>
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class HideIfDrawer : PropertyDrawer
    {
        private bool IsVisible(SerializedProperty property)
        {
            HideIfAttribute hideIf = attribute as HideIfAttribute;
            SerializedProperty conditionProperty =
                property.GetParent()?.FindPropertyRelative(hideIf.condition);
            // If it wasn't found relative to the property, check siblings.
            if (null == conditionProperty)
            {
                conditionProperty = property.serializedObject.FindProperty(hideIf.condition);
            }

            if (conditionProperty != null)
            {
                if (conditionProperty.type == "bool") return !conditionProperty.boolValue;
                return conditionProperty.objectReferenceValue == null;
            }

            return true;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsVisible(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }
    }



    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfAttributeDrawer : PropertyDrawer
    {
        private bool IsVisible(SerializedProperty property)
        {
            ShowIfAttribute hideIf = attribute as ShowIfAttribute;
            SerializedProperty conditionProperty =
                property.FindPropertyRelative(hideIf.condition);
            //property.serializedObject.FindProperty(hideIf.condition);
            // If it wasn't found relative to the property, check siblings.
            if (null == conditionProperty)
            {
                conditionProperty = property.serializedObject.FindProperty(hideIf.condition);
            }

            if (conditionProperty != null)
            {
                if (conditionProperty.type == "bool") return !conditionProperty.boolValue;
                return conditionProperty.objectReferenceValue == null;
            }

            return true;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsVisible(property))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }


    }
    /*
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {


            var showIfAttribute = (ShowIfAttribute)attribute;

            var conditionProperty = property.serializedObject.FindProperty(showIfAttribute.condition);

            if (conditionProperty != null)
            {
                // Se for do tipo bool
                if (conditionProperty.propertyType == SerializedPropertyType.Boolean)
                {
                    if (!conditionProperty.boolValue)
                    {
                        return;  // N�o desenha a propriedade se a condi��o booleana for falsa
                    }
                }
            }

            // Se for um objeto ou struct, precisamos "expandir" o conte�do
            if (property.propertyType == SerializedPropertyType.ObjectReference || property.propertyType == SerializedPropertyType.Generic)
            {
                Debug.LogWarning($"{property.name} StartY: {position.y}");

                Color originalColor = GUI.color;

                GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };

                GUI.Label(new Rect(position.x, position.y, position.width, 16f), property.displayName + ":", boldLabelStyle); // Aplica a label com o estilo
                position.y += 16f;

                var y = DrawStructFields(position, property);


                Debug.LogWarning($"{property.name} Height: {y}");
                position.y = y + 16f;

                Debug.LogWarning($"{property.name} FinalY: {position.y}");

            }
            else
            {
                // Caso a propriedade n�o seja um objeto, desenha normalmente
                EditorGUI.PropertyField(position, property, label);
                position.y += EditorGUI.GetPropertyHeight(property, true);
            }


            // Se a condi��o for atendida, desenha a propriedade normalmente
            //  EditorGUI.PropertyField(position, property, label);
        }


        private float DrawStructFields(Rect position, SerializedProperty property)
        {
            // Desenha os campos internos de uma struct
            if (property.hasChildren)
            {
                SerializedProperty iterator = property.Copy();
                iterator.NextVisible(true);  // Avan�a para o primeiro campo

                bool first = true;

                // Itera por todos os campos vis�veis e os desenha
                while (iterator.NextVisible(first))
                {
                    first = false;

                    Rect newPosition = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(iterator, true));
                    EditorGUI.PropertyField(newPosition, iterator, true);
                    position.y += 100f;
                }

            }
            return position.y;
        }

    }
    */

    [CustomPropertyDrawer(typeof(SubPanelAttribute))]
    public class SubPanelPropertyDrawer : PropertyDrawer
    {



        // M�todo principal para desenhar o conte�do no inspector
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            // Verifica se esta propriedade est� em um subpainel
            var subPanelAttribute = (SubPanelAttribute)attribute;
            bool isSubPanel = subPanelAttribute != null;

            if (isSubPanel && PropertyDrawerExtensions.isInSubPanel && PropertyDrawerExtensions.currentCaption != subPanelAttribute.caption)
            {
                PropertyDrawerExtensions.isInSubPanel = false;

            }

            Rect titleRect = new Rect(position.x, position.y, position.width, 16f);
            // Se estiver no in�cio de um subpainel, desenha o t�tulo
            if (isSubPanel && !PropertyDrawerExtensions.isInSubPanel)
            {

                PropertyDrawerExtensions.currentCaption = subPanelAttribute.caption;
                Color originalColor = GUI.color;
                // Define o t�tulo do subpainel (label)

                // Ajuste o espa�o para a label do subpainel
                EditorGUI.DrawRect(titleRect, subPanelAttribute.backgroundColor);

                // Aqui est� a label personalizada que ser� desenhada antes da propriedade
                GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel); // Cria uma nova GUIStyle para negrito
                boldLabelStyle.fontSize = 14; // Opcional: Defina o tamanho da fonte, caso queira personalizar

                GUI.color = subPanelAttribute.color;
                GUI.Label(titleRect, PropertyDrawerExtensions.currentCaption, boldLabelStyle); // Aplica a label com o estilo
                GUI.color = originalColor;
                position.y += 5f;

                // Atualiza a posi��o para desenhar a propriedade abaixo da label


                PropertyDrawerExtensions.isInSubPanel = true;  // Marca que estamos dentro de um subpainel
            }




            position.y += 16f;
            EditorGUI.PropertyField(position, property, label);

            if (isSubPanel && PropertyDrawerExtensions.isInSubPanel && PropertyDrawerExtensions.currentCaption != subPanelAttribute.caption)
            {
                position.y += 160f;
                PropertyDrawerExtensions.isInSubPanel = false;
                GUI.Label(titleRect, "CARALHO"); // Aplica a label com o estilo

            }




        }


        private bool IsPrimitive(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Enum:
                    return true;  // Tipos primitivos conhecidos
                default:
                    return false; // Outros tipos s�o considerados n�o primitivos (refer�ncias de objetos, structs, etc.)
            }
        }

    }

    public static class PropertyDrawerExtensions
    {



        public static bool isInSubPanel = false;
        public static string currentCaption = "";
        public static float spaceAfterSubPanelTitle = 5f;


        /// <summary>
        /// Gets the parent property of a SerializedProperty
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static SerializedProperty GetParent(this SerializedProperty property)
        {
            var segments = property.propertyPath.Split(new char[] { '.' });
            SerializedProperty matchedProperty = property.serializedObject.FindProperty(segments[0]);
            for (int i = 1; i < segments.Length - 1 && null != matchedProperty; i++)
            {
                matchedProperty = matchedProperty.FindPropertyRelative(segments[i]);
            }

            return matchedProperty;
        }

        public static T GetAttribute<T>(this SerializedProperty property) where T : PropertyAttribute
        {
            var fieldInfo = property.serializedObject.targetObject.GetType().GetField(property.name);
            return fieldInfo?.GetCustomAttribute<T>();
        }
    }

    [CustomPropertyDrawer(typeof(CustomButtonAttribute))]
    public class CustomButtonPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label); // Desenha o campo padr�o (se houver)

            // Verifique se o campo � uma string que cont�m o nome do m�todo
            if (property.propertyType == SerializedPropertyType.String)
            {
                string methodName = property.stringValue;

                // Desenha o bot�o no Inspector
                if (GUI.Button(new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, 20), "Clique para A��o"))
                {
                    var targetObject = property.serializedObject.targetObject;
                    var method = targetObject.GetType().GetMethod(methodName);

                    if (method != null)
                    {
                        // Chama o m�todo
                        method.Invoke(targetObject, null);
                    }
                    else
                    {
                        Debug.LogWarning("M�todo n�o encontrado: " + methodName);
                    }
                }
            }
        }

    }

    [CustomPropertyDrawer(typeof(VectorLabelsAttribute))]
    public class VectorLabelsAttributeDrawer : PropertyDrawer
    {
        private static readonly string[] defaultLabels = new string[] { "X", "Y", "Z", "W" };

        private const int twoLinesThreshold = 375;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int factor = Screen.width < twoLinesThreshold ? 2 : 1;
            return factor * base.GetPropertyHeight(property, label);
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            VectorLabelsAttribute vectorLabels = (VectorLabelsAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                int[] array = new int[] { property.vector2IntValue.x, property.vector2IntValue.y };
                array = DrawFields(position, array, ObjectNames.NicifyVariableName(property.name), EditorGUI.IntField, vectorLabels);
                property.vector2IntValue = new Vector2Int(array[0], array[1]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector2)
            {
                float[] array = new float[] { property.vector2Value.x, property.vector2Value.y };
                array = DrawFields(position, array, ObjectNames.NicifyVariableName(property.name), EditorGUI.FloatField, vectorLabels);
                property.vector2Value = new Vector2(array[0], array[1]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector3Int)
            {
                int[] array = new int[] { property.vector3IntValue.x, property.vector3IntValue.y, property.vector3IntValue.z };
                array = DrawFields(position, array, ObjectNames.NicifyVariableName(property.name), EditorGUI.IntField, vectorLabels);
                property.vector3IntValue = new Vector3Int(array[0], array[1], array[2]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector3)
            {
                float[] array = new float[] { property.vector3Value.x, property.vector3Value.y, property.vector3Value.z };
                array = DrawFields(position, array, ObjectNames.NicifyVariableName(property.name), EditorGUI.FloatField, vectorLabels);
                property.vector3Value = new Vector3(array[0], array[1], array[2]);
            }
            else if (property.propertyType == SerializedPropertyType.Vector4)
            {
                float[] array = new float[] { property.vector4Value.x, property.vector4Value.y, property.vector4Value.z, property.vector4Value.w };
                array = DrawFields(position, array, ObjectNames.NicifyVariableName(property.name), EditorGUI.FloatField, vectorLabels);
                property.vector4Value = new Vector4(array[0], array[1], array[2]);
            }
        }

        private T[] DrawFields<T>(Rect rect, T[] vector, string mainLabel, Func<Rect, T, T> fieldDrawer, VectorLabelsAttribute vectorLabels)
        {
            T[] result = vector;

            bool twoLinesLayout = Screen.width < twoLinesThreshold;

            // Get the rect of the main label
            Rect mainLabelRect = rect;
            mainLabelRect.width = EditorGUIUtility.labelWidth;
            if (twoLinesLayout)
                mainLabelRect.height *= 0.5f;

            // Get the size of each field rect
            Rect fieldRect = rect;
            if (twoLinesLayout)
            {
                fieldRect.height *= 0.5f;
                fieldRect.y += fieldRect.height;
                fieldRect.width = rect.width / vector.Length;
            }
            else
            {
                fieldRect.x += mainLabelRect.width;
                fieldRect.width = (rect.width - mainLabelRect.width) / vector.Length;
            }

            EditorGUI.LabelField(mainLabelRect, mainLabel);

            for (int i = 0; i < vector.Length; i++)
            {
                string label = vectorLabels.Labels.Length > i ? vectorLabels.Labels[i] : defaultLabels[i];
                Vector2 labelSize = EditorStyles.label.CalcSize(new GUIContent(label));

                Rect labelRect = fieldRect;
                labelRect.width = Mathf.Max(labelSize.x + 5, 0.3f * fieldRect.width);
                EditorGUI.LabelField(labelRect, label);


                Rect valueRect = fieldRect;
                valueRect.x += labelRect.width;
                valueRect.width -= labelRect.width;
                result[i] = fieldDrawer(valueRect, vector[i]);

                fieldRect.x += fieldRect.width;
            }


            return result;
        }
    }

    [CustomPropertyDrawer(typeof(DrawScriptableAttribute))]
    public class ScriptableObjectDrawer : PropertyDrawer
    {

        private bool isFoldedOut = false; // Variável que controla o estado do foldout

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Desenha o campo de referência (sempre, mesmo se null)
            EditorGUI.PropertyField(position, property, label, true);

            // Verifica se há um objeto atribuído
            if (property.objectReferenceValue != null)
            {
                // Verifica se é um ScriptableObject válido
                if (property.objectReferenceValue is ScriptableObject)
                {
                    // Foldout para mostrar as propriedades internas
                    Rect foldoutRect = new Rect(
                        position.x,
                        position.y + EditorGUIUtility.singleLineHeight + 2,
                        position.width,
                        EditorGUIUtility.singleLineHeight
                    );

                    isFoldedOut = EditorGUI.Foldout(foldoutRect, isFoldedOut, "Properties", true);

                    if (isFoldedOut)
                    {
                        SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                        SerializedProperty iterator = serializedObject.GetIterator();

                        float yOffset = foldoutRect.y + EditorGUIUtility.singleLineHeight + 2;
                        bool enterChildren = true;

                        // Campos a pular (como o m_Script padrão)
                        HashSet<string> skipFields = new HashSet<string> { "m_Script" };

                        while (iterator.NextVisible(enterChildren))
                        {
                            enterChildren = false;

                            // Pula campos indesejados
                            if (skipFields.Contains(iterator.name))
                                continue;

                            float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                            Rect fieldRect = new Rect(position.x, yOffset, position.width, propertyHeight);

                            EditorGUI.PropertyField(fieldRect, iterator, true);
                            yOffset += propertyHeight + 2;
                        }

                        serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    // Objeto atribuído não é um ScriptableObject
                    Rect warningRect = new Rect(
                        position.x,
                        position.y + EditorGUIUtility.singleLineHeight + 2,
                        position.width,
                        EditorGUIUtility.singleLineHeight
                    );

                    EditorGUI.LabelField(warningRect, "Assigned object is not a ScriptableObject.");
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Altura base: campo de referência + espaço para foldout (independente de estar expandido ou não)
            float height = EditorGUIUtility.singleLineHeight;

            if (property.objectReferenceValue != null)
            {
                height += EditorGUIUtility.singleLineHeight + 2; // foldout

                if (isFoldedOut)
                {
                    SerializedObject serializedObject = new SerializedObject(property.objectReferenceValue);
                    SerializedProperty iterator = serializedObject.GetIterator();

                    bool enterChildren = true;

                    HashSet<string> skipFields = new HashSet<string> { "m_Script" };

                    while (iterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;

                        if (skipFields.Contains(iterator.name))
                            continue;

                        height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
                    }
                }
            }

            return height;
        }



    }


}

#endif