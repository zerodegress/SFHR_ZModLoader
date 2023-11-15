#nullable enable

using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace SFHR_ZModLoader
{
    public class InputMonitor: MonoBehaviour
    {
        private ManualLogSource? Logger { get; set; } = SFHRZModLoaderPlugin.Logger;
        private Dictionary<KeyCode, Action> KeyboradListeners { get; set; }

        public InputMonitor()
        {
            KeyboradListeners = new();
        }

        public void Update()
        {
            foreach (var kvPair in KeyboradListeners)
            {
                if(Input.GetKeyUp(kvPair.Key))
                {
                    kvPair.Value.Invoke();
                }
            }
        }

        public void RegisterAction(KeyCode keyCode, Action action)
        {
            KeyboradListeners.Add(keyCode, action);
        }
    }
}