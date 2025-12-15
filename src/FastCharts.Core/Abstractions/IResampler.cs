using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Interface for data resampling/decimation algorithms
    /// Used to reduce dataset size while preserving visual characteristics
    /// </summary>
    public interface IResampler
    {
        /// <summary>
        /// Resamples a dataset to the specified target count
        /// </summary>
        /// <param name="data">Original data points</param>
        /// <param name="targetCount">Desired number of points after resampling</param>
        /// <returns>Resampled data points</returns>
        IReadOnlyList<PointD> Resample(IReadOnlyList<PointD> data, int targetCount);

        /// <summary>
        /// Gets the name of this resampling algorithm
        /// </summary>
        string AlgorithmName { get; }

        /// <summary>
        /// Gets whether this algorithm preserves visual shape characteristics
        /// </summary>
        bool PreservesShape { get; }

        /// <summary>
        /// Gets the optimal performance range for this algorithm
        /// </summary>
        (int MinPoints, int MaxPoints) OptimalRange { get; }
    }
}