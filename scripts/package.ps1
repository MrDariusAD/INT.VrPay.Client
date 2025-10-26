#!/usr/bin/env pwsh
# Package script for VrPay.Client

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$OutputDir = './artifacts',

    [Parameter()]
    [string]$VersionSuffix = ''
)

$ErrorActionPreference = 'Stop'

Write-Host "Packaging VrPay.Client..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Navigate to solution root
$solutionRoot = Split-Path $PSScriptRoot -Parent
Set-Location $solutionRoot

# Create output directory
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Build pack command
$packCommand = "dotnet pack ./src/VrPay.Client/VrPay.Client.csproj --configuration $Configuration --output $OutputDir --no-build --nologo"

if ($VersionSuffix) {
    $packCommand += " --version-suffix $VersionSuffix"
    Write-Host "Version Suffix: $VersionSuffix" -ForegroundColor Yellow
}

Write-Host "`nCreating NuGet package..." -ForegroundColor Cyan
Invoke-Expression $packCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "Packaging failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nPackage created successfully!" -ForegroundColor Green
Write-Host "Output: $OutputDir" -ForegroundColor Cyan

# List created packages
Get-ChildItem -Path $OutputDir -Filter *.nupkg | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
}
