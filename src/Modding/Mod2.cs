#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using SFHR_ZModLoader.Scripting;

namespace SFHR_ZModLoader.Modding;

[Serializable]
public struct Mod2Metadata
{
    public string id;
    public string version;
    public string? displayName;
    public string? description;
    [DefaultValue("index.js")]
    public string entry;
}

public struct Mod2
{
    public Mod2Metadata metadata;
    public string directory;

    public static Mod2 LoadFromDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            if (File.Exists(Path.Combine(directoryPath, "mod2.json")))
            {
                try
                {
                    var metadata = JsonConvert.DeserializeObject<Mod2Metadata>(File.ReadAllText(Path.Combine(directoryPath, "mod2.json")));
                    return new Mod2
                    {
                        metadata = metadata,
                        directory = directoryPath
                    };
                }
                catch (Exception e)
                {
                    throw new ModLoadingException($"Loading mod from {directoryPath} failed: Read mod2.json failed: {e}.");
                }
            }
            else
            {
                throw new ModLoadingException($"Loading mod from '{directoryPath}' failed: mod2.json not found.");
            }
        }
        else
        {
            throw new ModLoadingException($"Loading mod from '{directoryPath}' failed: Not existed.");
        }
    }
}