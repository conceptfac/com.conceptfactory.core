using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Concept.Core
{
    /// <summary>
    /// This static class provides a centralized handler for managing callbacks.
    /// It allows you to register, unregister, and invoke actions or tasks on callbacks of a specific type.
    /// </summary>
    public static class CallbackHub
    {
        /// <summary>
        /// A list that holds all registered callbacks.
        /// </summary>
        private static List<object> callBacks = new List<object>();
        /// <summary>
        /// Registers a callback of type <typeparamref name="T"/> to the handler.
        /// The callback will be stored and can later be invoked or unregistered.
        /// </summary>
        /// <typeparam name="T">The type of the callback to register.</typeparam>
        /// <param name="callback">The callback instance to register.</param>
        public static void RegisterCallback<T>(T callback) where T : class
        {
            if (!callBacks.Contains(callback))
            {
                callBacks.Add(callback);
            }
        }
        /// <summary>
        /// Unregisters a previously registered callback of type <typeparamref name="T"/>.
        /// The callback will no longer be invoked when an action or task is called.
        /// </summary>
        /// <typeparam name="T">The type of the callback to unregister.</typeparam>
        /// <param name="callback">The callback instance to unregister.</param>
        public static void UnregisterCallback<T>(T callback) where T : class
        {
            if (callBacks.Contains(callback))
            {
                callBacks.Remove(callback);
            }
        }
        /// <summary>
        /// Calls a given action on all registered callbacks of type <typeparamref name="T"/>.
        /// If a callback is of the specified type, the action is invoked with that callback as a parameter.
        /// </summary>
        /// <typeparam name="T">The type of the callbacks to invoke the action on.</typeparam>
        /// <param name="action">The action to perform on each callback.</param>
        public static void CallAction<T>(Action<T> action) where T : class
        {
            var snapshot = callBacks.ToArray(); // Cria uma cópia (snapshot) da lista atual

            foreach (var callback in snapshot)
            {
                if (callback is T)
                    action(callback as T);
            }
        }

        /// <summary>
        /// Calls an asynchronous task on all registered callbacks of type <typeparamref name="T"/>.
        /// If a callback is of the specified type, the task is executed with that callback as a parameter.
        /// </summary>
        /// <typeparam name="T">The type of the callbacks to execute the task on.</typeparam>
        /// <param name="task">The asynchronous task to perform on each callback.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task CallTask<T>(Func<T, Task> task) where T : class
        {
            foreach (var callback in callBacks)
            {
                await task(callback as T);
            }
        }

    }
}
