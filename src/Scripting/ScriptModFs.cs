#nullable enable

using System;
using System.IO;

namespace SFHR_ZModLoader.Scripting;

class ScriptObjectFs
{
    private string dir;
    public ScriptObjectFs(string dir)
    {
        this.dir = dir;
    }

    public string ReadAllText(string filePath)
    {
        if (!Path.GetFullPath(filePath).StartsWith(Path.GetFullPath(dir)))
        {
            throw new Exception($"__fs should only access the file inside its dir: {dir}");
        }
        return File.ReadAllText(Path.Combine(dir, filePath));
    }

    public byte[] ReadAllBytes(string filePath)
    {
        if (!Path.GetFullPath(filePath).StartsWith(Path.GetFullPath(dir)))
        {
            throw new Exception($"__fs should only access the file inside its dir: {dir}");
        }
        return File.ReadAllBytes(Path.Combine(dir, filePath));
    }
}