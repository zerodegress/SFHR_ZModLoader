#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SFHR_ZModLoader
{
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
    }

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

            var weaponDataConf = new ModWeaponDataConf {};
            if(File.Exists(Path.Combine(dir, "camo.json")))
            {
                var newConf = JsonConvert.DeserializeObject<ModWeaponDataConf>(File.ReadAllText(Path.Combine(dir, "camo.json")));
                weaponDataConf.equipTexture = newConf.equipTexture;
                weaponDataConf.equipTextureAlt = newConf.equipTextureAlt;
                weaponDataConf.menuTexture = newConf.menuTexture;
                weaponDataConf.unequipTexture = newConf.unequipTexture;
            }
            else
            {
                if(File.Exists(Path.Combine(dir, "equipTexture.png")))
                {
                    weaponDataConf.equipTexture = "equipTexture.png";
                }
                if(File.Exists(Path.Combine(dir, "equipTextureAlt.png")))
                {
                    weaponDataConf.equipTextureAlt = "equipTextureAlt.png";
                }
                if(File.Exists(Path.Combine(dir, "menuTexture.png")))
                {
                    weaponDataConf.menuTexture = "menuTexture.png";
                }
                if(File.Exists(Path.Combine(dir, "unequipTexture.png")))
                {
                    weaponDataConf.unequipTexture = "unequipTexture.png";
                }
            }
            if(weaponDataConf.equipTexture != null && File.Exists(Path.Combine(dir, weaponDataConf.equipTexture)))
            {
                if(equipTexture == null)
                {
                    equipTexture = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.equipTexture)}");
                ImageConversion.LoadImage(equipTexture, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.equipTexture)));
                equipTexture.name = $"zmod_weapon_{name}_equipTexture";
            }
            if(weaponDataConf.equipTextureAlt != null && File.Exists(Path.Combine(dir, weaponDataConf.equipTextureAlt)))
            {
                if(equipTextureAlt == null)
                {
                    equipTextureAlt = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.equipTextureAlt)}");
                ImageConversion.LoadImage(equipTextureAlt, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.equipTextureAlt)));
                equipTextureAlt.name = $"zmod_weapon_{name}_equipTextureAlt";
            }
            if(weaponDataConf.menuTexture != null && File.Exists(Path.Combine(dir, weaponDataConf.menuTexture)))
            {
                if(menuTexture == null)
                {
                    menuTexture = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.menuTexture)}");
                ImageConversion.LoadImage(menuTexture, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.menuTexture)));
                menuTexture.name = $"zmod_weapon_{name}_menuTexture";
            }
            if(weaponDataConf.unequipTexture != null && File.Exists(Path.Combine(dir, weaponDataConf.unequipTexture)))
            {
                if(unequipTexture == null)
                {
                    unequipTexture = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, weaponDataConf.unequipTexture)}");
                ImageConversion.LoadImage(unequipTexture, File.ReadAllBytes(Path.Combine(dir, weaponDataConf.unequipTexture)));
                unequipTexture.name = $"zmod_weapon_{name}_unequipTexture";
            }
            return new ModWeaponData {
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
            gctx.PatchWeaponData(namespaceName != null ? $"{namespaceName}:{name}" : name, weaponData => {
                if(self.equipTexture != null)
                {
                    if(weaponData.EquipTexture != null && weaponData.EquipTexture.isReadable)
                    {
                        ImageConversion.LoadImage(weaponData.EquipTexture, self.equipTexture.EncodeToPNG());
                    }
                    else
                    {
                        weaponData.EquipTexture = self.equipTexture;
                    }
                }
                if(self.equipTextureAlt != null)
                {
                    if(weaponData.EquipTextureAlt!= null && weaponData.EquipTextureAlt.isReadable)
                    {
                        ImageConversion.LoadImage(weaponData.EquipTextureAlt, self.equipTextureAlt.EncodeToPNG());
                    }
                    else
                    {
                        weaponData.EquipTextureAlt = self.equipTextureAlt;
                    }
                }
                if(self.menuTexture != null)
                {
                    if(weaponData.MenuTexture != null && weaponData.MenuTexture.isReadable)
                    {
                        ImageConversion.LoadImage(weaponData.MenuTexture, self.menuTexture.EncodeToPNG());
                    }
                    else
                    {
                        weaponData.MenuTexture = self.menuTexture;
                    }
                }
                if(self.unequipTexture != null)
                {
                    if(weaponData.UnequipTexture != null && weaponData.UnequipTexture.isReadable)
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

    [Serializable]
    public struct ModCamosConf
    {
        public List<string>? includes;
        public List<string>? excludes;
    }

    [Serializable]
    public struct ModCamoDataConf
    {
        public string? texture;
        public string? redCamo;
        public string? icon;
        public int? classTextureNum;
    }

    public struct ModCamoData
    {
        public string name;
        public Texture2D? texture;
        public Texture2D? redCamo;
        public Texture2D? icon;
        public int classTextureNum;

        public static ModCamoData LoadFromDirectory(string dir, ModCamoData? camoData = null)
        {
            if (!Directory.Exists(dir))
            {
                throw new ModLoadingException($"CamoData Directory '{dir}' not exists.");
            }
            var name = Path.GetFileName(dir);
            var texture = camoData?.texture;
            var redCamo = camoData?.redCamo;
            var icon = camoData?.icon;

            var camoDataConf = new ModCamoDataConf {
                classTextureNum = -1,
            };
            if(File.Exists(Path.Combine(dir, "camo.json")))
            {
                var newConf = JsonConvert.DeserializeObject<ModCamoDataConf>(File.ReadAllText(Path.Combine(dir, "camo.json")));
                camoDataConf.texture = newConf.texture;
                camoDataConf.redCamo = newConf.redCamo;
                camoDataConf.icon = newConf.icon;
            }
            else
            {
                if(File.Exists(Path.Combine(dir, "texture.png")))
                {
                    camoDataConf.texture = "texture.png";
                }
                if(File.Exists(Path.Combine(dir, "redCamo.png")))
                {
                    camoDataConf.redCamo = "redCamo.png";
                }
                if(File.Exists(Path.Combine(dir, "icon.png")))
                {
                    camoDataConf.icon = "icon.png";
                }
            }
            var classTextureNum = camoDataConf.classTextureNum ?? -1;
            if(camoDataConf.texture != null && File.Exists(Path.Combine(dir, camoDataConf.texture)))
            {
                if(texture == null)
                {
                    texture = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, camoDataConf.texture)}");
                ImageConversion.LoadImage(texture, File.ReadAllBytes(Path.Combine(dir, camoDataConf.texture)));
                texture.name = $"zmod_camo_{name}_texture";
            }
            if(camoDataConf.redCamo != null && File.Exists(Path.Combine(dir, camoDataConf.redCamo)))
            {
                if(redCamo == null)
                {
                    redCamo = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, camoDataConf.redCamo)}");
                ImageConversion.LoadImage(redCamo, File.ReadAllBytes(Path.Combine(dir, camoDataConf.redCamo)));
                redCamo.name = $"zmod_camo_{name}_redCamo";
            }
            if(camoDataConf.icon != null && File.Exists(Path.Combine(dir, camoDataConf.icon)))
            {
                if(icon == null)
                {
                    icon = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, camoDataConf.icon)}");
                ImageConversion.LoadImage(icon, File.ReadAllBytes(Path.Combine(dir, camoDataConf.icon)));
                icon.name = $"zmod_camo_{name}_icon";
            }
            return new ModCamoData {
                name = name,
                texture = texture,
                redCamo = redCamo,
                icon = icon,
                classTextureNum = classTextureNum,
            };
        }

        public readonly void PatchToGameContext(GameContext gctx, string? namespaceName)
        {
            var self = this;
            gctx.PatchCamoData(namespaceName != null ? $"{namespaceName}:{name}" : name, camoData => {
                camoData.ClassTextureNum = self.classTextureNum;
                if(self.texture != null)
                {
                    if(camoData.Texture != null && camoData.Texture.isReadable)
                    {
                        ImageConversion.LoadImage(camoData.Texture, self.texture.EncodeToPNG());
                    }
                    else
                    {
                        camoData.Texture = self.texture;
                    }
                }
                if(self.redCamo != null)
                {
                    if(camoData.RedCamo != null && camoData.RedCamo.isReadable)
                    {
                        ImageConversion.LoadImage(camoData.RedCamo, self.redCamo.EncodeToPNG());
                    }
                    else
                    {
                        camoData.RedCamo = self.redCamo;
                    }
                }
                if(self.icon != null)
                {
                    if(camoData.Icon != null && camoData.Icon.isReadable)
                    {
                        ImageConversion.LoadImage(camoData.Icon, self.icon.EncodeToPNG());
                    }
                    else
                    {
                        camoData.Icon = self.icon;
                    }
                }
            });
        }
    }
}