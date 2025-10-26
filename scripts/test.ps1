#!/usr/bin/env pwsh
# Test script for VrPay.Client

param(
    [Parameter()]
    [ValidateSet('Unit', 'Integration', 'All')]
    [string]$TestType = 'Unit',
    
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter()]
    [switch]$Coverage
)

$ErrorActionPreference = 'Stop'

Write-Host "Running tests for VrPay.Client..." -ForegroundColor Cyan
Write-Host "Test Type: $TestType" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Navigate to solution root
$solutionRoot = Split-Path $PSScriptRoot -Parent
Set-Location $solutionRoot

# Build test command based on parameters
$testCommand = "dotnet test --configuration $Configuration --no-build --nologo"

switch ($TestType) {
    'Unit' {
        Write-Host "`nRunning unit tests..." -ForegroundColor Cyan
        $testCommand += ' --filter "FullyQualifiedName~VrPay.Client.Tests"'
    }
    'Integration' {
        Write-Host "`nRunning integration tests..." -ForegroundColor Cyan
        $testCommand += ' --filter "FullyQualifiedName~VrPay.Client.IntegrationTests"'
    }
    'All' {
        Write-Host "`nRunning all tests..." -ForegroundColor Cyan
    }
}

if ($Coverage) {
    Write-Host "Collecting code coverage..." -ForegroundColor Yellow
    $testCommand += ' --collect:"XPlat Code Coverage"'
}

# Execute tests
Invoke-Expression $testCommand

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nTests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nAll tests passed!" -ForegroundColor Green
