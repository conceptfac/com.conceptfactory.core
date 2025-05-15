#if UNITY_EDITOR
using Concept.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScreenMonitor))]
[CanEditMultipleObjects]
public class ScreenMonitorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "This component is part of the Core system and may be required by other packages. Removing it can cause dependent components to stop working.",
            MessageType.Warning
        );

    }

    public static void CheckScreenMonitor()
    {
        ScreenMonitor monitor = FindFirstObjectByType<ScreenMonitor>();
        if (!monitor)
        {

            EditorGUILayout.HelpBox(
                "This component depends on an 'ScreenMonitor' MonoBehaviour to function correctly at runtime.",
                MessageType.Error
            );

            if (GUILayout.Button("Add ScreenMonitor to Scene"))
            {
                new GameObject($"[CF] ScreenMonitor", typeof(ScreenMonitor));
            }


        }
    }

}
#endif