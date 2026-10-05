using System;

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
}