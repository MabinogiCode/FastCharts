using FastCharts.Core;
using FastCharts.Core.Interactivity;
using FastCharts.Core.Primitives;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Interaction
{
    /// <summary>
    /// Tests for cross-chart X axis linking (v1.3, P2-AX-LINK)
    /// </summary>
    public class ChartLinkGroupTests
    {
        [Fact]
        public void LinkedCharts_ShareXRange_WhenOneZooms()
        {
            using var price = new ChartModel();
            using var indicator = new ChartModel();
            using var link = new ChartLinkGroup();
            link.Add(price);
            link.Add(indicator);

            price.XAxis.VisibleRange = new FRange(10, 20);

            indicator.XAxis.VisibleRange.Min.Should().Be(10);
            indicator.XAxis.VisibleRange.Max.Should().Be(20);
        }

        [Fact]
        public void LinkedCharts_SyncBothDirections()
        {
            using var a = new ChartModel();
            using var b = new ChartModel();
            using var link = new ChartLinkGroup();
            link.Add(a);
            link.Add(b);

            b.XAxis.VisibleRange = new FRange(-5, 5);

            a.XAxis.VisibleRange.Min.Should().Be(-5);
            a.XAxis.VisibleRange.Max.Should().Be(5);
        }

        [Fact]
        public void NewMember_AdoptsGroupRange()
        {
            using var a = new ChartModel();
            using var b = new ChartModel();
            using var link = new ChartLinkGroup();
            link.Add(a);
            a.XAxis.VisibleRange = new FRange(100, 200);

            link.Add(b);

            b.XAxis.VisibleRange.Min.Should().Be(100);
            b.XAxis.VisibleRange.Max.Should().Be(200);
        }

        [Fact]
        public void RemovedChart_StopsSyncing()
        {
            using var a = new ChartModel();
            using var b = new ChartModel();
            using var link = new ChartLinkGroup();
            link.Add(a);
            link.Add(b);

            link.Remove(b).Should().BeTrue();
            a.XAxis.VisibleRange = new FRange(1, 2);

            b.XAxis.VisibleRange.Max.Should().NotBe(2);
        }

        [Fact]
        public void PanOperation_PropagatesThroughLink()
        {
            using var a = new ChartModel();
            using var b = new ChartModel();
            using var link = new ChartLinkGroup();
            link.Add(a);
            link.Add(b);
            a.XAxis.VisibleRange = new FRange(0, 10);

            a.Pan(5, 0); // shift X by +5

            b.XAxis.VisibleRange.Min.Should().Be(5);
            b.XAxis.VisibleRange.Max.Should().Be(15);
        }

        [Fact]
        public void LogAxisSwitch_KeepsLinkAlive()
        {
            using var a = new ChartModel();
            using var b = new ChartModel();
            using var link = new ChartLinkGroup();
            link.Add(a);
            link.Add(b);

            a.XAxis.VisibleRange = new FRange(1, 100);
            a.SetXAxisLogarithmic(); // replaces the X axis instance

            a.XAxis.VisibleRange = new FRange(1, 50);
            b.XAxis.VisibleRange.Max.Should().Be(50);
        }

        [Fact]
        public void Dispose_UnlinksEverything()
        {
            using var a = new ChartModel();
            using var b = new ChartModel();
            var link = new ChartLinkGroup();
            link.Add(a);
            link.Add(b);

            link.Dispose();
            a.XAxis.VisibleRange = new FRange(7, 8);

            b.XAxis.VisibleRange.Max.Should().NotBe(8);
        }
    }
}
