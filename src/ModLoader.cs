#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public string path;
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

    public class ModCamoData
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
        public ModMetadata metadata;
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
                        SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading Camo: {Path.GetFileName(ns)}:{Path.GetFileName(camoName)} from Mod '{metadata.id}'...");
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
        private Dictionary<string, Mod> mods;
        private Dictionary<string, ModFile> modFiles;
        // private V8ScriptEngine engine = new();

        private Texture2D LoadModFileTexture2D(string path, bool forceReload = false)
        {
            if (modFiles.TryGetValue(path, out ModFile modFile))
            {
                if (forceReload)
                {
                    Texture2D texture;
                    if (modFile.texture2D != null && modFile.texture2D.isReadable)
                    {
                        Logger?.LogInfo($"Reloading texture: {modFile.path}...");
                        texture = modFile.texture2D;
                    }
                    else
                    {
                        Logger?.LogInfo($"Loading new texture: {modFile.path}...");
                        texture = new Texture2D(1, 1);
                    }
                    modFile = new ModFile
                    {
                        path = path,
                    };
                    ImageConversion.LoadImage(texture, modFile.ReadAllBytes());
                    modFile.texture2D = texture;
                    modFiles[path] = modFile;
                    return texture;
                }
                else if (modFile.texture2D != null)
                {
                    return modFile.texture2D;
                }
                else
                {
                    var texture = new Texture2D(1, 1);
                    ImageConversion.LoadImage(texture, modFile.ReadAllBytes());
                    modFile.texture2D = texture;
                    modFiles[path] = modFile;
                    return texture;
                }
            }
            else
            {
                modFile = new ModFile
                {
                    path = path,
                };
                var texture = new Texture2D(1, 1);
                ImageConversion.LoadImage(texture, modFile.ReadAllBytes());
                modFile.texture2D = texture;
                modFiles[path] = modFile;
                return texture;
            }
        }

        private void LoadModFilesForMod(string modName, bool forceReload = false)
        {
            if (mods.TryGetValue(modName, out var mod))
            {
                foreach(var ns in mod.namespaces)
                {
                    foreach(var camoData in ns.Value.camoDatas)
                    {
                        if(camoData.Value.texture != null)
                        {
                            ns.Value.camoDatas[camoData.Key].texture = new ModFile {
                                path = camoData.Value.texture.Value.path,
                                texture2D = LoadModFileTexture2D(camoData.Value.texture.Value.path, forceReload)
                            };
                        }
                        if(camoData.Value.icon != null)
                        {
                            ns.Value.camoDatas[camoData.Key].icon = new ModFile {
                                path = camoData.Value.icon.Value.path,
                                texture2D = LoadModFileTexture2D(camoData.Value.icon.Value.path, forceReload)
                            };
                        }
                        if(camoData.Value.redCamo != null)
                        {
                            ns.Value.camoDatas[camoData.Key].redCamo = new ModFile {
                                path = camoData.Value.redCamo.Value.path,
                                texture2D = LoadModFileTexture2D(camoData.Value.redCamo.Value.path)
                            };
                        }
                    }
                }
            }
            else
            {
                throw new ModLoadingException($"Mod '{modName}' not exists.");
            }
        }

        public ModLoader(string dir)
        {
            this.dir = dir;
            this.mods = new();
            this.modFiles = new();
        }

        public void LoadMods(bool forceReload = false)
        {
            this.mods.Clear();
            foreach (var modName in Directory.EnumerateDirectories(dir))
            {
                if (File.Exists(Path.Combine(modName, "mod.json")))
                {
                    try
                    {
                        Logger?.LogInfo($"Loading Mod from directory: {modName}...");
                        var mod = Mod.LoadModFromDirectory(modName);
                        this.mods.Add(mod.metadata.id, mod);
                        LoadModFilesForMod(mod.metadata.id, forceReload);
                        Logger?.LogInfo($"Loading Mod '{mod.metadata.id}' completed.");
                    }
                    catch(Exception e)
                    {
                        Logger?.LogError($"Load Mod in '{modName}' failed: {e}.");
                    }
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
                    continue;
                }
                var ns = mod.Value.namespaces["sfh"];
                
                if(!ns.camoDatas.ContainsKey(src.name))
                {
                    continue;
                }
                var camoData = ns.camoDatas[src.name];
                if (camoData.texture != null)
                {
                    var texture = camoData.texture.Value;
                    src.ClassTextureNum = -1;
                    if (texture.texture2D != null)
                    {
                        src.Texture = texture.texture2D;
                    }
                    else
                    {
                        try
                        {
                            Logger?.LogInfo($"Loading image '{texture.path}'...");
                            src.Texture = new Texture2D(1, 1);
                            ImageConversion.LoadImage(src.Texture, texture.ReadAllBytes());
                        } 
                        catch(Exception e)
                        {
                            Logger?.LogError($"Load image '{texture.path}' failed: {e}.");
                        }
                        camoData.texture = new ModFile{
                            path = texture.path,
                            texture2D = src.Texture
                        };
                    }
                }
                if (camoData.redCamo != null)
                {
                    var redCamo = camoData.redCamo.Value;
                    if (redCamo.texture2D != null)
                    {
                        src.RedCamo = camoData.redCamo.Value.texture2D;
                    }
                    else
                    {
                        try
                        {
                            Logger?.LogInfo($"Loading image '{redCamo.path}'...");
                            src.RedCamo = new Texture2D(1, 1);
                            ImageConversion.LoadImage(src.RedCamo, redCamo.ReadAllBytes());
                        }
                        catch(Exception e)
                        {
                            Logger?.LogError($"Load image '{redCamo.path}' failed: {e}.");
                        }
                        camoData.redCamo = new ModFile{
                            path = redCamo.path,
                            texture2D = src.RedCamo
                        };
                    }
                }
                if (camoData.icon != null)
                {
                    var icon = camoData.icon.Value;
                    if (icon.texture2D != null)
                    {
                        src.Icon = icon.texture2D;
                    }
                    else
                    {
                        try
                        {
                            Logger?.LogInfo($"Loading image '{icon.path}'...");
                            src.Icon = new Texture2D(1, 1);
                            ImageConversion.LoadImage(src.Icon, icon.ReadAllBytes());
                        }
                        catch(Exception e)
                        {
                            Logger?.LogError($"Load image '{icon.path}' failed: {e}.");
                        }
                        camoData.icon = new ModFile{
                            path = icon.path,
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