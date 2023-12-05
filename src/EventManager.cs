#nullable enable
using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SFHR_ZModLoader
{
    public struct Event
    {
        public string type;
        public object data;
    }

    public class EventManager
    {
        private readonly Dictionary<string, Action<Event>> eventHandlers;
        private readonly ManualLogSource logger;

        public EventManager(ManualLogSource logger)
        {
            eventHandlers = new();
            this.logger = logger;
        }

        public void EmitEvent(Event ev)
        {
            logger.LogInfo($"Event: {ev.type}");
            foreach(var handler in eventHandlers)
            {
                if(handler.Key == ev.type)
                {
                    handler.Value(ev);
                }
            }
        }

        public void RegisterEventHandler(string type, Action<Event> handler, string? handlerId = null)
        {
            if(eventHandlers.TryGetValue(type, out var curHandler))
            {
                eventHandlers[type] = ev => {
                    curHandler(ev);
                    handler(ev);
                };
            }
            else
            {
                eventHandlers[type] = handler;
            }
        }

        public void ClearEventHandler(string type)
        {
            eventHandlers.Remove(type);
        }
    }
}