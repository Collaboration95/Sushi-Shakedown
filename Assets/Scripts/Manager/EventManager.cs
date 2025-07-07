using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private Dictionary<string, Delegate> _eventTable = new Dictionary<string, Delegate>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 0-ARG SUBSCRIBE
    public void Subscribe(string eventName, Action listener)
    {
        if (_eventTable.TryGetValue(eventName, out var existing))
            _eventTable[eventName] = Delegate.Combine(existing, listener);
        else
            _eventTable[eventName] = listener;
    }

    // 0-ARG UNSUBSCRIBE
    public void Unsubscribe(string eventName, Action listener)
    {
        if (!_eventTable.TryGetValue(eventName, out var existing)) return;
        var updated = Delegate.Remove(existing, listener);
        if (updated == null)
            _eventTable.Remove(eventName);
        else
            _eventTable[eventName] = updated;
    }

    // 0-ARG TRIGGER
    public void Trigger(string eventName)
    {
        if (_eventTable.TryGetValue(eventName, out var del) && del is Action callback)
            callback.Invoke();
    }

    // 1-ARG SUBSCRIBE
    public void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (_eventTable.TryGetValue(eventName, out var existing))
            _eventTable[eventName] = Delegate.Combine(existing, listener);
        else
            _eventTable[eventName] = listener;
    }

    // 1-ARG UNSUBSCRIBE
    public void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        if (!_eventTable.TryGetValue(eventName, out var existing)) return;
        var updated = Delegate.Remove(existing, listener);
        if (updated == null)
            _eventTable.Remove(eventName);
        else
            _eventTable[eventName] = updated;
    }

    // 1-ARG TRIGGER
    public void Trigger<T>(string eventName, T arg)
    {
        if (_eventTable.TryGetValue(eventName, out var del) && del is Action<T> callback)
            callback.Invoke(arg);
    }
}
