using System;

namespace FastCharts.Core.DependencyInjection
{
    /// <summary>
    /// Interface for a simple dependency injection container.
    /// </summary>
    public interface IServiceContainer
    {
        void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface;

        void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface;

        void RegisterInstance<T>(T instance);

        T Resolve<T>();
        object Resolve(Type type);
    }
}