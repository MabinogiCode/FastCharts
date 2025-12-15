using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Services;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Default implementation of axis management service
    /// </summary>
    public sealed class AxisManagementService : IAxisManagementService
    {
        /// <summary>
        /// Replaces the X axis with a new axis, preserving data range
        /// </summary>
        /// <param name="currentAxis">Current X axis</param>
        /// <param name="newAxis">New X axis to replace with</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The new X axis, or current axis if replacement failed</returns>
        public IAxis<double> ReplaceXAxis(IAxis<double> currentAxis, IAxis<double> newAxis, IList<AxisBase> axesList)
        {
            if (newAxis == null)
            {
                return currentAxis;
            }

            // Preserve data range from current axis
            newAxis.DataRange = currentAxis.DataRange;

            // Update axes list (X axis is typically at index 0)
            if (axesList.Count > 0)
            {
                axesList[0] = (AxisBase)newAxis;
            }

            return newAxis;
        }

        /// <summary>
        /// Replaces the Y axis with a new axis, preserving data range
        /// </summary>
        /// <param name="currentAxis">Current Y axis</param>
        /// <param name="newAxis">New Y axis to replace with</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The new Y axis, or current axis if replacement failed</returns>
        public IAxis<double> ReplaceYAxis(IAxis<double> currentAxis, IAxis<double> newAxis, IList<AxisBase> axesList)
        {
            if (newAxis == null)
            {
                return currentAxis;
            }

            // Preserve data range from current axis
            newAxis.DataRange = currentAxis.DataRange;

            // Update axes list (Y axis is typically at index 1)
            if (axesList.Count > 1)
            {
                axesList[1] = (AxisBase)newAxis;
            }

            return newAxis;
        }

        /// <summary>
        /// Replaces the secondary Y axis with a new axis, preserving data range
        /// </summary>
        /// <param name="currentAxis">Current secondary Y axis (can be null)</param>
        /// <param name="newAxis">New secondary Y axis to replace with</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The new secondary Y axis</returns>
        public IAxis<double> ReplaceSecondaryYAxis(IAxis<double>? currentAxis, IAxis<double> newAxis, IList<AxisBase> axesList)
        {
            if (newAxis == null)
            {
                return currentAxis ?? new NumericAxis();
            }

            // Preserve data range from current axis if it exists
            if (currentAxis != null)
            {
                newAxis.DataRange = currentAxis.DataRange;
            }

            // Ensure secondary Y axis exists in list (typically at index 2)
            EnsureSecondaryYAxisInList(axesList);

            if (axesList.Count > 2)
            {
                axesList[2] = (AxisBase)newAxis;
            }

            return newAxis;
        }

        /// <summary>
        /// Ensures a secondary Y axis exists, creating one if necessary
        /// </summary>
        /// <param name="currentSecondaryAxis">Current secondary Y axis (can be null)</param>
        /// <param name="axesList">Mutable list of axes to update</param>
        /// <returns>The secondary Y axis (existing or newly created)</returns>
        public IAxis<double> EnsureSecondaryYAxis(IAxis<double>? currentSecondaryAxis, IList<AxisBase> axesList)
        {
            if (currentSecondaryAxis != null)
            {
                return currentSecondaryAxis;
            }

            var newSecondaryAxis = new NumericAxis();
            EnsureSecondaryYAxisInList(axesList);

            if (axesList.Count > 2)
            {
                axesList[2] = (AxisBase)newSecondaryAxis;
            }
            else
            {
                axesList.Add((AxisBase)newSecondaryAxis);
            }

            return newSecondaryAxis;
        }

        /// <summary>
        /// Updates scales for all provided axes based on pixel dimensions
        /// </summary>
        /// <param name="xAxis">X axis to update</param>
        /// <param name="yAxis">Y axis to update</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to update</param>
        /// <param name="viewport">Viewport providing visible ranges</param>
        /// <param name="widthPx">Width in pixels</param>
        /// <param name="heightPx">Height in pixels</param>
        public void UpdateScales(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            IViewport viewport,
            double widthPx,
            double heightPx)
        {
            // Sync visible ranges from viewport
            xAxis.VisibleRange = viewport.X;
            yAxis.VisibleRange = viewport.Y;

            // Update scales for pixel mapping
            ((AxisBase)xAxis).UpdateScale(0, widthPx);
            ((AxisBase)yAxis).UpdateScale(heightPx, 0);

            if (yAxisSecondary != null)
            {
                yAxisSecondary.VisibleRange = yAxis.VisibleRange;
                ((AxisBase)yAxisSecondary).UpdateScale(heightPx, 0);
            }
        }

        /// <summary>
        /// Clears data ranges for all axes and resets to default values
        /// </summary>
        /// <param name="xAxis">X axis to clear</param>
        /// <param name="yAxis">Y axis to clear</param>
        /// <param name="yAxisSecondary">Optional secondary Y axis to clear</param>
        /// <param name="viewport">Viewport to update with default range</param>
        public void ClearAxisRanges(
            IAxis<double> xAxis,
            IAxis<double> yAxis,
            IAxis<double>? yAxisSecondary,
            IViewport viewport)
        {
            var defaultRange = new FRange(0, 1);

            xAxis.DataRange = defaultRange;
            yAxis.DataRange = defaultRange;

            if (yAxisSecondary != null)
            {
                yAxisSecondary.DataRange = defaultRange;
            }

            viewport.SetVisible(xAxis.DataRange, yAxis.DataRange);
        }

        private static void EnsureSecondaryYAxisInList(IList<AxisBase> axesList)
        {
            // Ensure the axes list has space for secondary Y axis (index 2)
            while (axesList.Count < 3)
            {
                // Fill with placeholder if needed - this shouldn't normally happen
                // but provides safety for the indexing operations
                axesList.Add(new NumericAxis());
            }
        }
    }
}