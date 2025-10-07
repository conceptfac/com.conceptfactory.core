using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.AddressableAssets.Addressables;
using System.Linq;

namespace Concept.Addressables
{
    /// <summary>
    /// Static utility class for managing Addressables operations with different bundle packing strategies
    /// </summary>
    public static class AddressablesManager
    {
        /// <summary>
        /// Loads a single asset from a PackSeparately bundle group
        /// </summary>
        /// <typeparam name="T">Asset type (Texture2D, GameObject, ScriptableObject, etc)</typeparam>
        /// <param name="assetKey">Specific asset addressable key</param>
        /// <param name="progressCallback">Optional progress callback (0.0 to 1.0)</param>
        /// <returns>Loaded asset or null if failed</returns>
        public static async Task<T> LoadSeparateAssetAsync<T>(string assetKey, Action<float> progressCallback = null) where T : UnityEngine.Object
        {
            try
            {
                Debug.Log($"[AddressablesManager] Loading separate asset: {assetKey}");

                // Check download size
                AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(assetKey);
                await sizeOperation.Task;
                if (sizeOperation.Status == AsyncOperationStatus.Succeeded && sizeOperation.Result > 0)
                {
                    Debug.Log($"[AddressablesManager] Download size: {sizeOperation.Result} bytes");

                    // Download only this specific asset
                    AsyncOperationHandle downloadOperation = DownloadDependenciesAsync(assetKey, false);

                    // Download progress tracking
                    while (!downloadOperation.IsDone)
                    {
                        progressCallback?.Invoke(downloadOperation.PercentComplete);
                        await Task.Yield();
                    }

                    if (downloadOperation.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"[AddressablesManager] Download failed for: {assetKey}");
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return null;
                    }

                    Release(downloadOperation);
                    progressCallback?.Invoke(1f);
                }

                Release(sizeOperation);

                // Load the asset into memory
                AsyncOperationHandle<T> loadOperation = LoadAssetAsync<T>(assetKey);
                await loadOperation.Task;

                if (loadOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[AddressablesManager] Successfully loaded: {assetKey}");
                    T result = loadOperation.Result;
                    Release(loadOperation);
                    return result;
                }
                else
                {
                    Debug.LogError($"[AddressablesManager] Failed to load asset: {assetKey}");
                    Release(loadOperation);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Error loading {assetKey}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Downloads entire PackTogether bundle group
        /// </summary>
        /// <param name="groupName">Addressables group name</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>True if download successful</returns>
        /// 

        public static async Task<bool> DownloadEntireGroupAsync(string groupName, Action<float> progressCallback = null)
        {
            try
            {
                Debug.Log($"[AddressablesManager] Downloading entire group: {groupName}");

                // Primeiro, carrega as localizações para a chave (que é o nome do grupo)
                var locationsHandle = LoadResourceLocationsAsync(groupName);
                await locationsHandle.Task;

                if (locationsHandle.Status != AsyncOperationStatus.Succeeded || locationsHandle.Result == null || locationsHandle.Result.Count == 0)
                {
                    Debug.LogError($"[AddressablesManager] Group {groupName} not found or has no assets");
                    Release(locationsHandle);
                    return false;
                }

                // Extrai as chaves únicas das localizações
                var keys = locationsHandle.Result.Select(loc => loc.PrimaryKey).Distinct().ToList();
                Release(locationsHandle);

                // Verifica o tamanho do download para todas as chaves
                AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(keys);
                await sizeOperation.Task;

                if (sizeOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    if (sizeOperation.Result == 0)
                    {
                        Debug.Log($"[AddressablesManager] Group {groupName} already downloaded");
                        Release(sizeOperation);
                        return true;
                    }   

                    Debug.Log($"[AddressablesManager] Downloading group {groupName} - Size: {sizeOperation.Result} bytes");

                    // Download das dependências para todas as chaves
                    AsyncOperationHandle downloadOperation = DownloadDependenciesAsync(keys, MergeMode.Union, false);

                    // Monitorar progresso
                    while (!downloadOperation.IsDone)
                    {
                        progressCallback?.Invoke(downloadOperation.PercentComplete);
                        await Task.Yield();
                    }

                    if (downloadOperation.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log($"[AddressablesManager] Group {groupName} downloaded successfully");
                        progressCallback?.Invoke(1f);
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"[AddressablesManager] Failed to download group: {groupName}");
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return false;
                    }
                }

                Release(sizeOperation);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Error downloading group {groupName}: {e.Message}");
                return false;
            }
        }


        public static async Task<bool> DownloadEntireGroupAsyncOLD(string groupName, Action<float> progressCallback = null)
        {
            try
            {
                Debug.Log($"[AddressablesManager] Downloading entire group: {groupName}");

                // Check total download size for the group
                AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(groupName);
                await sizeOperation.Task;

                if (sizeOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    if (sizeOperation.Result == 0)
                    {
                        Debug.Log($"[AddressablesManager] Group {groupName} already downloaded");
                        Release(sizeOperation);
                        return true;
                    }

                    Debug.Log($"[AddressablesManager] Downloading group {groupName} - Size: {sizeOperation.Result} bytes");

                    // Download complete group bundle
                    AsyncOperationHandle downloadOperation = DownloadDependenciesAsync(groupName, false);

                    // Download progress
                    while (!downloadOperation.IsDone)
                    {
                        progressCallback?.Invoke(downloadOperation.PercentComplete);
                        await Task.Yield();
                    }

                    if (downloadOperation.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log($"[AddressablesManager] Group {groupName} downloaded successfully");
                        progressCallback?.Invoke(1f);
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"[AddressablesManager] Failed to download group: {groupName}");
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return false;
                    }
                }

                Release(sizeOperation);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Error downloading group {groupName}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Downloads assets by label (PackTogetherByLabel strategy)
        /// </summary>
        /// <param name="label">Label to filter assets</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>True if download successful</returns>
        public static async Task<bool> DownloadAssetsByLabelAsync(string label, Action<float> progressCallback = null)
        {
            try
            {
                Debug.Log($"[AddressablesManager] Downloading assets with label: {label}");

                // Check download size for labeled assets
                AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(label);
                await sizeOperation.Task;

                if (sizeOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    if (sizeOperation.Result == 0)
                    {
                        Debug.Log($"[AddressablesManager] Assets with label '{label}' already downloaded");
                        Release(sizeOperation);
                        return true;
                    }

                    Debug.Log($"[AddressablesManager] Downloading label '{label}' - Size: {sizeOperation.Result} bytes");

                    // Download assets with specified label
                    AsyncOperationHandle downloadOperation = DownloadDependenciesAsync(label, false);

                    // Download progress
                    while (!downloadOperation.IsDone)
                    {
                        progressCallback?.Invoke(downloadOperation.PercentComplete);
                        await Task.Yield();
                    }

                    if (downloadOperation.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log($"[AddressablesManager] Label '{label}' downloaded successfully");
                        progressCallback?.Invoke(1f);
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"[AddressablesManager] Failed to download label: {label}");
                        Release(downloadOperation);
                        Release(sizeOperation);
                        return false;
                    }
                }

                Release(sizeOperation);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Error downloading label {label}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads asset without download check (assumes already available)
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="assetKey">Asset addressable key</param>
        /// <returns>Loaded asset or null</returns>
        public static async Task<T> LoadAssetFromGroupAsync<T>(string assetKey) where T : UnityEngine.Object
        {
            try
            {
                AsyncOperationHandle<T> loadOperation = LoadAssetAsync<T>(assetKey);
                await loadOperation.Task;

                if (loadOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    T result = loadOperation.Result;
                    Release(loadOperation);
                    return result;
                }

                Release(loadOperation);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Error loading {assetKey}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if asset or group is already downloaded
        /// </summary>
        /// <param name="key">Asset key or group name</param>
        /// <returns>True if no download required</returns>
        public static async Task<bool> IsDownloadedAsync(string key)
        {
            AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(key);
            await sizeOperation.Task;
            bool isDownloaded = sizeOperation.Result == 0;
            Release(sizeOperation);
            return isDownloaded;
        }

        /// <summary>
        /// Releases asset from Addressables memory management
        /// </summary>
        /// <param name="asset">Asset to release</param>
        public static void ReleaseAsset(UnityEngine.Object asset)
        {
            if (asset != null)
            {
                Release(asset);
            }
        }
    }
}