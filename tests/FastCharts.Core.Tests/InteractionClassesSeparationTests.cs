using FastCharts.Core.Interaction;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Tests validating the 1-class-per-file separation for Interaction classes
/// </summary>
public class InteractionClassesSeparationTests
{
    [Fact]
    public void InteractionStateShouldBeCreatable()
    {
        // Arrange & Act
        var interactionState = new InteractionState();

        // Assert
        interactionState.Should().NotBeNull();
        interactionState.ShowCrosshair.Should().BeFalse(); // Default value
    }

    [Fact]
    public void LegendHitShouldBeCreatable()
    {
        // Arrange & Act
        var legendHit = new LegendHit();

        // Assert
        legendHit.Should().NotBeNull();
        legendHit.X.Should().Be(0.0); // Default value
        legendHit.Y.Should().Be(0.0); // Default value
    }

    [Fact]
    public void TooltipSeriesValueShouldBeCreatable()
    {
        // Arrange & Act
        var tooltipValue = new TooltipSeriesValue();

        // Assert
        tooltipValue.Should().NotBeNull();
        tooltipValue.Title.Should().BeNull(); // Default value
        tooltipValue.X.Should().Be(0.0); // Default value
        tooltipValue.Y.Should().Be(0.0); // Default value
    }

    [Fact]
    public void InteractionStateShouldReferenceOtherClasses()
    {
        // Arrange
        var interactionState = new InteractionState();

        // Act & Assert
        interactionState.TooltipSeries.Should().NotBeNull();
        interactionState.TooltipSeries.Should().BeEmpty();

        interactionState.LegendHits.Should().NotBeNull();
        interactionState.LegendHits.Should().BeEmpty();

        // Should be able to add instances
        interactionState.TooltipSeries.Add(new TooltipSeriesValue { Title = "Test" });
        interactionState.LegendHits.Add(new LegendHit { X = 10, Y = 20 });

        interactionState.TooltipSeries.Should().HaveCount(1);
        interactionState.LegendHits.Should().HaveCount(1);
    }

    [Fact]
    public void AllInteractionClassesShouldSupportReactiveUI()
    {
        // Arrange
        var interactionState = new InteractionState();
        var legendHit = new LegendHit();
        var tooltipValue = new TooltipSeriesValue();

        // Assert - All should inherit from ReactiveObject
        interactionState.Should().BeAssignableTo<ReactiveUI.ReactiveObject>();
        legendHit.Should().BeAssignableTo<ReactiveUI.ReactiveObject>();
        tooltipValue.Should().BeAssignableTo<ReactiveUI.ReactiveObject>();
    }
}