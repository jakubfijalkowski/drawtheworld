using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.UserData.Repositories;
using DrawTheWorld.Game.Helpers;
using FLib;

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// Default implementation of the <see cref="IGameManager"/>.
	/// </summary>
	sealed class GameManager
		: IGameManager
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.GameManager");

		private readonly IUIManager UIManager = null;
		private readonly IPartsProvider PartsProvider = null;
		private readonly IFeatureProvider Features = null;
		private readonly Core.UI.Notifications Notifications = null;

		private readonly UserStatistics UserStatistics = null;
		private readonly DesignerRepository DesignerRepository = null;
		private readonly CustomPackRepository CustomRepository = null;

		private Utilities.GameSounds GameSounds = null;

		private MutableBoardData DesignedBoard = null;
		private BoardData DesignedBoardClone = null;

		/// <inheritdoc />
		public IGame Game { get; private set; }

		/// <inheritdoc />
		public GameData GameData { get; private set; }

		/// <inheritdoc />
		public bool IsInGame
		{
			get { return this.Game != null; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="uiManager"></param>
		/// <param name="partsProvider"></param>
		/// <param name="lifecycleManager"></param>
		/// <param name="features"></param>
		/// <param name="userStats"></param>
		/// <param name="designerRepo"></param>
		public GameManager(
			IUIManager uiManager, IPartsProvider partsProvider, IFeatureProvider features, Core.UI.Notifications notifications,
			UserStatistics userStats, DesignerRepository designerRepo, CustomPackRepository customRepo)
		{
			this.UIManager = uiManager;
			this.PartsProvider = partsProvider;
			this.Features = features;
			this.Notifications = notifications;

			this.UserStatistics = userStats;
			this.DesignerRepository = designerRepo;
			this.CustomRepository = customRepo;
		}

		/// <inheritdoc />
		public async Task<bool> SelectModeAndStartGame(BoardData board)
		{
			Validate.Debug(() => this.Game, v => v.Null());

			Logger.Info("Preparing game.");

			var mode = await this.UIManager.SelectGameMode();
			if (mode == null)
			{
				Logger.Info("Aborted.");
				return false;
			}

			Logger.Info("Starting game. Board: '{0}'({1}) from pack {2}. Mode: {3}.", board.Name.DefaultTranslation, board.Id, board.PackId, mode.GetType().Name);

			await this.UIManager.ShowLoading();

			this.Game = await Task.Run(() => new Core.Game(board, mode));
			this.GameData = new GameData(this.Game, this.PartsProvider);
			this.GameSounds = new Utilities.GameSounds(this.Game, this.GameData);

			this.Game.GameFinished += this.OnGameFinished;

			this.UIManager.NavigateTo<Pages.GamePage>();
			this.UIManager.HideLoadingPopup();

			Logger.Trace("Starting game.");
			this.Game.Start();
			return true;
		}

		/// <inheritdoc />
		public async Task StartDesigner(MutableBoardData board)
		{
			Validate.Debug(() => this.Game, v => v.Null());

			Logger.Debug("Preparing designer.");
			if (!await this.Features.TestForFeature(f => f.RequestFeature(Feature.Designer), this.Notifications, Logger))
				return;

			Logger.Info("Sarting designer. Board: '{0}'({1}) from pack {2}.", board.Name.DefaultTranslation, board.Id, board.PackId);

			await this.UIManager.ShowLoading();

			this.Game = await Task.Run(() => new Core.Designer(board));
			this.GameData = new GameData(this.Game, this.PartsProvider);
			this.GameSounds = new Utilities.GameSounds(this.Game, this.GameData);

			this.DesignedBoard = board;
			this.DesignedBoardClone = board.ToReadOnly();

			this.Game.GameFinished += this.OnDesignerFinished;

			this.UIManager.NavigateTo<Pages.DesignerPage>();
			this.UIManager.HideLoadingPopup();

			Logger.Trace("Starting.");
		}

		private async void OnGameFinished(IGame game, FinishReason reason)
		{
			Logger.Info("Game finished, taking post-game actions.");

			this.GameSounds.Dispose();
			this.Game.GameFinished -= this.OnGameFinished;

			Logger.Debug("Saving statistics.");
			this.UserStatistics.AddStatistics(game.CreateStatistics());

			Logger.Trace("Showing finish UI.");
			await this.UIManager.GetCurrentPage<Pages.GamePage>().ShowFinishUI();
			Logger.Trace("Navigating to the game list page.");
			((Pages.GameList)this.UIManager.NavigateBack()).ShowPack(game.Board.BoardInfo.PackId);

			this.GameSounds = null;
			this.GameData = null;
			this.Game = null;
			Logger.Trace("Post-game actions done.");
		}

		private void OnDesignerFinished(IGame game, FinishReason reason)
		{
			Logger.Info("Game finished, taking post-designer actions.");

			this.GameSounds.Dispose();
			this.Game.GameFinished -= this.OnDesignerFinished;

			if (this.UIManager.GetCurrentPage<Pages.DesignerPage>().SaveRequested)
			{
				Logger.Debug("Saving changes.");
				((Core.Designer)this.Game).SaveChanges();

				var pack = this.DesignerRepository.Packs.First(p => p.Id == this.DesignedBoard.PackId);
				this.SavePack(pack);
			}
			else
			{
				Logger.Debug("Discarding changes.");
				this.DesignedBoard.Reset(this.DesignedBoardClone);
			}

			Logger.Trace("Navigating to the designer list.");
			((Pages.DesignerList)this.UIManager.NavigateBack()).ShowPack(this.DesignedBoard.PackId);

			this.DesignedBoardClone = null;
			this.DesignedBoard = null;
			this.GameSounds = null;
			this.GameData = null;
			this.Game = null;
			Logger.Trace("Post-designer actions done.");
		}

		private async void SavePack(MutablePack pack)
		{
			try
			{
				await this.DesignerRepository.CommitChanges(pack);
			}
			catch
			{
				this.Notifications.Notify(new Core.UI.Messages.PackSaveErrorMessage());
				return;
			}
		}
	}
}
