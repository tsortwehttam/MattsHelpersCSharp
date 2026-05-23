# MattsHelpers

Small C# helper library converted from the TypeScript utilities in `notes.txt`.

The library targets `netstandard2.0` and avoids reflection-heavy, source-generated, unsafe, dynamic, and package-dependent code. That keeps it friendly to Unity IL2CPP, Native AOT, MonoGame, browser/WebGL-style builds, iOS, macOS, Windows, and console ports.

## API Shape

- `MathHelpers`: clamp, normalize, modulo, rounding, parsing, midpoint helpers.
- `CurveHelpers`: deterministic curve and distribution transforms.
- `DeterministicRandom`: seeded Mulberry32-style PRNG with dice, shuffle, weighted key selection, normal sampling, and ULIDs.
- `HashHelpers`: FNV-1a 32/64, integer hash, pure C# SHA-1 hex.
- `TextHelpers`: slug/key helpers, whitespace cleanup, parameterization, quote stripping, presence/fallback helpers.
- `CollectionHelpers`: unique, intersection, wrapped indexing, percent-based picking.
- `DataHelpers`: serial-style deep clone and dictionary deep merge without serializers.
- `UrlHelpers`: HTTP URL validation and conservative HTTP verb parsing.

## Notes

Some TypeScript utilities were adjusted to be idiomatic C# rather than literal ports:

- Negative modulo wraps positively.
- Empty collections throw for value-returning pick methods, with `Try...` alternatives where useful.
- `DeepClone` only supports serial-style primitives, dictionaries, and enumerables. It intentionally does not reflect over arbitrary objects.
- SHA-1 is implemented directly instead of using platform crypto APIs, so deterministic key generation stays available in restricted runtimes.

## Verify

```bash
dotnet build src/MattsHelpers/MattsHelpers.csproj
dotnet run --project tests/MattsHelpers.Tests/MattsHelpers.Tests.csproj
```
