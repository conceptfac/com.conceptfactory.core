#if UNITY_EDITOR
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Threading;
using System.IO;
using UnityEditor;

namespace Concept.Helpers
{
    public static class IOExtensions
    {
        [System.Serializable]
        private class PackageJson
        {
            public string name;
        }

        /// <summary>
        /// [EDITOR-ONLY] Attempts to locate and return the Unity package name (e.g., "com.conceptfactory.core")
        /// by walking up the directory tree from the script file path until a <c>package.json</c> file is found.
        /// </summary>
        /// <param name="scriptFilePath">Absolute path to the C# script file calling this method (e.g., via <see cref="AssetDatabase.GetAssetPath"/>).</param>
        /// <returns>
        /// The name of the package as defined in its <c>package.json</c>,
        /// or <c>null</c> if the package could not be determined.
        /// </returns>
        /// <remarks>
        /// This method is intended to be used only in the Unity Editor and should not be called at runtime.
        /// </remarks>
        public static string GetCurrentPackageName(string scriptFilePath)
        {
            string directory = Path.GetDirectoryName(scriptFilePath);

            while (!string.IsNullOrEmpty(directory) && !File.Exists(Path.Combine(directory, "package.json")))
            {
                directory = Path.GetDirectoryName(directory);
            }

            if (string.IsNullOrEmpty(directory))
            {
                Debug.LogError("[IOExtensions] Impossible to found package.json");
                return null;
            }

            string packageJsonPath = Path.Combine(directory, "package.json");
            string json = File.ReadAllText(packageJsonPath);

            var info = JsonUtility.FromJson<PackageJson>(json);
            return info.name;
        }
   


        /// <summary>
        /// [EDITOR-ONLY] Returns the absolute path on disk of a Unity package given its name.
        /// </summary>
        /// <param name="packageName">The name of the package (e.g., "com.conceptfactory.core").</param>
        /// <returns>The absolute path to the package folder, or an empty string if not found.</returns>
        public static string GetPackageAbsolutePath(string packageName)
        {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
                Thread.Sleep(1);

            if (listRequest.Status == StatusCode.Success)
            {
                var package = listRequest.Result.FirstOrDefault(p => p.name == packageName);
                if (package != null)
                {
                    return package.resolvedPath;
                }
                else
                {
                    Debug.LogError($"[IOExtensions] Package '{packageName}' not found.");
                }
            }
            else
            {
                Debug.LogError($"[IOExtensions] Unable to retrieve package list. Error: {listRequest.Error.message}");
            }

            return string.Empty;
        }
        public static string GetPackageAbsolutePath(ScriptableObject scriptableObject)
        {
            return GetPackageAbsolutePath(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(scriptableObject)));
        }

    }


}
#endif