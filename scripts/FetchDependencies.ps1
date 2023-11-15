$gamePath = Resolve-Path $(Get-Content '.gamepath')
New-Item -ItemType Directory -Path 'deps' -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Assembly-CSharp.dll' -Value $(Join-Path -Path $gamePath -ChildPath 'Strike Force Heroes_Data\Managed\Assembly-CSharp.dll') -ErrorAction SilentlyContinue
New-Item -ItemType SymbolicLink -Path 'deps/Assembly-CSharp-firstpass.dll' -Value $(Join-Path -Path $gamePath -ChildPath 'Strike Force Heroes_Data\Managed\Assembly-CSharp-firstpass.dll') -ErrorAction SilentlyContinue
