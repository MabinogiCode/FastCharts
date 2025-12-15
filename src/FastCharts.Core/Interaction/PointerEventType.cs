namespace FastCharts.Core.Interaction;

public enum PointerEventType
{
    None,
    Move,
    Down,
    Up,
    Leave,
    Wheel,
    /// <summary>
    /// Keyboard key down event (P1-METRICS support)
    /// </summary>
    KeyDown
}
