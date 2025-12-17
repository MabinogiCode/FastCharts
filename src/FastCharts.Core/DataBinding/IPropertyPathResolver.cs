using System;
using System.Reflection;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Interface for resolving property paths on objects
    /// Supports nested properties (e.g., "Person.Address.City")
    /// </summary>
    public interface IPropertyPathResolver
    {
        /// <summary>
        /// Gets the value at the specified property path
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="path">Property path (e.g., "Value", "Price.Close")</param>
        /// <returns>Property value or null if path is invalid</returns>
        object? GetValue(object? source, string? path);

        /// <summary>
        /// Sets the value at the specified property path
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="path">Property path</param>
        /// <param name="value">Value to set</param>
        void SetValue(object? source, string? path, object? value);

        /// <summary>
        /// Checks if a property path is valid for the given type
        /// </summary>
        /// <param name="sourceType">Type to check</param>
        /// <param name="path">Property path</param>
        /// <returns>True if path is valid</returns>
        bool IsValidPath(Type sourceType, string? path);

        /// <summary>
        /// Gets the final property type for a given path
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="path">Property path</param>
        /// <returns>Property type or null if invalid</returns>
        Type? GetPropertyType(Type sourceType, string? path);
    }

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