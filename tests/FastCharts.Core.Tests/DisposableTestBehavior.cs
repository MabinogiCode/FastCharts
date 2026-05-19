using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Interaction;

namespace FastCharts.Core.Tests;

/// <summary>
/// Test helper disposable behavior for testing purposes
/// </summary>
public class DisposableTestBehavior : IBehavior, IDisposable
{
    public bool IsDisposed { get; private set; }

    public bool OnEvent(IChartModel model, InteractionEvent ev)
    {
        return false;
    }

    public void Dispose()
    {
        IsDisposed = true;
    }
}