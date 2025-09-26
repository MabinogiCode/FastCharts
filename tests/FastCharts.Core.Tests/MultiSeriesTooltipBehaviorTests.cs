using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using Xunit;

namespace FastCharts.Core.Tests
{
    public class MultiSeriesTooltipBehaviorTests
    {
        private static (ChartModel model, InteractionState st) CreateModel()
        {
            var m = new ChartModel();
            m.Series.Add(new LineSeries(new []
            {
                new PointD(0, 0), new PointD(1, 10), new PointD(2, 20)
            }) { Title = "L1" });
            m.Series.Add(new ScatterSeries(new []
            {
                new PointD(0, 5), new PointD(1, 6), new PointD(2, 7)
            }) { Title = "S1" });
            m.AutoFitDataRange();
            m.UpdateScales(300, 200);
            m.InteractionState = new InteractionState();
            return (m, m.InteractionState);
        }

        [Fact]
        public void MoveEventPopulatesTooltipSeries()
        {
            var (m, st) = CreateModel();
            var cross = new CrosshairBehavior();
            var multi = new MultiSeriesTooltipBehavior { XSnapToleranceFraction = 0.2 };
            // Simulate cursor near X=1
            st.DataX = 1.0; st.DataY = 10; // host sets coords
            cross.OnEvent(m, new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 150, 100, 0, 300, 200));
            multi.OnEvent(m, new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 150, 100, 0, 300, 200));
            Assert.NotNull(st.TooltipText);
            Assert.True(st.TooltipSeries.Count >= 2);
            Assert.Contains(st.TooltipSeries, v => v.Title == "L1");
            Assert.Contains(st.TooltipSeries, v => v.Title == "S1");
        }

        [Fact]
        public void LockOnClickPreservesValues()
        {
            var (m, st) = CreateModel();
            var cross = new CrosshairBehavior();
            var multi = new MultiSeriesTooltipBehavior { XSnapToleranceFraction = 0.2 };
            st.DataX = 2.0; st.DataY = 20;
            cross.OnEvent(m, new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 200, 100, 0, 300, 200));
            multi.OnEvent(m, new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 200, 100, 0, 300, 200));
            var before = st.TooltipSeries.Select(v => (v.Title, v.Y)).ToArray();
            // Click to lock
            multi.OnEvent(m, new InteractionEvent(PointerEventType.Down, PointerButton.Left, new PointerModifiers(), 200, 100, 0, 300, 200));
            Assert.True(st.TooltipLocked);
            // Move elsewhere with different DataX - should not rebuild
            st.DataX = 0.0;
            multi.OnEvent(m, new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 10, 100, 0, 300, 200));
            var after = st.TooltipSeries.Select(v => (v.Title, v.Y)).ToArray();
            Assert.Equal(before, after);
        }
    }
}
