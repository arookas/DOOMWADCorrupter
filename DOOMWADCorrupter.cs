using arookas.Collections;
using arookas.IO.Binary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace arookas {
	class DOOMWADCorrupter {
		static aCommandLine cmd;
		static CorrupterOptions options;
		static Random rnd;

		static readonly string sSeparator = new String('=', 79);
		static readonly string[] sZDOOMLumps = {
			"ALTHUDCF", "ANIMDEFS", "CVARINFO", "DECALDEF", "DECORATE", "DEHACKED", "DEHSUPP", "DMXGUS",
			"FSGLOBAL", "FONTDEFS", "GAMEINFO", "GLDEFS", "KEYCONF", "LANGUAGE", "LOADACS", "LOCKDEFS",
			"MAPINFO", "MENUDEF", "MODELDEF", "MUSINFO", "PALVERS", "SBARINFO", "SCRIPTS", "SECRETS",
			"SNDINFO", "SNDSEQ", "TEAMINFO", "TERRAIN", "TEXTCOLO", "TEXTURES", "VOXELDEF", "XHAIRS",
			"X11R6RGB", "ZMAPINFO", "ANIMATED", "BEHAVIOR", "GENMIDI", "SNDCURVE", "SWITCHES",
		};

		static void Main(string[] arguments) {
			Message("doomwadcorrupter v{0} arookas", new Version(0, 1, 12));
			Separator();
			if (arguments == null || arguments.Length < 2) {
				Message("Usage: doomwadcorrupter <input.wad> <output.wad> [options]");
				Message();
				Message("Options:");
				Message("    -start <value>");
				Message("    -end <value>");
				Message("    -inc <value>");
				Message("    -mode <type> [<value>]");
				Message("    -skip <filter> [<filter> [...]]");
				Message("    -only <filter> [<filter> [...]]");
				Message("    -zdoom");
				Message();
				Message("For more detailed instructions, refer to the official repo page.");
				Pause();
				Exit(false);
			}
			var inputWAD = arguments[0];
			var outputWAD = arguments[1];
			cmd = new aCommandLine(arguments.Skip(2).ToArray());
			options = new CorrupterOptions(cmd);
			DisplayOptions(inputWAD, outputWAD);

			int lumpCount;
			var lumpsCorrupted = 0;
			var lumpsSkipped = 0;
			var bytesCorrupted = 0;
			rnd = new Random((uint)options.CorruptSeed);
			var timeTaken = Stopwatch.StartNew();
			using (var instream = OpenWAD(inputWAD)) {
				var reader = new aBinaryReader(instream, Endianness.Little, Encoding.ASCII);

				// header
				var wadType = reader.ReadString(4);

				if (wadType != "IWAD" && wadType != "PWAD") {
					Error("Input file is not a DOOM WAD.");
				}

				lumpCount = reader.ReadS32();
				var directoryOffset = reader.ReadS32();

				// directory
				reader.Goto(directoryOffset);
				var lumps = aCollection.Initialize(lumpCount, () => new Lump(reader));

				using (var outstream = CreateWAD(outputWAD)) {
					var writer = new aBinaryWriter(outstream, Endianness.Little, Encoding.ASCII);
					// header
					writer.WriteString(wadType);
					writer.WriteS32(lumpCount);
					writer.WriteS32(directoryOffset);

					// data
					var corruptBuff = new byte[options.Increment];
					var startBuff = new byte[options.Start];
					var ns = LumpNamespace.Global;

					foreach (var lump in lumps) {
						reader.Goto(lump.Start);
						writer.Goto(lump.Start);
						CheckNamespaceMarker(lump, ref ns);
						if (options.Filter.IsCorruptable(lump.Name, ns) && !(options.ZDOOM && IsZDOOMLump(lump.Name))) {
							++lumpsCorrupted;
							var i = options.Start;
							var end = options.End ?? lump.Length;
							if (i > 0) {
								var count = (int)System.Math.Min(lump.Length, i);
								reader.Read(startBuff, count);
								writer.Write8s(startBuff, count);
							}
							while (i < lump.Length && i < end) {
								Status("Corrupting '{0}'... (0x{1:X8} / 0x{2:X8})", lump.Name, i, lump.Length);
								var count = (int)System.Math.Min(lump.Length - i, options.Increment);
								reader.Read(corruptBuff, count);
								CorruptByte(ref corruptBuff[0], options.CorruptMode, options.CorruptValue);
								writer.Write8s(corruptBuff, count);
								++bytesCorrupted;
								i += count;
							}
						}
						else {
							++lumpsSkipped;
							writer.Write8s(reader.Read8s(lump.Length));
						}
					}

					// directory
					writer.Goto(directoryOffset);
					foreach (var lump in lumps) {
						Status("Writing lump directory for '{0}'...", lump.Name);
						lump.ToStream(writer);
					}
				}
			}

			timeTaken.Stop();
			Status("Finished corrupting.\n");
			Separator();
			Message("                       Files : {0}", lumpCount);
			Message("             Files corrupted : {0}", lumpsCorrupted);
			Message("               Files skipped : {0}", lumpsSkipped);
			Message("Bytes mercilessly sacrificed : {0}", bytesCorrupted);
			Message("                  Time taken : {0}", timeTaken.Elapsed.ToString("g"));
			Message("                 Finished at : {0}", DateTime.Now.ToString("HH:mm:ss tt"));
			Pause();
		}
		static void DisplayOptions(string inputWAD, string outputWAD) {
			Message("    Input WAD : {0}", Path.GetFileName(inputWAD));
			Message("   Output WAD : {0}", Path.GetFileName(outputWAD));
			Message("        Start : {0}", options.Start);

			if (options.End != null) {
				Message("          End : {0}", options.End);
			}

			Message("    Increment : {0}", options.Increment);
			if (options.CorruptMode == CorruptMode.Random) {
				Message("         Mode : {0} ({1})", options.CorruptMode, options.CorruptSeed);
			}
			else {
				Message("         Mode : {0} {1}", options.CorruptMode, options.CorruptValue);
			}
			Message("Filters:");
			Message(options.Filter.ToString());
			if (options.ZDOOM) {
				Message("** Skip G/ZDOOM lumps enabled **");
			}
			Separator();
		}
		static void CheckNamespaceMarker(Lump lump, ref LumpNamespace ns) {
			switch (lump.Name) {
				case "F_START":
				case "FF_START": {
					ns = LumpNamespace.Flats;
					break;
				}
				case "S_START":
				case "SS_START": {
					ns = LumpNamespace.Sprites;
					break;
				}
				case "P_START":
				case "PP_START": {
					ns = LumpNamespace.Patches;
					break;
				}
				case "F_END":
				case "FF_END":
				case "S_END":
				case "SS_END":
				case "P_END":
				case "PP_END": {
					ns = LumpNamespace.Global;
					break;
				}
			}
		}
		static void CorruptByte(ref byte b, CorruptMode mode, byte value) {
			switch (mode) {
				case CorruptMode.Add: b += value; break;
				case CorruptMode.Sub: b -= value; break;
				case CorruptMode.Mul: b *= value; break;
				case CorruptMode.Div: b /= value; break;
				case CorruptMode.Mod: b %= value; break;
				case CorruptMode.OR: b |= value; break;
				case CorruptMode.XOR: b ^= value; break;
				case CorruptMode.AND: b &= value; break;
				case CorruptMode.NOT: b = (byte)~b; break;
				case CorruptMode.LSH: b <<= value; break;
				case CorruptMode.RSH: b >>= value; break;
				case CorruptMode.ROL: b = (byte)((b << value) | (b >> (8 - value))); break;
				case CorruptMode.ROR: b = (byte)((b >> value) | (b << (8 - value))); break;
				case CorruptMode.Replace: b = value; break;
				case CorruptMode.Random: b = (byte)rnd.NextInt32(); break;
			}
		}
		static FileStream OpenWAD(string path) {
			try {
				return File.OpenRead(path);
			}
			catch {
				Error("Failed to open the WAD file '{0}'. Check to make sure the file exists and is not already in use.", path);
				return null;
			}
		}
		static FileStream CreateWAD(string path) {
			try {
				return File.Create(path);
			}
			catch {
				Error("Failed to create WAD file '{0}'. Check to make sure the program has access to the given path.", path);
				return null;
			}
		}

		static bool IsZDOOMLump(string name) { return sZDOOMLumps.Any(i => name.StartsWith(i)); }

		public static void Separator() {
			Message(sSeparator);
		}
		public static void Message() {
			Console.WriteLine();
		}
		public static void Message(string format, params object[] args) {
			Console.WriteLine(format, args);
		}
		public static void Warning(string format, params object[] args) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING: ");
			Message(format, args);
			Console.ResetColor();
		}
		public static void Error(string format, params object[] args) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR: ");
			Message(format, args);
			Console.ResetColor();
			Pause();
			Exit(true);
		}
		public static void Status(string format, params object[] args) {
			Console.Write("\r{0,-79}", String.Format(format, args));
		}
		public static void Pause() {
			Console.ReadKey();
		}
		public static void Exit(bool error) {
			Environment.Exit(error ? 1 : 0);
		}
	}
}