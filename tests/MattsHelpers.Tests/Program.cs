using MattsHelpers;

static void Equal<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"{name}: expected {expected}, got {actual}");
}

static void True(bool value, string name)
{
    if (!value)
        throw new Exception($"{name}: expected true");
}

static void Near(double expected, double actual, string name, double tolerance = 0.0000001)
{
    if (Math.Abs(expected - actual) > tolerance)
        throw new Exception($"{name}: expected {expected}, got {actual}");
}

Near(0.5, MathHelpers.Normalize(5, 0, 10), nameof(MathHelpers.Normalize));
Near(15, MathHelpers.PercentToValue(0.5, 10, 20), nameof(MathHelpers.PercentToValue));
Equal(4, MathHelpers.PositiveModulo(-1, 5), nameof(MathHelpers.PositiveModulo));
Equal(7.5, MathHelpers.RoundToNearest(7.4, 2.5), nameof(MathHelpers.RoundToNearest));

Equal(0x4f9f2cabu, HashHelpers.Fnv1A32("hello"), nameof(HashHelpers.Fnv1A32));
Equal(0xa430d84680aabd0bUL, HashHelpers.Fnv1A64("hello"), nameof(HashHelpers.Fnv1A64));
Equal("a9993e364706816aba3e25717850c26c9cd0d89d", HashHelpers.Sha1Hex("abc"), nameof(HashHelpers.Sha1Hex));

True(UrlHelpers.IsValidHttpUrl("//example.com/path"), nameof(UrlHelpers.IsValidHttpUrl));
Equal(HttpVerb.Post, UrlHelpers.ToHttpVerb(" post "), nameof(UrlHelpers.ToHttpVerb));
Equal("POST", HttpVerb.Post.ToHttpMethodName(), nameof(UrlHelpers.ToHttpMethodName));

Equal("a_b_c", TextHelpers.Slugify("a b!c"), nameof(TextHelpers.Slugify));
Equal("Hello world", TextHelpers.Smoosh("  Hello \n\t world "), nameof(TextHelpers.Smoosh));
Equal("hello", TextHelpers.StripOuterQuotes("\"'hello'\""), nameof(TextHelpers.StripOuterQuotes));
Equal("prefix/Hello_world-7b502c3a.txt", TextHelpers.GeneratePredictableKey("prefix", "Hello world", "txt"), nameof(TextHelpers.GeneratePredictableKey));

Equal(2, CollectionHelpers.ElementAtWrapped(new[] { 1, 2, 3 }, -2), nameof(CollectionHelpers.ElementAtWrapped));
Equal("b", CollectionHelpers.PickByPercent(0.5, new[] { "a", "b" }), nameof(CollectionHelpers.PickByPercent));

var rng = new DeterministicRandom("seed");
var first = rng.NextDouble();
var secondRng = new DeterministicRandom("seed");
Near(first, secondRng.NextDouble(), nameof(DeterministicRandom));
Equal(1, rng.Cycle, nameof(DeterministicRandom.Cycle));

var merged = DataHelpers.DeepMerge(
    new Dictionary<string, object?> { ["a"] = 1, ["nested"] = new Dictionary<string, object?> { ["x"] = 1 } },
    new Dictionary<string, object?> { ["nested"] = new Dictionary<string, object?> { ["y"] = 2 } });
var nested = (Dictionary<string, object?>)merged["nested"]!;
Equal(1, nested["x"], nameof(DataHelpers.DeepMerge));
Equal(2, nested["y"], nameof(DataHelpers.DeepMerge));

Console.WriteLine("All smoke tests passed.");
