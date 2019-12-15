#if WINRT
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media.Imaging;
#endif

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Classes that implement this interface can be displayed as thumbs.
	/// </summary>
	public interface IHaveThumbnail
	{
		/// <summary>
		/// Gets the thumbnail.
		/// </summary>
		BitmapSource Thumbnail { get; }
	}
}
