# Démarrage rapide avec FastCharts

Bienvenue dans FastCharts ! Ce guide vous aidera à créer vos premiers graphiques haute performance dans vos applications .NET.

## Installation

### Pour les applications WPF (le plus courant)

Installez le package WPF qui inclut tout ce dont vous avez besoin :

```bash
dotnet add package FastCharts.Wpf
```

### Pour les applications multiplateformes

Installez les packages de base pour les applications console, services web ou scénarios non-WPF (rendu PNG/SVG) :

```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

## Votre premier graphique

### 1. Graphique WPF de base

Créez un graphique linéaire simple dans votre application WPF :

**MainWindow.xaml :**
```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf"
        Title="Mon premier FastChart" Height="450" Width="800">
    <Grid>
        <fc:FastChart Model="{Binding ChartModel}" />
    </Grid>
</Window>
```

**MainWindow.xaml.cs :**
```csharp
using System.Windows;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace MyApp
{
    public partial class MainWindow : Window
    {
        public ChartModel ChartModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            // Créer le modèle de graphique
            ChartModel = new ChartModel();

            // Données d'exemple
            var donnees = new[]
            {
                new PointD(0, 10),
                new PointD(1, 25),
                new PointD(2, 15),
                new PointD(3, 30),
                new PointD(4, 20)
            };

            // Créer et ajouter la série (les couleurs viennent de la palette du thème)
            var serie = new LineSeries(donnees)
            {
                Title = "Données d'exemple",
                StrokeThickness = 2
            };

            ChartModel.AddSeries(serie);

            // Contexte de données pour la liaison
            DataContext = this;
        }
    }
}
```

Pour un tracé rapide, encore moins de code :

```csharp
// Tout Dictionary<double, double> (ou des valeurs Y seules) devient une courbe triée
ChartModel.AddSeries(new Dictionary<double, double> { [0] = 10, [1] = 25, [2] = 15 }, "Mesures");
ChartModel.AddSeries(new[] { 10.0, 25.0, 15.0 }, "Valeurs"); // X = indice
```

### 2. Graphique avec plusieurs séries

Ajoutez plusieurs séries pour comparer différents jeux de données :

```csharp
public MainWindow()
{
    InitializeComponent();

    ChartModel = new ChartModel();

    // Ventes
    var ventes = new[]
    {
        new PointD(1, 100), new PointD(2, 150), new PointD(3, 120),
        new PointD(4, 180), new PointD(5, 200), new PointD(6, 175)
    };

    // Bénéfices
    var benefices = new[]
    {
        new PointD(1, 20), new PointD(2, 35), new PointD(3, 25),
        new PointD(4, 45), new PointD(5, 55), new PointD(6, 40)
    };

    // Chaque série prend la couleur suivante de la palette ; PaletteIndex en impose une
    ChartModel.AddSeries(new LineSeries(ventes) { Title = "Ventes", StrokeThickness = 2 });
    ChartModel.AddSeries(new LineSeries(benefices) { Title = "Bénéfices", StrokeThickness = 2, PaletteIndex = 2 });

    // Échelles différentes ? Placez une série sur l'axe Y secondaire (à droite)
    // ChartModel.AddSeries(new LineSeries(autres) { Title = "Marge %", YAxisIndex = 1 });

    DataContext = this;
}
```

### 3. Graphique temps réel

Créez un graphique mis à jour en temps réel. Le contrôle `FastChart` se redessine
automatiquement quand des points sont ajoutés — aucun rafraîchissement manuel n'est nécessaire :

```csharp
using System;
using System.Windows;
using System.Windows.Threading;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

public partial class MainWindow : Window
{
    private readonly StreamingLineSeries _serieTempsReel;
    private readonly DispatcherTimer _minuteur;
    private readonly Random _aleatoire = new();
    private double _tempsCourant;

    public ChartModel ChartModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        ChartModel = new ChartModel();

        // Conserver les 100 derniers points
        _serieTempsReel = new StreamingLineSeries(maxPointCount: 100)
        {
            Title = "Données en direct",
            StrokeThickness = 2
        };

        ChartModel.AddSeries(_serieTempsReel);

        // Minuteur de mise à jour
        _minuteur = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _minuteur.Tick += MettreAJourDonnees;
        _minuteur.Start();

        DataContext = this;
    }

    private void MettreAJourDonnees(object? sender, EventArgs e)
    {
        // Générer un point aléatoire
        var valeur = Math.Sin(_tempsCourant) * 50 + _aleatoire.NextDouble() * 10;
        _serieTempsReel.AppendPoint(new PointD(_tempsCourant, valeur));

        _tempsCourant += 0.1;

        // Garder les points les plus récents à l'écran
        ChartModel.AutoFitDataRange();
    }
}
```

Fenêtres temporelles : `new StreamingLineSeries(rollingWindow: TimeSpan.FromMinutes(2))` conserve
les deux dernières minutes lorsque les X sont des horodatages — utilisez `AppendRealTimePoint(valeur)`,
qui horodate le point avec l'heure courante (date OLE Automation), et un `DateTimeAxis`
(`ChartModel.ReplaceXAxis(new DateTimeAxis())`) pour afficher des dates.

Les points peuvent aussi être ajoutés depuis un thread d'arrière-plan : les données d'une série sont
protégées par `serie.SyncRoot`, et le contrôle renvoie son rafraîchissement sur le thread UI.

## Types de graphiques

FastCharts prend en charge plusieurs types de graphiques :

### Courbes
```csharp
var courbe = new LineSeries(donnees)
{
    Title = "Courbe",
    StrokeThickness = 2,
    ShowMarkers = true,
    MarkerShape = MarkerShape.Diamond,
    Smoothing = LineSmoothing.Spline // courbe lissée passant par les points
};
```

### Nuages de points
```csharp
var nuage = new ScatterSeries(donnees)
{
    Title = "Nuage de points",
    MarkerSize = 5,
    MarkerShape = MarkerShape.Circle
};
```

### Histogrammes en barres
```csharp
var barres = new[]
{
    new BarPoint(0, 10), new BarPoint(1, 15), new BarPoint(2, 8),
    new BarPoint(3, 20), new BarPoint(4, 12)
};

var serieBarres = new BarSeries(barres)
{
    Title = "Barres",
    Width = 0.8,        // en unités X ; omis = largeur déduite de l'espacement des X
    FillOpacity = 0.85
};
```

### Aires
```csharp
var aire = new AreaSeries(donnees)
{
    Title = "Aire",
    FillOpacity = 0.3,
    Baseline = 0
};
```

### Histogrammes de distribution
```csharp
// Regroupe automatiquement les valeurs brutes en classes (règle de Sturges) — ou précisez binCount
ChartModel.AddHistogram(mesures, title: "Distribution");
```

## Personnalisation

### Thèmes et couleurs
```csharp
using FastCharts.Core.Themes;

ChartModel.Theme = ChartThemes.Dark; // Light, Dark, HighContrast

// Palette personnalisée, à partir d'un thème intégré
ChartModel.Theme = new CustomTheme(ChartThemes.Light)
{
    SeriesPalette = new[]
    {
        new ColorRgba(33, 150, 243),
        new ColorRgba(76, 175, 80),
        new ColorRgba(244, 67, 54)
    }
};
```

### Configuration des axes
```csharp
using FastCharts.Core.Axes;
using FastCharts.Core.Formatting;

// Format des libellés (axes numériques)
if (ChartModel.YAxis is NumericAxis axeY)
{
    axeY.NumberFormatter = new SuffixNumberFormatter(); // 1.5k, 2M...
}

// Axes logarithmique / date
ChartModel.SetYAxisLogarithmic();
ChartModel.ReplaceXAxis(new DateTimeAxis());

// Afficher une fenêtre précise (zoom) ou revenir à l'étendue des données
ChartModel.XAxis.VisibleRange = new FRange(0, 100);
ChartModel.AutoFitDataRange();

// Grille secondaire
((AxisBase)ChartModel.XAxis).ShowMinorGrid = false;
```

### Ajouter des interactions

`FastChart` installe des comportements par défaut quand le modèle n'en a aucun : déplacement
(glisser clic gauche), zoom molette, rectangle de zoom (Maj + glisser), réticule, infobulle
multi-séries (clic pour la figer, Échap pour la libérer), mise en évidence du point le plus
proche et bascule via la légende. Pour choisir les vôtres :

```csharp
using FastCharts.Core.Interaction.Behaviors;

ChartModel.Behaviors.Add(new PanBehavior());
ChartModel.Behaviors.Add(new ZoomWheelBehavior());
ChartModel.Behaviors.Add(new ZoomRectBehavior());
ChartModel.Behaviors.Add(new CrosshairBehavior());
ChartModel.Behaviors.Add(new MultiSeriesTooltipBehavior());

// Infobulles épinglées (clic droit pour épingler)
ChartModel.Behaviors.Add(new PinnedTooltipBehavior());

// Panneau de performances : F3 l'affiche/le masque, F4 change le niveau de détail, F5 réinitialise
ChartModel.Behaviors.Add(new MetricsOverlayBehavior());
```

## Conseils de performance

### 1. Utiliser les séries de streaming pour le temps réel
```csharp
var serieStreaming = new StreamingLineSeries(maxPointCount: 1000); // Limite la mémoire
```

### 2. Garder le rééchantillonnage automatique pour les gros volumes
```csharp
var grosseSerie = new LineSeries(desMillionsDePoints)
{
    EnableAutoResampling = true // LTTB sur la fenêtre visible (par défaut)
};
```
Seule la plage X visible est décimée (quand les X sont triés) : en zoomant, tous les points apparaissent.

### 3. Ajouter les points par lots
```csharp
// Plutôt que plusieurs appels à AppendPoint
var nouveauxPoints = GenererPoints();
serieStreaming.AppendPoints(nouveauxPoints); // Un seul verrou, une seule demande de rafraîchissement
```

## Dépannage

### Le graphique ne s'affiche pas
1. Vérifiez que `FastChart.Model` est correctement lié
2. Assurez-vous que les séries contiennent des points valides
3. Vérifiez que le `DataContext` est bien défini

### Le graphique ne se met pas à jour
1. Les modifications via l'API des séries (`AddPoint`, `AppendPoint`, `ReplacePoints`...) redessinent automatiquement
2. Après une modification directe d'une liste `Data`, appelez `serie.NotifyChanged()` (ou `ChartModel.Invalidate()` depuis un ViewModel)
3. Si vous modifiez `Data` depuis un autre thread, verrouillez `serie.SyncRoot` pendant la modification

### Problèmes de performance
1. Gardez le rééchantillonnage automatique activé pour les gros volumes
2. Utilisez des séries de streaming avec une limite de points pour le temps réel
3. Préférez `AppendPoints` à de nombreux appels à `AppendPoint`

## Pour aller plus loin

- [README](../README.md) - Vue d'ensemble et autres exemples (finance, graphiques liés, export)
- [Démos](../demos/) - Applications WPF de démonstration complètes
- [CHANGELOG](../CHANGELOG.md) - Les changements de chaque version

Bons graphiques avec FastCharts !
