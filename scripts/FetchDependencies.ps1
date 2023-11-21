$gamePath = Resolve-Path $(Get-Content '.gamepath')
New-Item -ItemType Directory -Path 'deps' -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Assembly-CSharp.dll' -Value $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/Assembly-CSharp.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Assembly-CSharp-firstpass.dll' -Value $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/Assembly-CSharp-firstpass.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Il2Cppmscorlib.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/Il2Cppmscorlib.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/UnityEngine.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/UnityEngine.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/UnityEngine.InputModule.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/UnityEngine.InputModule.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/UnityEngine.InputLegacyModule.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/UnityEngine.InputLegacyModule.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/UnityEngine.CoreModule.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/UnityEngine.CoreModule.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/UnityEngine.AudioModule.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/UnityEngine.AudioModule.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/UnityEngine.ImageConversionModule.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/UnityEngine.ImageConversionModule.dll') -ErrorAction SilentlyContinue