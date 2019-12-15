using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace DrawTheWorld.Game.Helpers
{
	/// <summary>
	/// <see cref="AggregateException"/> that have partial result.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AggregateExceptionWithPartialResult<T>
		: AggregateException
	{
		/// <summary>
		/// Gets the partial result.
		/// </summary>
		public T Result { get; private set; }

		public AggregateExceptionWithPartialResult(T result, params Exception[] innerExceptions)
			: base(innerExceptions)
		{
			this.Result = result;
		}

		public AggregateExceptionWithPartialResult(T result, IEnumerable<Exception> innerExceptions)
			: base(innerExceptions)
		{
			this.Result = result;
		}
	}

	/// <summary>
	/// Helps managing user data.
	/// </summary>
	public static class UserDataHelper
	{
		private const string ResourceKey = "Helpers_UserData_";

		/// <summary>
		/// Asks user to select file and adds them.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="loadPack"></param>
		/// <param name="addPack"></param>
		/// <param name="single"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<T>> AddPacksFromUser<T>(Func<Stream, Task<T>> loadPack, bool single = false)
		{
			var picker = new FileOpenPicker();
			picker.CommitButtonText = Strings.Get(ResourceKey + "AddButton");
			picker.SuggestedStartLocation = PickerLocationId.Downloads;
			picker.ViewMode = PickerViewMode.List;
			picker.FileTypeFilter.Add(".dtw");

			var files = single ? new StorageFile[] { await picker.PickSingleFileAsync() } : await picker.PickMultipleFilesAsync();
			if (files.Count == 0 || files[0] == null)
				return Enumerable.Empty<T>();
			return await AddPacks(loadPack, files);
		}

		public static async Task<T> AddSinglePack<T>(Func<Stream, Task<T>> loadPack, Notifications notifications, MetroLog.ILogger logger)
			where T : class
		{
			logger.Trace("Adding existing pack.");
			T pack = null;
			try
			{
				pack = (await Helpers.UserDataHelper.AddPacksFromUser(loadPack, true)).FirstOrDefault();
			}
			catch (AggregateException ex)
			{
				logger.Warn("Cannot load user-selected pack.", ex);

				notifications.Notify(ex.InnerException is IOException ?
					(Core.UI.IMessage)new Core.UI.Messages.StorageProblemMessage() :
					(Core.UI.IMessage)new Core.UI.Messages.PackLoadErrorMessage());
				return null;
			}

			if (pack == null)
				logger.Debug("User didn't select any pack. Aborting.");
			return pack;
		}

		/// <summary>
		/// Loads packs from particular files.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="loadPack"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		public static async Task<IEnumerable<T>> AddPacks<T>(Func<Stream, Task<T>> loadPack, IEnumerable<StorageFile> files)
		{
			List<Exception> exceptions = new List<Exception>();
			List<T> result = new List<T>();
			foreach (var file in files)
			{
				try
				{
					using (var stream = await file.OpenStreamForReadAsync())
						result.Add(await loadPack(stream));
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}

			if (exceptions.Count > 0)
				throw new AggregateExceptionWithPartialResult<IEnumerable<T>>(result, exceptions);
			return result;
		}

		/// <summary>
		/// Exports pack.
		/// </summary>
		/// <param name="pack"></param>
		/// <returns></returns>
		public static async Task ExportPack(Pack pack)
		{
			PackValidator.ValidatePack(pack);

			var picker = new FileSavePicker();
			picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			picker.DefaultFileExtension = ".dtw";
			picker.SuggestedFileName = pack.Name.DefaultTranslation;
			picker.FileTypeChoices.Add(Strings.Get(ResourceKey + "PackName"), new List<string>() { ".dtw" });

			var file = await picker.PickSaveFileAsync();
			if (file != null)
			{
				using (var stream = await file.OpenStreamForWriteAsync())
				{
					stream.SetLength(0);
					await Task.Run(() => PackLoader.Save(pack, stream));
				}
			}
		}

		/// <summary>
		/// Imports images as boards.
		/// </summary>
		/// <returns></returns>
		public static async Task<IEnumerable<MutableBoardData>> Import()
		{
			var picker = new FileOpenPicker();
			picker.CommitButtonText = Strings.Get(ResourceKey + "ImportButton");
			picker.SuggestedStartLocation = PickerLocationId.Downloads;
			picker.ViewMode = PickerViewMode.Thumbnail;
			picker.FileTypeFilter.Add(".png");
			picker.FileTypeFilter.Add(".bmp");
			picker.FileTypeFilter.Add(".gif");

			var files = await picker.PickMultipleFilesAsync();
			if (files.Count == 0)
				return Enumerable.Empty<MutableBoardData>();

			List<MutableBoardData> result = new List<MutableBoardData>();
			List<Exception> exceptions = new List<Exception>();
			foreach (var file in files)
			{
				try
				{
					using (var stream = await file.OpenReadAsync())
					{
						var board = await Core.Helpers.BoardImporter.Import(stream, file.Name);
						PackValidator.ValidateImportedBoard(board);
						result.Add(board);
					}
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
			if (exceptions.Count > 0)
				throw new AggregateExceptionWithPartialResult<IEnumerable<MutableBoardData>>(result, exceptions);
			return result;
		}

		/// <summary>
		/// Adjusts new pack so that it can be displayed better in designer.
		/// </summary>
		/// <param name="pack"></param>
		/// <param name="settings"></param>
		public static void AdjustNewPackForDesigner(MutablePack pack, ISettingsProvider settings)
		{
			const string NewPackNameKey = ResourceKey + "NewPackName";

			pack.Name[Core.Config.DefaultLanguage] = Strings.GetForLanguage(NewPackNameKey, Core.Config.DefaultLanguage);

			foreach (var lng in settings.UserLanguages)
			{
				string str = Strings.GetForLanguage(NewPackNameKey, lng);
				if (!string.IsNullOrWhiteSpace(str))
					pack.Name[lng] = str;
			}
		}

		/// <summary>
		/// Adjusts new board so that it can be displayed better in designer.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="settings"></param>
		public static void AdjustNewBoardForDesigner(MutableBoardData board, ISettingsProvider settings)
		{
			const string NewBoardNameKey = ResourceKey + "NewBoardName";

			board.Name[Core.Config.DefaultLanguage] = Strings.GetForLanguage(NewBoardNameKey, Core.Config.DefaultLanguage);

			foreach (var lng in settings.UserLanguages)
			{
				string str = Strings.GetForLanguage(NewBoardNameKey, lng);
				if (!string.IsNullOrWhiteSpace(str))
					board.Name[lng] = str;
			}
		}
	}
}
