#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine.InputSystem;

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
        private ManualLogSource? Logger { get; set; } = SFHRZModLoaderPlugin.Logger;
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

        public void PatchSDLoad(SD instance)
        {
            var save = LoadFromSaveFile();
            instance.Money = save.Money;
            Traverse.Create(instance).Field<uint>("CurItemID").Value = save.CurItemID;
            instance.Missions = save.Missions;
            instance.CurClass = save.CurClass;
            instance.Misc = save.Misc;
            instance.Classes.Clear();
            foreach (var keyValuePair in save.Classes) 
            {
                instance.Classes.Add(keyValuePair.Key, keyValuePair.Value.ToDataInfo(keyValuePair.Key));
            }
		    instance.Items.Clear();
		    foreach (var keyValuePair in save.Items)
		    {
			    instance.Items.Add(keyValuePair.Key, keyValuePair.Value.ToDataInfo());
		    }
		    instance.ShopItems.Clear();
            foreach (var item in save.ShopItems)
            {
                instance.ShopItems.Add(item.ToDataInfo());
            }
            instance.ItemRewards.Clear();
            foreach (var item in save.ItemRewards)
            {
                instance.ItemRewards.Add(item.ToDataInfo());
            }
            instance.ClassCamos = save.ClassCamos;
            instance.Feats = save.Feats;
            foreach (var keyValuePair in instance.Items)
            {
                if (keyValuePair.Value.Level >= GI.VarsData.MaxLevel)
                {
                    var sitemInfo = keyValuePair.Value;
                    sitemInfo.Level = GI.VarsData.MaxLevel;
                    instance.Items[keyValuePair.Key] = sitemInfo;
                }
            }
            foreach (var keyValuePair in instance.Classes)
            {
                if(keyValuePair.Value.Level >= GI.VarsData.MaxLevel) 
                {
                    GI.SUnitInfo sunitInfo = keyValuePair.Value;
                    sunitInfo.Level = GI.VarsData.MaxLevel;
                    sunitInfo.Exp = 0f;
                    instance.Classes[keyValuePair.Key] = sunitInfo;
                }
            }
            foreach (var keyValuePair in instance.Items)
            {
                if (keyValuePair.Value.Level > GI.VarsData.MaxLevel)
                {
                    SItemInfo sitemInfo2 = keyValuePair.Value;
                    sitemInfo2.Level = GI.VarsData.MaxLevel;
                    instance.Items[keyValuePair.Key] = sitemInfo2;
                }
            }
            if (ExistsSettingsFile())
            {
                Dictionary<string, int> dictionary3;
                dictionary3 = LoadSettingsFromSettingFile().settings;
                foreach (var keyValuePair4 in instance.Settings)
                {
                    if (!dictionary3.ContainsKey(keyValuePair4.Key))
                    {
                        dictionary3.Add(keyValuePair4.Key, keyValuePair4.Value);
                    }
                }
                instance.Settings = dictionary3;
                try
                {
                    string text3 = LoadSettingsFromSettingFile().Controls;
                    UnityEngine.Object.FindObjectOfType<PlayerInput>(true).actions.LoadBindingOverridesFromJson(text3, true);
                }
                catch
                {
                }
            }
        }

        public void PatchSDSave(SD instance)
        {
            var save = new Save
            {
                Version = UT.GetVersionNum(),
                Money = instance.Money,
                SortOrder = instance.SortOrder,
                CurItemID = Traverse.Create(instance).Field<uint>("CurItemID").Value,
                Missions = instance.Missions,
                CurClass = instance.CurClass
            };
            var dictionary = new Dictionary<string, GI.SSaveUnitInfo>();
            foreach (var keyValuePair in instance.Classes)
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value.ToSaveInfo());
            }
            save.Classes = dictionary;
            var dictionary2 = new Dictionary<uint, SSaveItemInfo>();
            foreach (var keyValuePair2 in instance.Items)
            {
                dictionary2.Add(keyValuePair2.Key, keyValuePair2.Value.ToSaveInfo());
            }
            var list = new List<SSaveItemInfo>();
            for (int i = 0; i < instance.ShopItems.Count; i++)
            {
                list.Add(instance.ShopItems[i].ToSaveInfo());
            }
            List<SSaveItemInfo> list2 = new List<SSaveItemInfo>();
            for (int j = 0; j < instance.ItemRewards.Count; j++)
            {
                list2.Add(instance.ItemRewards[j].ToSaveInfo());
            }
            save.Items = dictionary2;
            save.ShopItems = list;
            save.ItemRewards = list2;
            save.ClassCamos = instance.ClassCamos;
            save.Feats = instance.Feats;
            UT.AddUnique<string, int>(instance.Misc, "firstSave", 1);
            save.Misc = instance.Misc;
            SaveToSaveFile(save);

            var settings = new Settings {};
            settings.settings = instance.Settings;
            SaveSettingsToSettingsFile(settings);
            SaveControlsToSettingsFile(UnityEngine.Object.FindObjectOfType<PlayerInput>(true).actions.SaveBindingOverridesAsJson());
        }
    }
}