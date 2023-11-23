#nullable enable
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

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

        ModLoader = new ModLoader(Path.Combine(Paths.GameRootPath, "mods"), Logger, EventManager);
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
                type = "GAMECONTEXT_PATCH",
                data = GameContext,
            });
        });
        InputMonitor.SetAction(UnityEngine.KeyCode.U, () => {
            foreach(var spriteRenderer in Resources.FindObjectsOfTypeAll<SpriteRenderer>())
            {
                if(spriteRenderer != null)
                {
                    Logger.LogInfo("az");
                    var texture = new Texture2D(1, 1);
                    spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                }
            }
            
            foreach(var mesh in Resources.FindObjectsOfTypeAll<MeshRenderer>())
            {
                if(mesh != null)
                {
                    mesh.material = new Material(mesh.material)
                    {
                        mainTexture = new Texture2D(1, 1)
                    };
                }
            }
        });

        Harmony.CreateAndPatchAll(typeof(Hooks));
    }
}
