using System;
using System.Collections.Generic;
using System.Text;

namespace MattsHelpers
{
    public sealed class DeterministicRandom
    {
        private const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private uint _state;
        private int _cycle;

        public DeterministicRandom(string seed, int initialCycles = 0)
            : this(HashHelpers.StableSeed(seed), initialCycles)
        {
        }

        public DeterministicRandom(int seed, int initialCycles = 0)
            : this(HashHelpers.StableSeed(seed.ToString(System.Globalization.CultureInfo.InvariantCulture)), initialCycles)
        {
        }

        public DeterministicRandom(uint seed, int initialCycles = 0)
        {
            _state = seed == 0 ? 1u : seed;
            if (initialCycles < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCycles));

            for (var i = 0; i < initialCycles; i++)
                NextDouble();
        }

        public int Cycle
        {
            get { return _cycle; }
        }

        public double NextDouble()
        {
            unchecked
            {
                _cycle++;
                var t = _state += 0x6d2b79f5u;
                t = (t ^ (t >> 15)) * (t | 1);
                t ^= t + ((t ^ (t >> 7)) * (t | 61));
                return ((t ^ (t >> 14)) & 0xffffffffu) / 4294967296.0;
            }
        }

        public bool CoinToss(double probability = 0.5)
        {
            return NextDouble() < MathHelpers.Clamp(probability, 0, 1);
        }

        public bool NextBoolean()
        {
            return NextDouble() < 0.5;
        }

        public int Dice(int sides = 6)
        {
            if (sides <= 0)
                throw new ArgumentOutOfRangeException(nameof(sides));

            return NextInt(1, sides);
        }

        public int[] RollDice(int rolls, int sides = 6)
        {
            if (rolls < 0)
                throw new ArgumentOutOfRangeException(nameof(rolls));

            var results = new int[rolls];
            for (var i = 0; i < rolls; i++)
                results[i] = Dice(sides);

            return results;
        }

        public string NextAlphaNumeric(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var builder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
                builder.Append(AlphaNumeric[(int)Math.Floor(NextDouble() * AlphaNumeric.Length)]);

            return builder.ToString();
        }

        public double NextDouble(double min, double max)
        {
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than or equal to min.");

            return NextDouble() * (max - min) + min;
        }

        public int NextInt(int minInclusive, int maxInclusive)
        {
            if (maxInclusive < minInclusive)
                throw new ArgumentOutOfRangeException(nameof(maxInclusive), "Max must be greater than or equal to min.");

            return (int)Math.Floor(NextDouble() * (maxInclusive - minInclusive + 1)) + minInclusive;
        }

        public double NextNormal(double min, double max)
        {
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), "Max must be greater than or equal to min.");

            var u1 = Math.Max(NextDouble(), double.Epsilon);
            var u2 = NextDouble();
            var standardNormal = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
            var mean = (max + min) / 2;
            var stdDev = (max - min) / 6;
            return standardNormal * stdDev + mean;
        }

        public int NextNormalInt(int min, int max)
        {
            return (int)Math.Round(NextNormal(min, max), MidpointRounding.AwayFromZero);
        }

        public T NextElement<T>(IReadOnlyList<T> values)
        {
            if (!TryNextElement(values, out var value))
                throw new ArgumentException("Cannot draw from an empty collection.", nameof(values));

            return value;
        }

        public bool TryNextElement<T>(IReadOnlyList<T>? values, out T value)
        {
            if (values == null || values.Count == 0)
            {
                value = default(T)!;
                return false;
            }

            value = values[(int)Math.Floor(NextDouble() * values.Count)];
            return true;
        }

        public T Draw<T>(IReadOnlyList<T> values)
        {
            return NextElement(Shuffle(values));
        }

        public TKey WeightedKey<TKey>(IReadOnlyDictionary<TKey, double> weights)
        {
            if (weights == null)
                throw new ArgumentNullException(nameof(weights));
            if (weights.Count == 0)
                throw new ArgumentException("Weights cannot be empty.", nameof(weights));

            var total = 0.0;
            foreach (var pair in weights)
            {
                if (pair.Value > 0)
                    total += pair.Value;
            }

            if (total <= 0)
                throw new ArgumentException("At least one weight must be positive.", nameof(weights));

            var target = NextDouble() * total;
            foreach (var pair in weights)
            {
                if (pair.Value <= 0)
                    continue;

                target -= pair.Value;
                if (target <= 0)
                    return pair.Key;
            }

            foreach (var pair in weights)
                return pair.Key;

            throw new InvalidOperationException("Unreachable weighted selection state.");
        }

        public T[] Shuffle<T>(IReadOnlyList<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var result = new T[values.Count];
            for (var i = 0; i < values.Count; i++)
                result[i] = values[i];

            for (var i = result.Length - 1; i > 0; i--)
            {
                var j = (int)Math.Floor(NextDouble() * (i + 1));
                var tmp = result[i];
                result[i] = result[j];
                result[j] = tmp;
            }

            return result;
        }

        public string NextUlid(long? timestampMilliseconds = null)
        {
            return TextHelpers.Ulid(NextDouble, timestampMilliseconds);
        }
    }
}
