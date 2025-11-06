using System.Collections.Generic;
using FastCharts.Core.Interaction;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Service for managing chart interaction behaviors.
    /// </summary>
    public interface IBehaviorManager
    {
        void ConfigureDefaultBehaviors(IList<IBehavior> behaviors);
        void EnsureBehaviorType<T>(IList<IBehavior> behaviors) where T : IBehavior, new();
        void RemoveBehaviorType<T>(IList<IBehavior> behaviors) where T : IBehavior;
    }
}