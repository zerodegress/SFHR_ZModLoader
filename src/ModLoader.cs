#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;

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

    public struct ModNamespace 
    {
        public string name;
        public Dictionary<string, ModCamoData> camoDatas;

        public Dictionary<string, ModWeaponData> weaponDatas;

        public static ModNamespace LoadFromDirectory(string dir, ModNamespace? ns = null)
        {
            if(!Directory.Exists(dir))
            {
                throw new ModLoadingException($"Namespace directory '{dir}' not exists.");
            }
            var nsname = Path.GetFileName(dir);
            var camoDatas = new Dictionary<string, ModCamoData>();
            var weaponDatas = new Dictionary<string, ModWeaponData>();
            var nsConf = new ModNamespaceConf {
                camos = "camos",
                weapons = "weapons",
            };
            if(File.Exists(Path.Combine(dir, "namespace.json")))
            {
                var newConf = JsonConvert.DeserializeObject<ModNamespaceConf>(File.ReadAllText(Path.Combine(dir, "namespace.json")));
                nsConf = new ModNamespaceConf {
                    camos = newConf.camos ?? nsConf.camos,
                    weapons = newConf.weapons ?? nsConf.weapons,
                };
            }

            var camosConf = new ModCamosConf {};
            if(File.Exists(Path.Combine(dir, nsConf.camos, "camos.json")))
            {
                var newConf = JsonConvert.DeserializeObject<ModCamosConf>(File.ReadAllText(Path.Combine(dir, nsConf.camos, "camos.json")));
                camosConf = newConf;
            }
            if(Directory.Exists(Path.Combine(dir, nsConf.camos)))
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

            var weaponsConf = new ModWeaponsConf {};
            if(File.Exists(Path.Combine(dir, nsConf.weapons, "weapons.json")))
            {
                var newConf = JsonConvert.DeserializeObject<ModWeaponsConf>(File.ReadAllText(Path.Combine(dir, nsConf.weapons, "camos.json")));
                weaponsConf = newConf;
            }
            if(Directory.Exists(Path.Combine(dir, nsConf.weapons)))
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
            

            return new ModNamespace {
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
    }

    public struct Mod {
        public ModMetadata metadata;
        public Dictionary<string, ModNamespace> namespaces;
        public Mod(ModMetadata metadata)
        {
            this.metadata = metadata;
            this.namespaces = new();
        }

        public static Mod LoadFromDirectory(string dir, Mod? mod = null)
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
            var namespaces = mod?.namespaces ?? new Dictionary<string, ModNamespace>();

            foreach (var nsdir in Directory.EnumerateDirectories(dir))
            {
                if(namespaces.TryGetValue(Path.GetFileName(nsdir), out var ns))
                {
                    namespaces[Path.GetFileName(nsdir)] = ModNamespace.LoadFromDirectory(nsdir, ns);
                }
                else
                {
                    namespaces.Add(Path.GetFileName(nsdir), ModNamespace.LoadFromDirectory(nsdir));
                }
            }
            return new Mod {
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
    }

    public class ModLoadingException: Exception
    {
        public ModLoadingException(string messages): base(messages)
        {}
    }

    public class ModLoader
    {
        private readonly string dir;
        private Dictionary<string, Mod> mods;
        private readonly ManualLogSource logger; 

        public ModLoader(string dir, ManualLogSource logger, EventManager eventManager)
        {
            this.dir = dir;
            this.mods = new();
            this.logger = logger;
            this.logger.LogInfo("ModLoader created.");
        }

        public void RegisterEvents(EventManager eventManager)
        {
            eventManager.RegisterEventHandler("MODS_LOAD", ev => {
                LoadMods();
                eventManager.EmitEvent(new Event {
                    type = "MODS_LOADED",
                });
            });
            eventManager.RegisterEventHandler("GAMECONTEXT_PATCH", ev => {
                if(ev.data == null || ev.data.GetType() != typeof(GameContext))
                {
                    logger.LogError("GAMECONTEXT_PATCH data incorrect!");
                    return;
                }
                LoadMods();
                var gctx = (GameContext)ev.data;
                logger.LogInfo("Game patching...");
                PatchToGameContext(gctx);
                logger.LogInfo("Game patch completed.");
            });
            eventManager.RegisterEventHandler("GAMECONTEXT_LOADED", ev => {
                var gctx = (GameContext)ev.data;
                eventManager.EmitEvent(new Event {
                    type = "GAMECONTEXT_PATCH", 
                    data = gctx,
                });
            });
        }

        public void LoadMods()
        {
            logger.LogInfo($"Loading Mods from directory: {dir}...");
            if(!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            foreach (var item in Directory.EnumerateDirectories(dir))
            {
                if (File.Exists(Path.Combine(item, "mod.json")))
                {
                    var metadata = JsonConvert.DeserializeObject<ModMetadata>(File.ReadAllText(Path.Combine(item, "mod.json")));
                    try
                    {
                        logger.LogInfo($"Loading Mod from directory: {item}...");
                        if(mods.TryGetValue(metadata.id, out var mod))
                        {
                            this.mods[mod.metadata.id] = Mod.LoadFromDirectory(item, mod);
                        }
                        else
                        {
                            this.mods.Add(metadata.id, Mod.LoadFromDirectory(item));
                        }
                        logger.LogInfo($"Loading Mod '{metadata.id}' completed.");
                    }
                    catch(Exception e)
                    {
                        logger.LogError($"Load Mod in '{item}' failed: {e}.");
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

        public void PatchToGameContext(GameContext gctx)
        {
            foreach (var item in mods)
            {
                item.Value.PatchToGameContext(gctx);
            }
        }
    }
}