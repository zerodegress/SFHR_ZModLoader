#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace SFHR_ZModLoader 
{
    [Serializable]
    public struct ModMetadata
    {
        public string id;
        public string displayName;
        public ulong versionCode;
        public string version;
    }

    public struct ModFile
    {
        public string? path;
        public Texture2D? texture2D;

        public readonly byte[] ReadAllBytes()
        {
            return File.ReadAllBytes(path);
        }

        public readonly string ReadAllText()
        {
            return File.ReadAllText(path);
        }
    }

    public struct ModCamoData
    {
        public ModFile? texture;
        public ModFile? redCamo;
        public ModFile? icon;
    }

    public struct ModNameSpace 
    {
        public string name;
        public Dictionary<string, ModCamoData> camoDatas;
    }

    public class Mod {
        public readonly ModMetadata metadata;
        public Dictionary<string, ModNameSpace> namespaces;
        public Mod(ModMetadata metadata)
        {
            this.metadata = metadata;
            this.namespaces = new();
        }

        public static Mod LoadModFromDirectory(string dir)
        {
            if(!Directory.Exists(dir))
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
            var mod = new Mod(metadata);

            foreach (var ns in Directory.EnumerateDirectories(dir))
            {
                var mnamespace = new ModNameSpace {
                    name = Path.GetFileName(ns),
                    camoDatas = new(),
                };
                if(Directory.Exists(Path.Combine(ns, "camos")))
                {
                    foreach (var camoName in Directory.EnumerateDirectories(Path.Combine(ns, "camos")))
                    {
                        SFHRZModLoaderPlugin.Logger?.LogInfo($"Loaded Mod '{metadata.id}' Camo: {Path.GetFileName(ns)}:{Path.GetFileName(camoName)}");
                        mnamespace.camoDatas.Add(Path.GetFileName(camoName), new ModCamoData {
                            texture = File.Exists(Path.Combine(camoName, "texture.png")) 
                                ? new ModFile { path = Path.Combine(camoName, "texture.png")  }
                                : null,
                            redCamo = File.Exists(Path.Combine(camoName, "red_camo.png")) 
                                ? new ModFile { path = Path.Combine(camoName, "red_camo.png")  }
                                : null,
                            icon = File.Exists(Path.Combine(camoName, "icon.png")) 
                                ? new ModFile { path = Path.Combine(camoName, "icon.png")  }
                                : null,
                        });
                    }
                }
                mod.AddNameSpace(mnamespace);
            }
            return mod;
        }

        public void AddNameSpace(ModNameSpace ns)
        {
            this.namespaces.Add(ns.name, ns);
        }
    }

    public class ModLoadingException: Exception
    {
        public ModLoadingException(string messages): base(messages)
        {}
    }

    public class ModLoader
    {
        private ManualLogSource? Logger { get; set; } = SFHRZModLoaderPlugin.Logger;
        private readonly string dir;
        private readonly Dictionary<string, Mod> mods;
        // private V8ScriptEngine engine = new();

        public ModLoader(string dir)
        {
            this.dir = dir;
            this.mods = new();
        }

        public void LoadMods()
        {
            mods.Clear();
            foreach (var modName in Directory.EnumerateDirectories(dir))
            {
                if (File.Exists(Path.Combine(modName, "mod.json")))
                {
                    this.mods.Add(Path.GetFileName(modName), Mod.LoadModFromDirectory(Path.Combine(modName)));
                    SFHRZModLoaderPlugin.Logger?.LogInfo($"Loaded Mod: {Path.GetFileName(modName)}");
                }
            }
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

        public void PatchCamoData(ref CamoData src)
        {
            foreach (var mod in mods)
            {
                if(!mod.Value.namespaces.ContainsKey("sfh"))
                {
                    return;
                }
                var ns = mod.Value.namespaces["sfh"];
                
                if(!ns.camoDatas.ContainsKey(src.name))
                {
                    return;
                }
                var camoData = ns.camoDatas[src.name];
                if (camoData.texture != null)
                {
                    src.ClassTextureNum = -1;
                    if (camoData.texture?.texture2D != null)
                    {
                        src.Texture = camoData.texture.Value.texture2D;
                    }
                    else
                    {
                        src.Texture = new Texture2D(src.Texture.width, src.Texture.height);
                        ImageConversion.LoadImage(src.Texture, camoData.texture?.ReadAllBytes());
                        camoData.texture = new ModFile{
                            path = camoData.texture?.path,
                            texture2D = src.Texture
                        };
                    }
                    
                    SFHRZModLoaderPlugin.Logger?.LogError("Patch completed.");
                }
                if (camoData.redCamo != null)
                {
                    if (camoData.redCamo?.texture2D != null)
                    {
                        src.RedCamo = camoData.redCamo.Value.texture2D;
                    }
                    else
                    {
                        src.RedCamo = new Texture2D(src.RedCamo.width, src.RedCamo.height);
                        ImageConversion.LoadImage(src.Texture, camoData.redCamo?.ReadAllBytes());
                        camoData.redCamo = new ModFile{
                            path = camoData.redCamo?.path,
                            texture2D = src.RedCamo
                        };
                    }
                }
                if (camoData.icon != null)
                {
                    if (camoData.icon?.texture2D != null)
                    {
                        src.Icon = camoData.icon.Value.texture2D;
                    }
                    else
                    {
                        src.Icon = new Texture2D(src.Icon.width, src.Icon.height);
                        ImageConversion.LoadImage(src.Texture, camoData.icon?.ReadAllBytes());
                        camoData.icon = new ModFile{
                            path = camoData.icon?.path,
                            texture2D = src.Icon
                        };
                    }
                }
            }
        }

        public void PatchGlobalDataLoad(GlobalData globalData)
        {
            if(!globalData.ItemTypeInfo.ContainsKey(GI.EItemType.Camo)) {
                return;
            }
            var objects = globalData.ItemTypeInfo[GI.EItemType.Camo].Objects;
            foreach (var obj in objects)
            {
                CamoData camoData = (CamoData)obj;
                if (!(camoData.name == ""))
                {
                    PatchCamoData(ref camoData);
                }
            }

            if (SFHRZModLoaderPlugin.DebugEmit)
            {
                if (!Directory.Exists(Path.Combine(Paths.GameRootPath, "DebugEmit/camos"))) {
                    Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "DebugEmit/camos"));
                }
                string text = "";
                foreach (var obj in objects)
                {
                    CamoData camoData = (CamoData)obj;
                    if (!(camoData.Name == ""))
                    {
                        text = string.Concat(new string[] { text, "CamoName.", camoData.name, "|", camoData.Name, "\n" });
                        text = string.Concat(new string[] { text, "CamoDesc.", camoData.name, "|", camoData.Desc, "\n" });
                        text += $"CamoTextureName.{camoData.Texture.name}\n";
                        text += $"CamoRedCamoName.{camoData.RedCamo.name}\n";
                    }
                }
                File.WriteAllText(Path.Combine(Paths.GameRootPath, "DebugEmit/camos.txt"), text);
            }
        }
    }
}