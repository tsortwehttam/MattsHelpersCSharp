using System;
using System.Collections.Generic;
using System.Globalization;

namespace MattsHelpers
{
    public static class MathHelpers
    {
        public static double Clamp(double value, double min = 0, double max = 1)
        {
            if (min > max)
            {
                var tmp = min;
                min = max;
                max = tmp;
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp(float value, float min = 0, float max = 1)
        {
            if (min > max)
            {
                var tmp = min;
                min = max;
                max = tmp;
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                var tmp = min;
                min = max;
                max = tmp;
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static double PercentToValue(double percent, double min, double max)
        {
            if (min == max)
                return min;

            return min + percent * (max - min);
        }

        public static double Normalize(double value, double min, double max)
        {
            if (min == max)
                return 0;

            return (value - min) / (max - min);
        }

        public static double Sum(IEnumerable<double> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var total = 0.0;
            foreach (var value in values)
                total += value;

            return total;
        }

        public static double AverageOrZero(IEnumerable<double> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var total = 0.0;
            var count = 0;
            foreach (var value in values)
            {
                total += value;
                count++;
            }

            return count == 0 ? 0 : total / count;
        }

        public static double RoundToPrecision(double value, double precision = 100)
        {
            if (precision == 0)
                return value;

            return Math.Round(value * precision, MidpointRounding.AwayFromZero) / precision;
        }

        public static double RoundToNearest(double value, double multiple)
        {
            if (multiple <= 0)
                return value;

            return Math.Round(value / multiple, MidpointRounding.AwayFromZero) * multiple;
        }

        public static double CeilToNearest(double value, double multiple)
        {
            if (multiple <= 0)
                return value;

            return Math.Ceiling(value / multiple) * multiple;
        }

        public static double FloorToNearest(double value, double multiple)
        {
            if (multiple <= 0)
                return value;

            return Math.Floor(value / multiple) * multiple;
        }

        public static int ParseIntOrZero(string? value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;

            return 0;
        }

        public static double? ParseFiniteDoubleOrNull(string? value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                && !double.IsNaN(parsed)
                && !double.IsInfinity(parsed))
                return parsed;

            return null;
        }

        public static double ModInRange(double value, double min, double max)
        {
            var range = max - min;
            if (range == 0)
                return min;

            return PositiveModulo(value - min, range) + min;
        }

        public static int ModInRange(int value, int min, int max)
        {
            var range = max - min;
            if (range == 0)
                return min;

            return PositiveModulo(value - min, range) + min;
        }

        public static double PositiveModulo(double value, double modulus)
        {
            if (modulus == 0)
                return 0;

            var result = value % modulus;
            return result < 0 ? result + Math.Abs(modulus) : result;
        }

        public static int PositiveModulo(int value, int modulus)
        {
            if (modulus == 0)
                return 0;

            var result = value % modulus;
            return result < 0 ? result + Math.Abs(modulus) : result;
        }

        public static double Midpoint(double a, double b)
        {
            return a + (b - a) / 2.0;
        }

        public static double MidpointWrapped(double a, double b, double min, double max)
        {
            var first = ModInRange(a, min, max);
            var second = ModInRange(b, min, max);
            var range = max - min;
            if (range == 0)
                return min;

            var diff = second - first;
            if (diff < 0)
                diff += range;

            if (diff > range / 2.0)
                return ModInRange(first - (range - diff) / 2.0, min, max);

            return ModInRange(first + diff / 2.0, min, max);
        }

        public static double MidpointThenWrap(double a, double b, double min, double max)
        {
            return ModInRange(Midpoint(a, b), min, max);
        }

        public static int PickModulo(int index, int lowerInclusive, int upperExclusive)
        {
            if (upperExclusive <= lowerInclusive)
                throw new ArgumentOutOfRangeException(nameof(upperExclusive), "Upper bound must be greater than lower bound.");

            return lowerInclusive + PositiveModulo(index, upperExclusive - lowerInclusive);
        }

        public static int Offset32Triangular(int value)
        {
            var hash = HashHelpers.Hash32(unchecked((uint)value));
            var offset = (int)(hash & 7) - (int)((hash >> 3) & 7);
            return Clamp(offset, -6, 6);
        }
    }
}
