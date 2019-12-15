using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DrawTheWorld.Game.Platform.Stores
{
	/// <summary>
	/// Provides encrypted pack store.
	/// </summary>
	sealed class EncryptedPackStore
		: IUserPackStore
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.EncryptedPackStore");

		private readonly DataProtectionProvider Protection = new DataProtectionProvider("LOCAL=user");
		private StorageFolder Folder = null;
		private string _UserId = null;

		/// <inheritdoc />
		public string UserId
		{
			get { return this._UserId; }
			set
			{
				this._UserId = value;
				this.Folder = null;
			}
		}

		/// <inheritdoc />
		public async Task LoadPacks(Func<System.IO.Stream, Guid, Task> installCallback)
		{
			await this.LoadFolder();

			var files = (await this.Folder.GetFilesAsync()).Where(f => f.Name.EndsWith(".dtw"));
			foreach (var p in files)
			{
				bool remove = false;
				try
				{
					var packId = Guid.ParseExact(p.Name.Remove(p.Name.Length - 4), "N");
					using (var stream = await p.OpenReadAsync())
					{
						if (stream.Size > Core.Config.MaxPackFileSize)
							throw new InvalidDataException("Pack size is greater than Config.MaxPackFileSize.");

						using (var intermediate = new MemoryStream((int)stream.Size))
						{
							await this.Protection.UnprotectStreamAsync(stream, intermediate.AsOutputStream());
							intermediate.Position = 0;
							await installCallback(intermediate, packId);
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Cannot load pack(file: {0}).".FormatWith(p.Name), ex);
					remove = true;
				}

				if (remove)
				{
					try
					{
						await p.DeleteAsync(StorageDeleteOption.PermanentDelete);
					}
					catch (Exception ex)
					{
						Logger.Warn("Cannot delete invalid pack(file: {0}).".FormatWith(p.Name), ex);
					}
				}
			}
		}

		/// <inheritdoc />
		public async Task RemovePack(Guid id)
		{
			await this.LoadFolder();

			try
			{
				await (await this.Folder.GetFileAsync(Settings.PackName(id))).DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot remove pack {0}.".FormatWith(id), ex);
			}
		}

		/// <inheritdoc />
		public async Task SavePack(Guid id, Action<Stream> saveCallback)
		{
			await this.LoadFolder();

			using (var intermediate = new InMemoryRandomAccessStream())
			{
				await Task.Run(() => saveCallback(intermediate.AsStreamForWrite()));
				intermediate.Seek(0);

				var file = await this.Folder.CreateFileAsync(Settings.PackName(id), CreationCollisionOption.OpenIfExists);
				using (var transaction = await file.OpenTransactedWriteAsync())
				{
					transaction.Stream.Size = 0;
					await this.Protection.ProtectStreamAsync(intermediate, transaction.Stream);
					await transaction.CommitAsync();
				}
			}
		}

		/// <inheritdoc />
		public async Task Clear()
		{
			await this.LoadFolder();

			IReadOnlyList<StorageFile> files = null;
			try
			{
				files = await this.Folder.GetFilesAsync();
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot clear folder for user {0}. Files cannot be listed.".FormatWith(this.UserId), ex);
				return;
			}

			foreach (var f in files)
			{
				try
				{
					await f.DeleteAsync(StorageDeleteOption.PermanentDelete);
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot delete file '{0}' from user's(id: {1}) pack store.".FormatWith(f.Name, this.UserId), ex);
				}
			}

			try
			{
				await this.Folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot user's(id: {0}) pack store's folder.".FormatWith(this.UserId), ex);
			}
		}

		private async Task LoadFolder()
		{
			Validate.Debug(() => this.UserId, v => v.NotNullAndNotWhiteSpace());

			if (this.Folder == null)
			{
				var root = await Settings.OpenPackStore(StoreType.User);
				this.Folder = await root.CreateFolderAsync(this.UserId, CreationCollisionOption.OpenIfExists);
			}
		}
	}
}
