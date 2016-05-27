using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace arookas
{
	class CorrupterOptions
	{
		Random rnd;

		public int Start { get; private set; }
		public int? End { get; private set; }
		public int Increment { get; private set; }
		public CorruptMode CorruptMode { get; private set; }
		public byte CorruptValue { get; private set; }
		public int CorruptSeed { get; private set; }
		public WADFilter Filter { get; private set; }
		public bool ZDOOM { get; private set; }

		static readonly Regex intRegex = new Regex(@"^(?'isNeg'-)?(?'value'[0-9a-f]+)(?'isHex'h)?$", RegexOptions.IgnoreCase);
		static readonly int corruptModes = Enum.GetValues(typeof(CorruptMode)).Length;

		public CorrupterOptions()
		{
			rnd = new Random();
			Filter = new WADFilter();
			Start = rnd % 50;
			End = null;
			Increment = 1 + (rnd % 32);
			CorruptMode = (CorruptMode)(rnd % corruptModes);
			CorruptValue = (byte)(rnd % 256);
			CorruptSeed = rnd;
			ZDOOM = false;
		}
		public CorrupterOptions(aCommandLine cmd)
			: this()
		{
			foreach (var param in cmd)
			{
				switch (param.Name.ToLowerInvariant())
				{
					case "-start": LoadStart(param); continue;
					case "-end": LoadEnd(param); continue;
					case "-inc": LoadInc(param); continue;
					case "-mode": LoadMode(param); continue;
					case "-skip": LoadSkip(param); continue;
					case "-only": LoadOnly(param); continue;
					case "-zdoom": LoadZDOOM(param); continue;
				}
				DOOMWADCorrupter.Warning("Skipping unknown option '{0}'...", param.Name);
			}
		}

		void LoadStart(aCommandLineParameter param)
		{
			if (param.Count == 0)
			{
				DOOMWADCorrupter.Warning("Missing argument for -start option.");
				return;
			}
			if (param[0] == "??")
			{
				if (param.Count < 3)
				{
					DOOMWADCorrupter.Warning("Missing argument for -start option.");
					return;
				}
				else if (param.Count > 3)
				{
					DOOMWADCorrupter.Warning("-start option has extra arguments.");
				}
				int min, max;
				if (!ParseInt(param[1], 0, Int32.MaxValue, out min))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -start option.", param[1]);
					return;
				}
				if (!ParseInt(param[2], 0, Int32.MaxValue, out max))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -start option.", param[2]);
					return;
				}
				if (min == max)
				{
					Start = min;
				}
				else if (max < min)
				{
					Start = max + (rnd % min + 1);
				}
				else
				{
					Start = min + (rnd % max + 1);
				}
			}
			else
			{
				int value;
				if (!ParseInt(param[0], 0, Int32.MaxValue, out value))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -start option.", param[2]);
					return;
				}
				Start = value;
			}
		}
		void LoadEnd(aCommandLineParameter param)
		{
			if (param.Count == 0)
			{
				DOOMWADCorrupter.Warning("Missing argument for -end option.");
				return;
			}
			if (param[0] == "??")
			{
				if (param.Count < 3)
				{
					DOOMWADCorrupter.Warning("Missing argument for -end option.");
					return;
				}
				else if (param.Count > 3)
				{
					DOOMWADCorrupter.Warning("-end option has extra arguments.");
				}
				int min, max;
				if (!ParseInt(param[1], 0, Int32.MaxValue, out min))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -end option.", param[1]);
					return;
				}
				if (!ParseInt(param[2], 0, Int32.MaxValue, out max))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -end option.", param[2]);
					return;
				}
				if (min == max)
				{
					End = min;
				}
				else if (max < min)
				{
					End = max + (rnd % min + 1);
				}
				else
				{
					End = min + (rnd % max + 1);
				}
			}
			else
			{
				int value;
				if (!ParseInt(param[0], 0, Int32.MaxValue, out value))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -end option.", param[2]);
					return;
				}
				End = value;
			}
		}
		void LoadInc(aCommandLineParameter param)
		{
			if (param.Count == 0)
			{
				DOOMWADCorrupter.Warning("Missing argument for -inc option.");
				return;
			}
			if (param[0] == "??")
			{
				if (param.Count < 3)
				{
					DOOMWADCorrupter.Warning("Missing argument for -inc option.");
					return;
				}
				else if (param.Count > 3)
				{
					DOOMWADCorrupter.Warning("-inc option has extra arguments.");
				}
				int min, max;
				if (!ParseInt(param[1], 0, Int32.MaxValue, out min))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -inc option.", param[1]);
					return;
				}
				if (!ParseInt(param[2], 0, Int32.MaxValue, out max))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -inc option.", param[2]);
					return;
				}
				if (min == max)
				{
					Increment = min;
				}
				else if (max < min)
				{
					Increment = max + (rnd % min + 1);
				}
				else
				{
					Increment = min + (rnd % max + 1);
				}
			}
			else
			{
				int value;
				if (!ParseInt(param[0], 0, Int32.MaxValue, out value))
				{
					DOOMWADCorrupter.Warning("Invalid argument '{0}' for -inc option.", param[2]);
					return;
				}
				Increment = value;
			}
		}
		void LoadMode(aCommandLineParameter param)
		{
			if (param.Count == 0)
			{
				DOOMWADCorrupter.Warning("Missing argument for -mode option.");
				return;
			}
			if (param[0] == "??")
			{
				if (param.Count != 1)
				{
					DOOMWADCorrupter.Warning("-mode option has extra arguments.");
				}
				return;
			}
			else if (param.Count > 2)
			{
				DOOMWADCorrupter.Warning("-mode option has extra arguments.");
			}

			CorruptMode mode;
			if (!Enum.TryParse(param[0], true, out mode))
			{
				DOOMWADCorrupter.Warning("Invalid argument '{0}' for -mode option.", param[0]);
				return;
			}
			CorruptMode = mode;
			if (mode == CorruptMode.NOT && param.Count > 1)
			{
				DOOMWADCorrupter.Warning("-mode option has extra arguments.");
				return;
			}
			if (param[1] == "??")
			{
				return;
			}
			int value;
			if (!ParseInt(param[1], 0, 255, out value))
			{
				DOOMWADCorrupter.Warning("Invalid argument '{0}' for -mode option.", param[1]);
				return;
			}
			CorruptValue = (byte)value;
		}
		void LoadSkip(aCommandLineParameter param)
		{
			if (param.Count == 0)
			{
				DOOMWADCorrupter.Warning("-skip has no filters.");
			}
			else
			{
				Filter.Skip(param);
			}
		}
		void LoadOnly(aCommandLineParameter param)
		{
			if (param.Count == 0)
			{
				DOOMWADCorrupter.Warning("-only has no filters.");
			}
			else
			{
				Filter.Only(param);
			}
		}
		void LoadZDOOM(aCommandLineParameter param)
		{
			if (param.Count > 0)
			{
				DOOMWADCorrupter.Warning("-zdoom option has extra arguments.");
			}
			ZDOOM = true;
		}

		static bool ParseInt(string str, int min, int max, out int result)
		{
			result = 0;
			var match = intRegex.Match(str);
			if (!match.Success)
			{
				return false;
			}
			string value = match.Groups["value"].Value;
			bool isHex = match.Groups["isHex"].Success;
			bool isNeg = match.Groups["isNeg"].Success;
			NumberStyles style = NumberStyles.None;
			if (isHex)
			{
				style |= NumberStyles.AllowHexSpecifier;
			}
			if (!Int32.TryParse(value, style, null, out result))
			{
				return false;
			}
			if (isNeg)
			{
				result = -result;
			}
			return result >= min && result <= max;
		}
	}

	enum CorruptMode
	{
		Add,
		Sub,
		Mul,
		Div,
		Mod,
		OR,
		XOR,
		AND,
		NOT,
		LSH,
		RSH,
		ROL,
		ROR,
		Replace,
		Random,
	}
}
