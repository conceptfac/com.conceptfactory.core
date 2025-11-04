using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.AddressableAssets.Addressables;
using System.Linq;
using UnityEngine.Video;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Reflection;
using UnityEditor;
using UnityEngine.SceneManagement;
using Concept.Helpers;

namespace Concept.Addressables
{
    /// <summary>
    /// Static utility class for managing Addressables operations with different bundle packing strategies
    /// </summary>
    public static class AddressablesManager
    {

        public static async Task<bool> InitializeAsync()
        {
            try
            {
                await UnityEngine.AddressableAssets.Addressables.InitializeAsync().Task;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Falha na inicialização: {e.Message}");
                return false;
            }
        }

        public static async Task<bool> LoadContentCatalogAsync(
    string catalogURL,
    Action<float> progressCallback = null
)
        {
            try
            {
                var handle = UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync(catalogURL, false);

                // Loop de progresso
                while (!handle.IsDone)
                {
                    progressCallback?.Invoke(handle.PercentComplete);
                    await Task.Yield();
                }

                bool success = handle.Status == AsyncOperationStatus.Succeeded;

                if (success)
                {
                    progressCallback?.Invoke(1f);
                    Debug.Log($"[AddressablesManager] Catálogo carregado com sucesso!\n{catalogURL}");
                }
                else
                {
                    Debug.LogError($"[AddressablesManager] Falha ao carregar catálogo do módulo: {catalogURL}");
                }

                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Erro ao carregar catálogo: {e.Message}");
                return false;
            }
        }


        public static async Task<bool> LoadContentCatalogAsyncOLD(string catalogURL)
        {
            var handle = UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync(catalogURL, false);
            await handle.Task;

            bool success = handle.Status == AsyncOperationStatus.Succeeded;
            if (success)
                Debug.Log("Catálogo do módulo carregado com sucesso!\n" + catalogURL);
            else
                Debug.LogError($"Falha ao carregar catálogo do módulo: {catalogURL}");



            return success;
        }

        public static async Task<(bool status, long length)> GetDownloadSize(string assetKey)
        {
            AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(assetKey);
            await sizeOperation.Task;

            return (sizeOperation.Status == AsyncOperationStatus.Succeeded, sizeOperation.Result);

        }

        /// <summary>
        /// Loads a single asset from a PackSeparately bundle group
        /// </summary>
        /// <typeparam name="T">Asset type (Texture2D, GameObject, ScriptableObject, etc)</typeparam>
        /// <param name="assetKey">Specific asset addressable key</param>
        /// <param name="progressCallback">Optional progress callback (0.0 to 1.0)</param>
        /// <returns>Loaded asset or null if failed</returns>
        public static async Task<T> LoadSeparateAssetAsync<T>(
            string assetKey,
            Action<float> progressCallback = null
        ) where T : UnityEngine.Object
        {
            try
            {
                // Verifica tamanho do download
                AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(assetKey);
                await sizeOperation.Task;

                if (sizeOperation.Status == AsyncOperationStatus.Succeeded && sizeOperation.Result > 0)
                {
                    Debug.LogWarning($"[AddressablesManager] Download size: {sizeOperation.Result.GetBytes()}");

                    // Faz o download das dependências
                    AsyncOperationHandle downloadOperation = DownloadDependenciesAsync(assetKey, false);

                    while (!downloadOperation.IsDone)
                    {
                        // 0f–0.5f → progresso de download
                        progressCallback?.Invoke(downloadOperation.PercentComplete * 0.5f);
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
                    progressCallback?.Invoke(0.5f);
                }

                Release(sizeOperation);

                // Carrega o asset em memória
                AsyncOperationHandle<T> loadOperation = LoadAssetAsync<T>(assetKey);

                while (!loadOperation.IsDone)
                {
                    // 0.5f–1f → progresso de load
                    progressCallback?.Invoke(0.5f + loadOperation.PercentComplete * 0.5f);
                    await Task.Yield();
                }

                if (loadOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"[AddressablesManager] Successfully loaded: {assetKey}");
                    progressCallback?.Invoke(1f);

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

        public static async Task<T> LoadSeparateAssetAsyncOLD<T>(string assetKey, Action<float> progressCallback = null) where T : UnityEngine.Object
        {
            try
            {
                //Debug.LogWarning($"[AddressablesManager] Loading separate asset: {assetKey}");

                // Check download size
                AsyncOperationHandle<long> sizeOperation = GetDownloadSizeAsync(assetKey);
                await sizeOperation.Task;
                if (sizeOperation.Status == AsyncOperationStatus.Succeeded && sizeOperation.Result > 0)
                {
                    Debug.LogWarning($"[AddressablesManager] Download size: {sizeOperation.Result.GetBytes()}");

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
        /// Baixa um asset de um Addressable remoto para o cache e retorna o path local.
        /// </summary>
        /// <param name="key">Key do Addressable</param>
        /// <param name="fileName">Nome do arquivo no cache</param>
        /// <returns>Path local no cache, ou null se falhar</returns>
        public static async Task<string> DownloadAssetToCacheAsync(string key)
        {
            string safeName = key.Replace("/", "_").Replace("\\", "_");
            string cachePath = Path.Combine(Application.temporaryCachePath, safeName);

            if (File.Exists(cachePath))
                return cachePath;

            // Baixa o asset como TextAsset (funciona para arquivos binários também)
            AsyncOperationHandle<TextAsset> handle = LoadAssetAsync<TextAsset>(key);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Falha ao carregar asset '{key}' do Addressable.");
                return null;
            }

            // Salva bytes no cache
            File.WriteAllBytes(cachePath, handle.Result.bytes);

            Release(handle);
            return cachePath;
        }



        public static async Task<string> DownloadAssetToCacheAsync2(string key, string fileName)
        {
            fileName = key.Replace("/", "_").Replace("\\", "_");

            string cachePath = Path.Combine(Application.temporaryCachePath, fileName);

            if (File.Exists(cachePath))
                return cachePath;

            AsyncOperationHandle<UnityEngine.Object> handle = LoadAssetAsync<UnityEngine.Object>(key);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Falha ao carregar asset '{key}' do Addressable.");
                return null;
            }

            UnityEngine.Object asset = handle.Result;

            byte[] bytes = null;

            // Dependendo do tipo de asset, pega os bytes
            if (asset is TextAsset textAsset)
            {
                bytes = textAsset.bytes;
            }
            else if (asset is VideoClip videoClip)
            {
                Debug.LogWarning("VideoClip não pode ser convertido em bytes diretamente. Use AssetBundle direto ou baixe como RawFile.");
            }
            else if (asset is Texture2D texture)
            {
                bytes = texture.EncodeToPNG();
            }
            else
            {
                Debug.LogError($"Tipo de asset não suportado para extração de bytes: {asset.GetType()}");
                return null;
            }

            if (bytes != null)
            {
                File.WriteAllBytes(cachePath, bytes);
            }

            Release(handle);

            return cachePath;
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

        public static async Task<string> GetRemoteAssetUrl(string assetKey)
        {
            try
            {
                Debug.Log($"[AddressablesManager] Obtendo URL remota para: {assetKey}");

                // Obtém as localizações do asset
                var resourceLocations = await LoadResourceLocationsAsync(assetKey).Task;
                if (resourceLocations == null || resourceLocations.Count == 0)
                {
                    Debug.LogError($"[AddressablesManager] Nenhuma localização encontrada para: {assetKey}");
                    return null;
                }

                // Itera por todas as localizações para encontrar a URL remota
                foreach (var location in resourceLocations)
                {
                    Debug.Log($"[AddressablesManager] Localização: {location.InternalId} - Provider: {location.ProviderId}");

                    // Se a localização é uma URL remota, retorna
                    if (location.InternalId.StartsWith("http"))
                    {
                        Debug.Log($"[AddressablesManager] ✅ URL remota encontrada: {location.InternalId}");
                        return location.InternalId;
                    }

                    // Para provedores de AssetBundle, podemos tentar obter a URL remota do bundle
                    if (location.ProviderId.Contains("AssetBundle"))
                    {
                        // O InternalId pode ser o caminho local, mas o Addressables pode ter uma URL remota associada
                        // Vamos tentar obter a URL remota a partir do catálogo
                        string remoteUrl = await GetRemoteUrlFromCatalog(assetKey);
                        if (!string.IsNullOrEmpty(remoteUrl))
                        {
                            return remoteUrl;
                        }
                    }
                }

                // Se não encontrou, tenta construir a URL manualmente
                string manualUrl = await BuildManualRemoteUrl(assetKey);
                if (!string.IsNullOrEmpty(manualUrl))
                {
                    return manualUrl;
                }

                Debug.LogError($"[AddressablesManager] ❌ Não foi possível obter URL remota para: {assetKey}");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Erro ao obter URL remota: {e.Message}");
                return null;
            }
        }

        private static async Task<string> GetRemoteUrlFromCatalog(string assetKey)
        {
            // Infelizmente, a API do Addressables não expõe diretamente a URL remota para um asset específico.
            // Vamos tentar obter a URL do catálogo e então substituir o nome do asset.
            try
            {
                // Obtém o catálogo atual
                var catalogs = await LoadResourceLocationsAsync("catalog").Task;
                if (catalogs != null && catalogs.Count > 0)
                {
                    // Pega a primeira localização do catálogo (que deve ser a URL remota do catálogo)
                    string catalogUrl = catalogs[0].InternalId;
                    Debug.Log($"[AddressablesManager] Catalog URL: {catalogUrl}");

                    // Extrai a base URL (remove o nome do arquivo do catálogo)
                    string baseUrl = catalogUrl.Substring(0, catalogUrl.LastIndexOf('/'));
                    Debug.Log($"[AddressablesManager] Base URL: {baseUrl}");

                    // Constrói a URL do asset
                    string assetUrl = $"{baseUrl}/{assetKey}";
                    Debug.Log($"[AddressablesManager] Asset URL construída: {assetUrl}");

                    // Testa se a URL é acessível
                    if (await TestUrlAccess(assetUrl))
                    {
                        return assetUrl;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesManager] Erro ao obter URL do catálogo: {e.Message}");
            }

            return null;
        }

        private static async Task<string> BuildManualRemoteUrl(string assetKey)
        {
            // URL base do seu servidor - ajuste conforme necessário
            string baseUrl = "https://twinnyvr.co/apps/twinny/projects";
            string platform = GetPlatformFolder();

            string remoteUrl = $"{baseUrl}/{platform}/{assetKey}";

            Debug.Log($"[AddressablesManager] Tentando URL manual: {remoteUrl}");

            // Testa se a URL é acessível
            if (await TestUrlAccess(remoteUrl))
            {
                return remoteUrl;
            }

            return null;
        }

        public static async Task<string> GetCachedBundlePathAsync(string key)
        {
            try
            {
                // Localiza o asset pelo key
                AsyncOperationHandle<IList<IResourceLocation>> locateHandle = LoadResourceLocationsAsync(key);
                await locateHandle.Task;

                if (locateHandle.Status != AsyncOperationStatus.Succeeded || locateHandle.Result.Count == 0)
                {
                    Debug.LogError($"[AddressablesUtils] Nenhum resource encontrado para '{key}'.");
                    Release(locateHandle);
                    return null;
                }

                var location = locateHandle.Result[0];

                // Garante o download do bundle (não apenas o asset)
                AsyncOperationHandle<IAssetBundleResource> bundleHandle = ResourceManager.ProvideResource<IAssetBundleResource>(location);
                await bundleHandle.Task;

                if (bundleHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[AddressablesUtils] Falha ao baixar bundle de '{key}'.");
                    Release(bundleHandle);
                    Release(locateHandle);
                    return null;
                }

                var bundleResource = bundleHandle.Result;

                // Reflexão pra pegar o campo interno "m_AssetBundleName" e "m_AssetBundleDataPath"
                var bundleType = bundleResource.GetType();
                string path = null;

                // Unity guarda o caminho interno em um campo privado chamado "m_StreamingAssetsPath" ou "m_CachedAssetBundlePath"
                FieldInfo[] fields = bundleType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var f in fields)
                {
                    if (f.Name.Contains("m_StreamingAssetsPath") || f.Name.Contains("m_CachedAssetBundlePath"))
                    {
                        path = f.GetValue(bundleResource)?.ToString();
                        break;
                    }
                }

                Release(bundleHandle);
                Release(locateHandle);

                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log($"[AddressablesUtils] 📂 Caminho real do bundle: {path}");
                    return path;
                }
                else
                {
                    Debug.LogWarning($"[AddressablesUtils] Bundle carregado, mas não foi possível obter o caminho físico.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AddressablesUtils] Erro ao tentar pegar bundle cache path: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Garante que o asset foi baixado e retorna o path físico no cache.
        /// </summary>
        public static async Task<string> GetLocalAssetPathAsync(string key)
        {
            try
            {
                // Localiza o asset
                AsyncOperationHandle<IList<IResourceLocation>> locateHandle = LoadResourceLocationsAsync(key);
                await locateHandle.Task;

                if (locateHandle.Status != AsyncOperationStatus.Succeeded || locateHandle.Result.Count == 0)
                {
                    Debug.LogError($"[Addressables] Nenhum resource encontrado pra key '{key}'.");
                    Release(locateHandle);
                    return null;
                }

                var location = locateHandle.Result[0];

                // Garante o download do bundle
                AsyncOperationHandle downloadHandle = DownloadDependenciesAsync(key, false);
                await downloadHandle.Task;

                if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[Addressables] Falha ao baixar dependências de '{key}'.");
                    Release(downloadHandle);
                    Release(locateHandle);
                    return null;
                }

                // Aqui vem o pulo do gato 🧠
                string internalId = location.InternalId;

                // Em builds remotas, o Unity substitui o InternalId pelo caminho do cache local
                Debug.Log($"[Addressables] Internal ID encontrado: {internalId}");

                Release(downloadHandle);
                Release(locateHandle);

                return internalId;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Addressables] Erro ao tentar pegar path do asset '{key}': {e.Message}");
                return null;
            }
        }

        private static async Task<bool> TestUrlAccess(string url)
        {
            try
            {
                using (var webRequest = UnityEngine.Networking.UnityWebRequest.Head(url))
                {
                    webRequest.timeout = 5;
                    var operation = webRequest.SendWebRequest();

                    // Aguarda com timeout
                    var timeoutTask = Task.Delay(6000);
                    var requestTask = operation.AsTask();
                    var completedTask = await Task.WhenAny(requestTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        webRequest.Abort();
                        return false;
                    }

                    await operation;

                    if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"[AddressablesManager] URL acessível: {url}");
                        return true;
                    }
                    else
                    {
                        Debug.Log($"[AddressablesManager] URL não acessível: {url} - {webRequest.error}");
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private static Task AsTask(this UnityWebRequestAsyncOperation operation)
        {
            var tcs = new TaskCompletionSource<bool>();
            operation.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }

        public static string GetPlatformFolder()
        {
#if UNITY_EDITOR
            // Pega o build target definido na Unity
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android: return "Android";
                case BuildTarget.iOS: return "iOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64: return "Windows";
                case BuildTarget.StandaloneOSX: return "OSX";
                default: return "Standalone";
            }
#else
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "OSX";
                default:
                    return "Standalone";
            }
#endif
        }


        public static async Task<SceneInstance> LoadAddressableScene(
            string sceneKey,
            bool additive = false,
            Action<float> onProgress = null)
        {
            LoadSceneMode mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;

            // Inicia o carregamento (pode incluir download se necessário)
            AsyncOperationHandle<SceneInstance> handle = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(sceneKey, mode);

            // Monitora o progresso até concluir
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                await Task.Yield(); // evita travar o thread principal
            }

            // Confere o resultado final
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Cena '{sceneKey}' carregada com sucesso!");
            }
            else
            {
                Debug.LogError($"Falha ao carregar a cena '{sceneKey}'");
            }

            return handle.Result;
        }



        public static async Task<SceneInstance> LoadAddressableSceneOLD(string sceneKey, bool additive = false)
        {
            LoadSceneMode mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;

            AsyncOperationHandle<SceneInstance> handle = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(sceneKey, mode);

            // Espera terminar o carregamento
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Cena '{sceneKey}' carregada com sucesso!");
            }
            else
            {
                Debug.LogError($"Falha ao carregar a cena '{sceneKey}'");
            }

            return handle.Result;
            // Opcional: se você não vai mais precisar do handle
            // Addressables.Release(handle);
        }

        public static async Task UnloadAddressableScene(SceneInstance sceneInstance, string sceneKey = "Unknown")
        {
            try
            {
                if (!sceneInstance.Scene.isLoaded)
                {
                    Debug.LogWarning($"Cena {sceneKey} já não está carregada");
                    return;
                }

                Debug.Log($"Descarregando cena: {sceneKey}");

                var unloadHandle = UnityEngine.AddressableAssets.Addressables.UnloadSceneAsync(sceneInstance);
                await unloadHandle.Task;

                if (unloadHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"Cena '{sceneKey}' descarregada com sucesso");
                }
                else
                {
                    Debug.LogError($"Falha ao descarregar cena '{sceneKey}': {unloadHandle.OperationException}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao descarregar cena {sceneKey}: {e}");
            }
        }


        public static string GetBytes(this long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return $"{kb:F2} KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return $"{mb:F2} MB";
            double gb = mb / 1024.0;
            if (gb < 1024) return $"{gb:F2} GB";
            double tb = gb / 1024.0;
            return $"{tb:F2} TB";
        }
    }



}