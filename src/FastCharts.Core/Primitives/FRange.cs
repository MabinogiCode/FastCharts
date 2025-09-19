namespace FastCharts.Core.Primitives;

public readonly struct FRange
{
    public double Min { get; }
    public double Max { get; }
    public FRange(double min, double max) { Min = min; Max = max; }
    public double Size => Max - Min;
    public bool Contains(double v) => v >= Min && v <= Max;
    public override string ToString() => $"[{Min}, {Max}]";
}
