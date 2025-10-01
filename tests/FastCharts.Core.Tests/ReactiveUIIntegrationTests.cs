using System.Collections.Generic;
using FastCharts.Core;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Themes.BuiltIn;
using FluentAssertions;
using ReactiveUI;
using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Tests validating ReactiveUI MVVM integration with ChartModel
/// </summary>
public class ReactiveUIIntegrationTests
{
    [Fact]
    public void ChartModelShouldImplementReactiveObject()
    {
        // Arrange & Act
        var chartModel = new ChartModel();

        // Assert
        chartModel.Should().BeAssignableTo<ReactiveObject>();
    }

    [Fact]
    public void ThemePropertyShouldTriggerPropertyChangedNotification()
    {
        // Arrange
        var chartModel = new ChartModel();
        var propertyChanged = false;
        var propertyName = string.Empty;

        chartModel.PropertyChanged += (sender, e) =>
        {
            propertyChanged = true;
            propertyName = e.PropertyName ?? string.Empty;
        };

        // Act
        chartModel.Theme = new DarkTheme();

        // Assert
        propertyChanged.Should().BeTrue();
        propertyName.Should().Be(nameof(ChartModel.Theme));
    }

    [Fact]
    public void PlotMarginsPropertyShouldTriggerPropertyChangedNotification()
    {
        // Arrange
        var chartModel = new ChartModel();
        var propertyChanged = false;

        chartModel.PropertyChanged += (sender, e) => propertyChanged = true;

        // Act
        chartModel.PlotMargins = new Primitives.Margins(10, 20, 30, 40);

        // Assert
        propertyChanged.Should().BeTrue();
    }

    [Fact]
    public void InteractionStatePropertyShouldTriggerPropertyChangedNotification()
    {
        // Arrange
        var chartModel = new ChartModel();
        var propertyChanged = false;

        chartModel.PropertyChanged += (sender, e) => propertyChanged = true;

        // Act
        chartModel.InteractionState = new Interaction.InteractionState();

        // Assert
        propertyChanged.Should().BeTrue();
    }

    [Fact]
    public void RaiseAndSetIfChangedShouldOnlyTriggerWhenValueChanges()
    {
        // Arrange
        var chartModel = new ChartModel();
        var changeCount = 0;

        chartModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(ChartModel.Theme))
            {
                changeCount++;
            }
        };

        var theme = new DarkTheme();

        // Act - Set same theme twice
        chartModel.Theme = theme;
        chartModel.Theme = theme; // Should not trigger change

        // Assert
        changeCount.Should().Be(1); // Only first assignment should trigger
    }

    [Fact]
    public void InteractionStateShouldSupportPropertyChanged()
    {
        // Arrange
        var interactionState = new Interaction.InteractionState();
        var propertyChanged = false;

        interactionState.PropertyChanged += (sender, e) => propertyChanged = true;

        // Act
        interactionState.ShowCrosshair = true;

        // Assert
        propertyChanged.Should().BeTrue();
    }

    [Fact]
    public void InteractionStateDataCoordinatesShouldTriggerPropertyChanged()
    {
        // Arrange
        var interactionState = new Interaction.InteractionState();
        var propertyChangedCount = 0;

        interactionState.PropertyChanged += (sender, e) => propertyChangedCount++;

        // Act
        interactionState.DataX = 10.5;
        interactionState.DataY = 20.7;

        // Assert
        propertyChangedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void LegendHitShouldSupportPropertyChanged()
    {
        // Arrange
        var legendHit = new Interaction.LegendHit();
        var propertyChanged = false;

        legendHit.PropertyChanged += (sender, e) => propertyChanged = true;

        // Act
        legendHit.X = 10.0;

        // Assert
        propertyChanged.Should().BeTrue();
    }

    [Fact]
    public void TooltipSeriesValueShouldSupportPropertyChanged()
    {
        // Arrange
        var tooltipValue = new Interaction.TooltipSeriesValue();
        var propertyChanged = false;

        tooltipValue.PropertyChanged += (sender, e) => propertyChanged = true;

        // Act
        tooltipValue.Title = "Series 1";

        // Assert
        propertyChanged.Should().BeTrue();
    }

    [Fact]
    public void ChartModelShouldPreserveExistingFunctionality()
    {
        // Arrange
        var chartModel = new ChartModel();

        // Act & Assert - All existing functionality should still work
        chartModel.Theme.Should().NotBeNull();
        chartModel.Series.Should().NotBeNull();
        chartModel.XAxis.Should().NotBeNull();
        chartModel.YAxis.Should().NotBeNull();
        chartModel.Legend.Should().NotBeNull();
        chartModel.Behaviors.Should().NotBeNull();

        // Methods should still work
        chartModel.Invoking(x => x.AutoFitDataRange()).Should().NotThrow();
        chartModel.Invoking(x => x.UpdateScales(800, 600)).Should().NotThrow();
        chartModel.Invoking(x => x.EnsureSecondaryYAxis()).Should().NotThrow();
    }

    [Fact]
    public void ChartModelThemeChangesShouldBeBindable()
    {
        // Arrange
        var chartModel = new ChartModel();
        var initialTheme = chartModel.Theme;

        // Act
        chartModel.Theme = new DarkTheme();
        
        // Assert
        chartModel.Theme.Should().NotBeSameAs(initialTheme);
        chartModel.Theme.Should().BeOfType<DarkTheme>();
    }

    [Fact]
    public void InteractionStatePropertiesShouldBeIndependentlyBindable()
    {
        // Arrange
        var state = new Interaction.InteractionState();
        var showCrosshairChanged = false;
        var dataXChanged = false;

        state.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Interaction.InteractionState.ShowCrosshair))
            {
                showCrosshairChanged = true;
            }
            if (e.PropertyName == nameof(Interaction.InteractionState.DataX))
            {
                dataXChanged = true;
            }
        };

        // Act
        state.ShowCrosshair = true;
        state.DataX = 42.0;

        // Assert
        showCrosshairChanged.Should().BeTrue();
        dataXChanged.Should().BeTrue();
    }
}