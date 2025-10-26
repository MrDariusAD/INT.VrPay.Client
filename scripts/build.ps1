#!/usr/bin/env pwsh
# Build script for VrPay.Client

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'

Write-Host "Building VrPay.Client solution..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Navigate to solution root
$solutionRoot = Split-Path $PSScriptRoot -Parent
Set-Location $solutionRoot

# Clean
Write-Host "`nCleaning previous build artifacts..." -ForegroundColor Cyan
dotnet clean --configuration $Configuration --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Clean failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Restore
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Cyan
dotnet restore --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build
Write-Host "`nBuilding solution..." -ForegroundColor Cyan
dotnet build --configuration $Configuration --no-restore --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
