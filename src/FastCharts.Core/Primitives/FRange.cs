namespace FastCharts.Core.Primitives;

public readonly struct FRange
{
    public FRange(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public double Min { get; }
    public double Max { get; }
    public double Size => Max - Min;

    public bool Contains(double v)
    {
        return v >= Min && v <= Max;
    }

    public override string ToString()
    {
        return $"[{Min}, {Max}]";
    }
}
