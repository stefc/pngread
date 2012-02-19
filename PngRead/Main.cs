using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/*
 * git add -n /\* 
 * git status
 * 
 * 
 * */

namespace PngRead
{
	class MainClass
	{
		
		static string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			
		
		public static void Main(string[] args)
		{
			string fileName = Path.Combine(desktop,"test.png");
			
			int n = 100;
			PngHeader header = new PngHeader(n,n,1,ColorModel.Gray);
			
			byte[] data = new byte[header.Stride*header.Height];
			
			// for(int i=0; i<n
			
			WritePng(fileName,header,null,data,null);			
		}
		
		public void SetPixel(byte[] data, int x, int y, int stride)
		{
			int rowOfs = y * stride;
			int colOfs = x / 8;
			int mask = 1 << (7 - (x % 8));
			
			data[rowOfs+colOfs] = (byte)mask;
			
		}
		
		public static void xMain (string[] args)
		{
			string fileName = //Path.Combine(desktop,"kopie_minmax.png");
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
					//"Dropbox/Bilder/chart-bw-8mp.png"
					"Dropbox/Bilder/stefc-bw.png"
					);
			
			desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string outFile = Path.Combine(desktop,"test.png");
			if(File.Exists(fileName))
				Console.WriteLine("ok");
			
			using (Stream stream = new FileStream(fileName,FileMode.Open))
			{
				using (PngBinaryReader reader = new PngBinaryReader(stream))
				{
					reader.ReadSignatur();
					
					// Header lesen
					Chunk chunk = reader.ReadChunk();
					if((chunk.Length!=13) || (chunk.Type!=Png.IHDR))
						throw new InvalidDataException("Wrong IHDR Chunk!");
					
					Chunk headerChunk = chunk;
					
					PngHeader header = PngHelper.ConvertIHDR(chunk);
					Console.WriteLine("{0}:{1} {2}-bits {3} stride-{4} ", header.Width, header.Height, 
						header.BitDepth, header.ColorModel, header.Stride);
					
					// 
					byte[] outData;
					byte[] compressedData;
					
					Dictionary<int,Chunk> chunks = new Dictionary<int, Chunk>();
					using (MemoryStream outMemoryStream = new MemoryStream())
					{
						// Daten Stream bis zum Ende
						while(chunk.Type != Png.IEND)
						{
							Console.WriteLine("{0} {1}",chunk.Length,PngHelper.IntToTxt(chunk.Type));
							if(chunk.Type == Png.IDAT)
								outMemoryStream.Write(chunk.Data,0,chunk.Data.Length);
							else if(chunk.Type == Png.PLTE)
							{
								for(int i=0; i<chunk.Length; i++)
									Console.WriteLine(chunk.Data[i]);
								chunks.Add(chunk.Type,chunk);
							}
							else 
								chunks.Add(chunk.Type,chunk);
							chunk = reader.ReadChunk();
						}
						compressedData = outMemoryStream.ToArray();
				      	outData = PngHelper.InflateDecode(outMemoryStream);
						Console.WriteLine("{0}->{1}", compressedData.Length, outData.Length);
						
						// 145:141:138			143:140:134  113:114:110  103:107:108
						// 0. Pixel oben links  18. 		 19. 		  20. 
					}
					
					Console.WriteLine((header.Stride + 1) * header.Height);
					
					byte[] rawData = PngHelper.CreateRawImage(header);
					PngHelper.Convert(header,outData,rawData);
					
					Console.WriteLine();
					
					Console.ReadLine();
					int stride=header.Stride;
					byte[] line = new byte[stride];
					for(int row=0; row < header.Height; row++)
					{
						Array.Copy(rawData,row*stride,line,0,stride);
						
						
						for(int col=0; col < header.Width; col++)
						{
							int b = line[col / 8];
							int mask = 1 << (7 - (col % 8));
							
							if((b & mask) != 0)
								Console.Write('*');
							else
								Console.Write(' ');
						}	
						Console.WriteLine();
					}
					
					WritePng(outFile,header,compressedData,rawData,chunks);
					// Console.WriteLine(outData.Length);
					
					// letztes Pixel 253:251:254
					// erstes Pixel  145:141:138
				}
			}
			Console.WriteLine ("Hello World!");
			
		}
		
		public static void WritePng(string fileName, PngHeader header, byte[] data, byte[] rawData, 
			Dictionary<int,Chunk> chunks)
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
					// writer.WriteData(data);
					
					writer.WriteEnd();
				}
			}
		}
		
		
	}
	
	
}
