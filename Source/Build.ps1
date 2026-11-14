param(
    [ValidateSet('1.6')][string]$GameVersion = '1.6'
)
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'

function Build-Version([string]$Ver) {
    $ReferencePath = Join-Path $env:USERPROFILE '.nuget/packages/krafs.rimworld.ref/1.6.4871/ref/net472'
    $names = @(
        'mscorlib.dll',
        'System.dll',
        'System.Core.dll',
        'netstandard.dll',
        'Assembly-CSharp.dll',
        'UnityEngine.CoreModule.dll',
        'UnityEngine.IMGUIModule.dll',
        'UnityEngine.TextRenderingModule.dll',
        'Unity.Mathematics.dll'
    )

    $HarmonyPath = "E:/SteamLibrary/steamapps/workshop/content/294100/2009463077/$Ver/Assemblies/0Harmony.dll"
    $refs = @($names | ForEach-Object { '/reference:' + (Join-Path $ReferencePath $_) })
    $refs += '/reference:' + $HarmonyPath

    $output = Join-Path (Split-Path -Parent $PSScriptRoot) "$Ver/Assemblies"
    New-Item -ItemType Directory -Force -Path $output | Out-Null
    $sources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' -Recurse | ForEach-Object FullName

    & $compiler /noconfig /nologo /nostdlib+ /target:library /optimize+ /warn:4 ('/out:' + (Join-Path $output 'RebornEvolvingMagic.dll')) $refs $sources
    if ($LASTEXITCODE -ne 0) {
        throw "RebornEvolvingMagic $Ver build failed."
    }
    Write-Output "RebornEvolvingMagic $Ver build passed -> $output/RebornEvolvingMagic.dll"
}

Build-Version '1.6'

$modRoot = Split-Path -Parent $PSScriptRoot
$localModsTarget = 'E:/SteamLibrary/steamapps/common/RimWorld/Mods/RebornEvolvingMagic'
if (Test-Path 'E:/SteamLibrary/steamapps/common/RimWorld/Mods') {
    # Remove existing target contents first to prevent nesting
    Remove-Item -Path "$localModsTarget/*" -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$modRoot/*" -Destination $localModsTarget -Recurse -Force -Exclude @('.git', '.gitignore')
    Write-Output "Clean synced mod to $localModsTarget"
}
