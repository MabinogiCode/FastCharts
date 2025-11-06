using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Interaction;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Default implementation of behavior manager.
    /// </summary>
    public class BehaviorManager : IBehaviorManager
    {
        public void ConfigureDefaultBehaviors(IList<IBehavior> behaviors)
        {
            if (behaviors.Count > 0)
            {
                return;
            }

            behaviors.Add(new Interaction.Behaviors.CrosshairBehavior());
            behaviors.Add(new Interaction.Behaviors.MultiSeriesTooltipBehavior());
            behaviors.Add(new Interaction.Behaviors.ZoomRectBehavior());
            behaviors.Add(new Interaction.Behaviors.NearestPointBehavior());
            behaviors.Add(new Interaction.Behaviors.LegendToggleBehavior());
            behaviors.Add(new Interaction.Behaviors.ZoomWheelBehavior());
            behaviors.Add(new Interaction.Behaviors.PanBehavior());
        }

        public void EnsureBehaviorType<T>(IList<IBehavior> behaviors)
            where T : IBehavior, new()
        {
            var hasType = behaviors.Any(behavior => behavior is T);

            if (!hasType)
            {
                behaviors.Add(new T());
            }
        }

        public void RemoveBehaviorType<T>(IList<IBehavior> behaviors)
            where T : IBehavior
        {
            var itemsToRemove = behaviors.Where(behavior => behavior is T).ToList();

            foreach (var item in itemsToRemove)
            {
                behaviors.Remove(item);
            }
        }
    }
}