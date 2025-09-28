namespace FastCharts.Core.Interaction
{
    /// <summary>
    /// A behavior consumes interaction events and may update the model's InteractionState.
    /// Returns true if the event was handled and a redraw is desired.
    /// </summary>
    public interface IBehavior
    {
        bool OnEvent(ChartModel model, InteractionEvent ev);
    }
}
