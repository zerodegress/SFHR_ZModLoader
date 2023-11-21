#nullable enable
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using BepInEx6.PluginTemplate.Unity.Il2Cpp;

namespace SFHR_ZModLoader
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class SFHRZModLoaderPlugin : BasePlugin
    {
        internal const string ZERO_COMPONENTS_NAME = "ZeroComponentsv1";
        internal static GameObject? ZeroComponents { get; set; }
        public static InputMonitor? InputMonitor { get; set; }
        internal static ManualLogSource? Logger { get; set; }
        //public static GameContext? Context { get; set; }
        public static ModLoader? ModLoader { get; set; }
        public static GameContext? GameContext { get; set; }
        public static EventManager? EventManager { get; set; }
        public static bool DebugEmit { get; set; } = true;

        public override void Load()
        {
            Logger = base.Log;
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            if (DebugEmit)
            {
                if(!Directory.Exists(Path.Combine(Paths.GameRootPath, "DebugEmit"))) {
                    Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "DebugEmit"));
                }
            }


            EventManager = new(Logger);
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
            ModLoader = new ModLoader(Path.Combine(Paths.GameRootPath, "mods"), Logger, EventManager);
            ModLoader.RegisterEvents(EventManager);
            EventManager.EmitEvent(new Event {
                type = "MODS_LOAD"
            });
            InputMonitor.SetAction(KeyCode.P, () => {
                if(GameContext == null)
                {
                    return;
                }
                EventManager.EmitEvent(new Event {
                    type = "GAMECONTEXT_PATCH",
                    data = GameContext,
                });
            });

            Harmony.CreateAndPatchAll(typeof(Hooks));
        }
    }
}
