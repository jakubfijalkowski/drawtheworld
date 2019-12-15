using System;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Configuration.
	/// </summary>
	public static class Config
	{
		/// <summary>
		/// Default language used in app.
		/// </summary>
		public const string DefaultLanguage = "en-us";

		/// <summary>
		/// Maximum size of the palette.
		/// </summary>
		/// <remarks>
		/// After publication, do not change this value to smaller one, because it will corrupt pack loaders.
		/// </remarks>
		public const int MaxPaletteSize = 32;

		/// <summary>
		/// Minimal board area.
		/// </summary>
		public const int MinBoardArea = 9;

		/// <summary>
		/// <see cref="IGameMode.MaximumFine"/> for <see cref="GameModes.SupervisedMode"/>.
		/// </summary>
		public const int MaxFineForSupervisedMode = 60;

		/// <summary>
		/// Default palette.
		/// </summary>
		public static readonly Color[] DefaultPalette = new Color[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Cyan, Colors.Black };

		/// <summary>
		/// Default board size.
		/// </summary>
		public static readonly Size DefaultBoardSize = new Size(15, 15);

		/// <summary>
		/// Default size of the thumbnail of <see cref="BoardDescription"/>.
		/// </summary>
		public const int DefaultBoardThumbnailSize = 160;

		/// <summary>
		/// Board thumbnail fill.
		/// BGRA format.
		/// </summary>
		public static readonly byte[] BoardThumbnailFill = new byte[] { 255, 255, 255, 255 };

		/// <summary>
		/// Address of the Draw the World web api.
		/// </summary>
		public static readonly Uri WebApiUrl = new Uri("http://dtw.fiolek.org/api/");

		/// <summary>
		/// Format of the link to the privacy statement.
		/// </summary>
		public static readonly string PrivacyStatementLink = "http://dtw.fiolek.org/PrivacyStatement/{0}";

		/// <summary>
		/// The maximum size of the pack file. Constrained to prevent useless packs.
		/// In bytes.
		/// </summary>
		/// <remarks>
		/// 100 MB is much too much, but forewarned is forearmed.
		/// </remarks>
		public const int MaxPackFileSize = 1024 * 1024 * 100;

		/// <summary>
		/// Retry count for the app.
		/// </summary>
		public const int RetryCount = 5;

		/// <summary>
		/// The delay between retries.
		/// </summary>
		public static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
	}
}
