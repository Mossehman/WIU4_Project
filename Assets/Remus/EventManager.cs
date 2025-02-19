using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EventManager is a centralized event system for dynamic communication between scripts.
/// This works very close to how Roblox handles their Bindable and Remote Events.
/// </summary>
public static class EventManager
{
    /// <summary>
    /// Stores all events and their listeners.
    /// Key: Event name (string), Value: List of subscribed listeners (Action<object[]>).
    /// </summary>
    private static Dictionary<string, List<Action<object[]>>> eventDictionary = new();

    private static bool debugMode = false; // Enables or disables debugging logs for event tracking.

    /// <summary>
    /// Creates a new event if it does not already exist.
    /// </summary>
    /// <param name="eventName">The name of the event to create.</param>
    public static void CreateEvent(string eventName)
    {
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = new List<Action<object[]>>();
            if (debugMode) Debug.Log($"[EventBus] Created event: {eventName}");
        }
    }

    /// <summary>
    /// Subscribes a listener to an event.
    /// If the event does not exist, it is automatically created.
    /// </summary>
    /// <param name="eventName">The name of the event to subscribe to.</param>
    /// <param name="callback">The function to be called when the event is fired.</param>
    public static void Connect(string eventName, Action<object[]> callback)
    {
        if (!eventDictionary.ContainsKey(eventName))
            CreateEvent(eventName);

        if (!eventDictionary[eventName].Contains(callback))
        {
            eventDictionary[eventName].Add(callback);
            if (debugMode) Debug.Log($"[EventBus] Listener added to event: {eventName} (Total: {eventDictionary[eventName].Count})");
        }
    }

    /// <summary>
    /// Fires an event, notifying all subscribed listeners.
    /// </summary>
    /// <param name="eventName">The name of the event to fire.</param>
    /// <param name="args">Optional parameters to send with the event.</param>
    public static void Fire(string eventName, params object[] args)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            List<Action<object[]>> listeners = new List<Action<object[]>>(eventDictionary[eventName]);

            if (debugMode) Debug.Log($"[EventBus] Firing event: {eventName} (Listeners: {listeners.Count})");

            foreach (var listener in listeners)
            {
                listener?.Invoke(args);
            }
        }
        else if (debugMode)
        {
            Debug.LogWarning($"[EventBus] Attempted to fire non-existent event: {eventName}");
        }
    }

    /// <summary>
    /// Unsubscribes a listener from an event.
    /// If the event has no more listeners, it is automatically removed.
    /// </summary>
    /// <param name="eventName">The name of the event to unsubscribe from.</param>
    /// <param name="callback">The function to remove from the event.</param>
    public static void Disconnect(string eventName, Action<object[]> callback)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName].Remove(callback);
            if (debugMode) Debug.Log($"[EventBus] Listener removed from event: {eventName} (Remaining: {eventDictionary[eventName].Count})");

            if (eventDictionary[eventName].Count == 0)
            {
                eventDictionary.Remove(eventName);
                if (debugMode) Debug.Log($"[EventBus] Removed empty event: {eventName}");
            }
        }
    }

    /// <summary>
    /// Clears all registered events and their listeners.
    /// Useful when resetting the game state or loading a new scene.
    /// </summary>
    public static void ClearAllEvents()
    {
        eventDictionary.Clear();
        if (debugMode) Debug.Log("[EventBus] All events cleared.");
    }

    /// <summary>
    /// Checks if an event exists.
    /// </summary>
    /// <param name="eventName">The name of the event to check.</param>
    /// <returns>True if the event exists, otherwise false.</returns>
    public static bool HasEvent(string eventName)
    {
        return eventDictionary.ContainsKey(eventName);
    }

    /// <summary>
    /// Gets the number of listeners subscribed to a specific event.
    /// </summary>
    /// <param name="eventName">The event name to check.</param>
    /// <returns>The number of listeners, or -1 if the event does not exist.</returns>
    public static int GetListenerCount(string eventName)
    {
        return eventDictionary.ContainsKey(eventName) ? eventDictionary[eventName].Count : -1;
    }

    /// <summary>
    /// Logs all currently registered events and their listener counts.
    /// </summary>
    public static void DebugAllEvents()
    {
        Debug.Log("======= [EventBus] Registered Events =======");
        foreach (var entry in eventDictionary)
        {
            Debug.Log($"Event: {entry.Key} | Listeners: {entry.Value.Count}");
        }
        Debug.Log("============================================");
    }

    /// <summary>
    /// Enables or disables debug mode for logging event activity.
    /// </summary>
    /// <param name="enable">True to enable debugging, false to disable.</param>
    public static void EnableDebugging(bool enable)
    {
        debugMode = enable;
        Debug.Log($"[EventBus] Debugging {(debugMode ? "Enabled" : "Disabled")}");
    }
}