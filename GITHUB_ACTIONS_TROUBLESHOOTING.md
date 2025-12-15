# GitHub Actions Troubleshooting Guide

This guide helps resolve common issues with FastCharts GitHub Actions workflows.

## ?? Common Issues

### 1. **NuGet Push Failed: "File does not exist (./nupkg/*.nupkg)"**

**Problem**: Wildcard expansion doesn't work in PowerShell for `dotnet nuget push`

**Solution**: 
- ? **Fixed in workflows** - Now using proper PowerShell loops
- Each package is pushed individually with explicit paths

**Example Fix**:
```powershell
# ? BAD - Doesn't work on Windows
dotnet nuget push "./nupkg/*.nupkg" --api-key $API_KEY --source https://api.nuget.org/v3/index.json

# ? GOOD - Works reliably
$packages = Get-ChildItem ./nupkg -Name "*.nupkg"
foreach ($package in $packages) {
    dotnet nuget push "./nupkg/$package" --api-key $API_KEY --source https://api.nuget.org/v3/index.json
}
```

### 2. **Missing NUGET_API_KEY Secret**

**Problem**: `NUGET_API_KEY` environment variable not set

**Solution**: 
1. Go to [NuGet.org API Keys](https://www.nuget.org/account/apikeys)
2. Create new API key with push permissions
3. Add to GitHub repository secrets:
   - Repository Settings ? Secrets and Variables ? Actions
   - New repository secret: `NUGET_API_KEY`

### 3. **Package Already Exists on NuGet**

**Problem**: Trying to push a package version that already exists

**Solution**: 
- Workflows use `--skip-duplicate` flag
- Increment version number for new releases
- Use pre-release versions for testing (e.g., `v1.0.1-beta1`)

### 4. **Build Failures on Multiple Frameworks**

**Problem**: Build fails on specific target frameworks

**Solution**:
- Enable Windows targeting: `/p:EnableWindowsTargeting=true`
- All workflows now include this flag
- Test locally first: `dotnet build --configuration Release /p:EnableWindowsTargeting=true`

### 5. **Missing Icon in Packages**

**Problem**: Mabinogi icon not included in NuGet packages

**Solution**: 
- ? **Fixed** - Icon path now correctly configured in all `.csproj` files
- Verify with test workflow: `.github/workflows/test-packaging.yml`

### 6. **GitHub Release Creation Failed**

**Problem**: `actions/create-release@v1` is deprecated

**Future Fix**: Update to newer release action
```yaml
# Current (works but deprecated)
uses: actions/create-release@v1

# Future (recommended)
uses: softprops/action-gh-release@v1
```

## ?? Debugging Steps

### 1. Check Workflow Logs
```bash
# View workflow runs
https://github.com/MabinogiCode/FastCharts/actions

# Look for specific errors in:
# - Build step
# - Pack step  
# - Publish step
```

### 2. Test Locally
```bash
# Test build
dotnet build FastChartsSolution.sln --configuration Release /p:EnableWindowsTargeting=true

# Test pack
dotnet pack src/FastCharts.Core/FastCharts.Core.csproj --configuration Release --output ./test-nupkg

# Check package contents
ls ./test-nupkg
```

### 3. Verify Package Contents
```powershell
# Install NuGet CLI
dotnet tool install -g NuGet.CommandLine

# List package contents  
nuget list -source ./test-nupkg
```

### 4. Test Manual Push
```bash
# Test with local API key
dotnet nuget push "./test-nupkg/FastCharts.Core.1.0.0.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
```

## ?? Workflow Status

### ? **Working Workflows**
- `ci.yml` - Basic CI with type checking
- `dotnet.yml` - Build and test  
- `test-packaging.yml` - Package verification
- `nuget-publish.yml` - **FIXED** NuGet publishing
- `nuget-prerelease.yml` - **FIXED** Pre-release publishing

### ?? **Recent Fixes Applied**
- Fixed PowerShell wildcard expansion in NuGet push
- Added proper error handling for missing API keys
- Added package verification steps
- Fixed emoji encoding in release descriptions
- Added comprehensive logging for debugging

## ?? **How to Release**

### Stable Release
```bash
git tag v1.0.0
git push origin v1.0.0
```

### Pre-release
```bash
git tag v1.0.1-beta1  
git push origin v1.0.1-beta1
```

### Test Packaging (without publishing)
```bash
# Triggers test-packaging workflow
git push origin main
```

## ?? **Getting Help**

If workflows still fail:

1. **Check Action Logs**: Look for specific error messages
2. **Test Locally**: Reproduce the issue on your machine
3. **Verify Secrets**: Ensure `NUGET_API_KEY` is configured  
4. **Create Issue**: Include full error logs and steps to reproduce

## ?? **Useful Links**

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet CLI Reference](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference)
- [PowerShell in GitHub Actions](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepsshell)

---

**All workflows are now fixed and ready for FastCharts v1.0.0 release! ??**