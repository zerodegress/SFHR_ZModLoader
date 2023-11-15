#nullable enable
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SFHR_ZModLoader 
{
    public class Hooks 
    {
        private static ManualLogSource? Logger { get => SFHRZModLoaderPlugin.Logger; }

        [HarmonyPrefix, HarmonyPatch(typeof(SD), nameof(SD.Load))]
        public static bool Prefix_SD_Load(SD __instance) {
            if (SFHRZModLoaderPlugin.SaveMgr == null) {
                return true;
            }
            var saveManager = SFHRZModLoaderPlugin.SaveMgr;
            var save = saveManager.LoadFromSaveFile();
            __instance.Money = save.Money;
            Traverse.Create(__instance).Field<uint>("CurItemID").Value = save.CurItemID;
            __instance.Missions = save.Missions;
            __instance.CurClass = save.CurClass;
            __instance.Misc = save.Misc;
            __instance.Classes.Clear();
            foreach (var keyValuePair in save.Classes) 
            {
                __instance.Classes.Add(keyValuePair.Key, keyValuePair.Value.ToDataInfo(keyValuePair.Key));
            }
		    __instance.Items.Clear();
		    foreach (var keyValuePair in save.Items)
		    {
			    __instance.Items.Add(keyValuePair.Key, keyValuePair.Value.ToDataInfo());
		    }
		    __instance.ShopItems.Clear();
            foreach (var item in save.ShopItems)
            {
                __instance.ShopItems.Add(item.ToDataInfo());
            }
            __instance.ItemRewards.Clear();
            foreach (var item in save.ItemRewards)
            {
                __instance.ItemRewards.Add(item.ToDataInfo());
            }
            __instance.ClassCamos = save.ClassCamos;
            __instance.Feats = save.Feats;
            foreach (var keyValuePair in __instance.Items)
            {
                if (keyValuePair.Value.Level >= GI.VarsData.MaxLevel)
                {
                    var sitemInfo = keyValuePair.Value;
                    sitemInfo.Level = GI.VarsData.MaxLevel;
                    __instance.Items[keyValuePair.Key] = sitemInfo;
                }
            }
            foreach (var keyValuePair in __instance.Classes)
            {
                if(keyValuePair.Value.Level >= GI.VarsData.MaxLevel) 
                {
                    GI.SUnitInfo sunitInfo = keyValuePair.Value;
                    sunitInfo.Level = GI.VarsData.MaxLevel;
                    sunitInfo.Exp = 0f;
                    __instance.Classes[keyValuePair.Key] = sunitInfo;
                }
            }
            foreach (var keyValuePair in __instance.Items)
            {
                if (keyValuePair.Value.Level > GI.VarsData.MaxLevel)
                {
                    SItemInfo sitemInfo2 = keyValuePair.Value;
                    sitemInfo2.Level = GI.VarsData.MaxLevel;
                    __instance.Items[keyValuePair.Key] = sitemInfo2;
                }
            }
            if (SFHRZModLoaderPlugin.SaveMgr.ExistsSettingsFile())
            {
                Dictionary<string, int> dictionary3;
                dictionary3 = SFHRZModLoaderPlugin.SaveMgr.LoadSettingsFromSettingFile().settings;
                foreach (var keyValuePair4 in __instance.Settings)
                {
                    if (!dictionary3.ContainsKey(keyValuePair4.Key))
                    {
                        dictionary3.Add(keyValuePair4.Key, keyValuePair4.Value);
                    }
                }
                __instance.Settings = dictionary3;
                try
                {
                    string text3 = SFHRZModLoaderPlugin.SaveMgr.LoadSettingsFromSettingFile().Controls;
                    UnityEngine.Object.FindObjectOfType<PlayerInput>(true).actions.LoadBindingOverridesFromJson(text3, true);
                }
                catch
                {
                }
            }
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SD), nameof(SD.Save))]
        public static bool Prefix_SD_Save(SD __instance) {
            if (SFHRZModLoaderPlugin.SaveMgr == null) {
                return true;
            }
            var save = new Save
            {
                Version = UT.GetVersionNum(),
                Money = __instance.Money,
                SortOrder = __instance.SortOrder,
                CurItemID = Traverse.Create(__instance).Field<uint>("CurItemID").Value,
                Missions = __instance.Missions,
                CurClass = __instance.CurClass
            };
            var dictionary = new Dictionary<string, GI.SSaveUnitInfo>();
            foreach (var keyValuePair in __instance.Classes)
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value.ToSaveInfo());
            }
            save.Classes = dictionary;
            var dictionary2 = new Dictionary<uint, SSaveItemInfo>();
            foreach (var keyValuePair2 in __instance.Items)
            {
                dictionary2.Add(keyValuePair2.Key, keyValuePair2.Value.ToSaveInfo());
            }
            var list = new List<SSaveItemInfo>();
            for (int i = 0; i < __instance.ShopItems.Count; i++)
            {
                list.Add(__instance.ShopItems[i].ToSaveInfo());
            }
            List<SSaveItemInfo> list2 = new List<SSaveItemInfo>();
            for (int j = 0; j < __instance.ItemRewards.Count; j++)
            {
                list2.Add(__instance.ItemRewards[j].ToSaveInfo());
            }
            save.Items = dictionary2;
            save.ShopItems = list;
            save.ItemRewards = list2;
            save.ClassCamos = __instance.ClassCamos;
            save.Feats = __instance.Feats;
            UT.AddUnique<string, int>(__instance.Misc, "firstSave", 1);
            save.Misc = __instance.Misc;
            SFHRZModLoaderPlugin.SaveMgr.SaveToSaveFile(save);

            var settings = new Settings {};
            settings.settings = __instance.Settings;
            SFHRZModLoaderPlugin.SaveMgr.SaveSettingsToSettingsFile(settings);
            SFHRZModLoaderPlugin.SaveMgr.SaveControlsToSettingsFile(UnityEngine.Object.FindObjectOfType<PlayerInput>(true).actions.SaveBindingOverridesAsJson());
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(SD), nameof(SD.SaveControls))]
        public static bool Prefix_SD_SaveControls(SD __instance)
        {
            if (SFHRZModLoaderPlugin.SaveMgr == null) {
                return true;
            }
            SFHRZModLoaderPlugin.SaveMgr.SaveControlsToSettingsFile(UnityEngine.Object.FindObjectOfType<PlayerInput>(true).actions.SaveBindingOverridesAsJson());
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GlobalData), nameof(GlobalData.Load))]
        public static void Postfix_GlobalData_Load() 
        {
            if (!GI.GlobalData)
            {
                return;
            }
            if(!GI.GlobalData.ItemTypeInfo.ContainsKey(GI.EItemType.Camo)) {
                return;
            }
            var objects = GI.GlobalData.ItemTypeInfo[GI.EItemType.Camo].Objects;
            foreach (var obj in objects)
            {
                CamoData camoData = (CamoData)obj;
                if (!(camoData.name == ""))
                {
                    SFHRZModLoaderPlugin.ModLdr?.PatchCamoData(ref camoData);
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