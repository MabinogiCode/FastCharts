using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Resampling;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Resampling;

public class LttbLastBucketTests
{
    [Fact]
    public void SecondToLastPointIsACandidateOfTheLastBucket()
    {
        // Arrange: 10 points, 4 kept -> buckets [1, 5) and [5, 9); the spike sits at index 8
        var data = Enumerable.Range(0, 10).Select(i => new PointD(i, i == 8 ? 100 : 0)).ToList();

        // Act
        var result = new LttbResampler().Resample(data, 4);

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(new PointD(8, 100));
        result[0].Should().Be(data[0]);
        result[result.Count - 1].Should().Be(data[9]);
    }
}
