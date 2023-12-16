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
        public GlobalData? GlobalData { get => GI.GlobalData; }
        public ManualLogSource Logger { get; }

        public GameContext(ManualLogSource logger)
        {
            Logger = logger;
        }

        public void PatchCamoData(string name, Action<CamoData> patcher)
        {
            if(GlobalData == null) {
                Logger.LogWarning($"GameContext: Patch CamoData failed: GlobalData not loaded.");
                return;
            }
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
            catch(Exception e)
            {
                Logger.LogWarning($"GameContext: Patch CamoData '{name}' failed: '{e}'.");
            }
        }

        public void PatchWeaponData(string name, Action<WeaponData> patcher)
        {
            if(GlobalData == null) {
                Logger.LogWarning($"GameContext: Patch WeaponData failed: GlobalData not loaded.");
                return;
            }
            Logger.LogInfo($"GameContext: Patching WeaponData '{name}'...");
            var obj = GlobalData.GetItem(name, GI.EItemType.All);
            if(obj == null)
            {
                Logger.LogWarning($"GameContext: Patch WeaponData '{name}' failed: Not exists.");
                return;
            }
            try
            {
                var weaponData = Il2CppObjectPool.Get<WeaponData>(IL2CPP.Il2CppObjectBaseToPtrNotNull(obj));
                patcher(weaponData);
                Logger.LogInfo($"GameContext: Patch WeaponData '{name}' completed.");
            }
            catch(Exception e)
            {
                Logger.LogWarning($"GameContext: Patch WeaponData '{name}' error: '{e}'.");
            }
        }
        
        public void InsertTexture(string name, Texture2D newTexture)
        {
            if(GlobalData == null) {
                Logger.LogWarning($"GameContext: Insert Texture failed: GlobalData not loaded.");
                return;
            }
            Logger.LogInfo($"GameContext: Inserting Texture '{name}'...");
            GlobalData.Textures.Add(name, newTexture);
            Logger.LogInfo($"GameContext: Insert Texture '{name}' completed.");
        }

        public void PatchTexture(string name, Action<Texture2D> patcher, bool fallbackInsert = false)
        {
            if(GlobalData == null) {
                Logger.LogWarning($"GameContext: Patch Texture failed: GlobalData not loaded.");
                return;
            }
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
            if(GlobalData == null) {
                Logger.LogWarning($"GameContext: Insert sound failed: GlobalData not loaded.");
                return;
            }
            Logger.LogInfo($"GameContext: Inserting sound '{name}'...");
            GlobalData.Sounds.Add(name, newSound);
            Logger.LogInfo($"GameContext: Insert sound '{name}' completed.");
        }

        public void InsertSong(string name, AudioClip newSong)
        {
            if(GlobalData == null) {
                Logger.LogWarning($"GameContext: Insert song failed: GlobalData not loaded.");
                return;
            }
            Logger.LogInfo($"GameContext: Inserting sound '{name}'...");
            GlobalData.Songs.Add(name, newSong);
            Logger.LogInfo($"GameContext: Insert sound '{name}' completed.");
        }
    }
}