#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Esprima;
using Esprima.Ast;
using Jint;
using Jint.Native;
using Jint.Runtime.Modules;

namespace SFHR_ZModLoader;

public class ModScriptModules 
{
    private Dictionary<string, Dictionary<string, Dictionary<string, string>>> moduleSourceDict = new();

    public void AddModule(string modId, string nsname, string path, string source)
    {
        if(!moduleSourceDict.ContainsKey(modId))
        {
            moduleSourceDict.Add(modId, new());
        }
        var modDict = moduleSourceDict[modId];
        if(!modDict.ContainsKey(nsname))
        {
            modDict.Add(nsname, new());
        }
        var nsDict = modDict[nsname];
        nsDict.Add(path, source);
    }

    public string GetModuleSource(Uri uri)
    {
        switch(uri.Scheme)
        {
            case "mod":
            {
                if(moduleSourceDict.TryGetValue(uri.Host, out var modDict))
                {
                    if(uri.Segments.Length >= 3)
                    {
                        var nsname = uri.Segments[1].TrimEnd('/');
                        var path = string.Join("", uri.Segments, 2, uri.Segments.Length - 2).TrimEnd('/');
                        if(modDict.TryGetValue(nsname, out var nsDict))
                        {
                            if(nsDict.TryGetValue(path, out var source))
                            {
                                return source;
                            }
                            else
                            {
                                throw new Exception($"Unknown path in namespace '{nsname}': {path}");
                            }
                        }
                        else
                        {
                            throw new Exception($"Unknown namespace in Mod '{uri.Host}': '{nsname}'.");
                        }
                    }
                    else
                    {
                        throw new Exception($"Path segments missing: '{uri.AbsolutePath}'.");
                    }
                }
                else
                {
                    throw new Exception($"Unknown Mod ID: '{uri.Host}'.");
                }
            }
            default:
                throw new Exception($"Unknown schema: '{uri.Scheme}'.");
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
        string code = ModScriptModules.GetModuleSource(realUri);
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
            if(referencingModuleLocation == null)
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