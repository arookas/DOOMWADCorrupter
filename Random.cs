using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arookas {
	public class Random {
		uint a, b, c, d;

		public Random()
			: this(DefaultSeed) {

		}
		public Random(uint seed) {
			a = 0x13200231;
			b = seed;
			c = seed;
			d = seed;
		}

		public static uint DefaultSeed { get { return (uint)DateTime.Now.Millisecond; } }

		uint ROL(uint x, int r) {
			return ((x << r) | (x >> (32 - r)));
		}

		public uint NextUInt32() {
			uint e = a - ROL(b, 27);
			a = b ^ ROL(c, 17);
			b = c + d;
			c = d + e;
			d = e + a;

			return d;
		}
		public int NextInt32() { return (int)(NextUInt32() & ~0x80000000); }
		public float NextSingle() { return (float)((double)NextUInt32() / UInt32.MaxValue); }

		public int Range(int max) { return (int)this % max; }
		public int Range(int min, int max) { return min + Range(max - min); }

		public static implicit operator bool(Random rnd) { return (rnd % 2) == 1; }
		public static implicit operator uint(Random rnd) { return rnd.NextUInt32(); }
		public static implicit operator int(Random rnd) { return rnd.NextInt32(); }
		public static implicit operator float(Random rnd) { return rnd.NextSingle(); }
		public static int operator %(Random rnd, int max) { return rnd.Range(max); }
	}
}
