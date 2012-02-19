using System;

namespace PngRead
{	
	public class PngHeader
	{
		public int Width { get ; private set; }
		public int Height { get ; private set; }
		public byte BitDepth { get ; private set; }
		public ColorModel ColorModel { get; private set; }
		
		public int BytesPerPixel { get; private set; }
		public int Stride { get; private set; }
		
		public PngHeader(int width, int height, byte bitdepth, ColorModel colorModel)
		{
			Width = width;
			Height = height;
			BitDepth = bitdepth;
			ColorModel = colorModel;
			
			bool alpha = (colorModel & ColorModel.Alpha) == ColorModel.Alpha;
			bool palette = (colorModel & ColorModel.Palette) == ColorModel.Palette;
			bool grayscale = (colorModel & ColorModel.Color) != ColorModel.Color;
			bool packed = bitdepth < 8;
			
			if (grayscale && palette)
				throw new ArgumentOutOfRangeException("palette and greyscale are exclusive");
			
			int channels = (grayscale || palette) ? 
					(alpha ? 2 : 1) :	// Grayscale or Palette
					(alpha ? 4 : 3);	// RGB 
			
			int bpp = channels * BitDepth;
			BytesPerPixel = (bpp + 7) / 8;
			Stride = (bpp * width + 7) / 8;
			int samplesPerRow = channels * width;
			int samplesPerRowPacked = (packed) ? Stride : samplesPerRow;
		}
		
		public bool IsMonochrome
		{
			get { return ColorModel == (ColorModel.Palette|ColorModel.Color) && BitDepth < 8; }
		}

	}
}

