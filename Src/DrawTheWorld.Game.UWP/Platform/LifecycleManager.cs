using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.UserData.Repositories;
using DrawTheWorld.Game.Helpers;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace DrawTheWorld.Game.Platform
{
	using PackLoadErrorType = Core.UI.Messages.PackLoadErrorMessage.MessageType;

	/// <summary>
	/// Manages the lifecycle of the app. Initializes all subsystems, manages not finished games.
	/// </summary>
	sealed class LifecycleManager
		: ILifecycleManager
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.LifecycleManager");

		private const string SavedGameContainerName = "SavedGameContainer";
		private const string SavedGameEntryName = "SavedGame";

		private readonly UIManager UIManager = null;
		private readonly SoundManager SoundManager = null;
		private readonly AccountManager Account = null;
		private readonly ISettingsProvider Settings = null;
		private readonly DataInitializer DataInitializer = null;
		private readonly FeatureProvider FeatureProvider = null;
		private readonly GameManager GameManager = null;
		private readonly UISettings UISettings = null;

		private readonly CustomPackRepository PackRepository = null;
		private readonly UserStatistics UserStatistics = null;
		private readonly Notifications Notifications = null;
		private readonly Utilities.Versioning.ApplicationDataVersioner Versioner = null;

		private readonly ApplicationDataContainer SavedGame = null;

		private readonly Lazy<Controls.RequireSignIn> RequireSignInControl = null;

		/// <inheritdoc />
		public bool IsSuspending { get; private set; }

		/// <summary>
		/// Initializes the manager.
		/// </summary>
		public LifecycleManager(UIManager uiManager, SoundManager soundManager, AccountManager account, ISettingsProvider settings,
			DataInitializer dataInitializer, FeatureProvider features, GameManager gameManager, UISettings uiSettings,
			CustomPackRepository packRepo, UserStatistics userStats, Notifications notifications,
			Lazy<Controls.RequireSignIn> requireSignIn, Utilities.Versioning.ApplicationDataVersioner versioner,
			Utilities.GlobalSettingsManager gsm)
		{
			this.UIManager = uiManager;
			this.SoundManager = soundManager;
			this.Account = account;
			this.Settings = settings;
			this.DataInitializer = dataInitializer;
			this.FeatureProvider = features;
			this.GameManager = gameManager;
			this.UISettings = uiSettings;

			this.PackRepository = packRepo;
			this.UserStatistics = userStats;
			this.Notifications = notifications;
			this.Versioner = versioner;

			this.RequireSignInControl = requireSignIn;

			this.SavedGame = ApplicationData.Current.LocalSettings.CreateContainer(SavedGameContainerName, ApplicationDataCreateDisposition.Always);
		}

		/// <summary>
		/// Should be called when application starts.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task Initialize(IActivatedEventArgs args)
		{
			this.IsSuspending = false;
			bool cleanStartup = args.PreviousExecutionState != ApplicationExecutionState.Running && args.PreviousExecutionState != ApplicationExecutionState.Suspended;
			bool normalStartup = args.Kind == ActivationKind.Launch;

			if (cleanStartup)
			{
				this.UISettings.Initialize();
				await this.FeatureProvider.Initialize();

				await this.UIManager.Initialize(args.SplashScreen);
				this.Notifications.Initialize();
				await this.Versioner.Initialize();
				await this.DataInitializer.Initialize();
				await this.SoundManager.Initialize(this.UIManager.MediaContainer);

				this.UIManager.NavigateTo<Pages.MainMenu>();

				if (normalStartup)
					this.UIManager.HideLoadingPopup();

				this.AutoSignIn();
			}
			else
				this.UIManager.ActivateOnly();

			this.RestoreSavedStatistics();
		}

		/// <summary>
		/// Should be called when application is activated using <see cref="Windows.UI.Xaml.Application.OnFileActivated"/>
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task OnFileActivated(FileActivatedEventArgs args)
		{
			Logger.Trace("File activated.");

			Core.UI.IMessage msg = null;
			if (await this.FeatureProvider.TestForFeature(f => f.RequestFeature(Feature.PackInstall), this.Notifications, Logger))
			{
				await this.UIManager.ShowLoading();
				IEnumerable<Pack> packs = null;
				try
				{
					Logger.Trace("Loading {0} files.", args.Files.Count);
					packs = await UserDataHelper.AddPacks(s => this.PackRepository.Add(s), args.Files.Cast<StorageFile>());
				}
				catch (AggregateExceptionWithPartialResult<IEnumerable<Pack>> ex)
				{
					Logger.Warn("Cannot load some of the packs. Ignoring.", ex);
					packs = ex.Result;
				}

				var originalCount = args.Files.Count;
				var loadedCount = packs.Count();

				Logger.Info("{0} files of {1} loaded.", loadedCount, originalCount);

				if (originalCount != loadedCount)
				{
					PackLoadErrorType type;
					if (originalCount == 1)
						type = PackLoadErrorType.SingleFailure;
					else if (loadedCount > 0)
						type = PackLoadErrorType.PartialFailure;
					else
						type = PackLoadErrorType.MultipleFailure;

					msg = new Core.UI.Messages.PackLoadErrorMessage { Type = type };
				}

				if (loadedCount > 0)
					this.UIManager.NavigateTo<Pages.GameList>().ShowPack(packs.First().Id);
			}

			this.UIManager.HideLoadingPopup();
			if (msg != null)
				this.Notifications.Notify(msg);
		}

		/// <summary>
		/// Should be called when application resumes from suspension.
		/// </summary>
		/// <returns></returns>
		public Task Resume()
		{
			Logger.Trace("Resuming.");
			this.IsSuspending = false;
			this.SavedGame.Values.Clear();

			return Task.FromResult<object>(null);
		}

		/// <summary>
		/// Should be called when application goes into "suspended" state.
		/// </summary>
		/// <returns></returns>
		public Task Suspend()
		{
			Logger.Trace("Suspending.");
			this.IsSuspending = true;

			if (this.GameManager.IsInGame && !(this.GameManager.Game is Core.Designer))
			{
				var record = this.GameManager.Game.CreateStatistics(Core.FinishReason.User, true).Dump();
				this.SavedGame.Values[SavedGameEntryName] = record;
			}

			return Task.FromResult<object>(null);
		}

		private async void AutoSignIn()
		{
			Logger.Trace("Signing in autoatically.");
			try
			{
				if (!this.Settings[SettingsState.FirstSignIn])
				{
					Logger.Info("Asking user to sign in for the first time.");
					this.RequireSignInControl.Value.Requirement = this.FeatureProvider.IsOldFullVersion ? Controls.RequirementType.FirstRunFullVersion : Controls.RequirementType.FirstRun;
					await this.UIManager.ShowMessageDialog(this.RequireSignInControl.Value);
					this.Settings[SettingsState.FirstSignIn] = true;
				}
				else if (this.Settings[SettingsState.UserSignedIn])
				{
					await this.Account.SignIn(true);
				}
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot auto sign-in.", ex);
			}
		}

		private void RestoreSavedStatistics()
		{
			var record = this.SavedGame.Get<ApplicationDataCompositeValue>(SavedGameEntryName);
			if (record != null)
			{
				Logger.Trace("Restoring saved statistics.");
				BoardStatistics stats = null;
				try
				{
					stats = record.RestoreStatistics();

					Logger.Info("Saved statistics restored. Saving to the statistics store.");
					this.UserStatistics.AddStatistics(stats);
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot restore saved statistics. Ignoring.", ex);
				}
			}

			this.SavedGame.Values.Clear();
		}
	}
}
