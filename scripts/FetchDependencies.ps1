$gamePath = Resolve-Path $(Get-Content '.gamepath')
New-Item -ItemType Directory -Path 'deps' -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Assembly-CSharp.dll' -Value $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/Assembly-CSharp.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Assembly-CSharp-firstpass.dll' -Value $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/Assembly-CSharp-firstpass.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Il2Cppmscorlib.dll' -Value  $(Join-Path -Path $gamePath -ChildPath 'BepInEx/Interop/Il2Cppmscorlib.dll') -ErrorAction SilentlyContinue