using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Helpers;
using FastCharts.Core.Primitives;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Axes;

/// <summary>
/// Maps data values to normalized axis positions (0 = range minimum, 1 = range maximum) and back,
/// honoring logarithmic axes. Shared by renderers and interaction behaviors so that drawing,
/// zooming, panning and hit-testing all use the same transform.
/// </summary>
public static class AxisCoordinates
{
    /// <summary>
    /// Smallest value considered positive on a logarithmic axis; non-positive values are clamped to it.
    /// </summary>
    public const double MinLogValue = LogNumericAxis.MinPositiveValue;

    /// <summary>
    /// Returns true when the axis maps values logarithmically, along with its logarithm base.
    /// </summary>
    public static bool IsLogarithmic(IAxis<double> axis, out double logBase)
    {
        switch (axis)
        {
            case LogNumericAxis logNumeric:
                {
                    logBase = logNumeric.LogBase;
                    return true;
                }
            case LogarithmicAxis logarithmic:
                {
                    logBase = logarithmic.LogBase;
                    return true;
                }
            default:
                {
                    logBase = 0;
                    return false;
                }
        }
    }

    /// <summary>
    /// Normalized position of <paramref name="value"/> within the axis' visible range.
    /// </summary>
    public static double ToNormalized(IAxis<double> axis, double value)
    {
        return ToNormalized(axis, axis.VisibleRange, value);
    }

    /// <summary>
    /// Normalized position of <paramref name="value"/> within <paramref name="range"/>,
    /// using the axis' transform (linear or logarithmic). Values outside the range map outside [0, 1].
    /// </summary>
    public static double ToNormalized(IAxis<double> axis, FRange range, double value)
    {
        if (IsLogarithmic(axis, out _))
        {
            var logMin = Math.Log(ClampLog(range.Min));
            var logMax = Math.Log(ClampLog(Math.Max(range.Max, range.Min)));
            var logSpan = logMax - logMin;
            if (logSpan <= 0 || double.IsNaN(logSpan))
            {
                return 0;
            }

            return (Math.Log(ClampLog(value)) - logMin) / logSpan;
        }

        var span = range.Max - range.Min;
        if (DoubleUtils.IsZero(span) || double.IsNaN(span))
        {
            return 0;
        }

        return (value - range.Min) / span;
    }

    /// <summary>
    /// Data value at normalized position <paramref name="t"/> of the axis' visible range.
    /// </summary>
    public static double FromNormalized(IAxis<double> axis, double t)
    {
        return FromNormalized(axis, axis.VisibleRange, t);
    }

    /// <summary>
    /// Data value at normalized position <paramref name="t"/> of <paramref name="range"/>,
    /// using the axis' transform (linear or logarithmic).
    /// </summary>
    public static double FromNormalized(IAxis<double> axis, FRange range, double t)
    {
        if (IsLogarithmic(axis, out _))
        {
            var logMin = Math.Log(ClampLog(range.Min));
            var logMax = Math.Log(ClampLog(Math.Max(range.Max, range.Min)));
            return Math.Exp(logMin + (t * (logMax - logMin)));
        }

        return range.Min + (t * (range.Max - range.Min));
    }

    /// <summary>
    /// Range covering the normalized interval [<paramref name="t0"/>, <paramref name="t1"/>]
    /// of <paramref name="range"/>.
    /// </summary>
    public static FRange SubRange(IAxis<double> axis, FRange range, double t0, double t1)
    {
        return new FRange(FromNormalized(axis, range, t0), FromNormalized(axis, range, t1));
    }

    /// <summary>
    /// Applies to <paramref name="follower"/> the same relative zoom/pan that moved
    /// <paramref name="leader"/> from <paramref name="oldLeaderRange"/> to <paramref name="newLeaderRange"/>.
    /// Used to keep a secondary Y axis aligned with the primary one while preserving its own scale.
    /// </summary>
    public static void FollowRelative(IAxis<double> leader, FRange oldLeaderRange, FRange newLeaderRange, IAxis<double> follower)
    {
        if (!ValidationHelper.IsValidRange(oldLeaderRange))
        {
            return;
        }

        var t0 = ToNormalized(leader, oldLeaderRange, newLeaderRange.Min);
        var t1 = ToNormalized(leader, oldLeaderRange, newLeaderRange.Max);
        if (double.IsNaN(t0) || double.IsNaN(t1) || double.IsInfinity(t0) || double.IsInfinity(t1))
        {
            return;
        }

        var followerRange = follower.VisibleRange;
        if (!ValidationHelper.IsValidRange(followerRange))
        {
            return;
        }

        follower.VisibleRange = SubRange(follower, followerRange, t0, t1);
    }

    private static double ClampLog(double value)
    {
        return value > MinLogValue ? value : MinLogValue;
    }
}
