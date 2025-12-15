# Démarrage Rapide avec FastCharts

Bienvenue dans FastCharts ! Ce guide vous aidera à créer vos premiers graphiques haute performance dans vos applications .NET.

## ?? Installation

### Pour les Applications WPF (Le Plus Courant)

Installez le package WPF qui inclut tout ce dont vous avez besoin :

```bash
dotnet add package FastCharts.Wpf
```

### Pour les Applications Cross-Platform

Installez les packages de base pour les applications console, services web, ou scénarios non-WPF :

```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

## ?? Votre Premier Graphique

### 1. Graphique WPF Basique

Créez un graphique linéaire simple dans votre application WPF :

**MainWindow.xaml:**
```xml
<Window x:Class="MonApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf"
        Title="Mon Premier FastChart" Height="450" Width="800">
    <Grid>
        <fc:FastChart Model="{Binding ChartModel}" />
    </Grid>
</Window>
```

**MainWindow.xaml.cs:**
```csharp
using System.Windows;
using FastCharts.Core;
using FastCharts.Core.Series;

namespace MonApp
{
    public partial class MainWindow : Window
    {
        public ChartModel ChartModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            
            // Créer le modèle de graphique
            ChartModel = new ChartModel();
            
            // Ajouter des données d'exemple
            var donnees = new[]
            {
                new PointD(0, 10),
                new PointD(1, 25),
                new PointD(2, 15),
                new PointD(3, 30),
                new PointD(4, 20)
            };
            
            // Créer et ajouter la série
            var serie = new LineSeries(donnees)
            {
                Title = "Données d'Exemple",
                Color = ColorRgba.Blue,
                StrokeWidth = 2
            };
            
            ChartModel.AddSeries(serie);
            
            // Définir le contexte de données pour le binding
            DataContext = this;
        }
    }
}
```

### 2. Graphique Multi-Séries

Ajoutez plusieurs séries de données pour comparer différents jeux de données :

```csharp
public MainWindow()
{
    InitializeComponent();
    
    ChartModel = new ChartModel();
    
    // Données des ventes
    var donneesVentes = new[]
    {
        new PointD(1, 100), new PointD(2, 150), new PointD(3, 120),
        new PointD(4, 180), new PointD(5, 200), new PointD(6, 175)
    };
    
    // Données des profits
    var donneesProfits = new[]
    {
        new PointD(1, 20), new PointD(2, 35), new PointD(3, 25),
        new PointD(4, 45), new PointD(5, 55), new PointD(6, 40)
    };
    
    // Ajouter série des ventes
    ChartModel.AddSeries(new LineSeries(donneesVentes)
    {
        Title = "Ventes",
        Color = ColorRgba.Blue,
        StrokeWidth = 2
    });
    
    // Ajouter série des profits
    ChartModel.AddSeries(new LineSeries(donneesProfits)
    {
        Title = "Profits",
        Color = ColorRgba.Green,
        StrokeWidth = 2
    });
    
    DataContext = this;
}
```

### 3. Graphique en Temps Réel

Créez un graphique qui se met à jour en temps réel :

```csharp
using System;
using System.Windows.Threading;
using FastCharts.Core.Series;

public partial class MainWindow : Window
{
    private StreamingLineSeries _serieTempsReel;
    private DispatcherTimer _minuteur;
    private Random _aleatoire = new();
    private double _tempsActuel = 0;

    public MainWindow()
    {
        InitializeComponent();
        
        ChartModel = new ChartModel();
        
        // Créer série temps réel avec fenêtre glissante
        _serieTempsReel = new StreamingLineSeries
        {
            Title = "Données Live",
            Color = ColorRgba.Red,
            StrokeWidth = 2,
            MaxPointCount = 100, // Garder les 100 derniers points
            RollingWindowDuration = TimeSpan.FromMinutes(2)
        };
        
        ChartModel.AddSeries(_serieTempsReel);
        
        // Configurer minuteur pour mises à jour temps réel
        _minuteur = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _minuteur.Tick += MettreAJourDonnees;
        _minuteur.Start();
        
        DataContext = this;
    }
    
    private void MettreAJourDonnees(object sender, EventArgs e)
    {
        // Générer point de données aléatoire
        var valeur = Math.Sin(_tempsActuel) * 50 + _aleatoire.NextDouble() * 10;
        _serieTempsReel.AppendPoint(new PointD(_tempsActuel, valeur));
        
        _tempsActuel += 0.1;
    }
}
```

## ?? Types de Graphiques

FastCharts supporte différents types de graphiques :

### Graphiques Linéaires
```csharp
var serieLigne = new LineSeries(donnees)
{
    Title = "Graphique Linéaire",
    Color = ColorRgba.Blue,
    StrokeWidth = 2,
    ShowMarkers = true
};
```

### Nuages de Points
```csharp
var seriePoints = new ScatterSeries(donnees)
{
    Title = "Nuage de Points",
    Color = ColorRgba.Green,
    MarkerSize = 5
};
```

### Graphiques en Barres
```csharp
var donneesBarres = new[]
{
    new BarPoint(0, 10), new BarPoint(1, 15), new BarPoint(2, 8),
    new BarPoint(3, 20), new BarPoint(4, 12)
};

var serieBarres = new BarSeries(donneesBarres)
{
    Title = "Graphique en Barres",
    FillColor = ColorRgba.Orange,
    Width = 0.8
};
```

### Graphiques en Aires
```csharp
var serieAire = new AreaSeries(donnees)
{
    Title = "Graphique en Aires",
    FillColor = ColorRgba.Blue.WithAlpha(0.3f),
    StrokeColor = ColorRgba.Blue,
    StrokeWidth = 2
};
```

## ?? Personnalisation

### Styler les Séries
```csharp
var serie = new LineSeries(donnees)
{
    Title = "Série Stylée",
    Color = ColorRgba.Purple,
    StrokeWidth = 3,
    StrokeDashArray = new[] { 5f, 2f }, // Ligne pointillée
    ShowMarkers = true,
    MarkerSize = 6,
    MarkerShape = MarkerShape.Circle
};
```

### Configurer les Axes
```csharp
// Personnaliser axe X
ChartModel.XAxis.Title = "Temps (secondes)";
ChartModel.XAxis.LabelFormat = "F1";
ChartModel.XAxis.ShowGrid = true;

// Personnaliser axe Y
ChartModel.YAxis.Title = "Valeur";
ChartModel.YAxis.LabelFormat = "N0";
ChartModel.YAxis.ShowGrid = true;
```

### Ajouter des Interactions
```csharp
// Activer pan et zoom
ChartModel.AddBehavior(new PanBehavior());
ChartModel.AddBehavior(new ZoomBehavior());

// Activer réticule
ChartModel.AddBehavior(new CrosshairBehavior());

// Activer tooltips
ChartModel.AddBehavior(new TooltipBehavior());

// Activer tooltips épinglées (clic droit pour épingler)
ChartModel.AddBehavior(new PinnedTooltipBehavior());
```

## ?? Conseils Performance

### 1. Utiliser les Séries Streaming pour les Données Temps Réel
```csharp
var serieStream = new StreamingLineSeries
{
    MaxPointCount = 1000, // Limiter usage mémoire
    RollingWindowDuration = TimeSpan.FromMinutes(5)
};
```

### 2. Activer l'Auto-Rééchantillonnage pour les Gros Jeux de Données
```csharp
var grosseSerie = new LineSeries(millionsDePoints)
{
    EnableAutoResampling = true // Utilise algorithme LTTB
};
```

### 3. Mises à Jour par Lot pour Plusieurs Points
```csharp
// Au lieu de plusieurs appels AppendPoint
var nouveauxPoints = GenererPlusieursPpints();
serieStream.AppendPoints(nouveauxPoints); // Plus efficace
```

## ?? Résolution de Problèmes

### Graphique Ne S'Affiche Pas
1. Vérifiez que `FastChart.Model` est correctement lié
2. Assurez-vous que les séries ont des points de données valides
3. Vérifiez que `DataContext` est défini correctement

### Problèmes de Performance
1. Activez l'auto-rééchantillonnage pour les gros jeux de données
2. Utilisez les séries streaming pour les scénarios temps réel
3. Limitez le nombre de points visibles
4. Vérifiez si plusieurs séries causent une surcharge

### Problèmes de Binding
1. Implémentez `INotifyPropertyChanged` dans vos ViewModels
2. Utilisez `ObservableCollection` pour les séries dynamiques
3. Assurez-vous du marshalling de thread approprié pour les mises à jour UI

## ?? Étapes Suivantes

- [Guide des Types de Graphiques](chart-types-fr.md) - Guide détaillé pour tous les types
- [Guide Performance](performance-fr.md) - Optimiser les graphiques pour gros jeux de données
- [Guide Style](styling-fr.md) - Personnaliser l'apparence et thèmes
- [Référence API](api-reference-fr.md) - Documentation API complète
- [Exemples](../demos/) - Exemples plus complexes et démos

## ?? Conseils

- Utilisez `StreamingLineSeries` pour les mises à jour temps réel
- Activez l'auto-rééchantillonnage pour les jeux de données >10K points
- Utilisez les behaviors pour ajouter l'interactivité (pan, zoom, tooltips)
- Épinglez les tooltips par clic droit (avec `PinnedTooltipBehavior`)
- Appuyez F3 pour activer/désactiver overlay métriques performance
- Utilisez le support multi-axe pour différentes plages de valeurs

Bon graphique avec FastCharts ! ??