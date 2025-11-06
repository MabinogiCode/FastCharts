namespace FastCharts.Core.DependencyInjection
{
    /// <summary>
    /// Lifetime of a registered service.
    /// </summary>
    public enum ServiceLifetime
    {
        Transient,
        Singleton,
        Instance
    }
}