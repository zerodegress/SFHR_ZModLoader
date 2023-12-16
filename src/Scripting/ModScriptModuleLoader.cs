#nullable enable
using System;
using System.Collections.Generic;
using Esprima;
using Esprima.Ast;
using Il2CppSystem.IO;
using Jint;
using Jint.Runtime.Modules;

namespace SFHR_ZModLoader.Scripting;

public class ModScriptModules
{
    private Dictionary<string, string> modDirectoryMap = new();

    public void AddModDirectory(string modId, string modDirectory)
    {
        modDirectoryMap.Remove(modId);
        modDirectoryMap.Add(modId, modDirectory);
    }

    public string GetModuleSource(Uri uri)
    {
        if (uri.Scheme != "mod")
        {
            throw new Exception($"Not mod scheme: '{uri.Scheme}'.");
        }
        if (modDirectoryMap.TryGetValue(uri.Authority, out var directory))
        {
            var scriptPath = Path.Combine(directory, uri.LocalPath.TrimStart('/'));
            if (!Path.GetFullPath(scriptPath).StartsWith(Path.GetFullPath(directory)))
            {
                throw new Exception($"Mod '{uri.Authority}' script module should be in its mod directory: '{uri}'.");
            }
            if (File.Exists(scriptPath))
            {
                try
                {
                    return File.ReadAllText(scriptPath);
                }
                catch (Exception e)
                {
                    throw new Exception($"Error while reading script file: '{Path.Combine(directory, uri.LocalPath)}': {e}.");
                }
            }
            else
            {
                throw new Exception($"Script file not found: '{Path.Combine(directory, uri.LocalPath)}'.");
            }
        }
        else
        {
            throw new Exception($"Mod not found: '{uri.Authority}'.");
        }
    }

    public byte[] GetModFileBytes(Uri uri)
    {
        if (uri.Scheme != "modfile")
        {
            throw new Exception($"Not file scheme: '{uri.Scheme}'.");
        }
        if (modDirectoryMap.TryGetValue(uri.Authority, out var directory))
        {
            if (File.Exists(Path.Combine(directory, uri.LocalPath)))
            {
                try
                {
                    return File.ReadAllBytes(Path.Combine(directory, uri.LocalPath));
                }
                catch (Exception e)
                {
                    throw new Exception($"Error while reading file: '{Path.Combine(directory, uri.LocalPath)}': {e}.");
                }
            }
            else
            {
                throw new Exception($"File not found: '{Path.Combine(directory, uri.LocalPath)}'.");
            }
        }
        else
        {
            throw new Exception($"Mod not found: '{uri.Authority}'.");
        }
    }
}

public class ModScriptModuleLoader : IModuleLoader
{
    public ModScriptModules ModScriptModules { get; } = new();

    public Module LoadModule(Engine engine, ResolvedSpecifier resolved)
    {
        if (resolved.Type != SpecifierType.RelativeOrAbsolute)
        {
            throw new Exception($"The default module loader can only resolve files. You can define modules directly to allow imports using {"Engine"}.{"AddModule"}(). Attempted to resolve: '{resolved.Specifier}'.");
        }

        if (resolved.Uri == null)
        {
            throw new Exception($"Module '{resolved.Specifier}' of type '{resolved.Type}' has no resolved URI.");
        }
        if (resolved.Uri.Segments.Length < 3)
        {
            throw new Exception($"Invalid module: {resolved.Uri}");
        }

        var realUri = new Uri($"{resolved.Uri.Segments[1].TrimEnd('/')}://{resolved.Uri.Segments[2].TrimEnd('/')}/{string.Join("", resolved.Uri.Segments, 3, resolved.Uri.Segments.Length - 3).TrimEnd('/')}");
        string code;
        // SFHRZModLoaderPlugin.Logger?.LogWarning(realUri);
        switch (realUri.Scheme)
        {
            case "mod":
                code = ModScriptModules.GetModuleSource(realUri);
                break;
            case "modfile":
                // TODO: 完成文件加载部分
                throw new Exception($"Unsupported scheme: {realUri.Scheme}");
            default:
                throw new Exception($"Unknown scheme: {realUri.Scheme}");
        }
        string path = resolved.Uri.ToString();
        try
        {
            var module = new JavaScriptParser(new ParserOptions()).ParseModule(code, path);
            return module;
        }
        catch (ParserException ex)
        {
            throw new Exception($"Error while loading module: error in module '{path}': {ex.Error}");
        }
        catch (Exception)
        {
            throw new Exception($"Could not load module: '{path}'.");
        }
    }

    public ResolvedSpecifier Resolve(string? referencingModuleLocation, string specifier)
    {
        SFHRZModLoaderPlugin.Logger?.LogWarning($"referencingModuleLocation: {referencingModuleLocation}");
        if (string.IsNullOrEmpty(specifier))
        {
            throw new Exception($"Invalid Module Specifier: '{specifier}' in '{referencingModuleLocation}'");
        }

        // Specifications from ESM_RESOLVE Algorithm: https://nodejs.org/api/esm.html#resolution-algorithm

        Uri resolved;
        if (Uri.TryCreate(specifier, UriKind.Absolute, out var uri))
        {
            resolved = new Uri($"vfs:///{uri.Scheme}/{uri.Host}{uri.LocalPath}");
        }
        else if (IsRelative(specifier))
        {
            if (referencingModuleLocation == null)
            {
                throw new Exception($"No base module location for '{specifier}'");
            }
            resolved = new Uri(new Uri($"vfs://{referencingModuleLocation}"), specifier);
        }
        else
        {
            return new ResolvedSpecifier(
                specifier,
                specifier,
                Uri: null,
                SpecifierType.Bare
            );
        }

        if (resolved.IsFile)
        {
            throw new Exception("Real module file access is not allowed.");
        }


        SFHRZModLoaderPlugin.Logger?.LogWarning($"resolved: {resolved}");
        return new ResolvedSpecifier(
            specifier,
            resolved.ToString(),
            resolved,
            SpecifierType.RelativeOrAbsolute
        );
    }

    private static bool IsRelative(string specifier)
    {
        return specifier.StartsWith('.');
    }
}