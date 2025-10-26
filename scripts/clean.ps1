#!/usr/bin/env pwsh
# Clean script for VrPay.Client

$ErrorActionPreference = 'Stop'

Write-Host "Cleaning VrPay.Client solution..." -ForegroundColor Cyan

# Navigate to solution root
$solutionRoot = Split-Path $PSScriptRoot -Parent
Set-Location $solutionRoot

# Clean with dotnet
Write-Host "`nRunning dotnet clean..." -ForegroundColor Cyan
dotnet clean --nologo

# Remove bin and obj folders
Write-Host "`nRemoving bin and obj folders..." -ForegroundColor Cyan

$foldersToDelete = Get-ChildItem -Path . -Include bin,obj -Recurse -Directory

foreach ($folder in $foldersToDelete) {
    Write-Host "Deleting: $($folder.FullName)" -ForegroundColor Yellow
    Remove-Item -Path $folder.FullName -Recurse -Force
}

# Remove artifacts folder
if (Test-Path './artifacts') {
    Write-Host "Removing artifacts folder..." -ForegroundColor Yellow
    Remove-Item -Path './artifacts' -Recurse -Force
}

# Remove test results
$testResults = Get-ChildItem -Path . -Filter TestResults -Recurse -Directory

foreach ($folder in $testResults) {
    Write-Host "Deleting: $($folder.FullName)" -ForegroundColor Yellow
    Remove-Item -Path $folder.FullName -Recurse -Force
}

Write-Host "`nClean completed successfully!" -ForegroundColor Green
