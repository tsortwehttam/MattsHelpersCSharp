using System;
using System.Collections;
using System.Collections.Generic;

namespace MattsHelpers
{
    public static class DataHelpers
    {
        public static Dictionary<string, object?> DeepMerge(
            IReadOnlyDictionary<string, object?> baseObject,
            IReadOnlyDictionary<string, object?> overlay)
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));
            if (overlay == null)
                throw new ArgumentNullException(nameof(overlay));

            var output = new Dictionary<string, object?>();
            foreach (var pair in baseObject)
                output[pair.Key] = DeepClone(pair.Value);

            foreach (var pair in overlay)
            {
                var previous = output.ContainsKey(pair.Key) ? output[pair.Key] : null;
                if (previous is IReadOnlyDictionary<string, object?> previousDictionary
                    && pair.Value is IReadOnlyDictionary<string, object?> overlayDictionary)
                {
                    output[pair.Key] = DeepMerge(previousDictionary, overlayDictionary);
                    continue;
                }

                output[pair.Key] = DeepClone(pair.Value);
            }

            return output;
        }

        public static object? DeepClone(object? value)
        {
            if (value == null)
                return null;

            if (value is string || value is decimal || value is bool || value is char)
                return value;

            if (IsScalar(value))
                return value;

            if (value is IReadOnlyDictionary<string, object?> dictionary)
            {
                var clone = new Dictionary<string, object?>();
                foreach (var pair in dictionary)
                    clone[pair.Key] = DeepClone(pair.Value);
                return clone;
            }

            if (value is IDictionary nonGenericDictionary)
            {
                var clone = new Dictionary<object, object?>();
                foreach (DictionaryEntry entry in nonGenericDictionary)
                    clone[entry.Key] = DeepClone(entry.Value);
                return clone;
            }

            if (value is IEnumerable enumerable)
            {
                var clone = new List<object?>();
                foreach (var item in enumerable)
                    clone.Add(DeepClone(item));
                return clone;
            }

            throw new NotSupportedException("DeepClone only supports primitive values, strings, dictionaries, and enumerable serial-style data.");
        }

        private static bool IsScalar(object value)
        {
            return value is byte
                   || value is sbyte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is DateTime
                   || value is DateTimeOffset
                   || value is TimeSpan
                   || value is Guid
                   || value is Enum;
        }
    }
}
