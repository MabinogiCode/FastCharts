using FastCharts.Core.Interaction;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

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