using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MattsHelpers
{
    public static class TextHelpers
    {
        private const string UlidChars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        private const string CharsToEncode = " ~`!@#$%^&*()+={}|[]\\/:\":'<>?,.、。！？「」『』・«»—¡¿„“‚";

        public static string Smoosh(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var source = value!;
            var builder = new StringBuilder(source.Length);
            var inWhitespace = false;
            var started = false;
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if (char.IsWhiteSpace(c))
                {
                    if (started)
                        inWhitespace = true;
                    continue;
                }

                if (inWhitespace && builder.Length > 0)
                    builder.Append(' ');

                builder.Append(c);
                started = true;
                inWhitespace = false;
            }

            return builder.ToString();
        }

        public static string Slugify(string? text, char replacement = '_')
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var source = text!;
            var builder = new StringBuilder(source.Length);
            var previousWasReplacement = false;
            for (var i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if (CharsToEncode.IndexOf(c) >= 0 || char.IsWhiteSpace(c))
                {
                    AppendCollapsedReplacement(builder, replacement, ref previousWasReplacement);
                    continue;
                }

                builder.Append(c);
                previousWasReplacement = false;
            }

            return builder.ToString().Trim(replacement);
        }

        public static string Parameterize(string? text, char replacement = '_')
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var normalized = text!.Normalize(NormalizationForm.FormKC);
            var builder = new StringBuilder(normalized.Length);
            var previousWasReplacement = false;

            for (var i = 0; i < normalized.Length; i++)
            {
                var c = normalized[i];
                if (ShouldParameterize(c))
                {
                    AppendCollapsedReplacement(builder, replacement, ref previousWasReplacement);
                    continue;
                }

                builder.Append(c);
                previousWasReplacement = false;
            }

            return builder.ToString().Trim(replacement);
        }

        public static bool IsBlank(object? value)
        {
            if (value == null)
                return true;

            if (value is string s)
                return string.IsNullOrWhiteSpace(s);

            if (value is ICollection collection)
                return collection.Count == 0;

            if (value is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                try
                {
                    return !enumerator.MoveNext();
                }
                finally
                {
                    var disposable = enumerator as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }

            return false;
        }

        public static bool IsPresent(object? value)
        {
            return !IsBlank(value);
        }

        public static T Fallback<T>(T? value, T fallback)
        {
            if (value == null)
                return fallback;

            if (value is string s && string.IsNullOrWhiteSpace(s))
                return fallback;

            return value;
        }

        public static string RemoveLeading(string? text, char character)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var start = 0;
            var source = text!;
            while (start < source.Length && source[start] == character)
                start++;

            return source.Substring(start);
        }

        public static string RemoveTrailing(string? text, char character)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var source = text!;
            var end = source.Length - 1;
            while (end >= 0 && source[end] == character)
                end--;

            return source.Substring(0, end + 1);
        }

        public static string[] CleanSplit(string? text, string separator = "\n")
        {
            if (text == null)
                return new string[0];
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            var pieces = text.Split(new[] { separator }, StringSplitOptions.None);
            var cleaned = new List<string>(pieces.Length);
            for (var i = 0; i < pieces.Length; i++)
            {
                var item = pieces[i].Trim();
                if (item.Length > 0)
                    cleaned.Add(item);
            }

            return cleaned.ToArray();
        }

        public static string StripOuterQuotes(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var source = text!;
            var start = 0;
            var end = source.Length - 1;
            while (end - start >= 1)
            {
                var first = source[start];
                var last = source[end];
                if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                {
                    start++;
                    end--;
                    continue;
                }

                break;
            }

            return source.Substring(start, end - start + 1);
        }

        public static string Ulid(Func<double> random, long? timestampMilliseconds = null)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var timestamp = timestampMilliseconds ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (timestamp < 0)
                throw new ArgumentOutOfRangeException(nameof(timestampMilliseconds));

            var time = new char[10];
            var t = timestamp;
            for (var i = 9; i >= 0; i--)
            {
                time[i] = UlidChars[(int)(t % 32)];
                t /= 32;
            }

            var output = new char[26];
            Array.Copy(time, output, time.Length);
            for (var i = 10; i < output.Length; i++)
            {
                var value = MathHelpers.Clamp(random(), 0, 0.999999999999);
                output[i] = UlidChars[(int)Math.Floor(value * 32)];
            }

            return new string(output);
        }

        public static string GeneratePredictableKey(string prefix, string prompt, string suffix)
        {
            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));
            if (suffix == null)
                throw new ArgumentNullException(nameof(suffix));

            var slug = Slugify(prompt);
            if (slug.Length > 32)
                slug = slug.Substring(0, 32);

            var hash = HashHelpers.Sha1Hex(prompt).Substring(0, 8);
            return prefix.TrimEnd('/') + "/" + slug + "-" + hash + "." + suffix.TrimStart('.');
        }

        private static void AppendCollapsedReplacement(StringBuilder builder, char replacement, ref bool previousWasReplacement)
        {
            if (builder.Length == 0 || previousWasReplacement)
                return;

            builder.Append(replacement);
            previousWasReplacement = true;
        }

        private static bool ShouldParameterize(char c)
        {
            if (c == '\u200B' || c == '\u200C' || c == '\u200D' || c == '\uFEFF' || c == '\u2060' || c == '\u00A0')
                return true;

            var category = char.GetUnicodeCategory(c);
            switch (category)
            {
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                case UnicodeCategory.OpenPunctuation:
                case UnicodeCategory.ClosePunctuation:
                case UnicodeCategory.InitialQuotePunctuation:
                case UnicodeCategory.FinalQuotePunctuation:
                case UnicodeCategory.OtherPunctuation:
                case UnicodeCategory.MathSymbol:
                case UnicodeCategory.CurrencySymbol:
                case UnicodeCategory.ModifierSymbol:
                case UnicodeCategory.OtherSymbol:
                case UnicodeCategory.Control:
                case UnicodeCategory.Format:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.EnclosingMark:
                    return true;
                default:
                    return char.IsWhiteSpace(c);
            }
        }
    }
}
