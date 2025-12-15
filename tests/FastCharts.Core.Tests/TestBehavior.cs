using FastCharts.Core.Interaction;

namespace FastCharts.Core.Tests;

/// <summary>
/// Test helper behavior for testing purposes
/// </summary>
public class TestBehavior : IBehavior
{
    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        return false;
    }
}