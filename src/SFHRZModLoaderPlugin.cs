#nullable enable
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace SFHR_ZModLoader;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class SFHRZModLoaderPlugin : BasePlugin
{
    internal const string ZERO_COMPONENTS_NAME = "ZeroComponentsv1";
    internal static UnityEngine.GameObject? ZeroComponents { get; set; }
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

        ClassInjector.RegisterTypeInIl2Cpp<InputMonitor>();

        ZeroComponents = UnityEngine.GameObject.Find(ZERO_COMPONENTS_NAME);
        if(ZeroComponents == null) 
        {
            ZeroComponents = new UnityEngine.GameObject(ZERO_COMPONENTS_NAME);
            UnityEngine.GameObject.DontDestroyOnLoad(ZeroComponents);
            ZeroComponents.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
        }
        InputMonitor = ZeroComponents.GetComponent<InputMonitor>();
        if(InputMonitor == null)
        {
            InputMonitor = ZeroComponents.AddComponent<InputMonitor>();
        }

        // 测试下UI

        ModLoader = new ModLoader(Path.Combine(Paths.GameRootPath, "mods"));
        ModLoader.RegisterEvents(EventManager);
        EventManager.EmitEvent(new Event {
            type = "MODS_LOAD"
        });
        InputMonitor.SetAction(UnityEngine.KeyCode.P, () => {
            if(GameContext == null)
            {
                return;
            }
            EventManager.EmitEvent(new Event {
                type = "MODS_RELOAD"
            });
        });
        Harmony.CreateAndPatchAll(typeof(Hooks));
    }
}
