using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Helpers
{
    /// <summary>
    ///  This class is to get network informations.
    /// </summary>
    public static class NetworkHelper 
    {

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


    }
}
