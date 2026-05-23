using System;
using System.Collections.Generic;

namespace MattsHelpers
{
    public static class CurveHelpers
    {
        public static int BellCurve(double x, int min, int max, double steepness = 1)
        {
            var clampedX = MathHelpers.Clamp(x, -1, 1);
            var s = MathHelpers.Clamp(steepness, 0.01, 10);
            var edge = Math.Exp(-0.5 * s * s);
            var bell = Math.Exp(-0.5 * Math.Pow(clampedX * s, 2));
            var normalized = (bell - edge) / (1 - edge);
            var value = (int)Math.Round(min + normalized * (max - min), MidpointRounding.AwayFromZero);
            return MathHelpers.Clamp(value, min, max);
        }

        public static int MortalityCurve(double x, int min, int max, double skew = 2)
        {
            var t = (MathHelpers.Clamp(x, -1, 1) + 1) / 2.0;
            var k = MathHelpers.Clamp(skew, 0.01, 10);
            var raw = MortalityRaw(t, k);
            var middle = MortalityRaw(0.5, k);
            var edge = Math.Max(MortalityRaw(0, k), MortalityRaw(1, k));
            var normalized = edge == middle ? 0 : MathHelpers.Clamp((raw - middle) / (edge - middle), 0, 1);
            var value = (int)Math.Round(min + normalized * (max - min), MidpointRounding.AwayFromZero);
            return MathHelpers.Clamp(value, min, max);
        }

        public static double SampleBell(double x, double steepness = 2, double center = 0.5)
        {
            x = MathHelpers.Clamp(x, 0.000001, 0.999999);
            if (Math.Abs(x - 0.5) < 0.0000001)
                return MathHelpers.Clamp(center, 0, 1);

            var z = -Math.Sqrt(-2 * Math.Log(x));
            var sample = center + z / (Math.Max(0.0001, steepness) * 4);
            return MathHelpers.Clamp(sample, 0, 1);
        }

        public static double SampleLog(double x, double bias = 4)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            bias = Math.Max(0.0001, bias);
            return MathHelpers.Clamp(Math.Log(1 + bias * x) / Math.Log(1 + bias), 0, 1);
        }

        public static double SampleExponential(double x, double rate = 2)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            rate = Math.Max(0.0001, rate);
            var sample = -Math.Log(1 - x) / rate;
            var maxValue = -Math.Log(0.001) / rate;
            return MathHelpers.Clamp(sample / maxValue, 0, 1);
        }

        public static double SampleParabolic(double x, double shape = 2, bool invert = false)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            shape = Math.Max(0.0001, shape);

            if (invert)
            {
                var sample = 1 - Math.Pow(2 * Math.Abs(x - 0.5), 1 / shape);
                return MathHelpers.Clamp(sample, 0, 1);
            }

            if (x < 0.5)
                return Math.Pow(2 * x, shape) / 2;

            return 1 - Math.Pow(2 * (1 - x), shape) / 2;
        }

        public static double SampleSCurve(double x, double steepness = 6, double center = 0.5)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            steepness = Math.Max(0.0001, steepness);
            center = MathHelpers.Clamp(center, 0.001, 0.999);
            var logitX = Math.Log(x / (1 - x));
            var shifted = logitX / steepness + Math.Log(center / (1 - center));
            return MathHelpers.Clamp(1 / (1 + Math.Exp(-shifted)), 0, 1);
        }

        public static double SampleReverseSCurve(double x, double steepness = 6, double center = 0.5)
        {
            return 1 - SampleSCurve(x, steepness, 1 - center);
        }

        public static double SamplePowerLaw(double x, double alpha = 2)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            alpha = alpha <= 1.0001 ? 1.0001 : alpha;
            var exponent = -1 / (alpha - 1);
            var sample = Math.Pow(1 - x, exponent);
            var maxValue = Math.Pow(0.001, exponent);
            return MathHelpers.Clamp((sample - 1) / (maxValue - 1), 0, 1);
        }

        public static double SampleBetaApproximation(double x, double alpha = 2, double beta = 2)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            alpha = Math.Max(0.0001, alpha);
            beta = Math.Max(0.0001, beta);

            var guess = x;
            for (var i = 0; i < 3; i++)
            {
                var betaValue = Math.Pow(guess, alpha - 1) * Math.Pow(1 - guess, beta - 1);
                var error = x - betaValue;
                guess = MathHelpers.Clamp(guess + error * 0.1, 0, 1);
            }

            return guess;
        }

        public static double SampleTriangular(double x, double peak = 0.5)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            peak = MathHelpers.Clamp(peak, 0, 1);

            if (peak == 0)
                return 1 - x;

            if (peak == 1)
                return x;

            if (x <= peak)
                return Math.Sqrt(x / peak);

            return Math.Sqrt((1 - x) / (1 - peak));
        }

        public static double SampleBimodal(double x, double separation = 0.6, double balance = 0.5)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            separation = MathHelpers.Clamp(separation, 0.1, 1);
            balance = MathHelpers.Clamp(balance, 0.0001, 0.9999);

            if (x < balance)
                return SampleBell(x / balance, 3, 0.5 - separation / 2);

            return SampleBell((x - balance) / (1 - balance), 3, 0.5 + separation / 2);
        }

        public static double SampleUniform(double x)
        {
            return MathHelpers.Clamp(x, 0, 1);
        }

        public static double SampleStep(double x, double threshold = 0.5, double lowValue = 0, double highValue = 1)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            threshold = MathHelpers.Clamp(threshold, 0, 1);
            return x < threshold ? lowValue : highValue;
        }

        public static double SampleSawtooth(double x, double frequency = 1, bool ascending = true)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            frequency = Math.Max(0, frequency);
            var phase = MathHelpers.PositiveModulo(x * frequency, 1);
            return ascending ? phase : 1 - phase;
        }

        public static double SampleSine(double x, double frequency = 1, double phase = 0, double amplitude = 0.5)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            var wave = Math.Sin(2 * Math.PI * frequency * x + phase);
            return MathHelpers.Clamp(0.5 + amplitude * wave, 0, 1);
        }

        public static double SampleGammaApproximation(double x, double shape = 2, double scale = 1)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            shape = Math.Max(0.0001, shape);

            if (shape < 1)
                return MathHelpers.Clamp(Math.Pow(x, 1 / shape) * scale, 0, 1);

            return MathHelpers.Clamp((-Math.Log(1 - x) * scale) / shape, 0, 1);
        }

        public static double SampleWeibull(double x, double shape = 2, double scale = 1)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            shape = Math.Max(0.0001, shape);
            scale = Math.Max(0.0001, scale);
            var sample = scale * Math.Pow(-Math.Log(1 - x), 1 / shape);
            var maxValue = scale * Math.Pow(-Math.Log(0.001), 1 / shape);
            return MathHelpers.Clamp(sample / maxValue, 0, 1);
        }

        public static double SampleLaplace(double x, double center = 0.5, double scale = 0.2)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            center = MathHelpers.Clamp(center, 0, 1);
            scale = Math.Max(0.0001, scale);
            var sample = x < 0.5
                ? center + scale * Math.Log(2 * x)
                : center - scale * Math.Log(2 * (1 - x));
            return MathHelpers.Clamp(sample, 0, 1);
        }

        public static double SampleCauchy(double x, double center = 0.5, double scale = 0.1)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            center = MathHelpers.Clamp(center, 0, 1);
            var sample = center + scale * Math.Tan(Math.PI * (x - 0.5));
            return MathHelpers.Clamp(sample, 0, 1);
        }

        public static double SampleSpline(double x, IReadOnlyList<double>? controlPoints = null)
        {
            x = MathHelpers.Clamp(x, 0, 1);
            controlPoints = controlPoints ?? DefaultSplineControlPoints;

            if (controlPoints.Count < 2)
                return x;

            var segments = controlPoints.Count - 1;
            var segmentSize = 1.0 / segments;
            var segmentIndex = Math.Min((int)Math.Floor(x / segmentSize), segments - 1);
            var segmentX = (x - segmentIndex * segmentSize) / segmentSize;
            var start = controlPoints[segmentIndex];
            var end = controlPoints[segmentIndex + 1];
            return MathHelpers.Clamp(start + segmentX * (end - start), 0, 1);
        }

        public static double SampleLogistic(double x, double center = 0.5, double scale = 0.1)
        {
            x = MathHelpers.Clamp(x, 0.001, 0.999);
            center = MathHelpers.Clamp(center, 0, 1);
            scale = Math.Max(0.0001, scale);
            var sample = center + scale * Math.Log(x / (1 - x));
            return MathHelpers.Clamp(sample, 0, 1);
        }

        public static double SampleQuantiles(double x, IReadOnlyList<Quantile> quantiles)
        {
            if (quantiles == null)
                throw new ArgumentNullException(nameof(quantiles));
            if (quantiles.Count == 0)
                return MathHelpers.Clamp(x, 0, 1);

            x = MathHelpers.Clamp(x, 0, 1);
            var sorted = new List<Quantile>(quantiles);
            sorted.Sort((a, b) => a.Percentile.CompareTo(b.Percentile));

            for (var i = 0; i < sorted.Count - 1; i++)
            {
                var current = sorted[i];
                var next = sorted[i + 1];
                if (x >= current.Percentile && x <= next.Percentile)
                {
                    var denominator = next.Percentile - current.Percentile;
                    var t = denominator == 0 ? 0 : (x - current.Percentile) / denominator;
                    return MathHelpers.Clamp(current.Value + t * (next.Value - current.Value), 0, 1);
                }
            }

            if (x <= sorted[0].Percentile)
                return MathHelpers.Clamp(sorted[0].Value, 0, 1);

            return MathHelpers.Clamp(sorted[sorted.Count - 1].Value, 0, 1);
        }

        private static readonly double[] DefaultSplineControlPoints = { 0, 0.3, 0.7, 1 };

        private static double MortalityRaw(double t, double k)
        {
            var left = 1 / (1 + Math.Exp((t - 0.15) * 10 * k));
            var right = 1 / (1 + Math.Exp((-t + 0.85) * 10 * k));
            return left + right;
        }
    }

    public struct Quantile
    {
        public Quantile(double percentile, double value)
        {
            Percentile = percentile;
            Value = value;
        }

        public double Percentile { get; }
        public double Value { get; }
    }
}
