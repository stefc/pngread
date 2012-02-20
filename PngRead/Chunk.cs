using System;

namespace stefc.pnglib
{
	public class Chunk 
	{
		public int Length { get; private set; }
		public int Type { get; private set; }
		public byte[] Data { get; private set; }
			
		public Chunk(int length, int type, byte[] data)
		{
			Length=length;
			Type=type;
			Data=data;
		}
	}
}

