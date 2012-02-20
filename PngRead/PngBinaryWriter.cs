using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace stefc.pnglib
{
	public class PngBinaryWriter : BinaryWriter
	{
		public PngBinaryWriter (Stream output) : base(output)
		{
		}
		
		
		public void WriteSignatur()
		{
			Write(Png.Signatur);
		}
		
		public void Write(Chunk chunk)
		{
			Write(PngHelper.DWordToInt(chunk.Length));
			Write(chunk.Type);
			Write(chunk.Data,0,chunk.Data.Length);
			Write(PngHelper.DWordToInt((int)Crc32.Calc(chunk.Type,chunk.Data)));
		}
		
		public void WriteEnd()
		{
			Write(new Chunk(0,Png.IEND,new byte[0]));			
		}
		
		public void WriteMonochromePalette()
		{
			byte[] buffer = new byte[]{255,255,255,0,0,0};
			
			Write(new Chunk(buffer.Length,Png.PLTE,buffer));
		}
		
		public void WriteHeader(PngHeader header)
		{
			byte[] buffer = new byte[13];
			
			Array.Copy(
				BitConverter.GetBytes(
					PngHelper.DWordToInt(header.Width)),
				0,buffer,0,4);
			Array.Copy(
				BitConverter.GetBytes(
					PngHelper.DWordToInt(header.Height)),
				0,buffer,4,4);
			buffer[8]=header.BitDepth;
			buffer[9]=(byte)header.ColorModel;
			buffer[10]=0; // compress
			buffer[11]=0; // filter
			buffer[12]=0; // no interlace

			Write(new Chunk(buffer.Length,Png.IHDR,buffer));
		}
		
		public void WriteData(byte[] data)
		{
			int ofs = 0;
			int len= data.Length;
			while(ofs < len)
			{
				int l = len - ofs;
				if (l>Png.BlockSize) l=Png.BlockSize;
				
				byte[] buffer=new byte[l];
				Array.Copy(data,ofs,buffer,0,l);
			
				Write(new Chunk(buffer.Length,Png.IDAT,buffer));
				ofs += Png.BlockSize;
			}
		}
		
	}
}

