using System;
using System.Linq;
using FluentAssertions;
using FastCharts.Core.Helpers;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests
{
    /// <summary>
    /// ? SOLID PRINCIPLES: Comprehensive unit tests for ValidationHelper
    /// Ensures all validation logic is properly tested and maintainable
    /// </summary>
    public class ValidationHelperTests
    {
        [Theory]
        [InlineData(0.0, true)]
        [InlineData(1.0, true)]
        [InlineData(-1.0, true)]
        [InlineData(double.MaxValue, true)]
        [InlineData(double.MinValue, true)]
        [InlineData(double.NaN, false)]
        [InlineData(double.PositiveInfinity, false)]
        [InlineData(double.NegativeInfinity, false)]
        public void IsFinite_ValidatesDoubleValues_Correctly(double value, bool expected)
        {
            // Act
            var result = ValidationHelper.IsFinite(value);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0.0, 1.0, true)]
        [InlineData(-10.0, 10.0, true)]
        [InlineData(1.0, 1.0, false)] // Min equals Max
        [InlineData(1.0, 0.0, false)] // Min greater than Max
        [InlineData(double.NaN, 1.0, false)]
        [InlineData(0.0, double.NaN, false)]
        [InlineData(double.PositiveInfinity, 1.0, false)]
        [InlineData(0.0, double.PositiveInfinity, false)]
        public void IsValidRange_WithMinMax_ValidatesCorrectly(double min, double max, bool expected)
        {
            // Act
            var result = ValidationHelper.IsValidRange(min, max);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void IsValidRange_WithFRange_ValidatesCorrectly()
        {
            // Arrange & Act & Assert
            ValidationHelper.IsValidRange(new FRange(0, 10)).Should().BeTrue();
            ValidationHelper.IsValidRange(new FRange(-5, 5)).Should().BeTrue();
            ValidationHelper.IsValidRange(new FRange(10, 10)).Should().BeFalse(); // Equal values
            ValidationHelper.IsValidRange(new FRange(10, 5)).Should().BeFalse(); // Inverted
        }

        [Theory]
        [InlineData(0.0, 0.0, true)]
        [InlineData(1.5, -2.3, true)]
        [InlineData(double.MaxValue, double.MinValue, true)]
        [InlineData(double.NaN, 1.0, false)]
        [InlineData(1.0, double.NaN, false)]
        [InlineData(double.NaN, double.NaN, false)]
        [InlineData(double.PositiveInfinity, 1.0, false)]
        [InlineData(1.0, double.NegativeInfinity, false)]
        public void AreValidCoordinates_ValidatesCoordinatePairs_Correctly(double x, double y, bool expected)
        {
            // Act
            var result = ValidationHelper.AreValidCoordinates(x, y);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(1.0, true)]
        [InlineData(0.5, true)]
        [InlineData(2.0, true)]
        [InlineData(0.1, true)]
        [InlineData(100.0, true)]
        [InlineData(0.0, false)] // Zero is not valid for zoom
        [InlineData(-1.0, false)] // Negative is not valid for zoom
        [InlineData(double.NaN, false)]
        [InlineData(double.PositiveInfinity, false)]
        [InlineData(double.NegativeInfinity, false)]
        public void IsValidZoomFactor_ValidatesZoomFactors_Correctly(double factor, bool expected)
        {
            // Act
            var result = ValidationHelper.IsValidZoomFactor(factor);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void ValidationHelper_IsStaticClass_AndCannotBeInstantiated()
        {
            // ? SIMPLIFIED APPROACH: Just verify we can use static members
            // If the class is truly static, these calls should work
            var isFinite = ValidationHelper.IsFinite(42.0);
            var isValidRange = ValidationHelper.IsValidRange(0.0, 100.0);
            var areValidCoords = ValidationHelper.AreValidCoordinates(10.0, 20.0);
            var isValidZoom = ValidationHelper.IsValidZoomFactor(1.5);

            // Assert basic functionality works
            isFinite.Should().BeTrue();
            isValidRange.Should().BeTrue();
            areValidCoords.Should().BeTrue();
            isValidZoom.Should().BeTrue();

            // ? ALTERNATIVE: Test that type has static characteristics
            var type = typeof(ValidationHelper);

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

        [Fact]
        public void IsValidRange_WithEdgeCases_HandlesCorrectly()
        {
            // Arrange & Act & Assert

            // Very small differences
            ValidationHelper.IsValidRange(0.0, double.Epsilon).Should().BeTrue();

            // Very large ranges
            ValidationHelper.IsValidRange(double.MinValue / 2, double.MaxValue / 2).Should().BeTrue();

            // Mixed finite/infinite values
            ValidationHelper.IsValidRange(0.0, double.PositiveInfinity).Should().BeFalse();
            ValidationHelper.IsValidRange(double.NegativeInfinity, 0.0).Should().BeFalse();
        }

        [Fact]
        public void AreValidCoordinates_WithEdgeCases_HandlesCorrectly()
        {
            // Arrange & Act & Assert

            // Very small values
            ValidationHelper.AreValidCoordinates(double.Epsilon, -double.Epsilon).Should().BeTrue();

            // Very large values
            ValidationHelper.AreValidCoordinates(double.MaxValue / 2, double.MinValue / 2).Should().BeTrue();

            // Edge precision values
            ValidationHelper.AreValidCoordinates(1e-300, 1e300).Should().BeTrue();
        }
    }
}