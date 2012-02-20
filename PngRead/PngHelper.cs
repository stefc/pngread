using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace stefc.pnglib
{
	public static class PngHelper
	{
		private static Encoding encoding = Encoding.GetEncoding("ISO-8859-1"); 
	
		public static PngHeader ConvertIHDR(Chunk chunk)
		{
			byte[] data = chunk.Data;
			
			int ofs = 0;
			int columns = DWordToInt(BitConverter.ToInt32(data,ofs)); ofs += 4;
			int rows = DWordToInt(BitConverter.ToInt32(data,ofs)); ofs += 4;
			
			// bit depth: number of bits per channel
			byte bitdepth = data[ofs++];
			byte colormodel = data[ofs++];
			
			byte compmeth = data[ofs++];
			byte filmeth = data[ofs++];
			
			byte interlaced = data[ofs++];
			if(interlaced>0)
				throw new NotSupportedException("interlaced png");
			
			return new PngHeader(columns,rows,bitdepth,(ColorModel)colormodel);
		}
		
		public static RenderingIntent ConvertSRGB(Chunk chunk)
		{
			return (RenderingIntent)chunk.Data[0];
		}
		
		public static DateTime ConvertTIME(Chunk chunk)
		{
			byte[] data = chunk.Data;
			
			int ofs = 0;
			int year = WordToInt(BitConverter.ToUInt16(data,ofs)); ofs+=sizeof(Int16);
			byte month = data[ofs++];
			byte day = data[ofs++];
			byte hour = data[ofs++];
			byte min = data[ofs++];
			byte sec = data[ofs++];
			
			return new DateTime(year,month,day,hour,min,sec);
		}
		
		public static int DWordToInt(int dword)
		{
			byte[] bytes = BitConverter.GetBytes(dword);
			return (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
		}
		
		public static int WordToInt(ushort word)
		{
			byte[] bytes= BitConverter.GetBytes(word);
			return (bytes[0] << 8) + bytes[1];
		}
		
		public static int TxtToInt(string txt)
		{
			return BitConverter.ToInt32(encoding.GetBytes(txt),0);
		}	
		
		public static string IntToTxt(int txt)
		{
			return encoding.GetString(BitConverter.GetBytes(txt));
		}
		
		public static int PaethPredictor(int a, int b, int c) 
		{
			// from http://www.libpng.org/pub/png/spec/1.2/PNG-Filters.html
			// a = left, b = above, c = upper left
			int p = a + b - c;// ; initial estimate
			int pa = Math.Abs(p - a); // distances to a, b, c
			int pb = Math.Abs(p - b);
			int pc = Math.Abs(p - c);
			// ; return nearest of a,b,c,
			// ; breaking ties in order a,b,c.
			if (pa <= pb && pa <= pc)
				return a;
			else if (pb <= pc)
				return b;
			else
				return c;
		}
		
		public static byte[] InflateDecode(Stream stream) 
		{
			stream.Flush();
			stream.Seek(0,SeekOrigin.Begin);
					  	
			byte[] result = null;
		    using(InflaterInputStream zip = new InflaterInputStream(stream))
			{
			    using(MemoryStream outp = new MemoryStream())
				{
				    byte[] buffer = new byte[4092];
				    int read;
					while ((read = zip.Read(buffer, 0, buffer.Length)) > 0) 
					{
			            outp.Write(buffer, 0, read);
			        }
			        result = outp.ToArray();
				}
			}
			return result;
		}
		
		public static byte[] InflateEncode(Stream stream)
		{
			stream.Flush();
			stream.Seek(0,SeekOrigin.Begin);
					  	
			byte[] result = null;
		    using(MemoryStream outp = new MemoryStream())
			{
				using(DeflaterOutputStream zip = new DeflaterOutputStream(outp, 
					new Deflater(Png.CompressionLevel), Png.BlockSize))
				{
				    byte[] buffer = new byte[Png.CompressionLevel];
				    int read;
					while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) 
					{
			            zip.Write(buffer, 0, read);
			        }        
				}
				result = outp.ToArray();
			}
			return result;
		}
		
		public static byte[] CreateRawImage(PngHeader header)
		{
			return new byte[header.Stride*header.Height];
		}
		
		public static void Convert(PngHeader header, byte[] inData, byte[] outData)
		{
			int stride = header.Stride;
			PngFilter decoder = new PngFilter(stride,header.BytesPerPixel);
			
			int inOfs=1;
			int outOfs= 0;
			for(int row = 0; row < header.Height; row++)
			{
				decoder.Decode(inData,outData,inOfs,outOfs);
				
				inOfs  += stride+1;
				outOfs += stride;
			}			
		}
		
		public static void WriteRows(byte[] data, PngHeader header, Stream stream)
		{
			int stride=header.Stride;
			
			if(header.IsMonochrome)
			{
				int ofs = 0;
				for(int row=0; row < header.Height; row++)
				{
					stream.WriteByte((byte)Filter.None);
					stream.Write(data,ofs,stride);
					
					ofs+=stride;
				};
			}
			else
			{
				var lines=new Dictionary<Filter, byte[]>();
				foreach(Filter filter in Enum.GetValues(typeof(Filter)))
					lines.Add(filter,new byte[stride]);
				
				PngFilter encoder = new PngFilter(stride,header.BytesPerPixel);
				
				int ofs = 0;
				for(int row=0; row < header.Height; row++)
				{
					Filter filter = encoder.Encode(data,ofs,lines);
							
					stream.WriteByte((byte)filter);
					stream.Write(lines[filter],0,stride);
					
					ofs+=stride;
				};
			}
		}
	}
}

