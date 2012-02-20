using System;
using System.Collections.Generic;

namespace stefc.pnglib
{
	internal class PngFilter
	{
		private readonly int bytesPerPixel;
		private readonly int stride;
		
		public PngFilter (int stride, int bytesPerPixel)
		{
			this.stride=stride;
			this.bytesPerPixel=bytesPerPixel;
		}
		
		public void Decode(byte[] inData, byte[] outData,int inOfs, int outOfs)
		{
			Filter filter = (Filter)inData[inOfs-1];
			Console.Write(filter);
			Console.Write(",");	
			int prevOfs = outOfs - stride;
			if(filter==Filter.None)
			{
				Array.Copy(inData,inOfs,outData,outOfs,stride);
			}
			else if(filter==Filter.Sub)
			{
				Array.Copy(inData,inOfs,outData,outOfs,bytesPerPixel);	
				for(int i=bytesPerPixel; i< stride; i++)
				{
					int a = outData[outOfs+i-bytesPerPixel];
					outData[outOfs+i]=(byte)(a+inData[inOfs+i]);
				}
			} 
			else if(filter == Filter.Up)
			{
				for(int i=0; i < stride; i++)
				{
					int b = outData[prevOfs+i];
					outData[outOfs+i]=(byte)(b+inData[inOfs+i]);
				}
			} 
			else if(filter == Filter.Avg)
			{
				for(int i=0; i < stride; i++)
				{
					int b = outData[prevOfs+i];
					int a = (i>=bytesPerPixel) ? outData[outOfs+i-bytesPerPixel] : (byte)0;
					outData[outOfs+i] = (byte)(((a+b)/2)+inData[inOfs+i]);
				}
			}
			else if(filter == Filter.Paeth)
			{
				for(int i=0; i < stride; i++)
				{
					int c = (i>=bytesPerPixel) ? outData[prevOfs+i-bytesPerPixel] : (byte)0;
					int b = outData[prevOfs+i];
					int a = (i>=bytesPerPixel) ? outData[outOfs+i-bytesPerPixel] : (byte)0;
					outData[outOfs+i] = (byte)(PaethPredictor(a,b,c)+inData[inOfs+i]);
				}
			}
		}
		
		public Filter Encode(byte[] data, int ofs, Dictionary<Filter,byte[]> lines)
		{
			Filter result = Filter.None;
			long minTotal = FilterLine(result,data,ofs,lines);
			foreach(Filter filter in GetFilters(ofs==0))
			{
				long total = FilterLine(filter,data,ofs,lines);
				if(total < minTotal)
				{
					result = Filter.Sub;
					minTotal = total;
				}
			}
			
			return result;
		}
		
		private int PaethPredictor(int a, int b, int c) 
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
		
		private long FilterNone(byte[] data, int ofs, byte[] result)
		{
			Array.Copy(data,ofs,result,0,stride);
			long total=0;
			for(int i=0; i< stride; i++)
				total += result[i];
			return total;
		}
		
		private long FilterSub(byte[] data, int ofs, byte[] result)
		{
			Array.Copy(data,ofs,result,0,bytesPerPixel);
			
			long total=0;
			for(int i=0; i<bytesPerPixel; i++)
				total+=result[i];
			
			for(int i=bytesPerPixel; i< stride; i++)
			{
				int a = data[ofs+i-bytesPerPixel];
				result[i]=(byte)(data[ofs+i]-a);
				total+=result[i];
			}
			return total;
		}
		
		private long FilterUp(byte[] data, int ofs, byte[] result)
		{
			long total=0;
			for(int i=0; i< stride; i++)
			{
				int b = data[ofs+i-stride];
				result[i]=(byte)(data[ofs+i]-b);
				total+=result[i];
			}
			return total;
		}
		
		private long FilterAvg(byte[] data, int ofs, byte[] result)
		{
			long total=0;
			for(int i=0; i< stride; i++)
			{
				int a = data[ofs+i-stride];
				int b = i>= bytesPerPixel ? (int)data[ofs+i-bytesPerPixel] : (int)0;
				result[i]=(byte)(data[ofs+i]-(a+b)/2);
				total+=result[i];
			}
			return total;
		}
		
		private long FilterPaeth(byte[] data, int ofs, byte[] result)
		{
			long total=0;
			for(int i=0; i< stride; i++)
			{
				int b = i>= bytesPerPixel ? (int)data[ofs+i-bytesPerPixel-stride] : (int)0;
				int a = data[ofs+i-stride];
				int c = i>= bytesPerPixel ? (int)data[ofs+i-bytesPerPixel] : (int)0;
				result[i]=(byte)(data[ofs+i]-PaethPredictor(c,a,b));
				
				total+=result[i];
			}
			return total;
		}
		
		private long FilterLine(Filter filter, byte[] data, int ofs, Dictionary<Filter,byte[]> lines)
		{
			switch (filter)
			{
				case Filter.None:
					return FilterNone(data,ofs,lines[filter]);
				case Filter.Sub:
					return FilterSub(data,ofs,lines[filter]);
				case Filter.Up:
					return FilterUp(data,ofs,lines[filter]);
				case Filter.Avg:
					return FilterAvg(data,ofs,lines[filter]);
				case Filter.Paeth:
					return FilterPaeth(data,ofs,lines[filter]);				
			}
			return 0;
		}
		
		private IEnumerable<Filter> GetFilters(bool firstRow)
		{
			return firstRow ? 
				new Filter[]{Filter.Sub} : 
				new Filter[]{Filter.Sub,Filter.Up,Filter.Avg,Filter.Paeth};
		}		
	}
}

