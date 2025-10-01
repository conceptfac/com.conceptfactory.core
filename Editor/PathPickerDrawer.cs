#if UNITY_EDITOR
using Concept.UI;
using UnityEditor;
using UnityEngine;

namespace Concept.Editor
{

    [CustomPropertyDrawer(typeof(PathPickerAttribute))]
    public class PathPickerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            PathPickerAttribute pathAttribute = (PathPickerAttribute)attribute;

            // Divide o espaço em campo de texto e botão
            Rect fieldRect = new Rect(position.x, position.y, position.width - 80, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 75, position.y, 70, position.height);

            // Campo de texto
            property.stringValue = EditorGUI.TextField(fieldRect, label, property.stringValue);

            // Botão de seleção
            if (GUI.Button(buttonRect, "Select"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Folder",
                    GetInitialPath(property.stringValue, pathAttribute.defaultPath), "");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    // Converte para caminho relativo se estiver dentro do projeto
                    string projectPath = Application.dataPath.Replace("/Assets", "");
                    if (selectedPath.StartsWith(projectPath))
                    {
                        selectedPath = selectedPath.Substring(projectPath.Length + 1);
                    }

                    property.stringValue = selectedPath;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private string GetInitialPath(string currentPath, string defaultPath)
        {
            string initialPath = Application.dataPath; // Fallback padrão

            try
            {
                // Prioridade 1: Caminho atual da propriedade
                if (!string.IsNullOrEmpty(currentPath))
                {
                    if (System.IO.Path.IsPathRooted(currentPath))
                    {
                        // Se já é caminho absoluto, verifica se existe
                        if (System.IO.Directory.Exists(currentPath))
                            return currentPath;
                    }
                    else
                    {
                        // Caminho relativo - converte para absoluto
                        string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", currentPath));
                        if (System.IO.Directory.Exists(fullPath))
                            return fullPath;
                    }
                }

                // Prioridade 2: Caminho padrão do atributo
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    if (System.IO.Path.IsPathRooted(defaultPath))
                    {
                        if (System.IO.Directory.Exists(defaultPath))
                            return defaultPath;
                    }
                    else
                    {
                        string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", defaultPath));
                        if (System.IO.Directory.Exists(fullPath))
                            return fullPath;
                    }
                }

                // Prioridade 3: Pasta do projeto (Assets)
                return Application.dataPath;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error getting initial path: {e.Message}");
                return Application.dataPath;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}