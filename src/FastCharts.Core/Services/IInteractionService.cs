using FastCharts.Core.Abstractions;
using FastCharts.Core.Interaction;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Service interface for handling chart interactions such as zooming and panning
    /// </summary>
    public interface IInteractionService
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
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when parameters are invalid</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when resulting range is invalid</exception>
        void ZoomAt(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            double factorX,
            double factorY,
            double centerDataX,
            double centerDataY);

        /// <summary>
        /// Performs pan operation by specified deltas
        /// </summary>
        /// <param name="xAxis">X axis to apply pan to</param>
        /// <param name="yAxis">Y axis to apply pan to</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to apply pan to</param>
        /// <param name="deltaDataX">Pan delta for X axis in data coordinates</param>
        /// <param name="deltaDataY">Pan delta for Y axis in data coordinates</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when parameters are invalid</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when resulting range is invalid</exception>
        void Pan(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            double deltaDataX,
            double deltaDataY);

        /// <summary>
        /// Updates interaction state for the chart
        /// </summary>
        /// <param name="interactionState">New interaction state</param>
        void UpdateInteractionState(InteractionState? interactionState);
    }
}