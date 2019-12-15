using System;
using System.IO;
using System.Threading.Tasks;
using FLib;

#if WINRT
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core.Helpers
{
	/// <summary>
	/// Helps drawing thumbnails of the board.
	/// </summary>
	/// <remarks>
	/// Colors are written in R8G8B8A8 format.
	/// </remarks>
	public static class BoardDrawer
	{
		/// <summary>
		/// Draws thumbnail of specified <see cref="IBoard"/>.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="thumbnailSize"></param>
		/// <param name="stream"></param>
		public static void DrawThumbnail(IBoard board, Size thumbnailSize, Stream stream)
		{
			Validate.Debug(() => board, v => v.NotNull());
			Validate.Debug(() => thumbnailSize, v => v.That(s => s.Width, v2 => v2.Min(1)).That(s => s.Height, v2 => v2.Min(1)));
			Validate.Debug(() => stream, v => v.NotNull().That(s => s.CanWrite, v2 => v2.True()));

			DrawThumbnail(board.BoardInfo.Size, thumbnailSize, stream, i => board.Fields[i].Fill);
		}

		/// <summary>
		/// Draws thumbnail of specified <see cref="IBoard"/>. The output thumbnail will have default size.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="stream"></param>
		public static void DrawThumbnail(IBoard board, Stream stream)
		{
			DrawThumbnail(board, new Size(Config.DefaultBoardThumbnailSize, Config.DefaultBoardThumbnailSize), stream);
		}

		/// <summary>
		/// Draws thumbnail of specified <see cref="UserData.BoardData"/>.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="thumbnailSize"></param>
		/// <param name="stream"></param>
		public static void DrawThumbnail(UserData.BoardData data, Size thumbnailSize, Stream stream)
		{
			Validate.Debug(() => data, v => v.NotNull());
			Validate.Debug(() => thumbnailSize, v => v.That(s => s.Width, v2 => v2.Min(1)).That(s => s.Height, v2 => v2.Min(1)));
			Validate.Debug(() => stream, v => v.NotNull().That(s => s.CanWrite, v2 => v2.True()));

			DrawThumbnail(data.Size, thumbnailSize, stream, i => data.Data[i]);
		}

		/// <summary>
		/// Draws thumbnail of specified <see cref="UserData.BoardData"/>. The output thumbnail will have default size.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="stream"></param>
		public static void DrawThumbnail(UserData.BoardData data, Stream stream)
		{
			DrawThumbnail(data, new Size(Config.DefaultBoardThumbnailSize, Config.DefaultBoardThumbnailSize), stream);
		}

		/// <summary>
		/// Draws thumbnail of specified <see cref="UserData.MutableBoardData"/>.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="thumbnailSize"></param>
		/// <param name="stream"></param>
		public static void DrawThumbnail(UserData.MutableBoardData data, Size thumbnailSize, Stream stream)
		{
			Validate.Debug(() => data, v => v.NotNull());
			Validate.Debug(() => thumbnailSize, v => v.That(s => s.Width, v2 => v2.Min(1)).That(s => s.Height, v2 => v2.Min(1)));
			Validate.Debug(() => stream, v => v.NotNull().That(s => s.CanWrite, v2 => v2.True()));

			DrawThumbnail(data.Size, thumbnailSize, stream, i => data.Data[i]);
		}

		/// <summary>
		/// Draws thumbnail of specified <see cref="UserData.MutableBoardData"/>. The output thumbnail will have default size.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="stream"></param>
		public static void DrawThumbnail(UserData.MutableBoardData data, Stream stream)
		{
			DrawThumbnail(data, new Size(Config.DefaultBoardThumbnailSize, Config.DefaultBoardThumbnailSize), stream);
		}

#if WINRT
		/// <summary>
		/// Draws thumbnail of <see cref="IBoard"/>.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="thumbnailSize"></param>
		/// <param name="bmp">Bitmap that was previously created using this method.</param>
		/// <returns></returns>
		public static async Task<WriteableBitmap> DrawThumbnail(IBoard board, Size thumbnailSize, WriteableBitmap bmp = null)
		{
			Validate.Debug(() => board, v => v.NotNull());
			Validate.Debug(() => thumbnailSize, v => v.That(s => s.Width, v2 => v2.Min(1)).That(s => s.Height, v2 => v2.Min(1)));

			bool newlyCreated = bmp == null;
			if (newlyCreated)
				bmp = new WriteableBitmap(thumbnailSize.Width, thumbnailSize.Height);
			Validate.Debug(() => bmp, v => v.That(b => b.PixelWidth, v2 => v2.Equals(thumbnailSize.Width)).That(b => b.PixelHeight, v2 => v2.Equals(thumbnailSize.Height)));

			using (var stream = bmp.PixelBuffer.AsStream())
				await Task.Run(() => DrawThumbnail(board.BoardInfo.Size, thumbnailSize, stream, i => board.Fields[i].Fill));

			if (!newlyCreated)
				bmp.Invalidate();
			return bmp;
		}

		/// <summary>
		/// Draws thumbnail of <see cref="IBoard"/>. The output thumbnail will have default size.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="bmp"></param>
		/// <returns></returns>
		public static Task<WriteableBitmap> DrawThumbnail(IBoard board, WriteableBitmap bmp = null)
		{
			return DrawThumbnail(board, new Size(Config.DefaultBoardThumbnailSize, Config.DefaultBoardThumbnailSize), bmp);
		}
#endif

		private static void DrawThumbnail(Size boardSize, Size thumbnailSize, Stream stream, Func<int, Color?> colorGetter)
		{
			int tileSize = Math.Min(thumbnailSize.Width / boardSize.Width, thumbnailSize.Height / boardSize.Height);
			int boardWidth = boardSize.Width * tileSize;
			int boardHeight = boardSize.Height * tileSize;
			int marginLeft = (thumbnailSize.Width - boardWidth) / 2;
			int marginTop = (thumbnailSize.Height - boardHeight) / 2;

			int y = 0;
			// Top margin
			for (; y < marginTop; y++)
			{
				for (int x = 0; x < thumbnailSize.Width; x++)
					stream.Write(Config.BoardThumbnailFill, 0, Config.BoardThumbnailFill.Length);
			}

			// Main board
			for (; y < boardHeight + marginTop; ++y)
			{
				for (int x = 0; x < thumbnailSize.Width; x++)
				{
					// Left and right margin
					if (x <= marginLeft || x >= (marginLeft + boardWidth))
						stream.Write(Config.BoardThumbnailFill, 0, Config.BoardThumbnailFill.Length);
					// Color
					else
					{
						int fieldX = (x - marginLeft) / tileSize;
						int fieldY = (y - marginTop) / tileSize;
						int i = fieldX + fieldY * boardSize.Width;
						var color = colorGetter(i);
						if (color == null)
						{
							stream.Write(Config.BoardThumbnailFill, 0, Config.BoardThumbnailFill.Length);
						}
						else
						{
							stream.WriteByte(color.Value.B);
							stream.WriteByte(color.Value.G);
							stream.WriteByte(color.Value.R);
							stream.WriteByte(255);
						}
					}
				}
			}

			// Bottom margin
			for (; y < thumbnailSize.Height; y++)
			{
				for (int x = 0; x < thumbnailSize.Width; x++)
					stream.Write(Config.BoardThumbnailFill, 0, Config.BoardThumbnailFill.Length);
			}
		}
	}
}
