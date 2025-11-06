using System;

namespace FastCharts.Core.DependencyInjection
{
    /// <summary>
    /// Descriptor for a registered service.
    /// </summary>
    internal class ServiceDescriptor
    {
        public Type ServiceType { get; set; } = null!;
        public Type? ImplementationType { get; set; }
        public object? Instance { get; set; }
        public ServiceLifetime Lifetime { get; set; }
    }
}