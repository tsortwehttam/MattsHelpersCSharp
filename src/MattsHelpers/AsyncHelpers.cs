using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MattsHelpers
{
    public static class AsyncHelpers
    {
        public static Task<TResult[]> MapParallelAsync<T, TResult>(
            IReadOnlyList<T> items,
            Func<T, int, Task<TResult>> iteratee)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (iteratee == null)
                throw new ArgumentNullException(nameof(iteratee));

            var tasks = new Task<TResult>[items.Count];
            for (var i = 0; i < items.Count; i++)
                tasks[i] = iteratee(items[i], i);

            return Task.WhenAll(tasks);
        }

        public static async Task<Dictionary<string, TResult>> MapObjectAsync<TResult>(
            IReadOnlyDictionary<string, object?> value,
            Func<object?, string, Task<TResult>> iteratee)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (iteratee == null)
                throw new ArgumentNullException(nameof(iteratee));

            var tasks = new List<Task<KeyValuePair<string, TResult>>>(value.Count);
            foreach (var pair in value)
                tasks.Add(MapEntryAsync(pair.Key, pair.Value, iteratee));

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            var output = new Dictionary<string, TResult>(results.Length);
            for (var i = 0; i < results.Length; i++)
                output[results[i].Key] = results[i].Value;

            return output;
        }

        private static async Task<KeyValuePair<string, TResult>> MapEntryAsync<TResult>(
            string key,
            object? value,
            Func<object?, string, Task<TResult>> iteratee)
        {
            return new KeyValuePair<string, TResult>(key, await iteratee(value, key).ConfigureAwait(false));
        }
    }
}
