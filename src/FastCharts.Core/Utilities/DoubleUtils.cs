using System;

namespace FastCharts.Core.Utilities;

/// <summary>
/// Helper class for safe floating-point comparisons to avoid S1244 violations.
/// Provides methods to compare double values using appropriate epsilon values.
/// </summary>
public static class DoubleUtils
{
    /// <summary>
    /// Default epsilon value for double comparisons.
    /// Uses machine epsilon scaled by typical magnitude.
    /// </summary>
    public const double DefaultEpsilon = 1e-15;

    /// <summary>
    /// Determines if two double values are approximately equal using the default epsilon.
    /// </summary>
    /// <param name="a">First value to compare.</param>
    /// <param name="b">Second value to compare.</param>
    /// <returns>True if the values are approximately equal, false otherwise.</returns>
    public static bool AreEqual(double a, double b)
    {
        return AreEqual(a, b, DefaultEpsilon);
    }

    /// <summary>
    /// Determines if two double values are approximately equal using a specified epsilon.
    /// </summary>
    /// <param name="a">First value to compare.</param>
    /// <param name="b">Second value to compare.</param>
    /// <param name="epsilon">The tolerance for comparison.</param>
    /// <returns>True if the values are approximately equal, false otherwise.</returns>
    public static bool AreEqual(double a, double b, double epsilon)
    {
        // Handle special cases
        if (double.IsNaN(a) || double.IsNaN(b))
        {
            return double.IsNaN(a) && double.IsNaN(b);
        }

        if (double.IsInfinity(a) || double.IsInfinity(b))
        {
            return a.Equals(b);
        }

        // Use absolute difference for comparison
        return Math.Abs(a - b) <= epsilon;
    }

    /// <summary>
    /// Determines if a double value is approximately zero using the default epsilon.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is approximately zero, false otherwise.</returns>
    public static bool IsZero(double value)
    {
        return IsZero(value, DefaultEpsilon);
    }

    /// <summary>
    /// Determines if a double value is approximately zero using a specified epsilon.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="epsilon">The tolerance for comparison.</param>
    /// <returns>True if the value is approximately zero, false otherwise.</returns>
    public static bool IsZero(double value, double epsilon)
    {
        return Math.Abs(value) <= epsilon;
    }

    /// <summary>
    /// Determines if two double values are not approximately equal using the default epsilon.
    /// </summary>
    /// <param name="a">First value to compare.</param>
    /// <param name="b">Second value to compare.</param>
    /// <returns>True if the values are not approximately equal, false otherwise.</returns>
    public static bool AreNotEqual(double a, double b)
    {
        return !AreEqual(a, b, DefaultEpsilon);
    }

    /// <summary>
    /// Determines if two double values are not approximately equal using a specified epsilon.
    /// </summary>
    /// <param name="a">First value to compare.</param>
    /// <param name="b">Second value to compare.</param>
    /// <param name="epsilon">The tolerance for comparison.</param>
    /// <returns>True if the values are not approximately equal, false otherwise.</returns>
    public static bool AreNotEqual(double a, double b, double epsilon)
    {
        return !AreEqual(a, b, epsilon);
    }

    /// <summary>
    /// Determines if a double value is not approximately zero using the default epsilon.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is not approximately zero, false otherwise.</returns>
    public static bool IsNotZero(double value)
    {
        return !IsZero(value, DefaultEpsilon);
    }

    /// <summary>
    /// Determines if a double value is not approximately zero using a specified epsilon.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="epsilon">The tolerance for comparison.</param>
    /// <returns>True if the value is not approximately zero, false otherwise.</returns>
    public static bool IsNotZero(double value, double epsilon)
    {
        return !IsZero(value, epsilon);
    }
}