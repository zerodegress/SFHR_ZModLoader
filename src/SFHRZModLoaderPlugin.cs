#nullable enable
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SFHR_ZModLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SFHRZModLoaderPlugin : BaseUnityPlugin
    {
        internal const string ZERO_COMPONENTS_NAME = "ZeroComponentsv1";
        internal static GameObject? ZeroComponents { get; set; }
        public static InputMonitor? InputMonitor { get; set; }
        internal static new ManualLogSource? Logger { get; set; }
        //public static GameContext? Context { get; set; }
        public static SaveManager? SaveManager { get; set; }
        public static ModLoader? ModLoader { get; set; }
        public static GameContext? GameContext { get; set; }
        public static bool DebugEmit { get; set; } = false;

        private void Awake()
        {
            Logger = base.Logger;
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            if (DebugEmit)
            {
                if(!Directory.Exists(Path.Combine(Paths.GameRootPath, "DebugEmit"))) {
                    Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "DebugEmit"));
                }
            }

            ZeroComponents = GameObject.Find(ZERO_COMPONENTS_NAME);
            if(ZeroComponents == null) 
            {
                ZeroComponents = new GameObject(ZERO_COMPONENTS_NAME);
                GameObject.DontDestroyOnLoad(ZeroComponents);
                ZeroComponents.hideFlags = HideFlags.HideAndDontSave;
            }
            InputMonitor = ZeroComponents.GetComponent<InputMonitor>();
            if(InputMonitor == null)
            {
                InputMonitor = ZeroComponents.AddComponent<InputMonitor>();
            }

            // SaveMgr = new SaveManager(Path.Combine(Paths.GameRootPath, "saves"));
            ModLoader = new ModLoader(Path.Combine(Paths.GameRootPath, "mods"));
            ModLoader.LoadMods();
            InputMonitor.RegisterAction(KeyCode.P, () => ModLoader.LoadMods(true));

            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
    }
}
