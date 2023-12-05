#nullable enable

using Jint;

namespace SFHR_ZModLoader;

public class ModScriptEngineWrapper
{
    public ModScriptModuleLoader ModScriptModuleLoader { get; } = new();
    public ModScriptModules ModScriptModules { get => ModScriptModuleLoader.ModScriptModules; }

    public Engine Engine { get; private set; }

    public ModScriptEngineWrapper()
    {
        Engine = new(options => {
            options.AllowClr(typeof(GlobalData).Assembly)
                .EnableModules(ModScriptModuleLoader);
        });
    }
}