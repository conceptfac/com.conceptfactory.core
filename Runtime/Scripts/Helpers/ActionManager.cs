using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Helpers
{
    /// <summary>
    /// This class is used to handle custom actions.
    /// </summary>
    public static class ActionManager
    {

        private static Dictionary<string,Action> _customActions = new Dictionary<string,Action>();

        /// <summary>
        /// Register a new action in registry
        /// </summary>
        /// <param name="actionName">Name of action</param>
        /// <param name="action">Action reference.</param>
        public static void RegisterAction(string actionName, Action action)
        {
            if (!_customActions.ContainsKey(actionName))
                _customActions.Add(actionName, action);
            else
                Debug.LogWarning($"[ActionManager] Action '{actionName}' already existis in CustomActions registry.");
        }


        /// <summary>
        /// Call a especific action in CustomActions registry
        /// </summary>
        /// <param name="actionName">Name of action</param>
        public static void CallAction(string actionName)
        {
            if (_customActions.ContainsKey(actionName))
                _customActions[actionName].Invoke();
            else
                Debug.LogWarning($"[ActionManager] Action '{actionName}' not founded in CustomActions registry.");
        }


        /// <summary>
        /// Remove a custom action from registry
        /// </summary>
        /// <param name="actionName">Name of action</param>
        public static void RemoveAction(string actionName) {
            if (_customActions.ContainsKey(actionName))
                _customActions.Remove(actionName);
            else
                Debug.LogWarning($"[ActionManager] Action '{actionName}' not founded in CustomActions registry.");
        }
    }
}
