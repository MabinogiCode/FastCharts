using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Service interface for managing chart axes operations
    /// </summary>
    public interface IAxisManagementService
    {
        /// <summary>
        /// Replaces the X axis with a new axis, preserving data range
        /// </summary>
        /// <param name="currentAxis">Current X axis</param>
        /// <param name="newAxis">New X axis to replace with</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The new X axis, or current axis if replacement failed</returns>
        IAxis<double> ReplaceXAxis(IAxis<double> currentAxis, IAxis<double> newAxis, IList<AxisBase> axesList);

        /// <summary>
        /// Replaces the Y axis with a new axis, preserving data range
        /// </summary>
        /// <param name="currentAxis">Current Y axis</param>
        /// <param name="newAxis">New Y axis to replace with</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The new Y axis, or current axis if replacement failed</returns>
        IAxis<double> ReplaceYAxis(IAxis<double> currentAxis, IAxis<double> newAxis, IList<AxisBase> axesList);

        /// <summary>
        /// Replaces the secondary Y axis with a new axis, preserving data range
        /// </summary>
        /// <param name="currentAxis">Current secondary Y axis (can be null)</param>
        /// <param name="newAxis">New secondary Y axis to replace with</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The new secondary Y axis</returns>
        IAxis<double> ReplaceSecondaryYAxis(IAxis<double>? currentAxis, IAxis<double> newAxis, IList<AxisBase> axesList);

        /// <summary>
        /// Ensures a secondary Y axis exists, creating one if necessary
        /// </summary>
        /// <param name="currentSecondaryAxis">Current secondary Y axis (can be null)</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The secondary Y axis (existing or newly created)</returns>
        IAxis<double> EnsureSecondaryYAxis(IAxis<double>? currentSecondaryAxis, IList<AxisBase> axesList);

        /// <summary>
        /// Updates scales for all provided axes based on pixel dimensions
        /// </summary>
        /// <param name="xAxis">X axis to update</param>
        /// <param name="yAxis">Y axis to update</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to update</param>
        /// <param name="viewport">Viewport providing visible ranges</param>
        /// <param name="widthPx">Width in pixels</param>
        /// <param name="heightPx">Height in pixels</param>
        void UpdateScales(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            IViewport viewport,
            double widthPx,
            double heightPx);

        /// <summary>
        /// Clears data ranges for all axes and resets to default values
        /// </summary>
        /// <param name="xAxis">X axis to clear</param>
        /// <param name="yAxis">Y axis to clear</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to clear</param>
        /// <param name="viewport">Viewport to update with default range</param>
        void ClearAxisRanges(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            IViewport viewport);
    }
}