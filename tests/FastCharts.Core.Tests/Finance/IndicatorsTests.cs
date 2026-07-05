using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Finance;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Finance
{
    /// <summary>
    /// Tests for financial indicators (v1.3): SMA, EMA, Bollinger Bands
    /// </summary>
    public class IndicatorsTests
    {
        private static IReadOnlyList<PointD> Constant(double value, int count)
        {
            return Enumerable.Range(0, count).Select(i => new PointD(i, value)).ToList();
        }

        [Fact]
        public void Sma_ConstantInput_ReturnsConstantOutput()
        {
            var sma = Indicators.Sma(Constant(42, 10), 3);

            sma.Data.Should().HaveCount(8); // 10 - 3 + 1
            sma.Data.Should().OnlyContain(p => Math.Abs(p.Y - 42) < 1e-9);
            sma.Title.Should().Be("SMA 3");
        }

        [Fact]
        public void Sma_KnownValues_AreCorrect()
        {
            var source = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }
                .Select((v, i) => new PointD(i, v)).ToList();

            var sma = Indicators.Sma(source, 3);

            // Windows: (1+2+3)/3=2, (2+3+4)/3=3, (3+4+5)/3=4
            sma.Data.Select(p => p.Y).Should().ContainInOrder(2.0, 3.0, 4.0);
            sma.Data[0].X.Should().Be(2); // aligned on last point of window
        }

        [Fact]
        public void Ema_KnownValues_AreCorrect()
        {
            var source = new[] { 2.0, 4.0, 6.0, 8.0 }
                .Select((v, i) => new PointD(i, v)).ToList();

            var ema = Indicators.Ema(source, 2);

            // Seed = SMA(2,4) = 3 ; alpha = 2/3
            // ema(6) = (6-3)*2/3 + 3 = 5 ; ema(8) = (8-5)*2/3 + 5 = 7
            ema.Data.Should().HaveCount(3);
            ema.Data[0].Y.Should().BeApproximately(3.0, 1e-9);
            ema.Data[1].Y.Should().BeApproximately(5.0, 1e-9);
            ema.Data[2].Y.Should().BeApproximately(7.0, 1e-9);
        }

        [Fact]
        public void Ema_ShorterThanPeriod_ReturnsEmptySeries()
        {
            var ema = Indicators.Ema(Constant(1, 3), 5);

            ema.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void BollingerBands_ConstantInput_CollapsesToMiddle()
        {
            var bb = Indicators.BollingerBands(Constant(10, 30), 20, 2);

            bb.Middle.Data.Should().HaveCount(11);
            bb.Band.Data.Should().HaveCount(11);
            bb.Middle.Data.Should().OnlyContain(p => Math.Abs(p.Y - 10) < 1e-9);
            bb.Band.Data.Should().OnlyContain(b => Math.Abs(b.YHigh - b.YLow) < 1e-9);
        }

        [Fact]
        public void BollingerBands_VaryingInput_EnvelopeContainsMiddle()
        {
            var rng = new Random(42);
            var source = Enumerable.Range(0, 100)
                .Select(i => new PointD(i, 50 + rng.NextDouble() * 10)).ToList();

            var bb = Indicators.BollingerBands(source, 20, 2);

            for (var i = 0; i < bb.Middle.Data.Count; i++)
            {
                bb.Band.Data[i].YLow.Should().BeLessThan(bb.Middle.Data[i].Y);
                bb.Band.Data[i].YHigh.Should().BeGreaterThan(bb.Middle.Data[i].Y);
                bb.Band.Data[i].X.Should().Be(bb.Middle.Data[i].X);
            }
        }

        [Fact]
        public void Indicators_FromOhlcSeries_UseClosePrices()
        {
            var ohlc = new OhlcSeries(Enumerable.Range(0, 10)
                .Select(i => new OhlcPoint(i, 1, 2, 0.5, 7.0)));

            var sma = Indicators.Sma(ohlc, 5);

            sma.Data.Should().OnlyContain(p => Math.Abs(p.Y - 7.0) < 1e-9);
        }

        [Fact]
        public void Indicators_InvalidArguments_Throw()
        {
            FluentActions.Invoking(() => Indicators.Sma((IReadOnlyList<PointD>)null!, 3)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => Indicators.Sma(Constant(1, 5), 0)).Should().Throw<ArgumentOutOfRangeException>();
            FluentActions.Invoking(() => Indicators.Ema(Constant(1, 5), -1)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Indicators_AreRenderableSeries()
        {
            var sma = Indicators.Sma(Constant(5, 20), 4);
            var bb = Indicators.BollingerBands(Constant(5, 30), 10);

            sma.Should().BeAssignableTo<SeriesBase>();
            bb.Middle.Should().BeAssignableTo<SeriesBase>();
            bb.Band.Should().BeAssignableTo<SeriesBase>();
        }
    }
}
