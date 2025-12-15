using System;
using FastCharts.Core.Axes;
using FastCharts.Core.Interaction;
using FastCharts.Core.Primitives;
using FastCharts.Core.Services;
using Xunit;

namespace FastCharts.Core.Tests.Services
{
    public class InteractionServiceTests
    {
        [Fact]
        public void InteractionService_ZoomAt_ValidParameters_UpdatesAxes()
        {
            // Arrange
            var service = new InteractionService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();
            xAxis.VisibleRange = new FRange(0, 10);
            yAxis.VisibleRange = new FRange(0, 10);

            // Act
            service.ZoomAt(xAxis, yAxis, null, 0.5, 0.5, 5, 5);

            // Assert
            Assert.True(xAxis.VisibleRange.Size < 10); // Should be zoomed in
            Assert.True(yAxis.VisibleRange.Size < 10); // Should be zoomed in
        }

        [Theory]
        [InlineData(0.0, 1.0, 5.0, 5.0)] // Zero zoom factor X
        [InlineData(1.0, 0.0, 5.0, 5.0)] // Zero zoom factor Y
        [InlineData(-1.0, 1.0, 5.0, 5.0)] // Negative zoom factor X
        [InlineData(1.0, -1.0, 5.0, 5.0)] // Negative zoom factor Y
        [InlineData(1.0, 1.0, double.NaN, 5.0)] // NaN center X
        [InlineData(1.0, 1.0, 5.0, double.NaN)] // NaN center Y
        public void InteractionService_ZoomAt_InvalidParameters_ThrowsArgumentException(
            double factorX, double factorY, double centerX, double centerY)
        {
            // Arrange
            var service = new InteractionService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                service.ZoomAt(xAxis, yAxis, null, factorX, factorY, centerX, centerY));
        }

        [Fact]
        public void InteractionService_Pan_ValidParameters_UpdatesAxes()
        {
            // Arrange
            var service = new InteractionService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();
            xAxis.VisibleRange = new FRange(0, 10);
            yAxis.VisibleRange = new FRange(0, 10);

            // Act
            service.Pan(xAxis, yAxis, null, 5, -3);

            // Assert
            Assert.Equal(new FRange(5, 15), xAxis.VisibleRange);
            Assert.Equal(new FRange(-3, 7), yAxis.VisibleRange);
        }

        [Theory]
        [InlineData(double.NaN, 0.0)] // NaN delta X
        [InlineData(0.0, double.NaN)] // NaN delta Y
        [InlineData(double.PositiveInfinity, 0.0)] // Infinite delta X
        [InlineData(0.0, double.NegativeInfinity)] // Infinite delta Y
        public void InteractionService_Pan_InvalidParameters_ThrowsArgumentException(
            double deltaX, double deltaY)
        {
            // Arrange
            var service = new InteractionService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                service.Pan(xAxis, yAxis, null, deltaX, deltaY));
        }

        [Fact]
        public void InteractionService_ZoomAt_WithSecondaryYAxis_UpdatesBothYAxes()
        {
            // Arrange
            var service = new InteractionService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();
            var yAxisSecondary = new NumericAxis();

            xAxis.VisibleRange = new FRange(0, 10);
            yAxis.VisibleRange = new FRange(0, 10);
            yAxisSecondary.VisibleRange = new FRange(0, 10);

            // Act
            service.ZoomAt(xAxis, yAxis, yAxisSecondary, 0.5, 0.5, 5, 5);

            // Assert
            Assert.Equal(yAxis.VisibleRange, yAxisSecondary.VisibleRange);
        }

        [Fact]
        public void InteractionService_UpdateInteractionState_ExecutesWithoutException()
        {
            // Arrange
            var service = new InteractionService();
            var interactionState = new InteractionState();

            // Act & Assert - should not throw
            var exception1 = Record.Exception(() => service.UpdateInteractionState(interactionState));
            var exception2 = Record.Exception(() => service.UpdateInteractionState(null));

            Assert.Null(exception1);
            Assert.Null(exception2);
        }
    }
}