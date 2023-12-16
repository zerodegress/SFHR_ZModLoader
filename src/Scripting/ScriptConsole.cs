#nullable enable

using BepInEx.Logging;
using System.Linq;

namespace SFHR_ZModLoader.Scripting;

class ScriptObjectConsole
{
    private ManualLogSource Logger { get; set; }
    public ScriptObjectConsole(ManualLogSource logger)
    {
        Logger = logger;
    }
    public void log(params object[] objs)
    {
        Logger.LogInfo(string.Join(" ", objs.Select(obj => obj.ToString())));
    }

    public void info(params object[] objs)
    {
        log(objs);
    }

    public void warn(params object[] objs)
    {
        Logger.LogWarning(string.Join(" ", objs.Select(obj => obj.ToString())));
    }

    public void error(params object[] objs)
    {
        Logger.LogError(string.Join(" ", objs.Select(obj => obj.ToString())));
    }
}