using System.Drawing;
using System.IO;

namespace Luna.Assets
{
	class LEmbTexture
	{
		public uint Scaled;
		public uint MIPSGenerated;
		public Image Texture;
		public LEmbTexture(Game _assets, BinaryReader _reader){
			_reader.ReadInt32();//scaled
			_reader.ReadInt32();//mips generated
			Image.FromStream(_reader.BaseStream);
		}
	}
}