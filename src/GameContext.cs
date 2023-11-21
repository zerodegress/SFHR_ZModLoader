#nullable enable

using System;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Runtime;
using UnityEngine;

namespace SFHR_ZModLoader
{
    public class GameContext
    {
        public GlobalData GlobalData { get; }
        public ManualLogSource Logger { get; }

        public GameContext(GlobalData gd, ManualLogSource logger)
        {
            GlobalData = gd;
            Logger = logger;
        }

        public void PatchCamoData(string name, Action<CamoData> patcher)
        {
            Logger.LogInfo($"GameContext: Patching CamoData '{name}'...");
            var obj = GlobalData.GetItem(name, GI.EItemType.Camo);
            if(obj == null)
            {
                Logger.LogWarning($"GameContext: Patch CamoData '{name}' failed: Not exists.");
                return;
            }
            try 
            {
                var camoData = Il2CppObjectPool.Get<CamoData>(IL2CPP.Il2CppObjectBaseToPtrNotNull(obj));
                patcher(camoData);
                Logger.LogInfo($"GameContext: Patch CamoData '{name}' completed.");
            }
            catch
            {
                Logger.LogError($"The type is {obj.GetType().Name}");
                Logger.LogError($"GameContext: Patch CamoData '{name}' failed.");
            }
        }

        public void PatchWeaponData(string name, Action<WeaponData> patcher)
        {
            Logger.LogInfo($"GameContext: Patching WeaponData '{name}'...");
            var obj = GlobalData.GetItem(name, GI.EItemType.All);
            if(obj == null)
            {
                Logger.LogWarning($"GameContext: Patch WeaponData '{name}' failed: Not exists.");
                return;
            }
            var weaponData = (WeaponData)obj;
            patcher(weaponData);
            Logger.LogInfo($"GameContext: Patch WeaponData '{name}' completed.");
        }
        
        public void InsertTexture(string name, Texture2D newTexture)
        {
            Logger.LogInfo($"GameContext: Inserting Texture '{name}'...");
            GlobalData.Textures.Add(name, newTexture);
            Logger.LogInfo($"GameContext: Insert Texture '{name}' completed.");
        }

        public void PatchTexture(string name, Action<Texture2D> patcher, bool fallbackInsert = false)
        {
            Logger.LogInfo($"GameContext: Patching texture '{name}'...");
            if (GlobalData.Textures.ContainsKey(name))
            {
                var texture = GlobalData.Textures[name];
                if(texture.isReadable)
                {
                    patcher(GlobalData.Textures[name]);
                    Logger.LogInfo($"GameContext: Patch texture '{name}' completed.");
                }
                else
                {
                    if(fallbackInsert)
                    {
                        Texture2D newTexture = new(1, 1);
                        patcher(newTexture);
                        GlobalData.Textures.Add(name, newTexture);
                    }
                    else
                    {
                        Logger.LogWarning($"GameContext: Patch texture '{name}' failed: Texture is not readable.");
                    }
                }
            }
            else
            {
                Logger.LogWarning($"GameContext: Patch texture '{name}' failed: Texture not exists.");
            }
        }

        public void InsertSound(string name, AudioClip newSound)
        {
            Logger.LogInfo($"GameContext: Inserting sound '{name}'...");
            GlobalData.Sounds.Add(name, newSound);
            Logger.LogInfo($"GameContext: Insert sound '{name}' completed.");
        }

        public void InsertSong(string name, AudioClip newSong)
        {
            Logger.LogInfo($"GameContext: Inserting sound '{name}'...");
            GlobalData.Songs.Add(name, newSong);
            Logger.LogInfo($"GameContext: Insert sound '{name}' completed.");
        }
    }
}