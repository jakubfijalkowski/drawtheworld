using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace DrawTheWorld.Game.Utilities.Versioning
{
	static class Utils
	{
		public static async Task ClearFolder(StorageFolder folder)
		{
			var items = await folder.GetItemsAsync();
			foreach (var item in items)
			{
				if (item.IsOfType(StorageItemTypes.Folder))
					await ClearFolder((StorageFolder)item);
				else
					await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
			await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
		}

		public static Guid ParsePackId(string filename)
		{
			return Guid.Parse(filename.Remove(filename.Length - 4));
		}
	}
}
