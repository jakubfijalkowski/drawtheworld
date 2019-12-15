using System.Threading.Tasks;
using DrawTheWorld.Core.UI;

#if WINRT
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media.Imaging;
#endif

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Interfaces that provides access to the thumbnails store(thumbnails do not need to be persisted on disk).
	/// </summary>
	public interface IThumbnailStore<T>
		where T : IHaveThumbnail
	{
		/// <summary>
		/// Gets the thumbnail from the store or returns null, if it is not available.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		Task<BitmapSource> GetThumbnail(T obj);

		/// <summary>
		/// Forcibly generates thumbnails and saves it for future use.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		Task<BitmapSource> RegenerateThumbnail(T obj);

		/// <summary>
		/// Cleans the thumbnails store of selected object.
		/// </summary>
		/// <param name="obj"></param>
		void Clean(T obj);
	}
}
