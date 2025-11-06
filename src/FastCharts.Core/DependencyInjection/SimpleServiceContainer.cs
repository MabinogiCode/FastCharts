using System;
using System.Collections.Generic;

namespace FastCharts.Core.DependencyInjection
{
    /// <summary>
    /// Simple implementation of an IoC container.
    /// </summary>
    public class SimpleServiceContainer : IServiceContainer
    {
        private readonly Dictionary<Type, ServiceDescriptor> _services = new();
        private readonly Dictionary<Type, object> _singletonInstances = new();

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Singleton
            };
        }

        public void RegisterTransient<TInterface, TImplementation>()
            where TImplementation : class, TInterface
        {
            _services[typeof(TInterface)] = new ServiceDescriptor
            {
                ServiceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                Lifetime = ServiceLifetime.Transient
            };
        }

        public void RegisterInstance<T>(T instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _services[typeof(T)] = new ServiceDescriptor
            {
                ServiceType = typeof(T),
                Instance = instance,
                Lifetime = ServiceLifetime.Instance
            };
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (!_services.TryGetValue(type, out var descriptor))
            {
                throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
            }

            return descriptor.Lifetime switch
            {
                ServiceLifetime.Instance => descriptor.Instance!,
                ServiceLifetime.Singleton => GetOrCreateSingleton(descriptor),
                ServiceLifetime.Transient => CreateInstance(descriptor.ImplementationType!),
                _ => throw new InvalidOperationException($"Unknown service lifetime: {descriptor.Lifetime}")
            };
        }

        private object GetOrCreateSingleton(ServiceDescriptor descriptor)
        {
            if (_singletonInstances.TryGetValue(descriptor.ServiceType, out var instance))
            {
                return instance;
            }

            instance = CreateInstance(descriptor.ImplementationType!);
            _singletonInstances[descriptor.ServiceType] = instance;
            return instance;
        }

        private object CreateInstance(Type type)
        {
            var constructors = type.GetConstructors();
            var constructor = constructors[0]; // Simplified: take the first constructor

            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                args[i] = Resolve(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(type, args)!;
        }
    }
}