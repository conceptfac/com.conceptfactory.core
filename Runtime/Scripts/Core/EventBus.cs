using System;
using System.Collections.Generic;
using UnityEngine;

namespace Concept.Core
{
    /// <summary>
    /// A simple publish-subscribe event management system.
    /// Allows components to subscribe, unsubscribe, and publish events identified by name.
    /// </summary>
    public static class EventBus
    {
        /// <summary>
        /// Internal dictionary mapping event names to their subscribed callbacks.
        /// </summary>
        private static readonly Dictionary<string, Action> eventTable = new();
        private static readonly Dictionary<string, Action<object[]>> eventArgsTable = new();


        /// <summary>
        /// Subscribes a method to be called when the specified event is published.
        /// </summary>
        /// <param name="eventName">The name of the event to subscribe to.</param>
        /// <param name="callback">The method to invoke when the event is triggered.</param>

        public static void Subscribe(string eventName, Action callback)
        {
            if (!eventTable.ContainsKey(eventName))
                eventTable[eventName] = callback;
            else
                eventTable[eventName] += callback;
        }
        /// <summary>
        /// Subscribes a method to be called when the specified event is published.
        /// </summary>
        /// <param name="eventName">The name of the event to subscribe to.</param>
        /// <param name="callback">The method to invoke when the event is triggered.</param>
        public static void Subscribe(string eventName, Action<object[]> callback)
        {
            if (eventArgsTable.TryGetValue(eventName, out var existing))
                eventArgsTable[eventName] = existing + callback;
            else
                eventArgsTable[eventName] = callback;
        }

        /// <summary>
        /// Unsubscribes a method from the specified event.
        /// </summary>
        /// <param name="eventName">The name of the event to unsubscribe from.</param>
        /// <param name="callback">The method to remove from the event invocation list.</param>
        public static void Unsubscribe(string eventName, Action callback)
        {
            if (eventTable.ContainsKey(eventName))
            {
                eventTable[eventName] -= callback;
                if (eventTable[eventName] == null)
                    eventTable.Remove(eventName);
            }
        }


        /// <summary>
        /// Unsubscribes a method from the specified event.
        /// </summary>
        /// <param name="eventName">The name of the event to unsubscribe from.</param>
        /// <param name="callback">The method to remove from the event invocation list.</param>
        public static void Unsubscribe(string eventName, Action<object[]> callback)
        {
            if (eventArgsTable.TryGetValue(eventName, out var existing))
            {
                eventArgsTable[eventName] = existing - callback;

                if (eventArgsTable[eventName] == null)
                    eventArgsTable.Remove(eventName);
            }
        }

        /// <summary>
        /// Publishes the specified event, invoking all subscribed methods.
        /// </summary>
        /// <param name="eventName">The name of the event to publish.</param>
        public static void Publish(string eventName)
        {
            if (eventTable.TryGetValue(eventName, out Action action))
                action?.Invoke();
        }

        /// <summary>
        /// Publishes the specified event, invoking all subscribed methods.
        /// </summary>
        /// <param name="eventName">The name of the event to publish.</param>
        /// <param name="args">Params of event to publish.</param>
        public static void Publish(string eventName, params object[] args)
        {
            if (eventArgsTable.TryGetValue(eventName, out var callback))
            {
                callback?.Invoke(args);
            }
            else
            {
                Debug.LogWarning($"[EventBus] No listeners for event '{eventName}'.");
            }
        }




    }
}
