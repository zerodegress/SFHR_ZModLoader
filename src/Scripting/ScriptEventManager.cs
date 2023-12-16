#nullable enable

using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;

namespace SFHR_ZModLoader.Scripting;

public class ScriptObjectEventManager
{

    public class AddEventListenerOptions
    {
        public bool? once;
    }

    private readonly Dictionary<string, List<string>> eventIds = new();
    private readonly EventManager eventManager;
    private Engine engine;
    public ScriptObjectEventManager(Engine engine, EventManager eventManager)
    {
        this.eventManager = eventManager;
        this.engine = engine;
    }
    public string addEventListener(string type, Delegate listener, AddEventListenerOptions? options = null)
    {
        string id;
        if (options != null)
        {
            if (options.once ?? false)
            {
                id = Guid.NewGuid().ToString();
                eventManager.RegisterEventHandler(type, ev =>
                {
                    listener.DynamicInvoke(null, new JsValue[] { JsValue.FromObject(engine, ev) });
                    removeEventListener(type, id);
                }, id);
            }
            else
            {
                id = eventManager.RegisterEventHandler(type, ev =>
                {
                    listener.DynamicInvoke(null, new JsValue[] { JsValue.FromObject(engine, ev) });
                });
            }
        }
        else
        {
            id = eventManager.RegisterEventHandler(type, ev =>
            {
                listener.DynamicInvoke(null, new JsValue[] { JsValue.FromObject(engine, ev) });
            });
        }
        List<string> list;
        if (eventIds.TryGetValue(type, out var _list))
        {
            list = _list;
        }
        else
        {
            List<string> __list = new();
            eventIds.Add(type, __list);
            list = __list;
        }
        list.Add(id);
        return id;
    }

    public void removeEventListener(string type, string handlerId)
    {
        eventManager.UnregisterEventHandler(type, handlerId);
        eventIds.Remove(handlerId);
    }

    public void dispatchEvent(string type, object? data = null)
    {
        eventManager.EmitEvent(new Event
        {
            type = type,
            data = data!
        });
    }

    public void clearEventListeners()
    {
        foreach (var ids in eventIds)
        {
            foreach (var id in ids.Value)
            {
                removeEventListener(ids.Key, id);
            }
        }
        eventIds.Clear();
    }
}