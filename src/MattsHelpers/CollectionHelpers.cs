using System;
using System.Collections.Generic;

namespace MattsHelpers
{
    public static class CollectionHelpers
    {
        public static T[] Unique<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var seen = new HashSet<T>();
            var result = new List<T>();
            foreach (var value in values)
            {
                if (seen.Add(value))
                    result.Add(value);
            }

            return result.ToArray();
        }

        public static T Last<T>(IReadOnlyList<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Count == 0)
                throw new ArgumentException("Collection cannot be empty.", nameof(values));

            return values[values.Count - 1];
        }

        public static bool TryLast<T>(IReadOnlyList<T>? values, out T value)
        {
            if (values == null || values.Count == 0)
            {
                value = default(T)!;
                return false;
            }

            value = values[values.Count - 1];
            return true;
        }

        public static T[] Intersection<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var lookup = new HashSet<T>(second);
            var result = new List<T>();
            foreach (var item in first)
            {
                if (lookup.Contains(item))
                    result.Add(item);
            }

            return result.ToArray();
        }

        public static T ElementAtWrapped<T>(IReadOnlyList<T> values, int index)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Count == 0)
                throw new ArgumentException("Collection cannot be empty.", nameof(values));

            return values[MathHelpers.PositiveModulo(index, values.Count)];
        }

        public static T PickByPercent<T>(double percent, IReadOnlyList<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Count == 0)
                throw new ArgumentException("Collection cannot be empty.", nameof(values));

            var index = (int)Math.Floor(MathHelpers.Clamp(percent, 0, 0.999999999999) * values.Count);
            return values[index];
        }

        public static TKey PickKeyByPercent<TKey>(double percent, IReadOnlyList<PercentRange<TKey>> ranges)
        {
            if (ranges == null)
                throw new ArgumentNullException(nameof(ranges));
            if (ranges.Count == 0)
                throw new ArgumentException("Ranges cannot be empty.", nameof(ranges));

            var pc = MathHelpers.Clamp(percent, 0, 1);
            for (var i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                if (pc >= range.StartInclusive && pc < range.EndExclusive)
                    return range.Key;
            }

            return ranges[0].Key;
        }
    }

    public struct PercentRange<TKey>
    {
        public PercentRange(TKey key, double startInclusive, double endExclusive)
        {
            Key = key;
            StartInclusive = startInclusive;
            EndExclusive = endExclusive;
        }

        public TKey Key { get; }
        public double StartInclusive { get; }
        public double EndExclusive { get; }
    }
}
