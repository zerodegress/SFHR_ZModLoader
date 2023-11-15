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

        [HarmonyPrefix, HarmonyPatch(typeof(SD), nameof(SD.Load))]
        public static bool Prefix_SD_Load(SD __instance) {
            if (SFHRZModLoaderPlugin.SaveManager == null) {
                return true;
            }
            var saveManager = SFHRZModLoaderPlugin.SaveManager;
            saveManager.PatchSDLoad(__instance);
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SD), nameof(SD.Load))]
        public static void Postfix_SD_Load(SD __instance)
        {
            saveData = __instance;
            if(globalData != null && saveData != null)
            {
                SFHRZModLoaderPlugin.GameContext = new(globalData, saveData);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SD), nameof(SD.Save))]
        public static bool Prefix_SD_Save(SD __instance) {
            if (SFHRZModLoaderPlugin.SaveManager == null) {
                return true;
            }
            var saveManager = SFHRZModLoaderPlugin.SaveManager;
            saveManager.PatchSDSave(__instance);
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SD), nameof(SD.SaveControls))]
        public static bool Prefix_SD_SaveControls(SD __instance)
        {
            if (SFHRZModLoaderPlugin.SaveManager == null) {
                return true;
            }
            SFHRZModLoaderPlugin.SaveManager.SaveControlsToSettingsFile(UnityEngine.Object.FindObjectOfType<PlayerInput>(true).actions.SaveBindingOverridesAsJson());
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GlobalData), nameof(GlobalData.Load))]
        public static void Postfix_GlobalData_Load() 
        {
            if (!GI.GlobalData)
            {
                return;
            }
            globalData = GI.GlobalData;
            if(saveData != null)
            {
                SFHRZModLoaderPlugin.GameContext = new(globalData, saveData);
            }
            SFHRZModLoaderPlugin.ModLoader?.PatchGlobalDataLoad(globalData);
        }
    }
}