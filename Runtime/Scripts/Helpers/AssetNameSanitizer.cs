#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace Concept.Helpers
{
    public class AssetNameSanitizer : MonoBehaviour
    {
        [MenuItem("Tools/Sanitize Asset Names")]
        public static void SanitizeAssetNames()
        {
            string[] allAssetPaths = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);

            int renameCount = 0;

            foreach (string path in allAssetPaths)
            {
                if (path.EndsWith(".meta")) continue;
                if (path.Contains("/Library/") || path.Contains("/.git/")) continue;

                string fileName = Path.GetFileName(path);
                string dir = Path.GetDirectoryName(path);
                string sanitizedFileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\-\.]", "_");

                if (fileName != sanitizedFileName)
                {
                    string newPath = Path.Combine(dir, sanitizedFileName).Replace("\\", "/");
                    string assetPath = path.Replace("\\", "/");

                    if (AssetDatabase.LoadAssetAtPath<Object>(newPath) != null)
                    {
                        Debug.LogWarning($"File name cleaned already: {newPath}. Next.");
                        continue;
                    }

                    AssetDatabase.MoveAsset(assetPath, newPath);
                    Debug.Log($"Renamed: {assetPath} ➜ {newPath}");
                    renameCount++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Sanitize finished. {renameCount} renamed files.");
        }
    }
}
#endif