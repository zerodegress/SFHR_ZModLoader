#nullable enable

using BepInEx.Logging;

namespace SFHR_ZModLoader.Scripting;

class ScriptObjectConsole {
    private ManualLogSource Logger { get; set; }
    public ScriptObjectConsole(ManualLogSource logger)
    {
        Logger = logger;
    }
    public void log(string message)
    {
        Logger.LogInfo(message);
    }
}