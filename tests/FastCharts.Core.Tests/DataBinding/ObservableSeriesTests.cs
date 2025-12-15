using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FastCharts.Core.DataBinding;
using FastCharts.Core.DataBinding.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// Tests for observable series data binding functionality
    /// </summary>
    public class ObservableSeriesTests
    {
        [Fact]
        public void ObservableLineSeries_WithItemsSource_ShouldBindData()
        {
            // Arrange
            var data = new ObservableCollection<SensorReading>
            {
                new() { Time = DateTime.Now, Temperature = 20.5 },
                new() { Time = DateTime.Now.AddMinutes(1), Temperature = 21.0 },
                new() { Time = DateTime.Now.AddMinutes(2), Temperature = 19.8 }
            };

            var series = new ObservableLineSeries(data, nameof(SensorReading.Time), nameof(SensorReading.Temperature))
            {
                Title = "Temperature Data"
            };

            // Act & Assert
            series.Data.Should().HaveCount(3);
            series.IsEmpty.Should().BeFalse();
            series.Data[1].Y.Should().Be(21.0);
        }

        [Fact]
        public void ObservableLineSeries_WhenItemAdded_ShouldAutoUpdate()
        {
            // Arrange
            var data = new ObservableCollection<SensorReading>
            {
                new() { Time = DateTime.Now, Temperature = 20.5 }
            };

            var series = new ObservableLineSeries(data, nameof(SensorReading.Time), nameof(SensorReading.Temperature));

            var eventFired = false;
            series.DataBindingUpdated += (_, _) => eventFired = true;

            // Act
            data.Add(new SensorReading { Time = DateTime.Now.AddMinutes(1), Temperature = 22.0 });

            // Assert
            series.Data.Should().HaveCount(2);
            eventFired.Should().BeTrue();
        }

        [Fact]
        public void ObservableScatterSeries_WithComplexPath_ShouldBindNestedProperties()
        {
            // Arrange
            var data = new ObservableCollection<ComplexDataPoint>
            {
                new() { Location = new Point2D { X = 1.0, Y = 2.0 }, Value = 10 },
                new() { Location = new Point2D { X = 2.0, Y = 3.0 }, Value = 15 }
            };

            var series = new ObservableScatterSeries(data, "Location.X", "Location.Y");

            // Act & Assert
            series.Data.Should().HaveCount(2);
            series.Data[0].X.Should().Be(1.0);
            series.Data[0].Y.Should().Be(2.0);
            series.Data[1].X.Should().Be(2.0);
            series.Data[1].Y.Should().Be(3.0);
        }

        [Fact]
        public void ObservableBarSeries_WithCategoryData_ShouldGenerateCorrectBars()
        {
            // Arrange
            var data = new ObservableCollection<CategoryValue>
            {
                new() { Category = "A", Value = 10 },
                new() { Category = "B", Value = 15 },
                new() { Category = "C", Value = 8 }
            };

            var series = new ObservableBarSeries(data, nameof(CategoryValue.Category), nameof(CategoryValue.Value));

            // Act & Assert
            series.Data.Should().HaveCount(3);
            series.Data[1].Y.Should().Be(15);
        }

        [Fact]
        public void PropertyPathResolver_WithValidPath_ShouldResolveValue()
        {
            // Arrange
            var resolver = ReflectionPropertyPathResolver.Instance;
            var data = new ComplexDataPoint
            {
                Location = new Point2D { X = 5.5, Y = 10.2 },
                Value = 42
            };

            // Act
            var xValue = resolver.GetValue(data, "Location.X");
            var yValue = resolver.GetValue(data, "Location.Y");
            var simpleValue = resolver.GetValue(data, "Value");

            // Assert
            xValue.Should().Be(5.5);
            yValue.Should().Be(10.2);
            simpleValue.Should().Be(42);
        }

        [Fact]
        public void PropertyPathResolver_WithInvalidPath_ShouldReturnNull()
        {
            // Arrange
            var resolver = ReflectionPropertyPathResolver.Instance;
            var data = new SensorReading { Time = DateTime.Now, Temperature = 20.0 };

            // Act
            var invalidValue = resolver.GetValue(data, "NonExistent.Property");

            // Assert
            invalidValue.Should().BeNull();
        }

        [Fact]
        public void DataBindingConverter_WithVariousTypes_ShouldConvertToDouble()
        {
            // Arrange & Act & Assert
            DataBindingConverter.ToDouble(42).Should().Be(42.0);
            DataBindingConverter.ToDouble(3.14f).Should().BeApproximately(3.14, 0.001);
            DataBindingConverter.ToDouble("123.45").Should().Be(123.45);
            DataBindingConverter.ToDouble(true).Should().Be(1.0);
            DataBindingConverter.ToDouble(false).Should().Be(0.0);
            DataBindingConverter.ToDouble(null).Should().Be(double.NaN);
            DataBindingConverter.ToDouble("invalid").Should().Be(double.NaN);
        }

        [Fact]
        public void ObservableLineSeries_WithPropertyChangedItems_ShouldAutoUpdate()
        {
            // Arrange
            var reading = new ObservableSensorReading { Time = DateTime.Now, Temperature = 20.0 };
            var data = new ObservableCollection<ObservableSensorReading> { reading };

            var series = new ObservableLineSeries(data, nameof(ObservableSensorReading.Time), nameof(ObservableSensorReading.Temperature));

            var originalTemperature = series.Data[0].Y;

            // Act
            reading.Temperature = 25.5;

            // Assert
            series.Data[0].Y.Should().Be(25.5);
            series.Data[0].Y.Should().NotBe(originalTemperature);
        }

        [Fact]
        public async Task ObservableLineSeries_WithThrottling_ShouldBatchUpdates()
        {
            // Arrange
            var data = new ObservableCollection<SensorReading>();
            var series = new ObservableLineSeries(data, nameof(SensorReading.Time), nameof(SensorReading.Temperature))
            {
                RefreshThrottle = TimeSpan.FromMilliseconds(50)
            };

            var updateCount = 0;
            series.DataBindingUpdated += (_, _) => updateCount++;

            // Act
            data.Add(new SensorReading { Time = DateTime.Now, Temperature = 20.0 });
            data.Add(new SensorReading { Time = DateTime.Now.AddMinutes(1), Temperature = 21.0 });
            data.Add(new SensorReading { Time = DateTime.Now.AddMinutes(2), Temperature = 22.0 });

            // Wait for throttling
            await Task.Delay(100);

            // Assert
            series.Data.Should().HaveCount(3);
            updateCount.Should().BeGreaterThan(0);
        }
    }
}