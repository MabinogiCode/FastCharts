# Session Handoff — reprise locale (2026-07-06)

> Document de passation d'une session Claude Code distante (Linux, sans WPF) vers une
> session locale Windows. À lire en début de session locale, puis supprimer quand obsolète.

## 1. Où on en est

Tout est mergé dans `main`. Versions csproj à **1.3.0** (non publiée). NuGet.org : 1.0.0 + 1.2.0 publiées (tag v1.2.0 posé par Florent).

| Version | Contenu | État |
|---------|---------|------|
| v1.1 (PR #62) | **Fixes critiques** : StreamingLineSeries ne rendait rien (`new` au lieu de `virtual/override` sur GetRenderData/GetXRange/GetYRange) ; séries Observable* MVVM non-attachables à ChartModel (n'héritaient pas de SeriesBase) → réécrites sur LineSeries/ScatterSeries/BarSeries avec `SeriesDataBinder`. Perf : zéro-copie GetRenderData, `CachedPropertyPathResolver` (délégués compilés), SeriesColorResolver sans allocations. API KISS : `model.AddSeries(Dictionary<double,double>)`. | ✅ mergé |
| v1.2 (PR #63) | Marqueurs sur LineSeries (rendus), spline Catmull-Rom (`Smoothing`), export SVG (`ExportSvg`/`ISvgChartExporter`), thèmes dynamiques (`ChartThemes.Light/Dark/HighContrast`, `CustomTheme`), `AddSeries(data, ChartKind.…)`. | ✅ mergé + publié |
| v1.3 (PR #64) | Finance : OhlcSeries BullColor/BearColor + Volume (bande basse), `Indicators.Sma/Ema/BollingerBands`, `ChartLinkGroup` (synchro X multi-charts) + `AxisBase.VisibleRangeChanged`. Release depuis le navigateur : workflow *Publish NuGet Packages* en `workflow_dispatch` (crée le tag lui-même). | ✅ mergé, non publié |
| v1.4 (PRs #65, #66) | Benchmarks BenchmarkDotNet (`benchmarks/`) ; cache de géométrie LineLayer (`LineSeries.DataVersion` + cache SKPath/pixels, frames identiques au pixel près). Mesure réelle : LTTB 1M pts = 4,8 ms sur la machine de Florent. | ✅ mergé (GPU restant) |

Tests : **537** verts (`tests/FastCharts.Core.Tests`, tournent sous Linux et Windows). `tests/FastCharts.Tests` est Windows-only (CI).

## 2. 🔴 Bug ouvert — PRIORITÉ 1 : les démos ne démarrent pas

**Symptôme (Net8, persiste après PR #66)** :
`FileNotFoundException: Could not load file or assembly 'ReactiveUI.XamForms'`

**Ce qui a déjà été fait (PR #66)** :
- `ReactiveUI.WPF` 22.3.1 ajouté à FastCharts.Wpf (sauf TFM net6.0-windows, pas de cible)
- `PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Wpf)` dans les deux App.xaml.cs
- Référence morte `System.Runtime.CompilerServices.Unsafe` retirée de Core (cause du FileLoadException net48)

**Pistes à vérifier EN LOCAL (dans l'ordre)** :
1. **Est-ce un vrai crash ?** Lancer sans débogueur (Ctrl+F5) ou cliquer Continuer : ReactiveUI *catch* normalement cette exception pendant son probing — Visual Studio peut simplement s'arrêter sur une first-chance exception. Si l'app tourne en Ctrl+F5 → décocher le break sur FileNotFoundException dans Debug > Windows > Exception Settings, terminé.
2. Vérifier que le code exécuté est bien celui de main (`git pull`, **supprimer bin/ et obj/** des démos, rebuild).
3. Si vrai crash : dans App (constructeur, avant tout RxApp), remplacer par l'init explicite :
   ```csharp
   using Splat; using ReactiveUI;
   Locator.CurrentMutable.InitializeSplat();
   Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);
   ```
   (Vérifié : `InitializeReactiveUI(params RegistrationNamespace[])` existe bien dans ReactiveUI 22.3.1.)
4. Pour net48, si FileLoadException revient : regarder le `DemoApp.Net48.exe.config` généré dans bin — les redirects doivent pointer vers les versions d'assembly effectivement présentes dans bin (AutoGenerateBindingRedirects est activé).

## 3. Prochaines briques (plan validé par Florent, RoadMap.md §11bis)

1. **v1.4 GPU (dernière brique)** : backend GPU opt-in dans le contrôle WPF — `SKGLElement` au lieu de `SKElement` quand `UseGpu=true` (défaut false, zéro risque). Maintenant testable en local ! Valider visuellement + mesurer avec les benchmarks/démos.
2. **v1.5 « Analytics »** : Heatmap + axe couleur, histogramme auto-binning (`model.AddHistogram(values)`), annotations draggables.
3. **En continu** : tests visuels par hash d'image (T-QA-VISUAL — prérequis avant de merger le GPU idéalement), fusion/pont ChartModelEnhanced ↔ FastChart.

## 4. Process de travail (validé par Florent)

- Branche de travail : `claude/mvvm-lib-review-optimize-5ow7rq` (reset depuis main après chaque merge)
- PR vers `main`, **merge autorisé dès que la CI est verte** (méthode merge)
- Garde CI : `python3 check_one_type_per_file.py .` (⚠️ un seul type par fichier .cs, regex naïve — les modificateurs `sealed`/`readonly` entre visibilité et `class` la contournent)
- Analyzers stricts : TreatWarningsAsErrors sur src/ et tests/ (StyleCop + Sonar sur Core : S4136 surcharges adjacentes, S2292 auto-properties, IDE0007 var, IDE0010 switch avec default…)
- Philosophie **KISS** : toute feature doit rester utilisable en une ligne (`model.AddSeries(dico)`). Cible long terme : parité SciChart WPF.
- Release : GitHub → Actions → *Publish NuGet Packages* → Run workflow → saisir `X.Y.Z` (crée le tag, builde, publie). Florent préfère de grosses versions cumulatives tant que la lib n'a pas d'utilisateurs.
- Ne PAS faire : 3D/Sankey/TreeMap (hors scope), grosses PRs, breaking changes sans note CHANGELOG.

## 5. Détails techniques utiles

- SDK : global.json exige 9.x (rollForward latestMajor). Tests Linux : `SkiaSharp.NativeAssets.Linux.NoDependencies` référencé dans Core.Tests.
- Le rendu synchronise les axes depuis `Viewport` à chaque frame (`AxisManagementService.UpdateScales`) : pour zoomer par code, passer par `model.Viewport.SetVisible(...)` ou `ZoomAt`, pas par `XAxis.VisibleRange` seul.
- `LineSeries.Data` est exposé en `IList<PointD>` : les mutations directes contournent `DataVersion`/cache resampling → appeler `InvalidateCache()` après édition directe (documenté dans le XML doc).
- Benchmarks : `cd benchmarks/FastCharts.Benchmarks && dotnet run -c Release -- --filter *Lttb*`.
- Dossiers dupliqués hérités (non traités, candidats nettoyage 1.5+) : `Scales/LogScale + LogarithmicScale`, `Ticks/` racine vs `Axes/Ticks/`, `Margin + Margins`, `ChartModel` vs `ChartModelEnhanced` (DynamicData, non branché au contrôle WPF).
