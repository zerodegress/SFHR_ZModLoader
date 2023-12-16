#nullable enable
using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Cpp2IL.Core.Extensions;

namespace SFHR_ZModLoader
{
    public struct Event
    {
        public string type;
        public object data;
    }

    public class EventManager
    {
        private readonly Dictionary<string, Dictionary<string, Action<Event>>> eventHandlers;
        private readonly ManualLogSource logger;

        public EventManager(ManualLogSource logger)
        {
            eventHandlers = new();
            this.logger = logger;
        }

        public void EmitEvent(Event ev)
        {
            logger.LogInfo($"Event: {ev.type}");
            if (eventHandlers.TryGetValue(ev.type, out var handlers))
            {
                foreach (var handler in handlers.Clone())
                {
                    handler.Value.Invoke(ev);
                }
            }
        }

        public string RegisterEventHandler(string type, Action<Event> handler, string? handlerId = null)
        {
            Dictionary<string, Action<Event>> handlers;
            if (eventHandlers.TryGetValue(type, out var _handlers))
            {
                handlers = _handlers;
            }
            else
            {
                eventHandlers.Add(type, new());
                handlers = eventHandlers[type];
            }
            var id = handlerId ?? Guid.NewGuid().ToString();
            handlers.Add(id, handler);
            return id;
        }

        public void UnregisterEventHandler(string type, string handlerId)
        {
            if (eventHandlers.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handlerId);
            }
        }
    }
}