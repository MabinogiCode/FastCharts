using FastCharts.Core.Interaction;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class InteractionStateTests
{
    [Fact]
    public void InteractionStateShouldHaveDefaultValuesWhenCreated()
    {
        // Act
        var state = new InteractionState();

        // Assert
        state.ShowCrosshair.Should().BeFalse();
        state.PixelX.Should().Be(0);
        state.PixelY.Should().Be(0);
        state.DataX.Should().BeNull();
        state.DataY.Should().BeNull();
        state.TooltipText.Should().BeNull();
        state.TooltipSeries.Should().NotBeNull().And.BeEmpty();
        state.TooltipLocked.Should().BeFalse();
        state.TooltipAnchorX.Should().BeNull();
        state.ShowSelectionRect.Should().BeFalse();
        state.SelX1.Should().Be(0);
        state.SelY1.Should().Be(0);
        state.SelX2.Should().Be(0);
        state.SelY2.Should().Be(0);
        state.ShowNearest.Should().BeFalse();
        state.NearestDataX.Should().Be(0);
        state.NearestDataY.Should().Be(0);
        state.LegendHits.Should().NotBeNull().And.BeEmpty();
        state.IsPanning.Should().BeFalse();
    }

    [Fact]
    public void InteractionStateShouldAllowSettingAllPropertiesWhenModified()
    {
        // Arrange
        var state = new InteractionState();

        // Act
        state.ShowCrosshair = true;
        state.PixelX = 123.45;
        state.PixelY = 678.90;
        state.DataX = 10.5;
        state.DataY = 20.7;
        state.TooltipText = "Test tooltip";
        state.TooltipLocked = true;
        state.TooltipAnchorX = 15.3;
        state.ShowSelectionRect = true;
        state.SelX1 = 100;
        state.SelY1 = 200;
        state.SelX2 = 150;
        state.SelY2 = 250;
        state.ShowNearest = true;
        state.NearestDataX = 42.1;
        state.NearestDataY = 84.2;
        state.IsPanning = true;

        // Assert
        state.ShowCrosshair.Should().BeTrue();
        state.PixelX.Should().Be(123.45);
        state.PixelY.Should().Be(678.90);
        state.DataX.Should().Be(10.5);
        state.DataY.Should().Be(20.7);
        state.TooltipText.Should().Be("Test tooltip");
        state.TooltipLocked.Should().BeTrue();
        state.TooltipAnchorX.Should().Be(15.3);
        state.ShowSelectionRect.Should().BeTrue();
        state.SelX1.Should().Be(100);
        state.SelY1.Should().Be(200);
        state.SelX2.Should().Be(150);
        state.SelY2.Should().Be(250);
        state.ShowNearest.Should().BeTrue();
        state.NearestDataX.Should().Be(42.1);
        state.NearestDataY.Should().Be(84.2);
        state.IsPanning.Should().BeTrue();
    }

    [Fact]
    public void TooltipSeriesShouldAllowAddingItemsWhenItemAdded()
    {
        // Arrange
        var state = new InteractionState();
        var tooltipValue = new TooltipSeriesValue
        {
            Title = "Series 1",
            X = 10.0,
            Y = 20.0,
            PaletteIndex = 0
        };

        // Act
        state.TooltipSeries.Add(tooltipValue);

        // Assert
        state.TooltipSeries.Should().HaveCount(1);
        state.TooltipSeries[0].Should().BeSameAs(tooltipValue);
    }

    [Fact]
    public void LegendHitsShouldAllowAddingItemsWhenItemAdded()
    {
        // Arrange
        var state = new InteractionState();
        var legendHit = new LegendHit
        {
            X = 10.0,
            Y = 20.0,
            Width = 100.0,
            Height = 50.0,
            SeriesReference = "Test Series"
        };

        // Act
        state.LegendHits.Add(legendHit);

        // Assert
        state.LegendHits.Should().HaveCount(1);
        state.LegendHits[0].Should().BeSameAs(legendHit);
    }

    [Fact]
    public void DataCoordinatesShouldAllowNullValuesWhenSetToNull()
    {
        // Arrange
        var state = new InteractionState
        {
            DataX = 10.5,
            DataY = 20.7
        };

        // Act
        state.DataX = null;
        state.DataY = null;

        // Assert
        state.DataX.Should().BeNull();
        state.DataY.Should().BeNull();
    }

    [Fact]
    public void TooltipAnchorXShouldAllowNullValueWhenSetToNull()
    {
        // Arrange
        var state = new InteractionState
        {
            TooltipAnchorX = 123.45
        };

        // Act
        state.TooltipAnchorX = null;

        // Assert
        state.TooltipAnchorX.Should().BeNull();
    }
}

public class TooltipSeriesValueTests
{
    [Fact]
    public void TooltipSeriesValueShouldHaveDefaultValuesWhenCreated()
    {
        // Act
        var value = new TooltipSeriesValue();

        // Assert
        value.Title.Should().BeNull();
        value.X.Should().Be(0);
        value.Y.Should().Be(0);
        value.PaletteIndex.Should().BeNull();
    }

    [Fact]
    public void TooltipSeriesValueShouldAllowSettingAllPropertiesWhenModified()
    {
        // Arrange
        var value = new TooltipSeriesValue();

        // Act
        value.Title = "Test Series";
        value.X = 123.45;
        value.Y = 678.90;
        value.PaletteIndex = 2;

        // Assert
        value.Title.Should().Be("Test Series");
        value.X.Should().Be(123.45);
        value.Y.Should().Be(678.90);
        value.PaletteIndex.Should().Be(2);
    }

    [Fact]
    public void PaletteIndexShouldAllowNullWhenSetToNull()
    {
        // Arrange
        var value = new TooltipSeriesValue
        {
            PaletteIndex = 5
        };

        // Act
        value.PaletteIndex = null;

        // Assert
        value.PaletteIndex.Should().BeNull();
    }
}

public class LegendHitTests
{
    [Fact]
    public void LegendHitShouldHaveDefaultValuesWhenCreated()
    {
        // Act
        var hit = new LegendHit();

        // Assert
        hit.X.Should().Be(0);
        hit.Y.Should().Be(0);
        hit.Width.Should().Be(0);
        hit.Height.Should().Be(0);
        hit.SeriesReference.Should().BeNull();
    }

    [Fact]
    public void LegendHitShouldAllowSettingAllPropertiesWhenModified()
    {
        // Arrange
        var hit = new LegendHit();
        var seriesRef = new { Name = "Test" };

        // Act
        hit.X = 10.5;
        hit.Y = 20.7;
        hit.Width = 100.0;
        hit.Height = 50.0;
        hit.SeriesReference = seriesRef;

        // Assert
        hit.X.Should().Be(10.5);
        hit.Y.Should().Be(20.7);
        hit.Width.Should().Be(100.0);
        hit.Height.Should().Be(50.0);
        hit.SeriesReference.Should().BeSameAs(seriesRef);
    }

    [Fact]
    public void SeriesReferenceShouldAcceptDifferentTypesWhenAssigned()
    {
        // Arrange
        var hit = new LegendHit();

        // Act & Assert - Should accept string
        hit.SeriesReference = "String reference";
        hit.SeriesReference.Should().Be("String reference");

        // Act & Assert - Should accept object
        var objRef = new { Id = 1, Name = "Test" };
        hit.SeriesReference = objRef;
        hit.SeriesReference.Should().BeSameAs(objRef);

        // Act & Assert - Should accept null
        hit.SeriesReference = null;
        hit.SeriesReference.Should().BeNull();
    }
}