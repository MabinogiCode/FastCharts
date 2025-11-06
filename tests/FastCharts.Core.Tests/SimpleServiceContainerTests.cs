using System;
using FastCharts.Core.DependencyInjection;
using FastCharts.Core.Services;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests
{
    /// <summary>
    /// Tests for SimpleServiceContainer
    /// </summary>
    public class SimpleServiceContainerTests
    {
        private readonly SimpleServiceContainer _container;

        public SimpleServiceContainerTests()
        {
            _container = new SimpleServiceContainer();
        }

        [Fact]
        public void RegisterSingletonAndResolveReturnsSameInstance()
        {
            // Arrange
            _container.RegisterSingleton<IDataRangeCalculatorService, DataRangeCalculatorService>();

            // Act
            var instance1 = _container.Resolve<IDataRangeCalculatorService>();
            var instance2 = _container.Resolve<IDataRangeCalculatorService>();

            // Assert
            instance1.Should().NotBeNull();
            instance2.Should().NotBeNull();
            instance1.Should().BeSameAs(instance2);
        }

        [Fact]
        public void RegisterTransientAndResolveReturnsDifferentInstances()
        {
            // Arrange
            _container.RegisterTransient<IDataRangeCalculatorService, DataRangeCalculatorService>();

            // Act
            var instance1 = _container.Resolve<IDataRangeCalculatorService>();
            var instance2 = _container.Resolve<IDataRangeCalculatorService>();

            // Assert
            instance1.Should().NotBeNull();
            instance2.Should().NotBeNull();
            instance1.Should().NotBeSameAs(instance2);
        }

        [Fact]
        public void RegisterInstanceAndResolveReturnsExactInstance()
        {
            // Arrange
            var service = new DataRangeCalculatorService();
            _container.RegisterInstance<IDataRangeCalculatorService>(service);

            // Act
            var resolved = _container.Resolve<IDataRangeCalculatorService>();

            // Assert
            resolved.Should().BeSameAs(service);
        }

        [Fact]
        public void RegisterInstanceWithNullThrowsArgumentNullException()
        {
            // Act & Assert
            Action act = () => _container.RegisterInstance<IDataRangeCalculatorService>(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ResolveUnregisteredServiceThrowsInvalidOperationException()
        {
            // Act & Assert
            Action act = () => _container.Resolve<IDataRangeCalculatorService>();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*IDataRangeCalculatorService*not registered*");
        }

        [Fact]
        public void ResolveWithDependenciesResolvesCorrectly()
        {
            // Arrange
            _container.RegisterSingleton<IDataRangeCalculatorService, DataRangeCalculatorService>();
            _container.RegisterTransient<IBehaviorManager, BehaviorManager>();

            // Act
            var behaviorManager = _container.Resolve<IBehaviorManager>();

            // Assert
            behaviorManager.Should().NotBeNull();
            behaviorManager.Should().BeOfType<BehaviorManager>();
        }
    }
}