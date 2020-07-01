using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Luna.Types;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Luna.Assets
{
    class LTexturePageEntry
    {
        public int Base;
        public short X;
        public short Y;
        public short Width;
        public short Height;
        public short XOrigin;
        public short YOrigin;
        public short CropWidth;
        public short CropHeight;
        public short OriginalWidth;
        public short OriginalHeight;
        public LTexture TexturePage;
        public Bitmap SubImage;
        public int GLTexture;
        public LTexturePageEntry(Game _assets, BinaryReader _reader)
        {
            Base = (int) _reader.BaseStream.Position;
            X = _reader.ReadInt16();
            Y = _reader.ReadInt16();
            Width = _reader.ReadInt16();
            Height = _reader.ReadInt16();
            XOrigin = _reader.ReadInt16();
            YOrigin = _reader.ReadInt16();
            CropWidth = _reader.ReadInt16();
            CropHeight = _reader.ReadInt16();
            OriginalWidth = _reader.ReadInt16();
            OriginalHeight = _reader.ReadInt16();
            //ow and oh are same as width/height? guessing those two are the original width and height and it's some weird mipmaps stuff
            TexturePage = _assets.TexturePages[_reader.ReadInt16()];
        }

        public void PrepareGlTexture() {
            SubImage = TexturePage.BitmapData.Clone(new Rectangle(X, Y, Width, Height), TexturePage.BitmapData.PixelFormat);
            GLTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, GLTexture);
            BitmapData _bmp = SubImage.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, SubImage.PixelFormat);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, SubImage.Width, SubImage.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, _bmp.Scan0);
            SubImage.UnlockBits(_bmp);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
    }
}