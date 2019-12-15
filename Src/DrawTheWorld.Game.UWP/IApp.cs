using DrawTheWorld.Core.Platform;

namespace DrawTheWorld.Game
{
	/// <summary>
	/// Base interface for Application - provides access to app subsystems.
	/// </summary>
	public interface IApp
	{
		IUIManager UIManager { get; }
		ISoundManager SoundManager { get; }
		IGameManager GameManager { get; }
		ISettingsProvider Settings { get; }
		UISettings UISettings { get; }
	}

	/// <summary>
	/// Empty application, used inside designer to prevent <see cref="System.NullReferenceException"/>.
	/// </summary>
	sealed class StubApp
		: IApp
	{
		public static readonly IApp Instance = new StubApp();

		/// <inheritdoc />
		public IUIManager UIManager
		{
			get { return null; }
		}

		/// <inheritdoc />
		public ISoundManager SoundManager
		{
			get { return null; }
		}

		/// <inheritdoc />
		public IGameManager GameManager
		{
			get { return null; }
		}

		/// <inheritdoc />
		public ISettingsProvider Settings
		{
			get { return null; }
		}

		/// <inheritdoc />
		public UISettings UISettings
		{
			get { return null; }
		}

		private StubApp()
		{ }
	}
}
