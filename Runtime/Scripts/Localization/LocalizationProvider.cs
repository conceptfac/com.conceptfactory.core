using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Concept.Localization
{
    /// <summary>
    /// Provides centralized access to Unity Localization features, 
    /// including initialization, locale management, and string retrieval. 
    /// Supports registering update actions triggered when the locale changes.
    /// </summary>
    public static class LocalizationProvider
    {
        public static bool isRunning;

        private static List<Func<Task>> m_updateActions = new List<Func<Task>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        /// <summary>
        /// Initializes the localization system after the scene load.
        /// </summary>
        public static void Initialize()
        {
            _ = LocalizationInitialize();
        }

        /// <summary>
        /// Performs asynchronous initialization of the Unity Localization system
        /// and subscribes to locale change events.
        /// </summary>
        private static async Task LocalizationInitialize()
        {
            // Wait a few frames to ensure subsystems are fully loaded
            await Task.Yield();
            await Task.Yield();
            await Task.Yield();

            var init = LocalizationSettings.InitializationOperation;
            await init.Task;

            isRunning = init.Status == AsyncOperationStatus.Succeeded;
            LocalizationSettings.SelectedLocaleChanged += UpdateLocalizedElements;
        }

        /// <summary>
        /// Sets the active locale asynchronously using the locale code.
        /// </summary>
        /// <param name="localeCode">The identifier code of the locale (e.g., "en", "fr").</param>
        public static async Task SetLocaleAsync(string localeCode)
        {
            await LocalizationSettings.InitializationOperation.Task;

            var currentLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (currentLocale != null && currentLocale != LocalizationSettings.SelectedLocale)
            {
                LocalizationSettings.SelectedLocale = currentLocale;
            }
        }

        /// <summary>
        /// Sets the active locale synchronously using the locale code.
        /// </summary>
        /// <param name="localeCode">The identifier code of the locale (e.g., "en", "fr").</param>
        public static void SetLocale(string localeCode)
        {
            var currentLocale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (currentLocale != null && currentLocale != LocalizationSettings.SelectedLocale)
            {
                LocalizationSettings.SelectedLocale = currentLocale;
            }
        }

        /// <summary>
        /// Retrieves a localized string asynchronously by collection and key.
        /// </summary>
        /// <param name="collection">The name of the string table collection to search.</param>
        /// <param name="key">The key of the localized entry within the collection.</param>
        /// <returns>
        /// A tuple where <c>success</c> indicates if the string was found, 
        /// and <c>text</c> contains the localized value or fallback key.
        /// </returns>
        public static async Task<(bool success, string text)> GetLocalizedStringAsync(string collection, string key)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(collection))
                return (false, null);

            var init = LocalizationSettings.InitializationOperation;
            await init.Task;

            if (init.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LocalizationProvider] Localization initialization failed with status: {init.Status}");
                return (false, key);
            }

            var currentLocale = LocalizationSettings.SelectedLocale;

            var tableHandle = LocalizationSettings.StringDatabase.GetTableAsync(collection, currentLocale);
            await tableHandle.Task;

            if (tableHandle.Status != AsyncOperationStatus.Succeeded || tableHandle.Result == null)
            {
                Debug.LogWarning($"[LocalizationProvider] Table '{collection}' not found for locale '{currentLocale.Identifier.Code}'.");
                return (false, key);
            }

            var table = tableHandle.Result as StringTable;
            if (table == null)
            {
                Debug.LogWarning($"[LocalizationProvider] Table '{collection}' is not a StringTable!");
                return (false, key);
            }

            var entry = table.GetEntry(key);
            if (entry == null)
            {
              //  Debug.LogWarning($"[LocalizationProvider] Entry '{key}' not found in table '{collection}' for locale '{currentLocale.Identifier.Code}'.");
                return (false, key);
            }
            return (true, entry.Value);
        }

        /// <summary>
        /// Registers an async update action to be called whenever the locale changes.
        /// </summary>
        /// <param name="action">A function returning a <see cref="Task"/> to execute on locale change.</param>
        public static void RegisterUpdateAction(Func<Task> action)
        {
            if (!m_updateActions.Contains(action))
                m_updateActions.Add(action);
        }

        /// <summary>
        /// Removes a previously registered async update action.
        /// </summary>
        /// <param name="action">The update action to remove from the registry.</param>
        public static void RemoveUpdateAction(Func<Task> action)
        {
            if (m_updateActions.Contains(action))
                m_updateActions.Remove(action);
        }

        /// <summary>
        /// Handles the locale changed event and triggers all registered update actions.
        /// </summary>
        /// <param name="locale">The newly selected locale.</param>
        private static void UpdateLocalizedElements(Locale locale)
        {
            _ = UpdateLocalizedElementsAsync();
        }

        /// <summary>
        /// Executes all registered update actions asynchronously.
        /// </summary>
        private static async Task UpdateLocalizedElementsAsync()
        {
            foreach (var action in m_updateActions.ToList())
            {
                if (action != null)
                    await action();
            }
        }
    }
}
