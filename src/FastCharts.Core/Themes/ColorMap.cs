using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes
{
    /// <summary>
    /// Continuous color gradient used by color axes (heatmaps): evenly spaced color stops,
    /// linearly interpolated. See <see cref="ColorMaps"/> for ready-made maps.
    /// </summary>
    public sealed class ColorMap
    {
        private readonly ColorRgba[] _stops;
        private readonly ReadOnlyCollection<ColorRgba> _readOnlyStops;

        /// <summary>
        /// Creates a color map from at least two evenly spaced color stops (low to high).
        /// </summary>
        /// <param name="name">Display name (e.g. "Viridis")</param>
        /// <param name="stops">Colors from the lowest to the highest value</param>
        public ColorMap(string name, IEnumerable<ColorRgba> stops)
        {
            if (stops == null)
            {
                throw new ArgumentNullException(nameof(stops));
            }

            _stops = stops.ToArray();
            if (_stops.Length < 2)
            {
                throw new ArgumentException("A color map needs at least two color stops", nameof(stops));
            }

            // Built-in maps are shared instances: expose the stops without letting callers mutate them
            _readOnlyStops = Array.AsReadOnly(_stops);
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// Display name of the map.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Color stops from the lowest to the highest value.
        /// </summary>
        public IReadOnlyList<ColorRgba> Stops => _readOnlyStops;

        /// <summary>
        /// Color at normalized position <paramref name="t"/> (0 = lowest, 1 = highest).
        /// Values outside [0, 1] are clamped; NaN maps to the lowest color.
        /// </summary>
        public ColorRgba Map(double t)
        {
            if (double.IsNaN(t) || t <= 0)
            {
                return _stops[0];
            }

            if (t >= 1)
            {
                return _stops[_stops.Length - 1];
            }

            var position = t * (_stops.Length - 1);
            var index = (int)position;
            var fraction = position - index;
            var a = _stops[index];
            var b = _stops[index + 1];
            return new ColorRgba(Lerp(a.R, b.R, fraction), Lerp(a.G, b.G, fraction), Lerp(a.B, b.B, fraction), Lerp(a.A, b.A, fraction));
        }

        /// <summary>
        /// Same colors in reverse order (highest value gets the first color).
        /// </summary>
        public ColorMap Reversed()
        {
            return new ColorMap(Name + " (reversed)", _stops.Reverse());
        }

        private static byte Lerp(byte from, byte to, double fraction)
        {
            return (byte)Math.Round(from + ((to - from) * fraction));
        }
    }
}
