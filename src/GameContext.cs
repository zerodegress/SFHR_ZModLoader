#nullable enable

using System;
using BepInEx.Logging;

namespace SFHR_ZModLoader
{
    public class GameContext
    {
        public GlobalData GlobalData { get; }
        public SD SaveData { get; }
        public ManualLogSource Logger { get; }

        public GameContext(GlobalData gd, SD sd, ManualLogSource logger)
        {
            GlobalData = gd;
            SaveData = sd;
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
            var camoData = (CamoData)obj;
            patcher(camoData);
            Logger.LogInfo($"GameContext: Patch CamoData '{name}' completed.");
        }
    }
}