using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// High-performance property path resolver.
    /// Compiles property access chains into cached delegates (one per concrete type + path),
    /// so repeated reads (e.g. refreshing a bound series with thousands of items) avoid
    /// per-item reflection cost. Falls back to plain reflection when compilation is not possible.
    /// </summary>
    public sealed class CachedPropertyPathResolver : IPropertyPathResolver
    {
        /// <summary>
        /// Shared singleton instance
        /// </summary>
        public static readonly CachedPropertyPathResolver Instance = new CachedPropertyPathResolver();

        private readonly ConcurrentDictionary<GetterKey, Func<object, object?>?> _getterCache =
            new ConcurrentDictionary<GetterKey, Func<object, object?>?>();

        private CachedPropertyPathResolver()
        {
        }

        /// <inheritdoc />
        public object? GetValue(object? source, string? path)
        {
            if (source == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            var getter = _getterCache.GetOrAdd(new GetterKey(source.GetType(), path!), BuildGetter);
            if (getter == null)
            {
                return null;
            }

            try
            {
                return getter(source);
            }
            catch
            {
                // Match reflection semantics: invalid access resolves to null
                return null;
            }
        }

        /// <inheritdoc />
        public void SetValue(object? source, string? path, object? value)
        {
            // Setting is rare in charting scenarios; delegate to the reflection implementation.
            ReflectionPropertyPathResolver.Instance.SetValue(source, path, value);
        }

        /// <inheritdoc />
        public bool IsValidPath(Type sourceType, string? path)
        {
            return ReflectionPropertyPathResolver.Instance.IsValidPath(sourceType, path);
        }

        /// <inheritdoc />
        public Type? GetPropertyType(Type sourceType, string? path)
        {
            return ReflectionPropertyPathResolver.Instance.GetPropertyType(sourceType, path);
        }

        private static Func<object, object?>? BuildGetter(GetterKey key)
        {
            try
            {
                var parts = key.Path.Split('.');

                // Validate the chain first; abort when any segment is missing
                var currentType = key.Type;
                var properties = new PropertyInfo[parts.Length];
                for (var i = 0; i < parts.Length; i++)
                {
                    var property = currentType.GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                    {
                        return null;
                    }

                    properties[i] = property;
                    currentType = property.PropertyType;
                }

                // Compile one getter per segment: (object o) => (object)((TDecl)o).Prop
                // Then chain them with null-propagation. Single-segment paths (the common
                // case) resolve to exactly one compiled delegate invocation.
                var segmentGetters = new Func<object, object?>[properties.Length];
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var declaringType = property.DeclaringType ?? key.Type;
                    var parameter = Expression.Parameter(typeof(object), "o");
                    Expression access = Expression.Property(Expression.Convert(parameter, declaringType), property);
                    if (access.Type != typeof(object))
                    {
                        access = Expression.Convert(access, typeof(object));
                    }

                    segmentGetters[i] = Expression.Lambda<Func<object, object?>>(access, parameter).Compile();
                }

                if (segmentGetters.Length == 1)
                {
                    return segmentGetters[0];
                }

                return source =>
                {
                    var current = (object?)source;
                    for (var i = 0; i < segmentGetters.Length && current != null; i++)
                    {
                        current = segmentGetters[i](current);
                    }

                    return current;
                };
            }
            catch
            {
                // Compilation failed (AOT restrictions, exotic types...) — reflect per call instead
                return source => ReflectionPropertyPathResolver.Instance.GetValue(source, key.Path);
            }
        }

        private readonly struct GetterKey : IEquatable<GetterKey>
        {
            public GetterKey(Type type, string path)
            {
                Type = type;
                Path = path;
            }

            public Type Type { get; }

            public string Path { get; }

            public bool Equals(GetterKey other)
            {
                return Type == other.Type && string.Equals(Path, other.Path, StringComparison.Ordinal);
            }

            public override bool Equals(object? obj)
            {
                return obj is GetterKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Type.GetHashCode() * 397) ^ StringComparer.Ordinal.GetHashCode(Path);
                }
            }
        }
    }
}
