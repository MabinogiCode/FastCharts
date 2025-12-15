# FastCharts Release Process

## ?? Automatic NuGet Release via Git Tags

FastCharts uses an automated release process that publishes to NuGet when you push a version tag.

### ? Quick Release (Recommended)

1. **Ensure you're on main branch:**
   ```bash
   git checkout main
   git pull origin main
   ```

2. **Create and push version tag:**
   ```bash
   # For stable release
   git tag v1.0.0
   git push origin v1.0.0
   
   # For pre-release (beta/alpha)
   git tag v1.0.0-beta1
   git push origin v1.0.0-beta1
   ```

3. **?? Done!** GitHub Actions will automatically:
   - ? Run all tests (594 tests)
   - ? Build all packages (Core, Rendering.Skia, Wpf)
   - ? Publish to NuGet.org
   - ? Create GitHub Release
   - ? Upload artifacts

### ?? Manual Release Process

If you prefer manual control or need to test locally:

```bash
# 1. Build and test everything
dotnet build FastChartsSolution.sln --configuration Release
dotnet test FastChartsSolution.sln --configuration Release

# 2. Pack all packages
dotnet pack src/FastCharts.Core/FastCharts.Core.csproj --configuration Release --output ./nupkg /p:PackageVersion=1.0.0
dotnet pack src/FastCharts.Rendering.Skia/FastCharts.Rendering.Skia.csproj --configuration Release --output ./nupkg /p:PackageVersion=1.0.0
dotnet pack src/FastCharts.Wpf/FastCharts.Wpf.csproj --configuration Release --output ./nupkg /p:PackageVersion=1.0.0

# 3. Publish to NuGet (requires API key)
dotnet nuget push "./nupkg/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## ?? Version Conventions

### Stable Releases
- `v1.0.0` - Major release
- `v1.1.0` - Minor release (new features)
- `v1.1.1` - Patch release (bug fixes)

### Pre-releases
- `v1.0.0-alpha1` - Alpha release (early testing)
- `v1.0.0-beta1` - Beta release (feature complete)
- `v1.0.0-rc1` - Release candidate (production ready)

## ?? Setup Requirements

### For Repository Maintainers

1. **Add NuGet API Key to GitHub Secrets:**
   - Go to repository Settings ? Secrets and variables ? Actions
   - Add secret: `NUGET_API_KEY` = your NuGet.org API key
   - Get API key from: https://www.nuget.org/account/apikeys

2. **Optional: Setup GitHub Environments:**
   - Create environments: `nuget-production`, `nuget-prerelease`
   - Add protection rules if desired

## ?? Current Release Status

### Phase 1 Complete - v1.0.0 Ready!
- ? **13/13 Phase 1 features** implemented
- ? **594 tests** passing (100%)
- ? **Multi-target** support (.NET Standard 2.0, .NET 6/8, Framework 4.8)
- ? **Cross-platform** ready (Windows, macOS, Linux)
- ? **Production quality** code with guidelines compliance

### Featured Capabilities
- ?? **Multi-axis support** (left/right Y axes)
- ?? **Logarithmic axes** with custom bases
- ?? **Real-time streaming** with rolling windows
- ? **LTTB decimation** for massive datasets (10M+ points)
- ?? **Interactive behaviors** (pan, zoom, tooltips, crosshair)
- ?? **PNG export** with clipboard support
- ?? **Pinned tooltips** (multi-tooltip support)
- ?? **Annotations** (lines, ranges, labels)
- ?? **Performance metrics** overlay

## ?? Next Steps

1. **Release v1.0.0:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Marketing & Communication:**
   - Blog post announcement
   - Documentation updates
   - Community outreach
   - Gather user feedback

3. **Phase 2 Planning:**
   - Pie/Donut charts (P2-SERIES-PIE)
   - HeatMap visualization (P2-HEATMAP)
   - SVG export (P2-EXPORT-SVG)
   - Data binding improvements (P2-DATA-BIND)

## ?? Troubleshooting

### Build Failures
- Ensure all 594 tests pass: `dotnet test FastChartsSolution.sln`
- Check .NET SDK versions are installed: `dotnet --list-sdks`

### NuGet Upload Issues
- Verify API key is valid and has push permissions
- Check package ID isn't already taken
- Ensure version number is unique

### GitHub Actions Issues
- Check workflow logs in Actions tab
- Verify secrets are configured correctly
- Ensure branch protection rules allow tag pushes

---

**Ready to release FastCharts v1.0.0? Let's make it happen! ??**