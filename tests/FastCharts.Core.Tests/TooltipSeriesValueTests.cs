using FastCharts.Core.Interaction;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

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