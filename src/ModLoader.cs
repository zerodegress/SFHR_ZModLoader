#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using Newtonsoft.Json;
using SFHR_ZModLoader.Scripting;
using SFHR_ZModLoader.Modding;

namespace SFHR_ZModLoader;

public class ModLoadingException : Exception
{
    public ModLoadingException(string messages) : base(messages)
    { }
}

public class ModLoader
{
    private readonly string dir;
    private Dictionary<string, Mod> mods;
    private List<string>? modLoadOrder;
    private ManualLogSource? Logger { get => SFHRZModLoaderPlugin.Logger; }

    public ModLoader(string dir)
    {
        this.dir = dir;
        mods = new();
        LoadModLoadOrder();
        Logger?.LogInfo("ModLoader created.");
    }

    public void LoadModLoadOrder()
    {
        if (File.Exists(Path.Combine(dir, "mod_load_order.txt")))
        {
            modLoadOrder = new();
            foreach (var line in File.ReadLines(Path.Combine(dir, "mod_load_order.txt")))
            {
                if (!line.TrimStart().StartsWith("#"))
                {
                    modLoadOrder.Add(line.Trim());
                }
            }
            Logger?.LogInfo("'mod_load_order.txt' loaded.");
        }
        else
        {
            modLoadOrder = null;
            Logger?.LogInfo("Skipped 'mod_load_order.txt' because it is missing, defaults to load all the mods.");
        }
    }

    public void RegisterEvents(EventManager eventManager)
    {
        eventManager.RegisterEventHandler("MODS_LOAD", ev =>
        {
            LoadMods();
            eventManager.EmitEvent(new Event
            {
                type = "MODS_LOADED",
            });
        });
        eventManager.RegisterEventHandler("GAMECONTEXT_PATCH", ev =>
        {
            if (ev.data == null || ev.data.GetType() != typeof(GameContext))
            {
                Logger?.LogError("GAMECONTEXT_PATCH data incorrect!");
                return;
            }
            LoadMods();
            var gctx = (GameContext)ev.data;
            Logger?.LogInfo("Game patching...");
            PatchToGameContext(gctx);
            Logger?.LogInfo("Game patch completed.");
        });
        eventManager.RegisterEventHandler("GAMECONTEXT_LOADED", ev =>
        {
            var gctx = (GameContext)ev.data;
            eventManager.EmitEvent(new Event
            {
                type = "GAMECONTEXT_PATCH",
                data = gctx,
            });
        });
        eventManager.RegisterEventHandler("MODS_RELOAD", ev =>
        {
            if (SFHRZModLoaderPlugin.GameContext != null)
            {
                UnpatchToGameContext(SFHRZModLoaderPlugin.GameContext);
            }
            UnloadMods();
            LoadModLoadOrder();
            LoadMods();
            if (SFHRZModLoaderPlugin.GameContext != null)
            {
                eventManager.EmitEvent(new Event
                {
                    type = "GAMECONTEXT_PATCH",
                    data = SFHRZModLoaderPlugin.GameContext,
                });
            }
        });
        eventManager.RegisterEventHandler("SCRIPT_ENGINE_READY", ev => {
            LoadModsScripts(SFHRZModLoaderPlugin.ScriptEngine);
        });
        Logger?.LogInfo("All ModLoader events registered.");
    }

    public void LoadMod(string dir, string modId)
    {
        try
        {
            Logger?.LogInfo($"Loading Mod from directory: {dir}...");
            if (mods.TryGetValue(modId, out var mod))
            {
                this.mods[modId] = Mod.LoadFromDirectory(dir, mod);
            }
            else
            {
                this.mods.Add(modId, Mod.LoadFromDirectory(dir));
            }
            Logger?.LogInfo($"Loading Mod '{modId}' completed.");
        }
        catch (Exception e)
        {
            Logger?.LogError($"Load Mod in '{dir}' failed: {e}.");
        }
    }

    public void LoadMods()
    {
        Logger?.LogInfo($"Loading Mods from directory: {dir}...");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var modDirToMetaData = Directory.EnumerateDirectories(dir).Select(modDir => {
            if(File.Exists(Path.Combine(modDir, "mod.json")))
            {
                var metadata = JsonConvert.DeserializeObject<ModMetadata>(File.ReadAllText(Path.Combine(modDir, "mod.json")));
                return (modDir, metadata);
            }
            else
            {
                return (modDir, (ModMetadata?)null);
            }
        }).ToList();
        var modIdToDir = modLoadOrder?.Select(modId => {
            return (modId, modDirToMetaData.Find(item => item.Item2?.id == modId).modDir);
        }).ToList();

        if(modIdToDir != null)
        {
            modIdToDir.ForEach(item => {
                if(item.modDir != null)
                {
                    LoadMod(item.modDir, item.modId);
                }
            });
        }
        else
        {
            modDirToMetaData.ForEach(item => {
                if(item.Item2 != null) 
                {
                    LoadMod(item.modDir, item.Item2.Value.id);
                }
            });
        }
    }

    public void LoadModsScripts(ModScriptEngineWrapper engine)
    {
        List<string> scriptEntries = new();
        mods.ToList().ForEach(item => {
            try
            {
                item.Value.LoadScripts(engine.ModScriptModules).ToList().ForEach(script => {
                    scriptEntries.Add(script);
                });
            } 
            catch(Exception e)
            {
                SFHRZModLoaderPlugin.Logger?.LogError($"Load Mod scripts failed in Mod '{item.Key}': '{e}'.");
            }
        });
        scriptEntries.ForEach(script => {
            Logger?.LogInfo($"Try import script module: '{script}'.");
            try
            {
                engine.Engine.ImportModule(script);
            } 
            catch(Exception e)
            {
                SFHRZModLoaderPlugin.Logger?.LogError($"Import Mod script module failed in script '{script}': '{e}'.");
            }
        });
    }

    public void UnloadMods()
    {

    }

    public Mod? GetMod(string name)
    {
        if (mods.ContainsKey(name))
        {
            return mods[name];
        }
        else
        {
            return null;
        }
    }

    public void PatchToGameContext(GameContext gctx)
    {
        foreach (var item in mods)
        {
            item.Value.PatchToGameContext(gctx);
        }
    }

    public void UnpatchToGameContext(GameContext gctx)
    {
        foreach (var item in mods)
        {
            item.Value.UnpatchToGameContext(gctx);
        }
    }
}