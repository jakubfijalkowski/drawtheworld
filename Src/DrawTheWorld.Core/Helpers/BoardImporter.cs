using System;
using System.IO;
using System.Threading.Tasks;
using DrawTheWorld.Core.UserData;
using FLib;

#if WINRT
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
#else
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace DrawTheWorld.Core.Helpers
{
	public static class BoardImporter
	{
		/// <summary>
		/// Imports board from stream.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filename">Name of the file being imported.</param>
		/// <returns></returns>
#if WINRT
		public async static Task<MutableBoardData> Import(IRandomAccessStream data, string filename)
#else
		public async static Task<MutableBoardData> Import(Stream data, string filename)
#endif
		{
			string ext = Path.GetExtension(filename);

			Validate.Debug(() => data, v => v.NotNull());
			Validate.Debug(() => ext, v => v.IsIn(new string[] { ".png", ".bmp", ".bmp" }));

#if WINRT
			BitmapDecoder decoder = await BitmapDecoder.CreateAsync(data);
			var pixelData = await decoder.GetPixelDataAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight, new BitmapTransform(),
				ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
			var pixels = pixelData.DetachPixelData();

			var board = new MutableBoardData(Guid.NewGuid())
			{
				Size = new Size((int)decoder.OrientedPixelWidth, (int)decoder.OrientedPixelHeight),
				Data = new Color?[(int)(decoder.OrientedPixelWidth * decoder.OrientedPixelHeight)]
			};
			board.Name[Config.DefaultLanguage] = filename;
			for (int i = 0, j = 0; i < pixels.Length; i += 4, ++j)
			{
				if (pixels[i + 3] == 255)
					board.Data[j] = Color.FromArgb(255, pixels[i], pixels[i + 1], pixels[i + 2]);
			}
			return board;
#else
			using (Bitmap bmp = new Bitmap(data))
			{
				var board = new MutableBoardData(Guid.NewGuid())
				{
					Size = new Size(bmp.Width, bmp.Height),
					Data = new System.Windows.Media.Color?[bmp.Width * bmp.Height]
				};
				board.Name[Config.DefaultLanguage] = filename;
				var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				await Task.Run(() =>
					{
						unsafe
						{
							byte* beginning = (byte*)bitmapData.Scan0;
							for (int y = 0; y < bitmapData.Height; y++)
							{
								for (int x = 0; x < bitmapData.Width; x++)
								{
									byte* pixel = beginning + bitmapData.Stride * y + x * 4;
									if (pixel[0] == 255)
										board.Data[x + y * board.Size.Width] = System.Windows.Media.Color.FromRgb(pixel[1], pixel[2], pixel[3]);
								}
							}
						}
					});
				bmp.UnlockBits(bitmapData);

				return board;
			}
#endif
		}
	}
}
