using System;
using System.Text;

namespace MattsHelpers
{
    public static class HashHelpers
    {
        public static uint Fnv1A32(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            unchecked
            {
                var hash = 0x811c9dc5u;
                for (var i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 0x01000193u;
                }

                return hash;
            }
        }

        public static ulong Fnv1A64(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            unchecked
            {
                var hash = 0xcbf29ce484222325UL;
                const ulong prime = 0x100000001b3UL;
                for (var i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= prime;
                }

                return hash;
            }
        }

        public static uint Hash32(uint value)
        {
            unchecked
            {
                value = ((value >> 16) ^ value) * 0x45d9f3bu;
                value = ((value >> 16) ^ value) * 0x45d9f3bu;
                return (value >> 16) ^ value;
            }
        }

        public static string Sha1Hex(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var bytes = Encoding.UTF8.GetBytes(input);
            var paddedLength = (((bytes.Length + 8) >> 6) + 1) << 6;
            var data = new byte[paddedLength];
            Buffer.BlockCopy(bytes, 0, data, 0, bytes.Length);
            data[bytes.Length] = 0x80;

            var bitLength = (ulong)bytes.Length * 8;
            for (var i = 0; i < 8; i++)
                data[paddedLength - 1 - i] = (byte)(bitLength >> (8 * i));

            unchecked
            {
                var h0 = 0x67452301u;
                var h1 = 0xefcdab89u;
                var h2 = 0x98badcfeu;
                var h3 = 0x10325476u;
                var h4 = 0xc3d2e1f0u;
                var w = new uint[80];

                for (var offset = 0; offset < data.Length; offset += 64)
                {
                    for (var i = 0; i < 16; i++)
                    {
                        var j = offset + i * 4;
                        w[i] = ((uint)data[j] << 24)
                               | ((uint)data[j + 1] << 16)
                               | ((uint)data[j + 2] << 8)
                               | data[j + 3];
                    }

                    for (var i = 16; i < 80; i++)
                        w[i] = RotateLeft(w[i - 3] ^ w[i - 8] ^ w[i - 14] ^ w[i - 16], 1);

                    var a = h0;
                    var b = h1;
                    var c = h2;
                    var d = h3;
                    var e = h4;

                    for (var i = 0; i < 80; i++)
                    {
                        uint f;
                        uint k;
                        if (i < 20)
                        {
                            f = (b & c) | (~b & d);
                            k = 0x5a827999u;
                        }
                        else if (i < 40)
                        {
                            f = b ^ c ^ d;
                            k = 0x6ed9eba1u;
                        }
                        else if (i < 60)
                        {
                            f = (b & c) | (b & d) | (c & d);
                            k = 0x8f1bbcdcu;
                        }
                        else
                        {
                            f = b ^ c ^ d;
                            k = 0xca62c1d6u;
                        }

                        var temp = RotateLeft(a, 5) + f + e + k + w[i];
                        e = d;
                        d = c;
                        c = RotateLeft(b, 30);
                        b = a;
                        a = temp;
                    }

                    h0 += a;
                    h1 += b;
                    h2 += c;
                    h3 += d;
                    h4 += e;
                }

                return ToHex8(h0) + ToHex8(h1) + ToHex8(h2) + ToHex8(h3) + ToHex8(h4);
            }
        }

        internal static uint StableSeed(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            unchecked
            {
                var hash = 0;
                for (var i = 0; i < value.Length; i++)
                    hash = ((hash << 5) - hash) + value[i];

                return (uint)hash;
            }
        }

        private static uint RotateLeft(uint value, int bits)
        {
            return (value << bits) | (value >> (32 - bits));
        }

        private static string ToHex8(uint value)
        {
            return value.ToString("x8");
        }
    }
}
