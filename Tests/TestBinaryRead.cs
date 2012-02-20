using System;
using System.IO;
using NUnit.Framework;
using stefc.pnglib;

namespace Tests
{
	[TestFixture]
	public class TestBinaryRead
	{		
		[Test]
		public void TestReadRGB24Bit()
		{
			PngHeader header = ReadFile("Samples/stefc-rgb.png");
			Assert.AreEqual(80, header.Width);
			Assert.AreEqual(93, header.Height);
			Assert.AreEqual(3, header.BytesPerPixel);
			Assert.AreEqual(8, header.BitDepth);
			Assert.AreEqual(80 * 3, header.Stride);
		}
		
		[Test]
		public void TestReadGrayscale8Bit()
		{
			PngHeader header = ReadFile("Samples/stefc-gray.png");
			Assert.AreEqual(80, header.Width);
			Assert.AreEqual(93, header.Height);
			Assert.AreEqual(1, header.BytesPerPixel);
			Assert.AreEqual(8, header.BitDepth);
			Assert.AreEqual(80 * 1, header.Stride);
		}
		
		[Test]
		public void TestReadBlackWhite1Bit()
		{
			PngHeader header = ReadFile("Samples/stefc-bw.png");
			Assert.AreEqual(80, header.Width);
			Assert.AreEqual(93, header.Height);
			Assert.AreEqual(1, header.BytesPerPixel);
			Assert.AreEqual(1, header.BitDepth);
			Assert.AreEqual(80 / 8, header.Stride);
		}
		
		private PngHeader ReadFile(string fileName)
		{
			PngHeader result = null;
			using (Stream stream = new FileStream(fileName,FileMode.Open))
			{
				using (PngBinaryReader reader = new PngBinaryReader(stream))
				{
					reader.ReadSignatur();
					
					// Header lesen
					Chunk chunk = reader.ReadChunk();
					if((chunk.Length!=13) || (chunk.Type!=Png.IHDR))
						throw new InvalidDataException("Wrong IHDR Chunk!");
					
					result = PngHelper.ConvertIHDR(chunk);
					using (MemoryStream outMemoryStream = new MemoryStream())
					{
						// Daten Stream bis zum Ende
						while(chunk.Type != Png.IEND)
						{
							if(chunk.Type == Png.IDAT)
								outMemoryStream.Write(chunk.Data,0,chunk.Data.Length);
							chunk = reader.ReadChunk();
						}
						byte[] outData = PngHelper.InflateDecode(outMemoryStream);
					}
				}
			}
			
			return result;
		}	
	}
}

