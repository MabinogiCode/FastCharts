using System;
using System.Reflection;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Default implementation of property path resolver using reflection
    /// Supports nested properties with dot notation (e.g., "Person.Name")
    /// </summary>
    public sealed class ReflectionPropertyPathResolver : IPropertyPathResolver
    {
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static readonly IPropertyPathResolver Instance = new ReflectionPropertyPathResolver();

        private ReflectionPropertyPathResolver()
        {
        }

        /// <inheritdoc />
        public object? GetValue(object? source, string? path)
        {
            if (source == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            try
            {
                var current = source;
                var parts = path!.Split('.');

                foreach (var part in parts)
                {
                    if (current == null)
                    {
                        return null;
                    }

                    var property = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                    {
                        return null;
                    }

                    current = property.GetValue(current);
                }

                return current;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc />
        public void SetValue(object? source, string? path, object? value)
        {
            if (source == null || string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                var parts = path!.Split('.');
                var current = source;

                // Navigate to the parent of the final property
                for (var i = 0; i < parts.Length - 1; i++)
                {
                    var property = current?.GetType().GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                    {
                        return;
                    }

                    current = property.GetValue(current);
                    if (current == null)
                    {
                        return;
                    }
                }

                // Set the final property (.NET Standard 2.0 compatible)
                var finalProperty = current?.GetType().GetProperty(parts[parts.Length - 1], BindingFlags.Public | BindingFlags.Instance);
                if (finalProperty?.CanWrite == true)
                {
                    finalProperty.SetValue(current, value);
                }
            }
            catch
            {
                // Ignore errors in property setting
            }
        }

        /// <inheritdoc />
        public bool IsValidPath(Type sourceType, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            try
            {
                var current = sourceType;
                var parts = path!.Split('.');

                foreach (var part in parts)
                {
                    var property = current.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                    {
                        return false;
                    }

                    current = property.PropertyType;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public Type? GetPropertyType(Type sourceType, string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            try
            {
                var current = sourceType;
                var parts = path!.Split('.');

                foreach (var part in parts)
                {
                    var property = current.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                    if (property == null)
                    {
                        return null;
                    }

                    current = property.PropertyType;
                }

                return current;
            }
            catch
            {
                return null;
            }
        }
    }
}
