using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace DrawTheWorld.Game.Platform.Stores
{
	/// <summary>
	/// Supported store types.
	/// </summary>
	public enum StoreType
	{
		Demo,
		Designer,
		Custom,
		User
	}

	/// <summary>
	/// Provides access to a packs store(and thumbnails store).
	/// </summary>
	static class Settings
	{
		private const string DemoPacksStore = "DemoPacks";
		private const string BaseDir = "Packs";

		public static async Task<StorageFolder> OpenPackStore(StoreType type)
		{
			// Special case - demo store
			if (type == StoreType.Demo)
				return await Package.Current.InstalledLocation.GetFolderAsync(DemoPacksStore);
			
			var baseDir = await ApplicationData.Current.LocalFolder.CreateFolderAsync(BaseDir, CreationCollisionOption.OpenIfExists);
			return await baseDir.CreateFolderAsync(type.ToString(), CreationCollisionOption.OpenIfExists);
		}

		public static string PackName(Guid id)
		{
			return id.ToString("N") + ".dtw";
		}
	}
}
