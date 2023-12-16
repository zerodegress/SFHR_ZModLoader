#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SFHR_ZModLoader.Scripting;

namespace SFHR_ZModLoader.Modding;

public struct Mod
{
    public ModMetadata metadata;
    public Dictionary<string, ModNamespace> namespaces;

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
                namespaces[Path.GetFileName(nsdir)] = ModNamespace.LoadFromDirectory(nsdir, metadata.id, ns);
            }
            else
            {
                namespaces.Add(Path.GetFileName(nsdir), ModNamespace.LoadFromDirectory(nsdir, metadata.id));
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