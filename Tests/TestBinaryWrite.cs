using System;
using System.IO;
using NUnit.Framework;
using stefc.pnglib;

namespace Tests
{
	[TestFixture]
	public class TestBinaryWrite
	{
		private string desktop;
		
		[SetUp]
		public void SetUp()
		{
			desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}
		
		[Test]
		public void TestWriteCross()
		{
			string fileName = Path.Combine(desktop,"cross.png");
			
			int n = 100;
			PngHeader header = new PngHeader(n,n,1,ColorModel.Gray);
			
			byte[] data = new byte[header.Stride*header.Height];
			
			for(int i=0; i<n; i++)
			{
				SetPixel(data,i,i,header.Stride);
				SetPixel(data,n-i,i,header.Stride);
				SetPixel(data,n/2,i,header.Stride);
				SetPixel(data,i,n/2,header.Stride);
			}
			
			WritePng(fileName,header,data);
		}
		
		private void SetPixel(byte[] data, int x, int y, int stride)
		{
			int rowOfs = y * stride;
			int colOfs = x / 8;
			int mask = 1 << (7 - (x % 8));
			
			data[rowOfs+colOfs] |= (byte)mask;
		}
		
		private void WritePng(string fileName, PngHeader header, byte[] rawData)
		{
			using (Stream stream = new FileStream(fileName,FileMode.Create))
			{
				using (PngBinaryWriter writer = new PngBinaryWriter(stream))
				{
					writer.WriteSignatur();
					writer.WriteHeader(header);
					using (Stream ms = new MemoryStream())
					{	
						PngHelper.WriteRows(rawData,header,ms);
						writer.WriteData(PngHelper.InflateEncode(ms));
					}
					writer.WriteEnd();
				}
			}
		}
		
	}
}

