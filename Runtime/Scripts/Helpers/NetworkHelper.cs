using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Concept.Helpers
{
    /// <summary>
    ///  This class is to get network informations.
    /// </summary>
    public static class NetworkHelper
    {
        #region Consts

        private const string IP_SERVICE_URL = "https://api.ipify.org";


        #endregion

        public delegate void onInternetConnectionChanged(bool status);
        public static onInternetConnectionChanged OnInternetConnectionChanged;


        static NetworkHelper() { CheckInternetConnection(); } //Start to check by initialization


        /// <summary>
        /// Checks internet connection
        /// </summary>
        /// <returns>Has Internet: (true or false)</returns>
        public static bool IsWiFiConnected()
        {

            if (UnityEngine.Application.platform == RuntimePlatform.Android)
            {

                try
                {
                    // Obter o contexto da UnityActivity
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        // Acessar o ConnectivityManager do Android
                        using (AndroidJavaClass connectivityManagerClass = new AndroidJavaClass("android.net.ConnectivityManager"))
                        {
                            AndroidJavaObject connectivityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "connectivity");
                            AndroidJavaObject activeNetwork = connectivityManager.Call<AndroidJavaObject>("getActiveNetworkInfo");

                            if (activeNetwork != null)
                            {
                                bool isConnected = activeNetwork.Call<bool>("isConnected");
                                bool isWiFi = activeNetwork.Call<int>("getType") == 1; // 1 significa Wi-Fi
                                return isConnected && isWiFi;
                            }
                            else
                            {
                                Debug.LogWarning("No active network information available.");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Erro ao acessar a conectividade: " + e.Message);
                    return false;
                }
            }
            else
                return UnityEngine.Application.internetReachability != NetworkReachability.NotReachable;
        }


        /// <summary>
        /// This coroutine check every a second if has internet connection
        /// </summary>
        public static async void CheckInternetConnection()
        {
            bool conected = NetworkHelper.IsWiFiConnected();
            while (true)
            {
                //Debug.Log("Teste: "+ conected);
                if (!conected && NetworkHelper.IsWiFiConnected())
                {
                    conected = true;
                    OnInternetConnectionChanged?.Invoke(true);
                }
                else if (conected && !NetworkHelper.IsWiFiConnected())
                {
                    conected = false;
                    OnInternetConnectionChanged?.Invoke(false);
                }
                await Task.Delay(1000);
            }
        }

        public static async Task<List<string>> GetImageListAsync(string directoryUrl)
        {
            List<string> imageUrls = new List<string>();

            using (UnityWebRequest request = UnityWebRequest.Get(directoryUrl))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Erro ao buscar lista de imagens: {request.error}");
                    return imageUrls;
                }

                string html = request.downloadHandler.text;

                // Regex pra pegar links de .jpg, .png, etc
                MatchCollection matches = Regex.Matches(html, @"href\s*=\s*[""']([^""']+\.(png|jpg|jpeg|gif))[""']", RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    string relativePath = match.Groups[1].Value;
                    string fullUrl = directoryUrl + relativePath;
                    imageUrls.Add(fullUrl);
                }
            }

            return imageUrls;
        }

        /// <summary>
        /// Request an image from web and return it as a Texture
        /// </summary>
        /// <param name="url">Image URL to load</param>
        /// <returns>Loaded image as texture.</returns>
        public static async Task<Texture2D> LoadTextureFromUrlAsync(string url)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
                {
                    Debug.LogError($"Failed to download image `{url}` Error: {request.error}");
                    return null;
                }
                else
                {
                    return DownloadHandlerTexture.GetContent(request);
                }
            }
        }

        public static async Task<string> GetPublicIP()
        {
            string publicIP = "0.0.0.0";
            using (UnityWebRequest request = UnityWebRequest.Get(IP_SERVICE_URL))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    publicIP = request.downloadHandler.text.Trim();
                }
                else
                {
                    Debug.LogError("Error to get Public IP: " + request.error);
                }
            }

            return publicIP;
        }

    }
}
