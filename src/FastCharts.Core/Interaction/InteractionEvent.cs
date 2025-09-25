namespace FastCharts.Core.Interaction
{
    /// <summary>
    /// UI-agnostic pointer event routed to behaviors. Coordinates are in SURFACE pixels (control coords).
    /// Pixel→data conversions are done by the host control/renderer when needed.
    /// </summary>
    public struct InteractionEvent
    {
        public PointerEventType Type;
        public PointerButton Button;
        public PointerModifiers Modifiers;

        public double PixelX;
        public double PixelY;

        /// <summary>Total surface width/height in pixels for conversions.</summary>
        public double SurfaceWidth;
        public double SurfaceHeight;

        /// <summary>Wheel delta in logical steps (positive = zoom in), 0 otherwise.</summary>
        public double WheelDelta;

        public InteractionEvent(
            PointerEventType type,
            PointerButton button,
            PointerModifiers modifiers,
            double pixelX,
            double pixelY,
            double wheelDelta = 0,
            double surfaceWidth = 0,
            double surfaceHeight = 0)
        {
            Type = type;
            Button = button;
            Modifiers = modifiers;
            PixelX = pixelX;
            PixelY = pixelY;
            WheelDelta = wheelDelta;
            SurfaceWidth = surfaceWidth;
            SurfaceHeight = surfaceHeight;
        }
    }
}
