using System.IO;

namespace Luna.Assets {
	class LTrack {
		public string ModelName;
		public LTrack(Game _assets, BinaryReader _reader) {
			ModelName = _assets.GetString(_reader.ReadInt32());
		}
	}
}