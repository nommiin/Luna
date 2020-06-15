using System.Drawing;
using System.IO;

namespace Luna.Assets
{
	class LEmbTexture
	{
		public int Scaled;
		public int MIPSGenerated;
		public Image Texture;
		public LEmbTexture(Game _assets, BinaryReader _reader){
			Scaled = _reader.ReadInt32();//scaled
			MIPSGenerated = _reader.ReadInt32();//mips generated
			Texture = Image.FromStream(_reader.BaseStream);
		}
	}
}