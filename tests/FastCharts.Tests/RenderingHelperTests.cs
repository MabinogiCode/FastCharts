using System.Linq;
using FluentAssertions;
using FastCharts.Rendering.Skia.Helpers;
using Xunit;

namespace FastCharts.Tests
{
    /// <summary>
    /// ? SOLID PRINCIPLES: Unit tests for RenderingHelper
    /// Ensures rendering utilities are properly tested
    /// </summary>
    public class RenderingHelperTests
    {
        [Theory]
        [InlineData(10.0, false, 10.0)]
        [InlineData(10.0, true, 48.0)] // Should use default secondary axis margin
        [InlineData(50.0, true, 50.0)] // Should keep larger base margin
        [InlineData(0.0, true, 48.0)] // Should use default when base is 0
        public void CalculateEffectiveRightMargin_ReturnsCorrectValue(double baseMargin, bool hasSecondaryAxis, double expected)
        {
            // Act
            var result = RenderingHelper.CalculateEffectiveRightMargin(baseMargin, hasSecondaryAxis);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(100, 50, 100, 50)]
        [InlineData(0, 0, 1, 1)] // Should enforce minimum
        [InlineData(-10, -5, 1, 1)] // Should handle negative values
        [InlineData(1000, 1, 1000, 1)] // Should preserve valid values
        public void EnsureValidPlotDimensions_ReturnsValidDimensions(int width, int height, int expectedWidth, int expectedHeight)
        {
            // Act
            var (resultWidth, resultHeight) = RenderingHelper.EnsureValidPlotDimensions(width, height);

            // Assert
            resultWidth.Should().Be(expectedWidth);
            resultHeight.Should().Be(expectedHeight);
        }

        [Fact]
        public void RenderingHelper_Constants_HaveExpectedValues()
        {
            // Assert
            RenderingHelper.DefaultSecondaryAxisMargin.Should().Be(48.0);
            RenderingHelper.MinimumPlotDimension.Should().Be(1);
            RenderingHelper.DefaultColorTolerance.Should().Be(6);
            RenderingHelper.DefaultPngQuality.Should().Be(100);
        }

        [Fact]
        public void RenderingHelper_IsStaticClass_AndCannotBeInstantiated()
        {
            // ? SIMPLIFIED APPROACH: Just verify we can use static members
            // If the class is truly static, these calls should work
            var margin = RenderingHelper.CalculateEffectiveRightMargin(10.0, true);
            var (width, height) = RenderingHelper.EnsureValidPlotDimensions(100, 200);
            var constant = RenderingHelper.DefaultSecondaryAxisMargin;
            
            // Assert basic functionality works
            margin.Should().Be(48.0);
            width.Should().Be(100);
            height.Should().Be(200);
            constant.Should().Be(48.0);
            
            // ? ALTERNATIVE: Test that type has static characteristics
            var type = typeof(RenderingHelper);
            
            // A static class should have no public constructors
            var publicConstructors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            publicConstructors.Should().BeEmpty("because static classes have no public instance constructors");
            
            // All public methods should be static
            var publicMethods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var methodsFromObject = publicMethods.Where(m => m.DeclaringType == typeof(object)).Count();
            var totalMethods = publicMethods.Length;
            
            // Static classes only have methods inherited from Object (GetType, ToString, etc.)
            totalMethods.Should().Be(methodsFromObject, "because static classes should not have public instance methods except from Object");
        }
    }
}