using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace FastCharts.Core.Tests.Interaction.Behaviors
{
    /// <summary>
    /// Tests for PinnedTooltipBehavior implementation (P1-TOOLTIP-PIN)
    /// Validates pinning, unpinning, and multi-tooltip management
    /// </summary>
    public class PinnedTooltipBehaviorTests
    {
        private static (ChartModel model, InteractionState state) CreateTestModel()
        {
            var model = new ChartModel();
            
            // Add test data
            model.AddSeries(new LineSeries(new[]
            {
                new PointD(0, 10),
                new PointD(1, 20),
                new PointD(2, 15),
                new PointD(3, 25)
            })
            { Title = "Test Series" });

            model.AutoFitDataRange();
            model.UpdateScales(400, 300);
            
            var state = new InteractionState();
            model.InteractionState = state;
            
            return (model, state);
        }

        [Fact]
        public void PinnedTooltipBehavior_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var behavior = new PinnedTooltipBehavior();

            // Assert
            behavior.MaxPinnedTooltips.Should().Be(10);
            behavior.AutoRemoveOldest.Should().BeTrue();
            behavior.ClickDetectionRadius.Should().Be(15.0);
            behavior.AutoRepositionOnTransform.Should().BeTrue();
        }

        [Fact]
        public void InteractionState_PinCurrentTooltip_CreatesNewPinnedTooltip()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            
            // Setup current tooltip state
            state.DataX = 1.0;
            state.DataY = 20.0;
            state.PixelX = 100;
            state.PixelY = 150;
            state.TooltipText = "Test Series: 20.0";
            state.TooltipSeries.Add(new TooltipSeriesValue { Title = "Test Series", X = 1.0, Y = 20.0 });

            PinnedTooltip? createdTooltip = null;
            state.PinnedTooltipChanged += (s, e) => 
            {
                if (e.Action == PinnedTooltipAction.Pinned)
                {
                    createdTooltip = e.Tooltip;
                }
            };

            // Act
            var pinnedTooltip = state.PinCurrentTooltip("Test Pin");

            // Assert
            pinnedTooltip.Should().NotBeNull();
            pinnedTooltip!.DataPosition.X.Should().Be(1.0);
            pinnedTooltip.DataPosition.Y.Should().Be(20.0);
            pinnedTooltip.Text.Should().Be("Test Series: 20.0");
            pinnedTooltip.Label.Should().Be("Test Pin");
            pinnedTooltip.SeriesValues.Should().HaveCount(1);
            pinnedTooltip.IsVisible.Should().BeTrue();
            
            state.PinnedTooltips.Should().HaveCount(1);
            state.PinnedTooltips[0].Should().Be(pinnedTooltip);
            
            createdTooltip.Should().Be(pinnedTooltip);
        }

        [Fact]
        public void InteractionState_PinCurrentTooltip_NoTooltipAvailable_ReturnsNull()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            
            // Don't set up any tooltip data

            // Act
            var pinnedTooltip = state.PinCurrentTooltip();

            // Assert
            pinnedTooltip.Should().BeNull();
            state.PinnedTooltips.Should().BeEmpty();
        }

        [Fact]
        public void InteractionState_UnpinTooltip_ExistingTooltip_RemovesTooltip()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            state.DataX = 1.0;
            state.DataY = 20.0;
            state.TooltipText = "Test";
            state.TooltipSeries.Add(new TooltipSeriesValue { Title = "Test", X = 1.0, Y = 20.0 });

            var pinned = state.PinCurrentTooltip();
            pinned.Should().NotBeNull();

            PinnedTooltipAction? lastAction = null;
            state.PinnedTooltipChanged += (s, e) => lastAction = e.Action;

            // Act
            var result = state.UnpinTooltip(pinned!.Id);

            // Assert
            result.Should().BeTrue();
            state.PinnedTooltips.Should().BeEmpty();
            lastAction.Should().Be(PinnedTooltipAction.Unpinned);
        }

        [Fact]
        public void InteractionState_UnpinTooltip_NonExistentTooltip_ReturnsFalse()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            var randomId = Guid.NewGuid();

            // Act
            var result = state.UnpinTooltip(randomId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void InteractionState_ToggleTooltipVisibility_ExistingTooltip_TogglesCorrectly()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            state.DataX = 1.0;
            state.DataY = 20.0;
            state.TooltipText = "Test";
            state.TooltipSeries.Add(new TooltipSeriesValue());

            var pinned = state.PinCurrentTooltip();
            pinned!.IsVisible.Should().BeTrue(); // Initial state

            // Act - Toggle to invisible
            var result1 = state.ToggleTooltipVisibility(pinned.Id);

            // Assert
            result1.Should().Be(false);
            pinned.IsVisible.Should().BeFalse();

            // Act - Toggle back to visible
            var result2 = state.ToggleTooltipVisibility(pinned.Id);

            // Assert
            result2.Should().Be(true);
            pinned.IsVisible.Should().BeTrue();
        }

        [Fact]
        public void InteractionState_ClearAllPinnedTooltips_RemovesAllTooltips()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            
            // Add multiple pinned tooltips
            for (var i = 0; i < 3; i++)
            {
                state.DataX = i;
                state.DataY = i * 10;
                state.TooltipText = $"Test {i}";
                state.TooltipSeries.Clear();
                state.TooltipSeries.Add(new TooltipSeriesValue());
                state.PinCurrentTooltip($"Pin {i}");
            }

            state.PinnedTooltips.Should().HaveCount(3);

            PinnedTooltipAction? lastAction = null;
            state.PinnedTooltipChanged += (s, e) => lastAction = e.Action;

            // Act
            state.ClearAllPinnedTooltips();

            // Assert
            state.PinnedTooltips.Should().BeEmpty();
            lastAction.Should().Be(PinnedTooltipAction.AllCleared);
        }

        [Fact]
        public void InteractionState_GetVisiblePinnedTooltips_FiltersCorrectly()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            
            // Add multiple tooltips with different visibility
            for (var i = 0; i < 4; i++)
            {
                state.DataX = i;
                state.DataY = i * 10;
                state.TooltipText = $"Test {i}";
                state.TooltipSeries.Clear();
                state.TooltipSeries.Add(new TooltipSeriesValue());
                var pinned = state.PinCurrentTooltip($"Pin {i}");
                
                // Make every other tooltip invisible
                if (i % 2 == 1)
                {
                    pinned!.IsVisible = false;
                }
            }

            // Act
            var visibleTooltips = state.GetVisiblePinnedTooltips().ToList();

            // Assert
            visibleTooltips.Should().HaveCount(2); // Tooltips 0 and 2 should be visible
            visibleTooltips.All(t => t.IsVisible).Should().BeTrue();
        }

        [Fact]
        public void InteractionState_FindNearestPinnedTooltip_FindsCorrectTooltip()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            
            // Create tooltips at different positions
            var tooltips = new[]
            {
                (x: 100.0, y: 100.0, label: "Tooltip 1"),
                (x: 200.0, y: 150.0, label: "Tooltip 2"),
                (x: 300.0, y: 200.0, label: "Tooltip 3")
            };

            foreach (var (x, y, label) in tooltips)
            {
                state.DataX = x / 100.0; // Some data coordinate
                state.DataY = y / 100.0;
                state.PixelX = x;
                state.PixelY = y;
                state.TooltipText = label;
                state.TooltipSeries.Clear();
                state.TooltipSeries.Add(new TooltipSeriesValue());
                var pinned = state.PinCurrentTooltip(label);
                pinned!.PixelPosition = new PointD(x, y); // Manually set pixel position
            }

            // Act - Search near tooltip 2
            var nearest = state.FindNearestPinnedTooltip(205, 155, maxDistance: 20);

            // Assert
            nearest.Should().NotBeNull();
            nearest!.Label.Should().Be("Tooltip 2");

            // Act - Search in empty area
            var notFound = state.FindNearestPinnedTooltip(50, 50, maxDistance: 20);

            // Assert
            notFound.Should().BeNull();
        }

        [Fact]
        public void PinnedTooltipBehavior_RightClick_PinsCurrentTooltip()
        {
            // Arrange
            var (model, state) = CreateTestModel();
            var behavior = new PinnedTooltipBehavior();
            
            // Setup current tooltip
            state.DataX = 1.5;
            state.DataY = 17.5;
            state.TooltipText = "Series: 17.5";
            state.TooltipSeries.Add(new TooltipSeriesValue { Title = "Series", X = 1.5, Y = 17.5 });

            var rightClickEvent = new InteractionEvent(
                PointerEventType.Down,
                PointerButton.Right,
                new PointerModifiers { Ctrl = false, Shift = false, Alt = false },
                150, 175, 0, 400, 300);

            // Act
            var handled = behavior.OnEvent(model, rightClickEvent);

            // Assert
            handled.Should().BeTrue();
            state.PinnedTooltips.Should().HaveCount(1);
            state.PinnedTooltips[0].DataPosition.X.Should().Be(1.5);
            state.PinnedTooltips[0].DataPosition.Y.Should().Be(17.5);
        }

        [Fact]
        public void PinnedTooltipBehavior_MaxPinnedTooltips_EnforcesLimit()
        {
            // Arrange
            var (model, state) = CreateTestModel();
            var behavior = new PinnedTooltipBehavior { MaxPinnedTooltips = 2, AutoRemoveOldest = true };

            // Act - Pin 3 tooltips (exceeds limit of 2)
            for (var i = 0; i < 3; i++)
            {
                state.DataX = i;
                state.DataY = i * 10;
                state.TooltipText = $"Test {i}";
                state.TooltipSeries.Clear();
                state.TooltipSeries.Add(new TooltipSeriesValue());

                var rightClickEvent = new InteractionEvent(
                    PointerEventType.Down,
                    PointerButton.Right,
                    new PointerModifiers { Ctrl = false, Shift = false, Alt = false },
                    100 + i * 50, 100, 0, 400, 300);

                behavior.OnEvent(model, rightClickEvent);
            }

            // Assert - Should have only 2 tooltips (limit), with oldest removed
            state.PinnedTooltips.Should().HaveCount(2);
            
            // The remaining tooltips should be the last 2 created (1 and 2)
            var remainingLabels = state.PinnedTooltips.Select(t => t.Label).OrderBy(l => l).ToList();
            remainingLabels.Should().BeEquivalentTo("Pin 2", "Pin 3");
        }

        [Fact]
        public void PinnedTooltipBehavior_GetUsageInstructions_ReturnsHelpText()
        {
            // Act
            var instructions = PinnedTooltipBehavior.UsageInstructions;

            // Assert
            instructions.Should().NotBeEmpty();
            instructions.Should().Contain("Right-click");
            instructions.Should().Contain("pin");
            instructions.Should().Contain("remove");
        }

        [Fact]
        public void PinnedTooltipBehavior_ExportPinnedTooltipsData_GeneratesReport()
        {
            // Arrange
            var (_, state) = CreateTestModel();
            
            // Add some pinned tooltips
            for (var i = 0; i < 2; i++)
            {
                state.DataX = i;
                state.DataY = i * 15;
                state.TooltipText = $"Data {i}";
                state.TooltipSeries.Clear();
                state.TooltipSeries.Add(new TooltipSeriesValue { Title = $"Series {i}", X = i, Y = i * 15 });
                state.PinCurrentTooltip($"Test Pin {i}");
            }

            // Act
            var export = PinnedTooltipBehavior.ExportPinnedTooltipsData(state);

            // Assert
            export.Should().NotBeEmpty();
            export.Should().Contain("Pinned Tooltips Export");
            export.Should().Contain("Total: 2");
            export.Should().Contain("Test Pin 0");
            export.Should().Contain("Test Pin 1");
            export.Should().Contain("Series 0");
            export.Should().Contain("Series 1");
        }

        [Fact]
        public void PinnedTooltip_Properties_AreSetCorrectly()
        {
            // Arrange
            var dataPos = new PointD(5.0, 25.0);
            var pixelPos = new PointD(250, 125);
            var text = "Test tooltip content";
            var seriesValues = new System.Collections.Generic.List<TooltipSeriesValue>
            {
                new() { Title = "Series A", X = 5.0, Y = 25.0 },
                new() { Title = "Series B", X = 5.0, Y = 30.0 }
            };

            // Act
            var tooltip = new PinnedTooltip(dataPos, pixelPos, text, seriesValues);

            // Assert
            tooltip.Id.Should().NotBeEmpty();
            tooltip.DataPosition.Should().Be(dataPos);
            tooltip.PixelPosition.Should().Be(pixelPos);
            tooltip.Text.Should().Be(text);
            tooltip.SeriesValues.Should().HaveCount(2);
            tooltip.SeriesValues.Should().BeEquivalentTo(seriesValues);
            tooltip.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            tooltip.IsVisible.Should().BeTrue();
            tooltip.Label.Should().BeNull();
            tooltip.Color.Should().BeNull();
        }

        [Fact]
        public void PinnedTooltip_Summary_ReturnsCorrectFormat()
        {
            // Arrange
            var tooltip = new PinnedTooltip(
                new PointD(10.5, 42.3),
                new PointD(100, 200),
                "Test",
                new System.Collections.Generic.List<TooltipSeriesValue>
                {
                    new(),
                    new(),
                    new()
                });

            // Act
            var summary = tooltip.Summary;

            // Assert
            summary.Should().Contain("10.50");
            summary.Should().Contain("3 series");
        }
    }
}