using System;
using System.IO;
using System.Text;

namespace stefc.pnglib
{	
	public class PngBinaryReader : BinaryReader
	{
		
		public PngBinaryReader (Stream input):base(input)
		{
		}
		
		public int ReadDWord()
		{
			return PngHelper.DWordToInt(ReadInt32());
		}
		
		public void ReadSignatur()
		{
			long signatur = ReadInt64();
			if(signatur != Png.Signatur)
				throw new InvalidDataException("Wrong PNG Signature");
		}
					
		public Chunk ReadChunk()
		{
			int dataLength = ReadDWord();
			int type = ReadInt32();
			byte[] data = ReadBytes(dataLength);
			if(ReadDWord() != (int)Crc32.Calc(type,data))
				throw new InvalidDataException("Checksum error!");
			return new Chunk(dataLength,type,data);
		}	
	}
}

