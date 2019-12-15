using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData;
using FLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace DrawTheWorld.Game.Platform.Stores
{
	/// <summary>
	/// Base class for thumbnail stores, because they share most of the functionality.
	/// </summary>
	class BaseThumbnailStore
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.ThumbsStore");

		private readonly string StoreType = string.Empty;
		private StorageFolder BaseFolder = null;

		protected BaseThumbnailStore(string type)
		{
			this.StoreType = type;
		}

		protected async Task<BitmapSource> GetThumbnail(Guid packId, Guid boardId)
		{
			var file = await this.GetThumbnailFile(packId, boardId, true);
			return file != null ? new BitmapImage(new Uri(file.Path)) : null;
		}

		protected async Task<BitmapSource> RegenerateThumbnail(Guid packId, Guid boardId, Action<Stream, Core.Size> drawThumbnail)
		{
			var file = await this.GetThumbnailFile(packId, boardId, false);
			if (file == null)
				return null;

			using (var ms = new MemoryStream(32 * Core.Config.DefaultBoardThumbnailSize * Core.Config.DefaultBoardThumbnailSize))
			{
				await Task.Run(() => drawThumbnail(ms, new Core.Size(Core.Config.DefaultBoardThumbnailSize, Core.Config.DefaultBoardThumbnailSize)));

				try
				{
					using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
					{
						var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
						encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, Core.Config.DefaultBoardThumbnailSize, Core.Config.DefaultBoardThumbnailSize, 96, 96, ms.ToArray());
						await encoder.FlushAsync();
					}
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot regenerate thumbnail for board {0} in pack {1}.".FormatWith(boardId, packId), ex);
				}
			}

			return new BitmapImage(new Uri(file.Path))
			{
				CreateOptions = BitmapCreateOptions.IgnoreImageCache
			};
		}

		protected async void Clean(Guid packId, Guid boardId)
		{
			var file = await this.GetThumbnailFile(packId, boardId, true);
			if (file != null)
			{
				try
				{
					await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot delete thumbnail for board {0} in pack {1}.".FormatWith(boardId, packId), ex);
				}
			}
		}

		private async Task<StorageFile> GetThumbnailFile(Guid packId, Guid boardId, bool openOnly)
		{
			if (this.BaseFolder == null)
				this.BaseFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Thumbnails", CreationCollisionOption.OpenIfExists);

			try
			{
				var packFolder = await this.OpenPackFolder(packId);
				if (openOnly)
					return await packFolder.GetFileAsync(boardId.ToString("N") + ".png");
				else
					return await packFolder.CreateFileAsync(boardId.ToString("N") + ".png", CreationCollisionOption.ReplaceExisting);
			}
			catch (Exception ex)
			{
				if (!openOnly)
					Logger.Warn("Cannot open thumbnail for board {0} in pack {1}.".FormatWith(boardId, packId), ex);
			}
			return null;
		}

		private async Task<StorageFolder> OpenPackFolder(Guid packId)
		{
			return await this.BaseFolder.CreateFolderAsync(packId.ToString("N") + "." + this.StoreType + "Thumbs", CreationCollisionOption.OpenIfExists);
		}
	}

	/// <summary>
	/// Thumbnail store for game.
	/// </summary>
	sealed class GameThumbnailStore
		: BaseThumbnailStore, IThumbnailStore<GameBoard>
	{
		public GameThumbnailStore()
			: base("Game")
		{ }

		/// <inheritdoc />
		public Task<BitmapSource> GetThumbnail(GameBoard obj)
		{
			return base.GetThumbnail(obj.Data.PackId, obj.Data.Id);
		}

		/// <inheritdoc />
		public Task<BitmapSource> RegenerateThumbnail(GameBoard obj)
		{
			return base.RegenerateThumbnail(obj.Data.PackId, obj.Data.Id, (stream, size) => BoardDrawer.DrawThumbnail(obj.Data, size, stream));
		}

		/// <inheritdoc />
		public void Clean(GameBoard obj)
		{
			base.Clean(obj.Data.PackId, obj.Data.Id);
		}
	}

	/// <summary>
	/// Thumbnail store for designer.
	/// </summary>
	sealed class DesignerThumbnailStore
		: BaseThumbnailStore, IThumbnailStore<MutableBoardData>
	{
		public DesignerThumbnailStore()
			: base("Designer")
		{ }

		/// <inheritdoc />
		public Task<BitmapSource> GetThumbnail(MutableBoardData obj)
		{
			return base.GetThumbnail(obj.PackId, obj.Id);
		}

		/// <inheritdoc />
		public Task<BitmapSource> RegenerateThumbnail(MutableBoardData obj)
		{
			return base.RegenerateThumbnail(obj.PackId, obj.Id, (stream, size) => BoardDrawer.DrawThumbnail(obj, size, stream));
		}

		/// <inheritdoc />
		public void Clean(MutableBoardData obj)
		{
			base.Clean(obj.PackId, obj.Id);
		}
	}
}
