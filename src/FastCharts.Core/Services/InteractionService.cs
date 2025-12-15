using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Helpers;
using FastCharts.Core.Interaction;
using FastCharts.Core.Primitives;
using FastCharts.Core.Utilities;
using FastCharts.Core.Services;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Default implementation of interaction service for handling chart zoom and pan operations
    /// </summary>
    public sealed class InteractionService : IInteractionService
    {
        /// <summary>
        /// Performs zoom operation at specified center point with given factors
        /// </summary>
        /// <param name="xAxis">X axis to apply zoom to</param>
        /// <param name="yAxis">Y axis to apply zoom to</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to apply zoom to</param>
        /// <param name="factorX">Zoom factor for X axis (> 0)</param>
        /// <param name="factorY">Zoom factor for Y axis (> 0)</param>
        /// <param name="centerDataX">Center point X in data coordinates</param>
        /// <param name="centerDataY">Center point Y in data coordinates</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when resulting range is invalid</exception>
        public void ZoomAt(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            double factorX,
            double factorY,
            double centerDataX,
            double centerDataY)
        {
            ValidateZoomParameters(factorX, factorY, centerDataX, centerDataY);

            var xRange = xAxis.VisibleRange;
            var yRange = yAxis.VisibleRange;

            var newSizeX = xRange.Size * factorX;
            var newSizeY = yRange.Size * factorY;

            var newMinX = centerDataX - (centerDataX - xRange.Min) * factorX;
            var newMaxX = newMinX + newSizeX;
            var newMinY = centerDataY - (centerDataY - yRange.Min) * factorY;
            var newMaxY = newMinY + newSizeY;

            // Handle degenerate ranges
            if (DoubleUtils.AreEqual(newMinX, newMaxX))
            {
                newMinX -= 1e-6;
                newMaxX += 1e-6;
            }

            if (DoubleUtils.AreEqual(newMinY, newMaxY))
            {
                newMinY -= 1e-6;
                newMaxY += 1e-6;
            }

            ValidateResultingRanges(newMinX, newMaxX, newMinY, newMaxY);

            // Apply zoom to axes
            xAxis.VisibleRange = new FRange(newMinX, newMaxX);
            yAxis.VisibleRange = new FRange(newMinY, newMaxY);

            if (yAxisSecondary != null)
            {
                yAxisSecondary.VisibleRange = yAxis.VisibleRange;
            }
        }

        /// <summary>
        /// Performs pan operation by specified deltas
        /// </summary>
        /// <param name="xAxis">X axis to apply pan to</param>
        /// <param name="yAxis">Y axis to apply pan to</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to apply pan to</param>
        /// <param name="deltaDataX">Pan delta for X axis in data coordinates</param>
        /// <param name="deltaDataY">Pan delta for Y axis in data coordinates</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when resulting range is invalid</exception>
        public void Pan(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            double deltaDataX,
            double deltaDataY)
        {
            ValidatePanParameters(deltaDataX, deltaDataY);

            var xRange = xAxis.VisibleRange;
            var yRange = yAxis.VisibleRange;

            var newMinX = xRange.Min + deltaDataX;
            var newMaxX = xRange.Max + deltaDataX;
            var newMinY = yRange.Min + deltaDataY;
            var newMaxY = yRange.Max + deltaDataY;

            ValidateResultingRanges(newMinX, newMaxX, newMinY, newMaxY);

            // Apply pan to axes
            xAxis.VisibleRange = new FRange(newMinX, newMaxX);
            yAxis.VisibleRange = new FRange(newMinY, newMaxY);

            if (yAxisSecondary != null)
            {
                yAxisSecondary.VisibleRange = yAxis.VisibleRange;
            }
        }

        /// <summary>
        /// Updates interaction state for the chart
        /// </summary>
        /// <param name="interactionState">New interaction state</param>
        public void UpdateInteractionState(InteractionState? interactionState)
        {
            // This is a placeholder for more complex interaction state management
            // In the future, this could handle state persistence, validation, or events
        }

        private static void ValidateZoomParameters(double factorX, double factorY, double centerDataX, double centerDataY)
        {
            if (double.IsNaN(factorX) || double.IsInfinity(factorX) || factorX <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(factorX), factorX, "Zoom factor X must be finite and positive");
            }

            if (double.IsNaN(factorY) || double.IsInfinity(factorY) || factorY <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(factorY), factorY, "Zoom factor Y must be finite and positive");
            }

            if (double.IsNaN(centerDataX) || double.IsInfinity(centerDataX))
            {
                throw new ArgumentOutOfRangeException(nameof(centerDataX), centerDataX, "Center X must be finite");
            }

            if (double.IsNaN(centerDataY) || double.IsInfinity(centerDataY))
            {
                throw new ArgumentOutOfRangeException(nameof(centerDataY), centerDataY, "Center Y must be finite");
            }
        }

        private static void ValidatePanParameters(double deltaDataX, double deltaDataY)
        {
            if (double.IsNaN(deltaDataX) || double.IsInfinity(deltaDataX))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaDataX), deltaDataX, "Delta X must be finite");
            }

            if (double.IsNaN(deltaDataY) || double.IsInfinity(deltaDataY))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaDataY), deltaDataY, "Delta Y must be finite");
            }
        }

        private static void ValidateResultingRanges(double newMinX, double newMaxX, double newMinY, double newMaxY)
        {
            if (!ValidationHelper.IsValidRange(newMinX, newMaxX))
            {
                throw new InvalidOperationException($"Resulting X range is invalid: [{newMinX}, {newMaxX}]");
            }

            if (!ValidationHelper.IsValidRange(newMinY, newMaxY))
            {
                throw new InvalidOperationException($"Resulting Y range is invalid: [{newMinY}, {newMaxY}]");
            }
        }
    }
}