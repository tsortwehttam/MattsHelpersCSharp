using System;

namespace MattsHelpers
{
    public static class UrlHelpers
    {
        public static bool IsValidHttpUrl(string? value, bool allowSchemeRelative = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var candidate = value!.Trim();
            if (ContainsWhiteSpace(candidate))
                return false;

            if (allowSchemeRelative && candidate.StartsWith("//", StringComparison.Ordinal))
                candidate = "http:" + candidate;

            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
                return false;

            return (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                   && !string.IsNullOrEmpty(uri.Host);
        }

        public static HttpVerb ToHttpVerb(string? method, HttpVerb fallback = HttpVerb.Get)
        {
            if (string.IsNullOrWhiteSpace(method))
                return fallback;

            switch (method!.Trim().ToUpperInvariant())
            {
                case "GET":
                    return HttpVerb.Get;
                case "POST":
                    return HttpVerb.Post;
                case "PUT":
                    return HttpVerb.Put;
                case "DELETE":
                    return HttpVerb.Delete;
                default:
                    return fallback;
            }
        }

        public static string ToHttpMethodName(this HttpVerb verb)
        {
            switch (verb)
            {
                case HttpVerb.Post:
                    return "POST";
                case HttpVerb.Put:
                    return "PUT";
                case HttpVerb.Delete:
                    return "DELETE";
                default:
                    return "GET";
            }
        }

        private static bool ContainsWhiteSpace(string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i]))
                    return true;
            }

            return false;
        }
    }
}
