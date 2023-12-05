#nullable enable
using System;

namespace SFHR_ZModLoader;


[Serializable]
public struct ModScriptsDataConf
{
    public string? entry;
    public string[]? includes;
    public string[]? excludes;
}