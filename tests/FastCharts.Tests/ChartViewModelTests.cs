using System;
using System.Windows.Input;
using FastCharts.Core;
using FastCharts.Core.Series;
using FastCharts.Core.Primitives;
using FastCharts.Wpf.ViewModels;
using FluentAssertions;
using Xunit;

namespace FastCharts.Tests
{
    /// <summary>
    /// Tests pour ChartViewModel
    /// </summary>
    public class ChartViewModelTests
    {
        [Fact]
        public void Constructor_WithNullModel_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => new ChartViewModel(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithValidModel_InitializesCorrectly()
        {
            // Arrange
            var model = new ChartModel();

            // Act
            var viewModel = new ChartViewModel(model);

            // Assert
            viewModel.Model.Should().Be(model);
            viewModel.IsInitialized.Should().BeFalse();
            viewModel.InitializeCommand.Should().NotBeNull();
            viewModel.AutoFitCommand.Should().NotBeNull();
        }

        [Fact]
        public void Initialize_FirstTime_ConfiguresBehaviorsAndSetsFlag()
        {
            // Arrange
            var model = new ChartModel();
            var viewModel = new ChartViewModel(model);

            // Act
            viewModel.InitializeCommand.Execute(null);

            // Assert
            viewModel.IsInitialized.Should().BeTrue();
            model.Behaviors.Should().HaveCount(7); // All default behaviors
        }

        [Fact]
        public void Initialize_SecondTime_DoesNotReconfigureBehaviors()
        {
            // Arrange
            var model = new ChartModel();
            var viewModel = new ChartViewModel(model);

            // Act
            viewModel.InitializeCommand.Execute(null);
            var behaviorCountAfterFirst = model.Behaviors.Count;
            viewModel.InitializeCommand.Execute(null);

            // Assert
            model.Behaviors.Should().HaveCount(behaviorCountAfterFirst); // Should not change
        }

        [Fact]
        public void AutoFitCommand_ExecutesModelAutoFit()
        {
            // Arrange
            var model = new ChartModel();
            // Capture original range BEFORE adding series (AddSeries triggers auto-fit internally)
            var originalXRange = model.XAxis.DataRange;
            model.AddSeries(new LineSeries(new[]
            {
                new PointD(0, 0),
                new PointD(10, 20)
            }));
            var viewModel = new ChartViewModel(model);

            // Act
            viewModel.AutoFitCommand.Execute(null);

            // Assert
            model.XAxis.DataRange.Should().NotBe(originalXRange); // Range should change from initial default
            model.XAxis.DataRange.Min.Should().Be(0);
            model.XAxis.DataRange.Max.Should().Be(10);
        }

        [Fact]
        public void Model_PropertyChanged_RaisesNotification()
        {
            // Arrange
            var originalModel = new ChartModel();
            var newModel = new ChartModel { Title = "New Chart" };
            var viewModel = new ChartViewModel(originalModel);

            var propertyChanged = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ChartViewModel.Model))
                {
                    propertyChanged = true;
                }
            };

            // Act
            viewModel.Model = newModel;

            // Assert
            propertyChanged.Should().BeTrue();
            viewModel.Model.Should().Be(newModel);
        }
    }
}