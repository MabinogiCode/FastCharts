using FastCharts.Core.Abstractions;
using FastCharts.Core.Interaction;

namespace FastCharts.Core.Tests;

/// <summary>
/// Test helper behavior for testing purposes
/// </summary>
public class TestBehavior : IBehavior
{
    public bool OnEvent(IChartModel model, InteractionEvent ev)
    {
        return false;
    }
}