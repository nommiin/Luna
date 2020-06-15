using System.Collections.Generic;
using System.IO;
using Luna.Types;

namespace Luna.Assets
{
	class LSequence {
		public string Name;
		public int Index;
		public int PlaybackType;
		public float PlaybackSpeed;
		public int PlaybackSpeedType;//???? what is this
		public float Length;
		public int XOrigin;
		public int YOrigin;
		public float Volume;
		public List<LTrack> Tracks;
		public Dictionary<int,LSeqEvent> Events = new Dictionary<int,LSeqEvent>();

		public LSequence(Game _assets, BinaryReader _reader) {
			Name = _assets.GetString(_reader.ReadInt32());
			PlaybackType = _reader.ReadInt32();
			PlaybackSpeed = _reader.ReadSingle();
			PlaybackSpeedType = _reader.ReadInt32();
			Length = _reader.ReadSingle();
			XOrigin = _reader.ReadInt32();
			YOrigin = _reader.ReadInt32();
			Volume = _reader.ReadSingle();
			ChunkHandler.HandleKVP(_assets, _reader, delegate(int _offset) {
				LTrack _track = new LTrack(_assets, _reader);
				Tracks.Add(_track);
			});
			ChunkHandler.HandleKVP(_assets, _reader, delegate(int _offset) {
				LSeqEvent _evt = new LSeqEvent(_assets, _reader);
				Events.Add(_evt.Key,_evt);
			});
		}
	}

	class LSeqEvent {
		public int Key;
		public string Value;
		public LSeqEvent(Game _assets, BinaryReader _reader) {
			Key = _reader.ReadInt32();
			Value = _assets.GetString(_reader.ReadInt32());
		}
	}
}