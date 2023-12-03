#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace SFHR_ZModLoader;

public struct Mod
{
    public ModMetadata metadata;
    public Dictionary<string, ModNamespace> namespaces;
    public Mod(ModMetadata metadata)
    {
        this.metadata = metadata;
        this.namespaces = new();
    }

    public static Mod LoadFromDirectory(string dir, Mod? mod = null)
    {
        if (!Directory.Exists(dir))
        {
            throw new ModLoadingException($"Mod directory '{dir}' not found.");
        }
        ModMetadata metadata;
        try
        {
            metadata = JsonConvert.DeserializeObject<ModMetadata>(File.ReadAllText(Path.Combine(dir, "mod.json")));
        }
        catch
        {
            throw new ModLoadingException($"Errors in the metadata file 'mod.json'.");
        }
        var namespaces = mod?.namespaces ?? new Dictionary<string, ModNamespace>();

        foreach (var nsdir in Directory.EnumerateDirectories(dir))
        {
            if (namespaces.TryGetValue(Path.GetFileName(nsdir), out var ns))
            {
                namespaces[Path.GetFileName(nsdir)] = ModNamespace.LoadFromDirectory(nsdir, ns);
            }
            else
            {
                namespaces.Add(Path.GetFileName(nsdir), ModNamespace.LoadFromDirectory(nsdir));
            }
        }
        return new Mod
        {
            metadata = metadata,
            namespaces = namespaces,
        };
    }

    public readonly void PatchToGameContext(GameContext gctx)
    {
        foreach (var item in namespaces)
        {
            item.Value.PatchToGameContext(gctx);
        }
    }

    public readonly void UnpatchToGameContext(GameContext gctx)
    {
        foreach (var item in namespaces)
        {
            item.Value.UnpatchToGameContext(gctx);
        }
    }
}

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
    private ManualLogSource? logger { get => SFHRZModLoaderPlugin.Logger; }

    public ModLoader(string dir)
    {
        this.dir = dir;
        mods = new();
        LoadModLoadOrder();
        logger?.LogInfo("ModLoader created.");
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
            logger?.LogInfo("'mod_load_order.txt' loaded.");
        }
        else
        {
            modLoadOrder = null;
            logger?.LogInfo("Skipped 'mod_load_order.txt' because it is missing, defaults to load all the mods.");
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
                logger?.LogError("GAMECONTEXT_PATCH data incorrect!");
                return;
            }
            LoadMods();
            var gctx = (GameContext)ev.data;
            logger?.LogInfo("Game patching...");
            PatchToGameContext(gctx);
            logger?.LogInfo("Game patch completed.");
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
        logger?.LogInfo("All ModLoader events registered.");
    }

    public void LoadMods()
    {
        logger?.LogInfo($"Loading Mods from directory: {dir}...");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (modLoadOrder == null)
        {
            foreach (var item in Directory.EnumerateDirectories(dir))
            {
                if (File.Exists(Path.Combine(item, "mod.json")))
                {
                    var metadata = JsonConvert.DeserializeObject<ModMetadata>(File.ReadAllText(Path.Combine(item, "mod.json")));
                    try
                    {
                        logger?.LogInfo($"Loading Mod from directory: {item}...");
                        if (mods.TryGetValue(metadata.id, out var mod))
                        {
                            this.mods[mod.metadata.id] = Mod.LoadFromDirectory(item, mod);
                        }
                        else
                        {
                            this.mods.Add(metadata.id, Mod.LoadFromDirectory(item));
                        }
                        logger?.LogInfo($"Loading Mod '{metadata.id}' completed.");
                    }
                    catch (Exception e)
                    {
                        logger?.LogError($"Load Mod in '{item}' failed: {e}.");
                    }
                }
            }
        }
        else
        {
            foreach (var modName in modLoadOrder)
            {
                if (File.Exists(Path.Combine(dir, modName, "mod.json")))
                {
                    var metadata = JsonConvert.DeserializeObject<ModMetadata>(File.ReadAllText(Path.Combine(dir, modName, "mod.json")));
                    try
                    {
                        logger?.LogInfo($"Loading Mod from directory: {Path.Combine(dir, modName)}...");
                        if (mods.TryGetValue(metadata.id, out var mod))
                        {
                            this.mods[mod.metadata.id] = Mod.LoadFromDirectory(Path.Combine(dir, modName), mod);
                        }
                        else
                        {
                            this.mods.Add(metadata.id, Mod.LoadFromDirectory(Path.Combine(dir, modName)));
                        }
                        logger?.LogInfo($"Loading Mod '{metadata.id}' completed.");
                    }
                    catch (Exception e)
                    {
                        logger?.LogError($"Load Mod in '{Path.Combine(dir, modName)}' failed: {e}.");
                    }
                }
                else
                {
                    logger?.LogWarning($"Skipped load Mod '{Path.Combine(dir, modName)}': Not exist.");
                }
            }
        }
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