# FastCharts — Roadmap

## Principes de base
- .NET Framework 4.8 / C# 7.3 (compat minimale).
- Une classe / un enum / une struct = un fichier.
- Pas de `dynamic` ; préférer types forts ou réflexion minimale encapsulée.
- Style code : accolades et sauts de ligne partout (pas de `if (x) return;`).
- Helpers réutilisables = méthode statique centralisée (ex : `PixelMapper`).
- SOLID + Clean Architecture : séparation nette `Core` (modèle/axes/séries/thème) / `Rendering.*` (backends).
- API stable : on évite les breaking changes ; si nécessaire → `Obsolete` + migration.

## État actuel
- Rendu Skia monolithique fonctionnel, avec Scatter en cours de correction.
- Séries : Line, Area, Scatter, Band.
- `PixelMapper` pour conversions data↔pixels.
- Thème + palette de couleurs (conversion RGBA).
- Démo WPF fonctionnelle.

## Phase 0 — Stabilisation immédiate
- Corriger ScatterSeries (mapping relatif au plot).
- Centraliser `plotRect` et utiliser partout.
- Adapter palette → RGBA sans `dynamic`.
- Tests “fumée” bitmap (Line/Area/Scatter/Band).
- **Done** : scatter sinus affiché correctement, tests bitmap passent.

## Phase 1 — Refactor Skia
- SkiaChartRenderer devient orchestrateur.
- Composants : Grid, Axes, Series, Overlay.
- SkiaPaintPack centralise `SKPaint`.
- PaletteColorAdapter + Rgba struct séparée.
- **Done** : rendu identique au monolithique, code lisible.

## Phase 2 — Axes & Ticks
- Unifier IAxis<double> (NumericAxis).
- Ticker configurable, bornes arrondies.
- Formatters (scientifique, suffixes k/M/G).
- Padding + inversion Y.
- **Done** : ticks lisibles, labels formatés, pas de chevauchement.

## Phase 3 — Interactions (Behaviors)
- IChartBehavior : ZoomWheel, Pan, ZoomRect.
- Crosshair/Tooltip via behaviors.
- InteractionState dans ChartModel.
- **Done** : démos Pan+ZoomWheel combinés, tests zoom/pan.

## Phase 4 — Séries additionnelles
- StepLine, Bar/Column, OHLC/Candlestick, ErrorBars.
- BandSeries : robustesse points non monotones.
- **Done** : démos pour chaque nouvelle série.

## Phase 5 — Annotations
- LineAnnotation, BoxAnnotation, TextAnnotation.
- ZIndex + snapping optionnel.
- **Done** : seuil horizontal, plages X, tags texte visibles.

## Phase 6 — Thèmes & Légende
- Thèmes clair/sombre, palettes prédéfinies.
- Legend dockable, clic → toggle série.
- Contraste AA pour labels.
- **Done** : demo thème switch, légende interactive.

## Phase 7 — Export & Performance
- Export PNG (Skia).
- Benchmarks (100k–1M points).
- SamplingMode pour séries.
- **Done** : export PNG, bench < cible temps.

## Phase 8 — Backends
- IRenderer<TSurface> stabilisé.
- Skia = référence, OpenGL prototype v2.
- Compat couches agnostiques.
- **Done** : doc comment écrire un renderer alternatif.

## Phase 9 — 3D (long terme)
- Projet FastCharts3D séparé.
- Axes/caméra 3D, Scatter/Surface3D.
- Backend OpenGL/Vulkan/DirectX.
- **Done** : POC scatter 3D orbitant, axes affichés.

## CI & Qualité
- GitHub Actions build + tests.
- Analyzers : StyleCop, pas de dynamic.
- Couverture 60% initiale, progression.
- Docs : `docs/` + `samples/`.

## Branch / Commit / PR conventions
- Branches : feature/*, fix/*, refactor/*, chore/*
- Commit msg (EN) : `fix(scatter): map markers relative to plot`
- PR : titre clair + Summary + Changes + Testing + Checklist
