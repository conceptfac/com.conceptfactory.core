using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Helpers
{
    /// <summary>
    /// Captures global exceptions and Unity logs and sends them to Discord via webhook using embeds.
    /// </summary>
    public class DiscordExceptionHandler : MonoBehaviour
    {
        public bool sendWarnings;
        [Header("Discord Webhook URL")]
        public string webhookUrl;


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDestroy()
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                SendExceptionEmbedAsync(ex);
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
                string fullMessage = $"```{stackTrace}```";
            if (type == LogType.Exception || type == LogType.Error)
                _ = DiscordUtils.SendEmbedAsync(webhookUrl, "Error / Exception", condition, fullMessage, 16711680); // Red color
            if (sendWarnings &&  type == LogType.Warning)
                _ = DiscordUtils.SendEmbedAsync(webhookUrl, "Warning", condition, fullMessage, 16753920); // Orange color

        }

        private async void SendExceptionEmbedAsync(Exception ex)
        {
            string stack = $"```{ex.StackTrace}```";
            string message = ex.Message;
            string title = ex.GetType().Name;

            await DiscordUtils.SendEmbedAsync(webhookUrl, title, message, stack, 16711680); // Red color
        }
       

#if UNITY_EDITOR
        [ContextMenu("SEND TEST MESSAGE")]
        private void SendTestMessage()
        {
            string testMessage = "Discord webhook connection test successful!";
            string stack = "No stack trace for test message.";
            _ = DiscordUtils.SendEmbedAsync(webhookUrl, "Test Message", testMessage, stack, 65280); // Green color
        }
#endif
    }
}
