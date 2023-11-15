#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SFHR_ZModLoader
{
    [Serializable]
    public struct Save
    {
        public float Version;
        public int SortOrder;
        public int Money;
        public uint CurItemID;
        public Dictionary<string, SMissionStatus> Missions;
        public string? CurClass;
        public Dictionary<string, int>? Misc;
        public Dictionary<string, GI.SSaveUnitInfo> Classes;
        public Dictionary<uint, SSaveItemInfo> Items;
        public List<SSaveItemInfo> ShopItems;
        public List<SSaveItemInfo> ItemRewards;
        public Dictionary<string, SCamoList> ClassCamos;
        public Dictionary<string, SFeatInfo> Feats;
    }

    [Serializable]
    public struct Settings {
        public Dictionary<string, int> settings;
        public string Controls;
    }

    public class SaveManager
    {
        readonly string dir;

        public SaveManager(string dir)
        {
            this.dir = dir;
        }

        public Save LoadFromSaveFile()
        {
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading save from: {Path.Combine(this.dir, "save.json")}");
            if (File.Exists(Path.Combine(this.dir, "save.json"))) {
                SFHRZModLoaderPlugin.Logger?.LogInfo($"Loading save file...");
                try 
                {
                    var save = JsonConvert.DeserializeObject<Save>(File.ReadAllText(Path.Combine(this.dir, "save.json")));
                    return save;
                }
                catch(Exception e) {
                    SFHRZModLoaderPlugin.Logger?.LogError(e);
                    return new Save {};
                }
            } else {
                SFHRZModLoaderPlugin.Logger?.LogInfo($"New game...");
                return new Save {};
            }
        }

        public bool ExistsSettingsFile() {
            return File.Exists(Path.Combine(this.dir, "settings.json"));
        }

        public Settings LoadSettingsFromSettingFile()
        {
            if (File.Exists(Path.Combine(this.dir, "settings.json"))) {
                try 
                {
                    var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(this.dir, "settings.json")));
                    return settings;
                }
                catch {
                    return new Settings {};
                }
            } else {
                return new Settings {};
            }
        }

        public bool SaveToSaveFile(Save save)
        {
            if(!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            SFHRZModLoaderPlugin.Logger?.LogInfo($"Saving to File...{Path.Combine(this.dir, "save.json")}");
            File.WriteAllText(Path.Combine(this.dir, "save.json"), JsonConvert.SerializeObject(save));
            return true;
        }

        public bool SaveSettingsToSettingsFile(Settings save)
        {
            if(!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(Path.Combine(this.dir, "settings.json"), JsonConvert.SerializeObject(save));
            return true;
        }

        public bool SaveControlsToSettingsFile(string controls)
        {
            if(!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            var settings = LoadSettingsFromSettingFile();
            settings.Controls = controls;
            SaveSettingsToSettingsFile(settings);
            return true;
        }
    }
}