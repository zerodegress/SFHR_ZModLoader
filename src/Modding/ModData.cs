#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SFHR_ZModLoader.Scripting;

namespace SFHR_ZModLoader.Modding;

[Serializable]
public struct ModVector3
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public struct ModColor
{
    public float r;
    public float g;
    public float b;
    public float a;
}

[Serializable]
public struct ModNamespaceConf
{
    public string? camos;
    public string? weapons;
    public string? scripts;
}

[Serializable]
public struct ModMetadata
{
    public string id;
    public string displayName;
    public ulong versionCode;
    public string version;
}

public struct ModNamespace
{
    public string modId;
    public string name;
    public Dictionary<string, ModCamoData> camoDatas;

    public Dictionary<string, ModWeaponData> weaponDatas;
    public Dictionary<string, string> scripts;
    public string? scriptsEntry;

    public static ModNamespace LoadFromDirectory(string dir, string modId, ModNamespace? ns = null)
    {
        // Namespace
        if (!Directory.Exists(dir))
        {
            throw new ModLoadingException($"Namespace directory '{dir}' not exists.");
        }
        var nsname = Path.GetFileName(dir);
        var camoDatas = new Dictionary<string, ModCamoData>();
        var weaponDatas = new Dictionary<string, ModWeaponData>();
        var nsConf = new ModNamespaceConf
        {
            camos = "camos",
            weapons = "weapons",
            scripts = "scripts",
        };
        if (File.Exists(Path.Combine(dir, "namespace.json")))
        {
            var newConf = JsonConvert.DeserializeObject<ModNamespaceConf>(File.ReadAllText(Path.Combine(dir, "namespace.json")));
            nsConf = new ModNamespaceConf
            {
                camos = newConf.camos ?? nsConf.camos,
                weapons = newConf.weapons ?? nsConf.weapons,
                scripts = newConf.scripts ?? nsConf.scripts,
            };
        }

        // CamoData
        var camosConf = new ModCamosConf { };
        if (File.Exists(Path.Combine(dir, nsConf.camos, "camos.json")))
        {
            var newConf = JsonConvert.DeserializeObject<ModCamosConf>(File.ReadAllText(Path.Combine(dir, nsConf.camos, "camos.json")));
            camosConf = newConf;
        }
        if (Directory.Exists(Path.Combine(dir, nsConf.camos)))
        {
            foreach (var item in Directory.EnumerateDirectories(Path.Combine(dir, nsConf.camos)))
            {
                // TODO: includes 和 excludes处理
                var camoName = Path.GetFileName(item);
                if (ns?.camoDatas.TryGetValue(item, out var camoData) ?? false)
                {
                    camoDatas[camoName] = ModCamoData.LoadFromDirectory(item, camoData);
                }
                else
                {
                    camoDatas.Add(camoName, ModCamoData.LoadFromDirectory(item));
                }
            }
        }
        else
        {
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Skips camos at '{Path.Combine(dir, nsConf.camos)}' beause it is missing.");
        }

        // WeaponData
        var weaponsConf = new ModWeaponsConf { };
        if (File.Exists(Path.Combine(dir, nsConf.weapons, "weapons.json")))
        {
            var newConf = JsonConvert.DeserializeObject<ModWeaponsConf>(File.ReadAllText(Path.Combine(dir, nsConf.weapons, "camos.json")));
            weaponsConf = newConf;
        }
        if (Directory.Exists(Path.Combine(dir, nsConf.weapons)))
        {
            foreach (var item in Directory.EnumerateDirectories(Path.Combine(dir, nsConf.weapons)))
            {
                // TODO: includes 和 excludes处理
                var weaponName = Path.GetFileName(item);
                if (ns?.weaponDatas.TryGetValue(item, out var weaponData) ?? false)
                {
                    weaponDatas[weaponName] = ModWeaponData.LoadFromDirectory(item, weaponData);
                }
                else
                {
                    weaponDatas.Add(weaponName, ModWeaponData.LoadFromDirectory(item));
                }
            }
        }
        else
        {
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Skipped weapons at '{Path.Combine(dir, nsConf.weapons)}' beause it is missing.");
        }

        var scripts = new Dictionary<string, string>();
        var scriptsEntry = (string?)null;
        // Script
        if (Directory.Exists(Path.Combine(dir, nsConf.scripts)))
        {
            var scriptsDirectory = Path.Combine(dir, nsConf.scripts);
            scriptsEntry = "index.js";
            Directory.GetFiles(scriptsDirectory).ToList().ForEach(script => {
                scripts.Add($"{Path.GetRelativePath(scriptsDirectory, script).Replace('\\', '/')}", File.ReadAllText(script));
            });
        }
        else
        {
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Skipped scripts at '{Path.Combine(dir, nsConf.scripts)}' beause it is missing.");
        }

        return new ModNamespace
        {
            name = nsname,
            camoDatas = camoDatas,
            weaponDatas = weaponDatas,
            scripts = scripts,
            scriptsEntry = scriptsEntry,
            modId = modId,
        };
    }

    public readonly void PatchToGameContext(GameContext gctx)
    {
        foreach (var item in camoDatas)
        {
            item.Value.PatchToGameContext(gctx, name == "sfh" ? null : name);
        }
        foreach (var item in weaponDatas)
        {
            item.Value.PatchToGameContext(gctx, name == "sfh" ? null : name);
        }
    }

    public readonly string? LoadScripts(string modId, ModScriptModules modScriptModules)
    {
        var scriptsEntry = this.scriptsEntry;
        var nsname = this.name;
        scripts.ToList().ForEach(item => {
            var moduleName = $"mod://{Path.Join(modId, nsname, item.Key).Replace('\\', '/')}";
            modScriptModules.AddModule(modId, nsname, item.Key.Replace('\\', '/'), item.Value);
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Loaded script: '{moduleName}'.");
        });
        return this.scriptsEntry != null ? $"mod://{Path.Join(modId, nsname, scriptsEntry).Replace('\\', '/')}" : null;
    }

    public readonly void UnpatchToGameContext(GameContext gctx)
    {
        // TODO: 实现反补丁游戏
    }
}