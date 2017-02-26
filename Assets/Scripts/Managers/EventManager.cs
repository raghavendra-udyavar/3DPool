using System;
using System.Collections.Generic;
using KsubakaPool.EventHandlers;
using UnityEngine;

namespace KsubakaPool.Managers
{
    /// <summary>
    /// the responsibility of this class is to register for events and dispath them
    /// for now this is just a basic event handler,
    /// this helps to make sure we have registered and deregistered all the events and are not dangling
    /// </summary>
    public static class EventManager
    {
        private static Dictionary<string, Action<object, IGameEvent>> _subscribers = new Dictionary<string, Action<object, IGameEvent>>();

        public static void Subscribe(string eventID, Action<object, IGameEvent> callback)
        {
            if (_subscribers.ContainsKey(eventID))
                _subscribers[eventID] += callback;
            else
                _subscribers.Add(eventID, callback);
        }

        public static void Unsubscribe(string eventID, Action<object, IGameEvent> callback)
        {
            if (_subscribers.ContainsKey(eventID))
                _subscribers[eventID] -= callback;
        }

        public static void Notify(string eventID, object sender, IGameEvent gameEvent)
        {
            if (_subscribers.ContainsKey(eventID))
            {
                // let this throw an exception so that it is properly handled during the development stage
                Action<object, IGameEvent> selectedCallback = _subscribers[eventID];
                Debug.Assert(selectedCallback != null, "There are no subscribed events for this " + eventID);

                selectedCallback(sender, gameEvent);
            }
        }
    }
}
