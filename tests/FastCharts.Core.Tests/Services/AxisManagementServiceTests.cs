using System.Collections.Generic;
using FastCharts.Core.Axes;
using FastCharts.Core.Interactivity;
using FastCharts.Core.Primitives;
using FastCharts.Core.Services;
using Xunit;

namespace FastCharts.Core.Tests.Services
{
    public class AxisManagementServiceTests
    {
        [Fact]
        public void AxisManagementService_ReplaceXAxis_ValidAxis_ReplacesAndPreservesDataRange()
        {
            // Arrange
            var service = new AxisManagementService();
            var currentAxis = new NumericAxis();
            var newAxis = new NumericAxis();
            var axesList = new List<AxisBase> { currentAxis, new NumericAxis() };
            
            currentAxis.DataRange = new FRange(5, 15);

            // Act
            var result = service.ReplaceXAxis(currentAxis, newAxis, axesList);

            // Assert
            Assert.Equal(newAxis, result);
            Assert.Equal(new FRange(5, 15), newAxis.DataRange);
            Assert.Equal(newAxis, axesList[0]);
        }

        [Fact]
        public void AxisManagementService_ReplaceXAxis_NullNewAxis_ReturnsCurrentAxis()
        {
            // Arrange
            var service = new AxisManagementService();
            var currentAxis = new NumericAxis();
            var axesList = new List<AxisBase> { currentAxis };

            // Act
            var result = service.ReplaceXAxis(currentAxis, null!, axesList);

            // Assert
            Assert.Equal(currentAxis, result);
        }

        [Fact]
        public void AxisManagementService_ReplaceYAxis_ValidAxis_ReplacesAndPreservesDataRange()
        {
            // Arrange
            var service = new AxisManagementService();
            var currentAxis = new NumericAxis();
            var newAxis = new NumericAxis();
            var axesList = new List<AxisBase> { new NumericAxis(), currentAxis };
            
            currentAxis.DataRange = new FRange(10, 20);

            // Act
            var result = service.ReplaceYAxis(currentAxis, newAxis, axesList);

            // Assert
            Assert.Equal(newAxis, result);
            Assert.Equal(new FRange(10, 20), newAxis.DataRange);
            Assert.Equal(newAxis, axesList[1]);
        }

        [Fact]
        public void AxisManagementService_EnsureSecondaryYAxis_WhenNull_CreatesNewAxis()
        {
            // Arrange
            var service = new AxisManagementService();
            var axesList = new List<AxisBase> { new NumericAxis(), new NumericAxis() };

            // Act
            var result = service.EnsureSecondaryYAxis(null, axesList);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NumericAxis>(result);
            Assert.Equal(3, axesList.Count);
        }

        [Fact]
        public void AxisManagementService_EnsureSecondaryYAxis_WhenExists_ReturnsExisting()
        {
            // Arrange
            var service = new AxisManagementService();
            var existingAxis = new NumericAxis();
            var axesList = new List<AxisBase> { new NumericAxis(), new NumericAxis(), existingAxis };

            // Act
            var result = service.EnsureSecondaryYAxis(existingAxis, axesList);

            // Assert
            Assert.Equal(existingAxis, result);
        }

        [Fact]
        public void AxisManagementService_UpdateScales_UpdatesAllAxes()
        {
            // Arrange
            var service = new AxisManagementService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();
            var yAxisSecondary = new NumericAxis();
            var viewport = new Viewport(new FRange(0, 10), new FRange(5, 15));

            // Act
            service.UpdateScales(xAxis, yAxis, yAxisSecondary, viewport, 800, 600);

            // Assert
            Assert.Equal(viewport.X, xAxis.VisibleRange);
            Assert.Equal(viewport.Y, yAxis.VisibleRange);
            Assert.Equal(yAxis.VisibleRange, yAxisSecondary.VisibleRange);
        }

        [Fact]
        public void AxisManagementService_ClearAxisRanges_ResetsAllRanges()
        {
            // Arrange
            var service = new AxisManagementService();
            var xAxis = new NumericAxis();
            var yAxis = new NumericAxis();
            var yAxisSecondary = new NumericAxis();
            var viewport = new Viewport(new FRange(10, 20), new FRange(30, 40));

            xAxis.DataRange = new FRange(10, 20);
            yAxis.DataRange = new FRange(30, 40);
            yAxisSecondary.DataRange = new FRange(50, 60);

            // Act
            service.ClearAxisRanges(xAxis, yAxis, yAxisSecondary, viewport);

            // Assert
            var expectedRange = new FRange(0, 1);
            Assert.Equal(expectedRange, xAxis.DataRange);
            Assert.Equal(expectedRange, yAxis.DataRange);
            Assert.Equal(expectedRange, yAxisSecondary.DataRange);
        }
    }
}