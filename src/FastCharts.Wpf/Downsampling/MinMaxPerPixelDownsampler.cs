using System;
using System.Collections.Generic;
namespace FastCharts.Wpf.Downsampling
{
    public sealed class MinMaxPerPixelDownsampler : IDownsampler
    {
        public IReadOnlyList<int> Downsample(double[] X, double[] Y, int pixelWidth, int maxPointsPerPixel = 2)
        {
            var result = new List<int>(Math.Min(X.Length, pixelWidth * maxPointsPerPixel + 2));
            if (X.Length == 0 || pixelWidth <= 0) 
            {
                return result;
            }
            double xmin = X[0], xmax = X[X.Length - 1];
            if (xmax <= xmin) { result.Add(0); return result; }
            var range = xmax - xmin; 
            if (range <= 0) 
            {
                range = 1e-9;
            }
            var start = 0;
            for (var px = 0; px < pixelWidth; px++)
            {
                var x0 = xmin + px * range / pixelWidth;
                var x1 = xmin + (px + 1) * range / pixelWidth;
                var i0 = start; 
                while (i0 < X.Length && X[i0] < x0) 
                {
                    i0++;
                }
                var i1 = i0; 
                while (i1 < X.Length && X[i1] < x1) 
                {
                    i1++;
                }
                if (i1 <= i0) 
                {
                    continue;
                }
                start = i0;
                int imin = i0, imax = i0; double ymin = Y[i0], ymax = Y[i0];
                for (var i = i0 + 1; i < i1; i++) { var v = Y[i]; if (v < ymin) { ymin = v; imin = i; } if (v > ymax) { ymax = v; imax = i; } }
                if (maxPointsPerPixel <= 1 || imin == imax) 
                {
                    result.Add(imin);
                }
                else { if (imin < imax) { result.Add(imin); result.Add(imax); } else { result.Add(imax); result.Add(imin); } }
            }
            if (result.Count == 0 || result[result.Count - 1] != X.Length - 1) 
            {
                result.Add(X.Length - 1);
            }
            return result;
        }
    }
}
