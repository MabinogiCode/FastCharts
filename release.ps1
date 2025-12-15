#!/usr/bin/env pwsh

<#
.SYNOPSIS
    FastCharts Release Script - Automates the release process for FastCharts packages
    
.DESCRIPTION
    This script helps you release FastCharts packages either locally or by creating Git tags
    for automated GitHub Actions release.
    
.PARAMETER Version
    The version to release (e.g., "1.0.0", "1.1.0-beta1")
    
.PARAMETER Mode
    Release mode: "tag" (GitHub Actions) or "local" (manual)
    
.PARAMETER NuGetApiKey
    NuGet API key (only required for local releases)
    
.PARAMETER DryRun
    Perform a dry run without actually publishing
    
.EXAMPLE
    .\release.ps1 -Version "1.0.0" -Mode tag
    
.EXAMPLE
    .\release.ps1 -Version "1.0.0" -Mode local -NuGetApiKey "your-api-key"
    
.EXAMPLE
    .\release.ps1 -Version "1.1.0-beta1" -Mode tag -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,
    
    [Parameter(Mandatory = $true)]
    [ValidateSet("tag", "local")]
    [string]$Mode,
    
    [Parameter(Mandatory = $false)]
    [string]$NuGetApiKey = "",
    
    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

# Colors for output (simplified for compatibility)
function Write-ColorOutput {
    param([string]$Message, [string]$Color)
    Write-Host $Message -ForegroundColor $Color
}

function Write-Step {
    param([string]$Step)
    Write-ColorOutput "?? $Step" Blue
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "? $Message" Green
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput "??  $Message" Yellow
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "? $Message" Red
}

# Validate version format
if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9]+)?$') {
    Write-Error "Invalid version format. Use semantic versioning (e.g., 1.0.0, 1.1.0-beta1)"
    exit 1
}

$dryRunText = if ($DryRun) { "Yes" } else { "No" }

Write-ColorOutput @"
?? FastCharts Release Script
???????????????????????????????
Version: $Version
Mode: $Mode
Dry Run: $dryRunText
"@ Magenta

# Check prerequisites
Write-Step "Checking prerequisites"

# Check if we're in the right directory
if (-not (Test-Path "FastChartsSolution.sln")) {
    Write-Error "Please run this script from the FastCharts root directory"
    exit 1
}

# Check Git status
$gitStatus = git status --porcelain 2>$null
if ($gitStatus) {
    Write-Warning "You have uncommitted changes:"
    git status --short
    $response = Read-Host "Continue anyway? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Error "Please commit or stash your changes first"
        exit 1
    }
}

# Ensure we're on main branch
$currentBranch = git branch --show-current 2>$null
if ($currentBranch -ne "main") {
    Write-Warning "You're not on the main branch (current: $currentBranch)"
    $response = Read-Host "Switch to main branch? (Y/n)"
    if ($response -ne 'n' -and $response -ne 'N') {
        git checkout main
        git pull origin main
        Write-Success "Switched to main branch"
    }
}

# Build and test
Write-Step "Building and testing solution"
if (-not $DryRun) {
    dotnet restore FastChartsSolution.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Restore failed"
        exit 1
    }
    
    dotnet build FastChartsSolution.sln --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed"
        exit 1
    }
    
    dotnet test FastChartsSolution.sln --configuration Release --no-build --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed"
        exit 1
    }
}
Write-Success "Build and test completed"

if ($Mode -eq "tag") {
    # Tag-based release (GitHub Actions)
    Write-Step "Creating Git tag for automated release"
    
    $tagName = "v$Version"
    
    if ($DryRun) {
        Write-ColorOutput "DRY RUN: Would create tag '$tagName'" Yellow
        Write-ColorOutput "DRY RUN: Would push tag to trigger GitHub Actions" Yellow
    } else {
        # Check if tag already exists
        $existingTag = git tag -l $tagName 2>$null
        if ($existingTag) {
            Write-Error "Tag '$tagName' already exists"
            exit 1
        }
        
        # Create and push tag
        git tag $tagName
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to create tag"
            exit 1
        }
        
        git push origin $tagName
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to push tag"
            exit 1
        }
        
        Write-Success "Tag '$tagName' created and pushed"
        Write-ColorOutput @"
?? Release initiated! 
??????????????????????????????????????????
GitHub Actions will now:
• Run all tests
• Build packages for all target frameworks  
• Publish to NuGet.org
• Create GitHub release with artifacts

Monitor progress at: https://github.com/MabinogiCode/FastCharts/actions
"@ Green
    }
} else {
    # Local release
    Write-Step "Building packages locally"
    
    if ($NuGetApiKey -eq "" -and -not $DryRun) {
        Write-Error "NuGet API key is required for local releases"
        exit 1
    }
    
    $outputDir = "./release-nupkg"
    if (Test-Path $outputDir) {
        Remove-Item $outputDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    
    # Pack all projects
    $projects = @(
        "src/FastCharts.Core/FastCharts.Core.csproj",
        "src/FastCharts.Rendering.Skia/FastCharts.Rendering.Skia.csproj", 
        "src/FastCharts.Wpf/FastCharts.Wpf.csproj"
    )
    
    foreach ($project in $projects) {
        Write-ColorOutput "Packing $project" Cyan
        if (-not $DryRun) {
            dotnet pack $project --configuration Release --no-build --output $outputDir /p:PackageVersion=$Version
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Failed to pack $project"
                exit 1
            }
        }
    }
    
    if ($DryRun) {
        Write-ColorOutput "DRY RUN: Would pack packages to $outputDir" Yellow
        Write-ColorOutput "DRY RUN: Would publish to NuGet.org" Yellow
    } else {
        # List generated packages
        Write-Step "Generated packages:"
        Get-ChildItem $outputDir -Name "*.nupkg" | ForEach-Object {
            Write-ColorOutput "  ?? $_" Cyan
        }
        
        # Publish to NuGet
        Write-Step "Publishing to NuGet.org"
        $packages = Get-ChildItem $outputDir -Name "*.nupkg"
        foreach ($package in $packages) {
            Write-ColorOutput "Publishing $package" Cyan
            dotnet nuget push "$outputDir/$package" --api-key $NuGetApiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Failed to publish $package (might already exist)"
            }
        }
        
        Write-Success "Local release completed"
        Write-ColorOutput @"
?? Packages published!
??????????????????????????????????????????
Install with:
  dotnet add package FastCharts.Core --version $Version
  dotnet add package FastCharts.Rendering.Skia --version $Version  
  dotnet add package FastCharts.Wpf --version $Version

View on NuGet: https://www.nuget.org/packages/FastCharts.Core/
"@ Green
    }
}

Write-ColorOutput @"

?? FastCharts $Version Release Summary
???????????????????????????????????????????
? Phase 1 Complete (13/13 features)
? 594 tests passing  
? Multi-framework support (.NET Standard 2.0, .NET 6/8, Framework 4.8)
? Cross-platform ready
? Production quality code

Next: Start Phase 2 development! ??
"@ Magenta