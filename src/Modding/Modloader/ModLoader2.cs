#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Jint;
using SFHR_ZModLoader.Scripting;

namespace SFHR_ZModLoader.Modding;

public partial class ModLoader
{
    private Dictionary<string, Mod2> mod2s = new();
    private ScriptObjectEventManager? scriptEventManager;
    public void LoadMod2s()
    {
        var engineWrapper = new ModScriptEngineWrapper();
        engineWrapper.Engine.SetValue("ModFS", new ScriptObjectFs(dir));

        scriptEventManager?.clearEventListeners();
        scriptEventManager = new ScriptObjectEventManager(engineWrapper.Engine, SFHRZModLoaderPlugin.EventManager!);
        engineWrapper.Engine.SetValue("EventManager", scriptEventManager);

        engineWrapper.Engine.SetValue("console", new ScriptObjectConsole(SFHRZModLoaderPlugin.Logger!));
        foreach (var directory in Directory.EnumerateDirectories(dir))
        {
            if (File.Exists(Path.Combine(directory, "mod2.json")))
            {
                Mod2 mod;
                try
                {
                    mod = Mod2.LoadFromDirectory(directory);
                }
                catch (Exception e)
                {
                    Logger?.LogError($"Load Mod failed in '{directory}': {e}.");
                    continue;
                }
                engineWrapper.ModScriptModuleLoader.ModScriptModules.AddModDirectory(mod.metadata.id, directory);
                mod2s.Remove(mod.metadata.id);
                mod2s.Add(mod.metadata.id, mod);
            }
        }
        foreach (var modPair in mod2s)
        {
            engineWrapper.Engine.AddModule($"$load-{modPair.Value.metadata.id}", $"import 'mod://{modPair.Value.metadata.id}/{modPair.Value.metadata.entry}'\n");
            engineWrapper.Engine.ImportModule($"$load-{modPair.Value.metadata.id}");
        }
    }
}