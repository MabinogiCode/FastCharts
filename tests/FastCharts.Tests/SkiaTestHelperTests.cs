using FastCharts.Tests.Helpers;
using FluentAssertions;
using SkiaSharp;
using System.Drawing;
using Xunit;

namespace FastCharts.Tests;

public class SkiaTestHelperTests
{
    [Fact]
    public void SameColor_ShouldReturnTrue_WhenColorsAreIdentical()
    {
        // Arrange
        var color1 = new SKColor(255, 128, 64, 255);
        var color2 = new SKColor(255, 128, 64, 255);

        // Act
        var result = SkiaTestHelper.SameColor(color1, color2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SameColor_ShouldReturnTrue_WhenColorsAreWithinTolerance()
    {
        // Arrange
        var color1 = new SKColor(255, 128, 64, 255);
        var color2 = new SKColor(250, 125, 60, 250); // 5 units difference in each channel
        byte tolerance = 6;

        // Act
        var result = SkiaTestHelper.SameColor(color1, color2, tolerance);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SameColor_ShouldReturnFalse_WhenColorsExceedTolerance()
    {
        // Arrange
        var color1 = new SKColor(255, 128, 64, 255);
        var color2 = new SKColor(240, 120, 50, 240); // 15 units difference in each channel
        byte tolerance = 6;

        // Act
        var result = SkiaTestHelper.SameColor(color1, color2, tolerance);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(255, 128, 64, 255, 255, 128, 64, 255, 0, true)] // Identical, tolerance 0
    [InlineData(255, 128, 64, 255, 254, 128, 64, 255, 0, false)] // 1 diff in red, tolerance 0
    [InlineData(255, 128, 64, 255, 254, 128, 64, 255, 1, true)] // 1 diff in red, tolerance 1
    [InlineData(255, 128, 64, 255, 250, 123, 59, 250, 5, true)] // 5 diff each, tolerance 5
    [InlineData(255, 128, 64, 255, 249, 123, 59, 250, 5, false)] // 6 diff in red, tolerance 5
    public void SameColor_ShouldHandleToleranceCorrectly(
        byte r1, byte g1, byte b1, byte a1,
        byte r2, byte g2, byte b2, byte a2,
        byte tolerance, bool expected)
    {
        // Arrange
        var color1 = new SKColor(r1, g1, b1, a1);
        var color2 = new SKColor(r2, g2, b2, a2);

        // Act
        var result = SkiaTestHelper.SameColor(color1, color2, tolerance);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SameColor_ShouldUseDefaultTolerance_WhenNotSpecified()
    {
        // Arrange
        var color1 = new SKColor(255, 128, 64, 255);
        var color2 = new SKColor(249, 122, 58, 249); // 6 units difference (within default tolerance of 6)

        // Act
        var result = SkiaTestHelper.SameColor(color1, color2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProbeNotColor_ShouldReturnTrue_WhenDifferentColorFound()
    {
        // Arrange
        using var bitmap = new SKBitmap(10, 10);
        
        // Fill most of bitmap with red
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap.SetPixel(x, y, SKColors.Red);
            }
        }
        
        // Set center pixel to blue (different color)
        bitmap.SetPixel(5, 5, SKColors.Blue);
        
        var center = new Point(5, 5);
        var forbiddenColor = SKColors.Red;
        var radius = 2;

        // Act
        var result = SkiaTestHelper.ProbeNotColor(bitmap, center, forbiddenColor, radius);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ProbeNotColor_ShouldReturnFalse_WhenAllPixelsAreForbiddenColor()
    {
        // Arrange
        using var bitmap = new SKBitmap(10, 10);
        
        // Fill entire bitmap with red
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap.SetPixel(x, y, SKColors.Red);
            }
        }
        
        var center = new Point(5, 5);
        var forbiddenColor = SKColors.Red;
        var radius = 2;

        // Act
        var result = SkiaTestHelper.ProbeNotColor(bitmap, center, forbiddenColor, radius);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ProbeNotColor_ShouldHandleBoundaryConditions()
    {
        // Arrange
        using var bitmap = new SKBitmap(5, 5);
        
        // Fill bitmap with red
        for (var y = 0; y < 5; y++)
        {
            for (var x = 0; x < 5; x++)
            {
                bitmap.SetPixel(x, y, SKColors.Red);
            }
        }
        
        // Test probing near edge - should not crash
        var center = new Point(0, 0); // Top-left corner
        var forbiddenColor = SKColors.Red;
        var radius = 3; // Radius extends beyond bitmap

        // Act & Assert
        var result = SkiaTestHelper.ProbeNotColor(bitmap, center, forbiddenColor, radius);
        result.Should().BeFalse(); // All visible pixels are red
    }

    [Fact]
    public void ProbeNotColor_ShouldRespectRadius()
    {
        // Arrange
        using var bitmap = new SKBitmap(10, 10);
        
        // Fill bitmap with red
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap.SetPixel(x, y, SKColors.Red);
            }
        }
        
        // Set a pixel outside the probe radius to blue
        bitmap.SetPixel(8, 5, SKColors.Blue);
        
        var center = new Point(5, 5);
        var forbiddenColor = SKColors.Red;
        var radius = 2; // Should not reach (8,5)

        // Act
        var result = SkiaTestHelper.ProbeNotColor(bitmap, center, forbiddenColor, radius);

        // Assert
        result.Should().BeFalse(); // Blue pixel is outside radius, so only red found within radius
    }

    [Fact]
    public void ProbeNotColor_ShouldUseToleranceFromSameColor()
    {
        // Arrange
        using var bitmap = new SKBitmap(10, 10);
        
        // Fill bitmap with slightly different reds (within tolerance)
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                bitmap.SetPixel(x, y, new SKColor(250, 0, 0, 255)); // Slightly different red
            }
        }
        
        var center = new Point(5, 5);
        var forbiddenColor = new SKColor(255, 0, 0, 255); // Pure red
        var radius = 2;

        // Act
        var result = SkiaTestHelper.ProbeNotColor(bitmap, center, forbiddenColor, radius);

        // Assert
        result.Should().BeFalse(); // Colors should be considered same within default tolerance
    }

    [Theory]
    [InlineData(0)] // Zero radius - only center pixel
    [InlineData(1)] // Small radius
    [InlineData(5)] // Large radius
    public void ProbeNotColor_ShouldWorkWithDifferentRadii(int radius)
    {
        // Arrange
        using var bitmap = new SKBitmap(15, 15);
        
        // Fill bitmap with red
        for (var y = 0; y < 15; y++)
        {
            for (var x = 0; x < 15; x++)
            {
                bitmap.SetPixel(x, y, SKColors.Red);
            }
        }
        
        var center = new Point(7, 7);
        var forbiddenColor = SKColors.Red;

        // Act & Assert - Should not throw regardless of radius
        var result = SkiaTestHelper.ProbeNotColor(bitmap, center, forbiddenColor, radius);
        result.Should().BeFalse(); // All pixels are red
    }
}