namespace FastCharts.Core.Interaction.Behaviors;

/// <summary>
/// Running best match for <see cref="NearestPointBehavior"/>.
/// </summary>
internal struct NearestCandidate
{
    public bool Found { get; private set; }

    public double DistanceSquared { get; private set; }

    public double X { get; private set; }

    public double Y { get; private set; }

    public int AxisIndex { get; private set; }

    public void Consider(double distanceSquared, double x, double y, int axisIndex)
    {
        if (double.IsInfinity(distanceSquared) || double.IsNaN(distanceSquared))
        {
            return;
        }

        if (Found && distanceSquared >= DistanceSquared)
        {
            return;
        }

        Found = true;
        DistanceSquared = distanceSquared;
        X = x;
        Y = y;
        AxisIndex = axisIndex;
    }
}
