$gamePath = Resolve-Path $(Get-Content '.gamepath')
New-Item -ItemType SymbolicLink -Path $(Join-Path -Path $gamePath -ChildPath "BepInEx\plugins\SFHR_ZModLoader.dll") -Value $(Resolve-Path "bin\Debug\net6.0\SFHR_ZModLoader.dll") -ErrorAction SilentlyContinue
#New-Item -ItemType SymbolicLink -Path $(Join-Path -Path $gamePath -ChildPath "BepInEx\plugins\Newtonsoft.Json.dll") -Value $(Resolve-Path "bin\Debug\net6.0\Newtonsoft.Json.dll") -ErrorAction SilentlyContinue
#New-Item -ItemType SymbolicLink -Path $(Join-Path -Path $gamePath -ChildPath "BepInEx\plugins\ClearScript.Core.dll") -Value "bin\Debug\net481\ClearScript.Core.dll"
#New-Item -ItemType SymbolicLink -Path $(Join-Path -Path $gamePath -ChildPath "BepInEx\plugins\ClearScript.V8.dll") -Value "bin\Debug\net481\ClearScript.V8.dll"