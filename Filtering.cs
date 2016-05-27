using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace arookas {
	class WADFilter {
		List<FilterGroup> mSkips, mOnlys;

		public WADFilter() {
			mSkips = new List<FilterGroup>(5);
			mOnlys = new List<FilterGroup>(5);
		}

		public void Skip(IEnumerable<string> filters) {
			mSkips.Add(new FilterGroup(filters.Select(f => Filter.CreateFilter(f))));
		}
		public void Only(IEnumerable<string> filters) {
			mOnlys.Add(new FilterGroup(filters.Select(f => Filter.CreateFilter(f))));
		}
		public bool IsCorruptable(string name, LumpNamespace ns) {
			return (mOnlys.Count == 0 && mSkips.Count == 0) || (mOnlys.Any(f => f.IsMatch(name, ns)) && !mSkips.Any(f => f.IsMatch(name, ns)));
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder(256);
			foreach (var only in mOnlys) {
				sb.Append("- only");
				sb.Append(only.ToString());
				sb.AppendLine();
			}
			foreach (var skip in mSkips) {
				sb.Append("- skip");
				sb.Append(skip.ToString());
				sb.AppendLine();
			}
			return sb.ToString();
		}

		class FilterGroup {
			Filter[] mFilters;

			public FilterGroup(string filter) {
				this.mFilters = new Filter[1] { Filter.CreateFilter(filter) };
			}
			public FilterGroup(IEnumerable<Filter> filters) {
				this.mFilters = filters.ToArray();
			}

			public bool IsMatch(string name, LumpNamespace ns) {
				return mFilters.All(f => f.IsMatch(name, ns));
			}

			public override string ToString() {
				StringBuilder str = new StringBuilder(256);
				foreach (var filter in mFilters) {
					str.Append(' ');
					str.Append(filter.ToString());
				}
				return str.ToString();
			}
		}
		abstract class Filter {
			public abstract bool IsMatch(string name, LumpNamespace ns);

			public static Filter CreateFilter(string filter) {
				var f = filter.ToLowerInvariant();
				switch (f) {
					case "<flats>": return new NamespaceFilter(LumpNamespace.Flats);
					case "<patches>": return new NamespaceFilter(LumpNamespace.Patches);
					case "<sprites>": return new NamespaceFilter(LumpNamespace.Sprites);
					case "<maps>": return new MapFilter(MapLump.Default);
				}
				if (f.StartsWith("<maps-") && f.EndsWith(">")) {
					MapLump lumps = MapLump.None;
					foreach (var split in f.Substring(6, f.Length - 7).Split('-')) {
						switch (split) {
							case "all": lumps |= MapLump.All; continue;
							case "bm": lumps |= MapLump.BLOCKMAP; continue;
							case "ld": lumps |= MapLump.LINEDEFS; continue;
							case "n": lumps |= MapLump.NODES; continue;
							case "r": lumps |= MapLump.REJECTS; continue;
							case "s": lumps |= MapLump.SECTORS; continue;
							case "sd": lumps |= MapLump.SIDEDEFS; continue;
							case "sg": lumps |= MapLump.SEGS; continue;
							case "ss": lumps |= MapLump.SSECTORS; continue;
							case "t": lumps |= MapLump.THINGS; continue;
							case "v": lumps |= MapLump.VERTEXES; continue;
						}
					}
					return new MapFilter(lumps);
				}
				return new RegexFilter(filter);
			}

			class RegexFilter : Filter {
				Regex regex;

				public RegexFilter(string pattern) {
					regex = new Regex(pattern);
				}

				public override bool IsMatch(string name, LumpNamespace ns) {
					return regex.IsMatch(name);
				}
				public override string ToString() {
					return regex.ToString();
				}
			}
			class NamespaceFilter : Filter {
				LumpNamespace ns;

				public NamespaceFilter(LumpNamespace ns) {
					this.ns = ns;
				}

				public override bool IsMatch(string name, LumpNamespace ns) {
					return this.ns == ns;
				}
				public override string ToString() {
					return String.Format("( {0} )", ns);
				}
			}
			class MapFilter : Filter {
				MapLump lumps;

				public MapFilter(MapLump lumps) {
					this.lumps = lumps;
				}

				public override bool IsMatch(string name, LumpNamespace ns) {
					switch (name) {
						case "BLOCKMAP": return HasFlag(MapLump.BLOCKMAP);
						case "LINEDEFS": return HasFlag(MapLump.LINEDEFS);
						case "NODES": return HasFlag(MapLump.NODES);
						case "REJECTS": return HasFlag(MapLump.REJECTS);
						case "SECTORS": return HasFlag(MapLump.SECTORS);
						case "SIDEDEFS": return HasFlag(MapLump.SIDEDEFS);
						case "SEGS": return HasFlag(MapLump.SEGS);
						case "SSECTORS": return HasFlag(MapLump.SSECTORS);
						case "THINGS": return HasFlag(MapLump.THINGS);
						case "VERTEXES": return HasFlag(MapLump.VERTEXES);
					}
					return false;
				}
				bool HasFlag(MapLump lumps) { return (this.lumps & lumps) == lumps; }
				public override string ToString() {
					return String.Format("( {0} )", lumps);
				}
			}

			[Flags]
			enum MapLump {
				None = 0,
				BLOCKMAP = 1,
				LINEDEFS = 2,
				NODES = 4,
				REJECTS = 8,
				SECTORS = 16,
				SIDEDEFS = 32,
				SEGS = 64,
				SSECTORS = 128,
				THINGS = 256,
				VERTEXES = 512,

				Default = THINGS | VERTEXES | SECTORS | SSECTORS,
				All = BLOCKMAP | LINEDEFS | NODES | REJECTS | SECTORS | SIDEDEFS | SEGS | SSECTORS | THINGS | VERTEXES,
			}
		}
	}
}
