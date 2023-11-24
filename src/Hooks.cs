#nullable enable
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
namespace SFHR_ZModLoader
{
    public class Hooks
    {
        private static bool isGameContextLoaded = false;
        private static GlobalData? globalData;
        private static ManualLogSource? Logger { get => SFHRZModLoaderPlugin.Logger; }
        private static EventManager? EventManager { get => SFHRZModLoaderPlugin.EventManager; }

        [HarmonyPostfix, HarmonyPatch(typeof(GlobalData), nameof(GlobalData.Load))]
        public static void Postfix_GlobalData_Load()
        {
            if (!GI.GlobalData)
            {
                return;
            }
            globalData = GI.GlobalData;
            EventManager?.EmitEvent(new Event
            {
                type = "GLOBALDATA_LOADED",
                data = globalData,
            });
            if (Logger != null && !isGameContextLoaded)
            {
                isGameContextLoaded = true;
                SFHRZModLoaderPlugin.GameContext = new(globalData, Logger);
                EventManager?.EmitEvent(new Event
                {
                    type = "GAMECONTEXT_LOADED",
                    data = SFHRZModLoaderPlugin.GameContext,
                });
            }
            try
            {
                if (SFHRZModLoaderPlugin.DebugEmit)
                {
                    {
                        var camosCsv = "id, name\n";
                        foreach (var item in GI.GlobalData.ItemTypeInfo[GI.EItemType.Camo].Objects)
                        {
                            var camoData = (CamoData)item;
                            if (camoData.Name != "")
                            {
                                camosCsv += $"{camoData.name}, {camoData.Name}\n";
                            }
                        }
                        File.WriteAllText(Path.Combine(Paths.GameRootPath, "DebugEmit", "camos.csv"), camosCsv);
                    }
                    {
                        var weaponsCsv = "id, name\n";
                        foreach (var item in GI.GlobalData.ItemTypeInfo[GI.EItemType.All].Objects)
                        {
                            var weaponData = (WeaponData)item;
                            if (weaponData.Name != "")
                            {
                                weaponsCsv += $"{weaponData.name}, {weaponData.Name}\n";
                            }
                            File.WriteAllText(Path.Combine(Paths.GameRootPath, "DebugEmit", "weapons.csv"), weaponsCsv);
                        }
                    }
                    {
                        var texturesCsv = "id, name\n";
                        foreach (var item in GI.GlobalData.Textures)
                        {
                            if (item.Key != "")
                            {
                                texturesCsv += $"{item.Key}, {item.Value.name}\n";
                            }
                        }
                        File.WriteAllText(Path.Combine(Paths.GameRootPath, "DebugEmit", "textures.csv"), texturesCsv);
                    }
                    {
                        var soundsCsv = "id, name\n";
                        foreach (var item in GI.GlobalData.Sounds)
                        {
                            if (item.Key != "")
                            {
                                soundsCsv += $"{item.Key}, {item.Value.name}\n";
                            }
                        }
                        File.WriteAllText(Path.Combine(Paths.GameRootPath, "DebugEmit", "sounds.csv"), soundsCsv);
                    }
                    {
                        var songsCsv = "id, name\n";
                        foreach (var item in GI.GlobalData.Songs)
                        {
                            if (item.Value.name != "")
                            {
                                songsCsv += $"{item.Key}, {item.Value.name}\n";
                            }
                        }
                        File.WriteAllText(Path.Combine(Paths.GameRootPath, "DebugEmit", "songs.csv"), songsCsv);
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }
    }
}