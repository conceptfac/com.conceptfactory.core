#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Concept.Editor
{
    public class BuildContentsExplorer : EditorWindow
    {
        private Vector2 _scroll;
        private List<AssetEntry> _assets;
        private string _selectedPath;

        class AssetEntry
        {
            public string path;
            public string fileName;
            public long size;
            public List<string> referencedBy = new List<string>();
        }

        [MenuItem("Tools/Concept Factory/Build Contents Explorer")]
        public static void Open()
        {
            var wnd = GetWindow<BuildContentsExplorer>();
            wnd.titleContent = new GUIContent("Build Contents Explorer");
            wnd.minSize = new Vector2(900, 500);
            wnd.RefreshList();
        }

        private void OnGUI()
        {
            if (_assets == null)
            {
                if (GUILayout.Button("Refresh List"))
                    RefreshList();
                return;
            }

            // Hover instantâneo
            Repaint();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                RefreshList();
            GUILayout.EndHorizontal();

            DrawTableHeader();

            _scroll = GUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _assets.Count; i++)
            {
                DrawTableRow(_assets[i], i);
            }

            GUILayout.EndScrollView();

            GUILayout.Label($"Total: {_assets.Count} assets, {(_assets.Sum(a => a.size) / (1024f * 1024f)):N2} MB", EditorStyles.boldLabel);
        }

        private void DrawTableHeader()
        {
            GUILayout.BeginHorizontal();

            DrawHeaderCell("File Name", 250);
            DrawHeaderCell("Referenced By", 300);
            DrawHeaderCell("Size", 80);
            DrawHeaderCell("Full Path", -1, true);

            GUILayout.EndHorizontal();

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
        }

        private void DrawHeaderCell(string text, float width, bool expand = false)
        {
            GUILayout.BeginVertical(expand ? GUILayout.ExpandWidth(true) : GUILayout.Width(width));
            GUILayout.Label(text, EditorStyles.boldLabel);
            GUILayout.EndVertical();

            if (!expand)
                DrawVerticalSeparator();
        }

        private void DrawVerticalSeparator()
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(new Rect(lastRect.x + lastRect.width - 1, lastRect.y, 1.5f, lastRect.height), new Color(0.2f, 0.2f, 0.2f) * 0.6f);
        }

        private void DrawTableRow(AssetEntry entry, int index)
        {
            // Zebra
            Color rowColor = index % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.16f, 0.16f, 0.16f);

            Rect rowRect = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(rowRect, rowColor);

            // Hover instantâneo
            if (rowRect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rowRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
            }

            // Seleção
            if (_selectedPath == entry.path)
            {
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, rowRect.width, 2), Color.cyan);
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.yMax - 2, rowRect.width, 2), Color.cyan);
                EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.y, 2, rowRect.height), Color.cyan);
                EditorGUI.DrawRect(new Rect(rowRect.xMax - 2, rowRect.y, 2, rowRect.height), Color.cyan);
            }

            // File Name (com quebra de linha)
            DrawCell(() =>
            {
                Rect rect = GUILayoutUtility.GetRect(250, EditorGUIUtility.singleLineHeight * 2);
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                var style = new GUIStyle(EditorStyles.linkLabel) { wordWrap = true, alignment = TextAnchor.MiddleLeft };
                if (GUI.Button(rect, entry.fileName, style))
                    PingAsset(entry.path);
            }, 250);

            // Referenced By (com quebra de linha)
            DrawCell(() =>
            {
                if (entry.referencedBy.Count == 0)
                {
                    GUILayout.Label("-", GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
                else
                {
                    foreach (var refFile in entry.referencedBy)
                    {
                        Rect rect = GUILayoutUtility.GetRect(300, EditorGUIUtility.singleLineHeight * 2);
                        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                        var style = new GUIStyle(EditorStyles.linkLabel) { wordWrap = true, alignment = TextAnchor.MiddleLeft };
                        if (GUI.Button(rect, refFile, style))
                        {
                            var refPath = AssetDatabase.GetAllAssetPaths().FirstOrDefault(p => Path.GetFileName(p) == refFile);
                            if (!string.IsNullOrEmpty(refPath))
                                PingAsset(refPath);
                        }
                    }
                }
            }, 300);

            // Size
            DrawCell(() =>
            {
                GUILayout.Label($"{entry.size / 1024f:N1} KB", GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }, 80);

            // Full Path (sem quebra de linha)
            DrawCell(() =>
            {
                GUILayout.Label(entry.path, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }, -1, true);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCell(Action content, float width, bool expand = false)
        {
            GUILayout.BeginVertical(expand ? GUILayout.ExpandWidth(true) : GUILayout.Width(width));
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            content.Invoke();
            GUILayout.Space(2);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.EndVertical();

            if (!expand)
                DrawVerticalSeparator();
        }

        private void PingAsset(string path)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
                EditorApplication.ExecuteMenuItem("Window/General/Project");
                _selectedPath = path;
            }
        }

        private void RefreshList()
        {
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                var scenePaths = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                var deps = new HashSet<string>(AssetDatabase.GetDependencies(scenePaths, true));
                var list = new List<AssetEntry>();

                foreach (var path in deps)
                {
                    if (path.EndsWith(".cs") || path.Contains("/Editor/"))
                        continue;

                    string abs = Path.Combine(projectRoot, path);
                    if (!File.Exists(abs))
                        continue;

                    var fi = new FileInfo(abs);
                    var entry = new AssetEntry
                    {
                        path = path,
                        fileName = Path.GetFileName(path),
                        size = fi.Length
                    };

                    // Quem referencia
                    foreach (var scene in scenePaths)
                    {
                        var directDeps = AssetDatabase.GetDependencies(scene, false);
                        if (directDeps.Contains(path))
                            entry.referencedBy.Add(Path.GetFileName(scene));
                    }

                    foreach (var other in deps)
                    {
                        if (other == path) continue;
                        var d = AssetDatabase.GetDependencies(other, false);
                        if (d.Contains(path))
                            entry.referencedBy.Add(Path.GetFileName(other));
                    }

                    entry.referencedBy = entry.referencedBy.Distinct().ToList();
                    list.Add(entry);
                }

                _assets = list;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao gerar lista: {ex}");
                _assets = new List<AssetEntry>();
            }
        }
    }
}
#endif
