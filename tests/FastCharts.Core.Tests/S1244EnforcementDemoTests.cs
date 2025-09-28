using FastCharts.Core.Utilities;

using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Demonstrates that S1244 enforcement works and DoubleUtils provides the solution.
/// </summary>
public class S1244EnforcementDemoTests
{
    [Fact]
    public void S1244_EnforcementDemo_DoubleUtilsPreventsViolations()
    {
        // Test the classic floating-point precision issue
        var a = 0.1 + 0.2;
        var b = 0.3;
        
        // Direct comparison would fail: Assert.True(a == b); // ? S1244 ERROR!
        // But our helper works correctly:
        Assert.True(DoubleUtils.AreEqual(a, b)); // ? SAFE
        
        // Test zero comparison
        var closeToZero = 1e-16;
        
        // Direct comparison would fail: Assert.True(closeToZero == 0.0); // ? S1244 ERROR!
        // But our helper works correctly:
        Assert.True(DoubleUtils.IsZero(closeToZero)); // ? SAFE
        
        // Test inequality
        var nonZero = 1.0;
        
        // Direct comparison would fail: Assert.True(nonZero != 0.0); // ? S1244 ERROR!
        // But our helper works correctly:
        Assert.True(DoubleUtils.IsNotZero(nonZero)); // ? SAFE
    }
}