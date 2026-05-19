namespace FastCharts.Tests.Helpers;

/// <summary>
/// Outcome of a visual-regression comparison between a render and its baseline.
/// </summary>
internal readonly struct VisualComparisonResult
{
    public VisualComparisonResult(bool isMatch, bool baselineCreated, double differingFraction)
    {
        IsMatch = isMatch;
        BaselineCreated = baselineCreated;
        DifferingFraction = differingFraction;
    }

    /// <summary>Gets a value indicating whether the render matches the baseline within tolerance.</summary>
    public bool IsMatch { get; }

    /// <summary>Gets a value indicating whether no baseline existed and one was created from this render.</summary>
    public bool BaselineCreated { get; }

    /// <summary>Gets the fraction (0-1) of pixels that differ from the baseline beyond tolerance.</summary>
    public double DifferingFraction { get; }
}
