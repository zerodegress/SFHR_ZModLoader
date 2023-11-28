$gamePath = Resolve-Path $(Get-Content '.gamepath')
$dependencies = @(
    "Assembly-CSharp.dll",
    "Assembly-CSharp-firstpass.dll",
    "Il2Cppmscorlib.dll",
    "UnityEngine.dll",
    "UnityEngine.InputModule.dll",
    "UnityEngine.InputLegacyModule.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.AudioModule.dll",
    "UnityEngine.ImageConversionModule.dll",
    "UnityEngine.ImageConversionModule.dll",
    "deps/FishNet.Runtime.dll"
)

New-Item -ItemType Directory -Path 'deps' -ErrorAction SilentlyContinue

foreach ($dependency in $dependencies) {
    New-Item -ItemType SymbolicLink -Path "deps/$dependency" -Value $(Join-Path -Path $gamePath -ChildPath "BepInEx/Interop/$dependency") -ErrorAction SilentlyContinue
}