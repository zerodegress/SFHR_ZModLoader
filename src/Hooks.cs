#nullable enable
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SFHR_ZModLoader 
{
    public class Hooks 
    {
        private static GlobalData? globalData;
        private static SD? saveData;
        private static ManualLogSource? Logger { get => SFHRZModLoaderPlugin.Logger; }
        private static EventManager? EventManager { get => SFHRZModLoaderPlugin.EventManager; }

        [HarmonyPostfix, HarmonyPatch(typeof(SD), nameof(SD.Load))]
        public static void Postfix_SD_Load(SD __instance)
        {
            saveData = __instance;
            EventManager?.EmitEvent(new Event {
                type = "SD_LOADED",
                data = __instance
            });
            if(globalData != null && Logger != null)
            {
                SFHRZModLoaderPlugin.GameContext = new(globalData, saveData, Logger);
                EventManager?.EmitEvent(new Event {
                    type = "GAMECONTEXT_LOADED",
                    data = SFHRZModLoaderPlugin.GameContext,
                });
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GlobalData), nameof(GlobalData.Load))]
        public static void Postfix_GlobalData_Load() 
        {
            if (!GI.GlobalData)
            {
                return;
            }
            globalData = GI.GlobalData;
            EventManager?.EmitEvent(new Event {
                type = "GLOBALDATA_LOADED",
                data = globalData,
            });
            if(saveData != null && Logger != null)
            {
                SFHRZModLoaderPlugin.GameContext = new(globalData, saveData, Logger);
                EventManager?.EmitEvent(new Event {
                    type = "GAMECONTEXT_LOADED",
                    data = SFHRZModLoaderPlugin.GameContext,
                });
            }
        }
    }
}