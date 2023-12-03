#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SFHR_ZModLoader;

[Serializable]
public struct ModWeaponsConf
{
    public List<string>? includes;
    public List<string>? excludes;
}

[Serializable]
public struct ModWeaponDataConf
{
    public string? equipTexture;
    public string? equipTextureAlt;
    public string? menuTexture;
    public string? unequipTexture;
}

public struct ModWeaponData
{
    public string name;
    public Texture2D? equipTexture;
    public Texture2D? equipTextureAlt;
    public Texture2D? menuTexture;
    public Texture2D? unequipTexture;

    public static ModWeaponData LoadFromDirectory(string dir, ModWeaponData? weaponData = null)
    {
        if (!Directory.Exists(dir))
        {
            throw new ModLoadingException($"CamoData Directory '{dir}' not exists.");
        }
        var name = Path.GetFileName(dir);
        var equipTexture = weaponData?.equipTexture;
        var equipTextureAlt = weaponData?.equipTextureAlt;
        var menuTexture = weaponData?.menuTexture;
        var unequipTexture = weaponData?.unequipTexture;

        var weaponDataConf = new ModWeaponDataConf { };
        if (File.Exists(Path.Combine(dir, "camo.json")))
        {
            var newConf = JsonConvert.DeserializeObject<ModWeaponDataConf>(File.ReadAllText(Path.Combine(dir, "camo.json")));
            weaponDataConf.equipTexture = newConf.equipTexture;
            weaponDataConf.equipTextureAlt = newConf.equipTextureAlt;
            weaponDataConf.menuTexture = newConf.menuTexture;
            weaponDataConf.unequipTexture = newConf.unequipTexture;
        }
        else
        {
            if (File.Exists(Path.Combine(dir, "equipTexture.png")))
            {
                weaponDataConf.equipTexture = "equipTexture.png";
            }
            if (File.Exists(Path.Combine(dir, "equipTextureAlt.png")))
            {
                weaponDataConf.equipTextureAlt = "equipTextureAlt.png";
            }
            if (File.Exists(Path.Combine(dir, "menuTexture.png")))
            {
                weaponDataConf.menuTexture = "menuTexture.png";
            }
            if (File.Exists(Path.Combine(dir, "unequipTexture.png")))
            {
                weaponDataConf.unequipTexture = "unequipTexture.png";
            }
        }
        if (weaponDataConf.equipTexture != null && File.Exists(Path.Combine(dir, weaponDataConf.equipTexture)))
        {
            if (equipTexture == null)
            {
                equipTexture = new Texture2D(1, 1);
            }
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.equipTexture)}");
            ImageConversion.LoadImage(equipTexture, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.equipTexture)));
            equipTexture.name = $"zmod_weapon_{name}_equipTexture";
        }
        if (weaponDataConf.equipTextureAlt != null && File.Exists(Path.Combine(dir, weaponDataConf.equipTextureAlt)))
        {
            if (equipTextureAlt == null)
            {
                equipTextureAlt = new Texture2D(1, 1);
            }
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.equipTextureAlt)}");
            ImageConversion.LoadImage(equipTextureAlt, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.equipTextureAlt)));
            equipTextureAlt.name = $"zmod_weapon_{name}_equipTextureAlt";
        }
        if (weaponDataConf.menuTexture != null && File.Exists(Path.Combine(dir, weaponDataConf.menuTexture)))
        {
            if (menuTexture == null)
            {
                menuTexture = new Texture2D(1, 1);
            }
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.menuTexture)}");
            ImageConversion.LoadImage(menuTexture, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.menuTexture)));
            menuTexture.name = $"zmod_weapon_{name}_menuTexture";
        }
        if (weaponDataConf.unequipTexture != null && File.Exists(Path.Combine(dir, weaponDataConf.unequipTexture)))
        {
            if (unequipTexture == null)
            {
                unequipTexture = new Texture2D(1, 1);
            }
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.unequipTexture)}");
            ImageConversion.LoadImage(unequipTexture, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.unequipTexture)));
            unequipTexture.name = $"zmod_weapon_{name}_unequipTexture";
        }
        return new ModWeaponData
        {
            name = name,
            equipTexture = equipTexture,
            equipTextureAlt = equipTextureAlt,
            menuTexture = menuTexture,
            unequipTexture = unequipTexture,
        };
    }

    public readonly void PatchToGameContext(GameContext gctx, string? namespaceName)
    {
        var self = this;
        gctx.PatchWeaponData(namespaceName != null ? $"{namespaceName}:{name}" : name, weaponData =>
        {
            if (self.equipTexture != null)
            {
                if (weaponData.EquipTexture != null && weaponData.EquipTexture.isReadable)
                {
                    ImageConversion.LoadImage(weaponData.EquipTexture, self.equipTexture.EncodeToPNG());
                }
                else
                {
                    weaponData.EquipTexture = self.equipTexture;
                }
            }
            if (self.equipTextureAlt != null)
            {
                if (weaponData.EquipTextureAlt != null && weaponData.EquipTextureAlt.isReadable)
                {
                    ImageConversion.LoadImage(weaponData.EquipTextureAlt, self.equipTextureAlt.EncodeToPNG());
                }
                else
                {
                    weaponData.EquipTextureAlt = self.equipTextureAlt;
                }
            }
            if (self.menuTexture != null)
            {
                if (weaponData.MenuTexture != null && weaponData.MenuTexture.isReadable)
                {
                    ImageConversion.LoadImage(weaponData.MenuTexture, self.menuTexture.EncodeToPNG());
                }
                else
                {
                    weaponData.MenuTexture = self.menuTexture;
                }
            }
            if (self.unequipTexture != null)
            {
                if (weaponData.UnequipTexture != null && weaponData.UnequipTexture.isReadable)
                {
                    ImageConversion.LoadImage(weaponData.UnequipTexture, self.unequipTexture.EncodeToPNG());
                }
                else
                {
                    weaponData.UnequipTexture = self.unequipTexture;
                }
            }
        });
    }
}