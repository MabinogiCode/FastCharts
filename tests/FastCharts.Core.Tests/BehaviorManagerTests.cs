using System.Collections.Generic;
using FastCharts.Core.Services;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests
{
    /// <summary>
    /// Tests for BehaviorManager
    /// </summary>
    public class BehaviorManagerTests
    {
        private readonly BehaviorManager _behaviorManager;

        public BehaviorManagerTests()
        {
            _behaviorManager = new BehaviorManager();
        }

        [Fact]
        public void ConfigureDefaultBehaviorsWithEmptyListAddsAllDefaultBehaviors()
        {
            // Arrange
            var behaviors = new List<IBehavior>();

            // Act
            _behaviorManager.ConfigureDefaultBehaviors(behaviors);

            // Assert
            behaviors.Should().HaveCount(7);
            behaviors.Should().ContainSingle(b => b is CrosshairBehavior);
            behaviors.Should().ContainSingle(b => b is MultiSeriesTooltipBehavior);
            behaviors.Should().ContainSingle(b => b is ZoomRectBehavior);
            behaviors.Should().ContainSingle(b => b is NearestPointBehavior);
            behaviors.Should().ContainSingle(b => b is LegendToggleBehavior);
            behaviors.Should().ContainSingle(b => b is ZoomWheelBehavior);
            behaviors.Should().ContainSingle(b => b is PanBehavior);
        }

        [Fact]
        public void ConfigureDefaultBehaviorsWithNonEmptyListDoesNotAddBehaviors()
        {
            // Arrange
            var behaviors = new List<IBehavior> { new CrosshairBehavior() };

            // Act
            _behaviorManager.ConfigureDefaultBehaviors(behaviors);

            // Assert
            behaviors.Should().HaveCount(1); // Should remain unchanged
        }

        [Fact]
        public void EnsureBehaviorTypeWithoutExistingTypeAddsBehavior()
        {
            // Arrange
            var behaviors = new List<IBehavior>();

            // Act
            _behaviorManager.EnsureBehaviorType<CrosshairBehavior>(behaviors);

            // Assert
            behaviors.Should().HaveCount(1);
            behaviors.Should().ContainSingle(b => b is CrosshairBehavior);
        }

        [Fact]
        public void EnsureBehaviorTypeWithExistingTypeDoesNotAddDuplicate()
        {
            // Arrange
            var behaviors = new List<IBehavior> { new CrosshairBehavior() };

            // Act
            _behaviorManager.EnsureBehaviorType<CrosshairBehavior>(behaviors);

            // Assert
            behaviors.Should().HaveCount(1); // Should not duplicate
        }

        [Fact]
        public void RemoveBehaviorTypeWithExistingTypeRemovesBehavior()
        {
            // Arrange
            var behaviors = new List<IBehavior> 
            { 
                new CrosshairBehavior(),
                new ZoomWheelBehavior(),
                new CrosshairBehavior() // Duplicate to test removal of all instances
            };

            // Act
            _behaviorManager.RemoveBehaviorType<CrosshairBehavior>(behaviors);

            // Assert
            behaviors.Should().HaveCount(1);
            behaviors.Should().NotContain(b => b is CrosshairBehavior);
            behaviors.Should().ContainSingle(b => b is ZoomWheelBehavior);
        }

        [Fact]
        public void RemoveBehaviorTypeWithoutExistingTypeDoesNothing()
        {
            // Arrange
            var behaviors = new List<IBehavior> { new ZoomWheelBehavior() };

            // Act
            _behaviorManager.RemoveBehaviorType<CrosshairBehavior>(behaviors);

            // Assert
            behaviors.Should().HaveCount(1);
            behaviors.Should().ContainSingle(b => b is ZoomWheelBehavior);
        }
    }
}