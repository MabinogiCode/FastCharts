using FastCharts.Core;
using FastCharts.Core.Interaction;
using FastCharts.Core.Interaction.Behaviors;

using Xunit;

namespace FastCharts.Tests
{
    public class CrosshairBehaviorTests
    {
        [Fact]
        public void MoveSetsShowCrosshairAndPixelPosition()
        {
            var model = new ChartModel();
            var b = new CrosshairBehavior();
            var ev = new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 123, 45);

            var handled = b.OnEvent(model, ev);

            Assert.True(handled);
            Assert.NotNull(model.InteractionState);
            Assert.True(model.InteractionState.ShowCrosshair);
            Assert.Equal(123, model.InteractionState.PixelX, 5);
            Assert.Equal(45, model.InteractionState.PixelY, 5);
        }

        [Fact]
        public void LeaveHidesCrosshair()
        {
            var model = new ChartModel { InteractionState = new InteractionState { ShowCrosshair = true } };
            var b = new CrosshairBehavior();
            var ev = new InteractionEvent(PointerEventType.Leave, PointerButton.None, new PointerModifiers(), 0, 0);

            var handled = b.OnEvent(model, ev);

            Assert.True(handled);
            Assert.NotNull(model.InteractionState);
            Assert.False(model.InteractionState.ShowCrosshair);
        }
        [Fact]
        public void TooltipComposesFromDataWhenAvailable()
        {
            var model = new ChartModel { InteractionState = new InteractionState { DataX = 10.5, DataY = 2.3 } };
            var b = new CrosshairBehavior();
            var ev = new InteractionEvent(PointerEventType.Move, PointerButton.None, new PointerModifiers(), 0, 0);

            var handled = b.OnEvent(model, ev);

            Assert.True(handled);
            Assert.NotNull(model.InteractionState);
            Assert.False(string.IsNullOrEmpty(model.InteractionState.TooltipText));
        }
    }
}
