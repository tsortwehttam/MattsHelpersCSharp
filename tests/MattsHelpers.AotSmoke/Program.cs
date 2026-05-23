using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MattsHelpers;

var hash = HashHelpers.Fnv1A32("aot-smoke");
if (hash == 0u) throw new Exception("hash zero");

var clamped = MathHelpers.Clamp(2.5, 0, 1);
if (clamped != 1d) throw new Exception("clamp mismatch");

var rng = new DeterministicRandom("seed", 3);
_ = rng.NextDouble();

var items = new List<int> { 1, 2, 3, 4 };
var mapped = await AsyncHelpers.MapParallelAsync(items, (x, i) => Task.FromResult(x * 2)).ConfigureAwait(false);
if (mapped.Length != 4 || mapped[3] != 8) throw new Exception("async map mismatch");

Console.WriteLine("MattsHelpers AOT smoke ok: hash=" + hash);
