namespace FastCharts.Core.Interaction;

public struct InteractionEvent
{
    public PointerEventType Type { get; set; }
    public PointerButton Button { get; set; }
    public PointerModifiers Modifiers { get; set; }
    public double PixelX { get; set; }
    public double PixelY { get; set; }
    public double SurfaceWidth { get; set; }
    public double SurfaceHeight { get; set; }
    public double WheelDelta { get; set; }
    
    /// <summary>
    /// Key pressed for keyboard events (P1-METRICS support)
    /// </summary>
    public string? Key { get; set; }

    public InteractionEvent(PointerEventType type, PointerButton button, PointerModifiers modifiers, double pixelX, double pixelY, double wheelDelta = 0, double surfaceWidth = 0, double surfaceHeight = 0, string? key = null)
    {
        Type = type;
        Button = button;
        Modifiers = modifiers;
        PixelX = pixelX;
        PixelY = pixelY;
        WheelDelta = wheelDelta;
        SurfaceWidth = surfaceWidth;
        SurfaceHeight = surfaceHeight;
        Key = key;
    }
}
