using System;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Cache key of <see cref="CachedPropertyPathResolver"/>: source type + property path.
    /// </summary>
    internal readonly struct PropertyGetterKey : IEquatable<PropertyGetterKey>
    {
        public PropertyGetterKey(Type type, string path)
        {
            Type = type;
            Path = path;
        }

        public Type Type { get; }

        public string Path { get; }

        public bool Equals(PropertyGetterKey other)
        {
            return Type == other.Type && string.Equals(Path, other.Path, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is PropertyGetterKey other && Equals(other);
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
