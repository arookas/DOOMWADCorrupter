using arookas.IO.Binary;

namespace arookas {
	struct Lump {
		public int Start { get; private set; }
		public int Length { get; private set; }
		public string Name { get; private set; }

		public Lump(aBinaryReader reader)
			: this() {
			Start = reader.ReadS32();
			Length = reader.ReadS32();
			Name = reader.ReadString<aCSTR>(8);
		}

		public void ToStream(aBinaryWriter writer) {
			writer.WriteS32(Start);
			writer.WriteS32(Length);
			writer.WriteString<aCSTR>(Name, 8);
		}
	}

	enum LumpNamespace {
		Global,
		Sprites,
		Patches,
		Flats,
	}
}
