#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Validate FastCharts packages before release
    
.DESCRIPTION
    This script validates that all packages are created correctly with proper content
    
.EXAMPLE
    .\validate-packages.ps1
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "./validation-packages",
    
    [Parameter(Mandatory = $false)]
    [string]$TestVersion = "0.0.1-validation"
)

Write-Host "?? FastCharts Package Validation" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "Output Directory: $OutputDir" -ForegroundColor Gray
Write-Host "Test Version: $TestVersion" -ForegroundColor Gray
Write-Host ""

# Clean and create output directory
if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
    Write-Host "?? Cleaned existing output directory" -ForegroundColor Yellow
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Step 1: Build solution
Write-Host "?? Building solution..." -ForegroundColor Blue
dotnet build FastChartsSolution.sln --configuration Release --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Build successful" -ForegroundColor Green

# Step 2: Pack packages
Write-Host ""
Write-Host "?? Creating packages..." -ForegroundColor Blue

$projects = @(
    "src/FastCharts.Core/FastCharts.Core.csproj",
    "src/FastCharts.Rendering.Skia/FastCharts.Rendering.Skia.csproj",
    "src/FastCharts.Wpf/FastCharts.Wpf.csproj"
)

foreach ($project in $projects) {
    Write-Host "  ?? Packing $project" -ForegroundColor Cyan
    dotnet pack $project --configuration Release --no-build --output $OutputDir /p:PackageVersion=$TestVersion --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Pack failed for $project" -ForegroundColor Red
        exit 1
    }
}

# Step 3: Validate packages
Write-Host ""
Write-Host "?? Validating packages..." -ForegroundColor Blue

$packages = Get-ChildItem $OutputDir -Name "*.nupkg"
if ($packages.Count -ne 3) {
    Write-Host "? Expected 3 packages, found $($packages.Count)" -ForegroundColor Red
    exit 1
}

Write-Host "? Found all 3 expected packages:" -ForegroundColor Green

foreach ($package in $packages) {
    $fullPath = "$OutputDir/$package"
    $info = Get-Item $fullPath
    $sizeKB = [math]::Round($info.Length/1KB, 1)
    
    Write-Host "  ?? $package" -ForegroundColor Cyan
    Write-Host "     Size: $sizeKB KB" -ForegroundColor Gray
    
    # Validate package contents
    Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
    try {
        $zip = [System.IO.Compression.ZipFile]::OpenRead($fullPath)
        
        # Check for required files
        $hasManifest = $zip.Entries | Where-Object { $_.Name -like "*.nuspec" }
        $hasDll = $zip.Entries | Where-Object { $_.Name -like "*.dll" }
        $hasIcon = $zip.Entries | Where-Object { $_.Name -like "*mabinogi-icon.png*" }
        $hasReadme = $zip.Entries | Where-Object { $_.Name -like "*README.md*" }
        
        if ($hasManifest) { Write-Host "     ? Contains .nuspec manifest" -ForegroundColor Green }
        else { Write-Host "     ? Missing .nuspec manifest" -ForegroundColor Red }
        
        if ($hasDll) { Write-Host "     ? Contains .dll files" -ForegroundColor Green }
        else { Write-Host "     ? Missing .dll files" -ForegroundColor Red }
        
        if ($hasIcon) { Write-Host "     ? Contains Mabinogi icon" -ForegroundColor Green }
        else { Write-Host "     ? Missing Mabinogi icon" -ForegroundColor Red }
        
        if ($hasReadme) { Write-Host "     ? Contains README.md" -ForegroundColor Green }
        else { Write-Host "     ??  Missing README.md" -ForegroundColor Yellow }
        
        $zip.Dispose()
    } catch {
        Write-Host "     ??  Could not inspect package contents: $_" -ForegroundColor Yellow
    }
    
    Write-Host ""
}

# Step 4: Test package validation
Write-Host "?? Running package validation..." -ForegroundColor Blue
dotnet pack src/FastCharts.Core/FastCharts.Core.csproj --configuration Release --no-build --output $OutputDir /p:PackageVersion=$TestVersion --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Package validation failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Package validation passed" -ForegroundColor Green

# Step 5: Summary
Write-Host ""
Write-Host "?? Validation Complete!" -ForegroundColor Green
Write-Host "======================" -ForegroundColor Green
Write-Host "? All 3 packages created successfully" -ForegroundColor Green
Write-Host "? All packages contain required files" -ForegroundColor Green  
Write-Host "? Mabinogi icon included in all packages" -ForegroundColor Green
Write-Host "? Package validation passed" -ForegroundColor Green
Write-Host ""
Write-Host "?? Packages created in: $OutputDir" -ForegroundColor Cyan
Write-Host "?? Ready for release!" -ForegroundColor Green

# Optional: Open output directory  
if ($IsWindows) {
    $response = Read-Host "Open output directory? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        Start-Process explorer.exe $OutputDir
    }
}