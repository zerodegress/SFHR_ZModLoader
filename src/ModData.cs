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
            }
            if(camoDataConf.redCamo != null && File.Exists(Path.Combine(dir, camoDataConf.redCamo)))
            {
                if(redCamo == null)
                {
                    redCamo = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, camoDataConf.redCamo)}");
                ImageConversion.LoadImage(redCamo, File.ReadAllBytes(Path.Combine(dir, camoDataConf.redCamo)));
            }
            if(camoDataConf.icon != null && File.Exists(Path.Combine(dir, camoDataConf.icon)))
            {
                if(icon == null)
                {
                    icon = new Texture2D(1, 1);
                }
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading texture: {Path.Combine(dir, camoDataConf.icon)}");
                ImageConversion.LoadImage(icon, File.ReadAllBytes(Path.Combine(dir, camoDataConf.icon)));
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