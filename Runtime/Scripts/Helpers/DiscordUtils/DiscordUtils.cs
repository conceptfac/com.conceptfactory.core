using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Helpers
{
    /// <summary>
    /// Provides utility methods to interact with Discord webhooks.
    /// Supports sending plain text messages asynchronously using HTTP POST requests.
    /// </summary>
    public static class DiscordUtils
    {
        // Shared HttpClient instance used for sending HTTP requests.
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Sends a plain text message to a Discord webhook asynchronously.
        /// </summary>
        /// <param name="webhookUrl">The URL of the Discord webhook.</param>
        /// <param name="message">The message content to send.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task SendMessageAsync(string webhookUrl, string message)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl) || string.IsNullOrWhiteSpace(message))
            {
                Debug.LogWarning("Webhook URL or message is empty.");
                return;
            }

            var payload = new DiscordMessage { content = message };
            string jsonPayload = JsonUtility.ToJson(payload);

            try
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Message successfully sent to Discord!");
                }
                else
                {
                    Debug.LogError($"Failed to send webhook: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"Error sending webhook: {e.Message}");
            }
        }

        /// <summary>
        /// Sends a Discord embed message using manual JSON (works with arrays & fields).
        /// </summary>
        public static async Task SendEmbedAsync(string webhookURL, string title, string description, string stackTrace = "", int color = 16777215)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                return;
            string fullTitle = $"[{Application.productName}] {title}";
            if (stackTrace == string.Empty) stackTrace = "No stack trace for this message.";
            // Manual JSON string for Discord embed
            string jsonPayload = @"{
                ""embeds"": [{
                    ""title"": """ + EscapeJson(fullTitle) + @""",
                    ""description"": """ + EscapeJson(description) + @""",
                    ""color"": " + color + @",
                    ""timestamp"": """ + DateTime.UtcNow.ToString("o") + @""",
                    ""fields"": [{
                        ""name"": ""Stack Trace"",
                        ""value"": """ + EscapeJson(stackTrace) + @""",
                        ""inline"": false
                    }]
                }]
            }";

            try
            {
                using var client = new HttpClient();
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhookURL, content);

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    Debug.LogError($"Discord webhook failed: {response.StatusCode} - {body}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send Discord embed: {e.Message}");
            }
        }

        /// <summary>
        /// Escapes special characters for JSON strings
        /// </summary>
        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }


        /// <summary>
        /// Internal class representing the payload structure for a Discord webhook message.
        /// </summary>
        [System.Serializable]
        private class DiscordMessage
        {
            public string content;
        }
    }
}
