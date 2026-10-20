using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Heatmap;

/// <summary>
/// The multi-series tooltip reports the value of the heatmap cell under the cursor.
/// </summary>
public class HeatmapTooltipTests
{
    private static readonly double[,] Grid =
    {
        { 0, 1 },
        { 2, 3 },
    };

    [Fact]
    public void TooltipShowsTheCellValue()
    {
        // Arrange
        using var model = new ChartModel();
        model.AddHeatmap(Grid, title: "Field");

        // Act
        var st = MoveTo(model, 1.5, 1.5);

        // Assert
        st.TooltipSeries.Should().ContainSingle();
        st.TooltipSeries[0].Title.Should().Be("Field");
        st.TooltipSeries[0].Y.Should().Be(3);
        st.TooltipText.Should().Contain("Field: 3");
    }

    [Fact]
    public void TooltipSkipsMissingCellsAndPointsOutsideTheGrid()
    {
        // Arrange
        using var model = new ChartModel();
        var values = (double[,])Grid.Clone();
        values[0, 0] = double.NaN;
        model.AddHeatmap(values);

        // Act / Assert
        MoveTo(model, 0.5, 0.5).TooltipText.Should().BeNull();
        MoveTo(model, 5, 5).TooltipText.Should().BeNull();
    }

    [Fact]
    public void SecondaryAxisHeatmapConvertsTheCursorY()
    {
        // Arrange: primary Y over [0, 10], heatmap rows over [100, 200] on the secondary axis
        using var model = new ChartModel();
        model.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(2, 10) }));
        model.AddSeries(new HeatmapSeries(Grid, new FRange(0, 2), new FRange(100, 200)) { Title = "Y2 map", YAxisIndex = 1 });
        model.XAxis.VisibleRange = new FRange(0, 2);
        model.YAxis.VisibleRange = new FRange(0, 10);
        model.YAxisSecondary!.VisibleRange = new FRange(100, 200);

        // Act: 7.5 on the primary axis is 175 on the secondary one → top row
        var st = MoveTo(model, 0.5, 7.5);

        // Assert
        st.TooltipSeries.Should().Contain(v => v.Title == "Y2 map" && v.Y == 2);
    }

    private static InteractionState MoveTo(ChartModel model, double x, double y)
    {
        var st = model.InteractionState ??= new InteractionState();
        st.DataX = x;
        st.DataY = y;
        new MultiSeriesTooltipBehavior().OnEvent(model, new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 100, 100, 0, 400, 300));
        return st;
    }
}
