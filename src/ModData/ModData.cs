#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SFHR_ZModLoader;

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
    public string name;
    public Dictionary<string, ModCamoData> camoDatas;

    public Dictionary<string, ModWeaponData> weaponDatas;
    public Dictionary<string, string> scripts;

    public static ModNamespace LoadFromDirectory(string dir, ModNamespace? ns = null)
    {
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
            };
        }

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
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Skips camos at '{Path.Combine(dir, nsConf.camos)}'.");
        }

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
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Skips weapons at '{Path.Combine(dir, nsConf.weapons)}'.");
        }


        return new ModNamespace
        {
            name = nsname,
            camoDatas = camoDatas,
            weaponDatas = weaponDatas,
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

    public readonly void UnpatchToGameContext(GameContext gctx)
    {
        // TODO: 实现反补丁游戏
    }
}