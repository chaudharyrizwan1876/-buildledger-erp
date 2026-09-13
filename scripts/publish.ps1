<#
.SYNOPSIS
  Publishes BuildLedger.App as a self-contained, single-folder Windows x64 build.
.EXAMPLE
  ./scripts/publish.ps1
#>
param(
    [string]$OutputDir = "$PSScriptRoot/../publish"
)

$ErrorActionPreference = "Stop"
$projectPath = "$PSScriptRoot/../src/BuildLedger.App/BuildLedger.App.csproj"

Write-Host "Publishing BuildLedger ERP (self-contained, win-x64) to $OutputDir ..."

dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $OutputDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Published to $OutputDir"
Write-Host "Run BuildLedger.App.exe from that folder to verify the release build."
