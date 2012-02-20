using System;
using System.Text;

namespace stefc.pnglib
{
	public static class Png
	{
		private static int iHDR; 
		private static int iDAT;
		private static int iEND;
		private static int sBIT;
		private static int sRGB;
		private static int tIME;
		private static int pLTE;
				
		static Png ()
		{
			iHDR = PngHelper.TxtToInt("IHDR");
			iDAT = PngHelper.TxtToInt("IDAT");
			iEND = PngHelper.TxtToInt("IEND");
			sBIT = PngHelper.TxtToInt("sBIT");
			sRGB = PngHelper.TxtToInt("sRGB");
			tIME = PngHelper.TxtToInt("tIME");
			pLTE = PngHelper.TxtToInt("PLTE");
			//cHRM = PngHelper.TxtToInt("cHRM");
			//gAMA = PngHelper.TxtToInt("gAMA");
			//iCCP = PngHelper.TxtToInt("iCCP");
			//bKGD = PngHelper.TxtToInt("bKGD");
			//hIST = PngHelper.TxtToInt("hIST");
			//tRNS = PngHelper.TxtToInt("tRNS");
			//pHYS = PngHelper.TxtToInt("pHYs");
			//sPLT = PngHelper.TxtToInt("sPLT");
			//iTXT = PngHelper.TxtToInt("iTXt");
			//tEXT = PngHelper.TxtToInt("tEXt");
			//zTXT = PngHelper.TxtToInt("zTXt");
		}
		
		public static int IHDR	{ get { return iHDR; } }
		public static int IEND { get { return iEND; } }
		public static int SBIT { get { return sBIT; } }
		public static int IDAT { get { return iDAT; } }
		public static int SRGB { get { return sRGB; } }
		public static int TIME	{ get { return tIME; } }
		public static int PLTE { get { return pLTE; } }
	
		public static long Signatur = 0x0a1a0a0d474e5089;
			
		public static int BlockSize = 8192;
		public static int CompressionLevel = 9;
	}
	
	[Flags]
	public enum ColorModel : byte
	{
		Gray  = 0x00,
		Palette = 0x01,
		Color = 0x02,
		Alpha = 0x04
	} 
	
	public enum RenderingIntent : byte
	{
		Perceptual = 0,
		RelativeColorimetric = 1,
		Saturation = 2,
		AbsoluteColorimetric = 3
	}
	
	public enum Filter : byte
	{
		None = 0,
		Sub	= 1,
		Up = 2,
		Avg = 3,
		Paeth = 4
	}
	
}

