using System;
using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData.Repositories;
using DrawTheWorld.Game.Helpers;
using DrawTheWorld.Game.Utilities;
using FLib;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Displays list with packs.
	/// </summary>
	sealed partial class GameList
		: UIPage
	{
		private const string ResourceKey = "Pages_GameList_";
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.GameList");

		private readonly IGameManager GameManager = null;
		private readonly CustomPackRepository CustomRepository = null;
		private readonly LinkedPackRepository LinkedRepository = null;
		private readonly GameDataManager GameData = null;
		private readonly Notifications Notifications = null;
		private readonly IFeatureProvider FeatureProvider = null;
		private readonly IUIManager UIManager = null;
		private readonly ISoundManager Sounds = null;

		private readonly Selector ZoomedIn = null;
		private readonly Selector ZoomedOut = null;

		private new bool IsLoaded = false;

		private GamePack CurrentlySelectedPack
		{
			get
			{
				return (this.ItemsContainer.IsZoomedInViewActive ? this.ZoomedIn.SelectedItem : this.ZoomedOut.SelectedItem) as GamePack;
			}
		}

        private Grid LastHighlightedBoard = null;

		/// <summary>
		/// Initializes the control.
		/// </summary>
		/// <param name="gameManager"></param>
		/// <param name="customRepository"></param>
		/// <param name="gdm"></param>
		/// <param name="notifications"></param>
		/// <param name="featureProvider"></param>
		/// <param name="uiManager"></param>
		public GameList(IGameManager gameManager, CustomPackRepository customRepository, LinkedPackRepository linkedRepo, GameDataManager gdm, Notifications notifications, IFeatureProvider featureProvider, IUIManager uiManager, ISoundManager sounds)
		{
			this.GameManager = gameManager;
			this.CustomRepository = customRepository;
			this.LinkedRepository = linkedRepo;
			this.GameData = gdm;
			this.Notifications = notifications;
			this.FeatureProvider = featureProvider;
			this.UIManager = uiManager;
			this.Sounds = sounds;

			this.InitializeComponent();

			this.ZoomedIn = (Selector)this.ItemsContainer.ZoomedInView;
			this.ZoomedOut = (Selector)this.ItemsContainer.ZoomedOutView;

			this.Loaded += (s, e) => this.IsLoaded = true;
			this.Unloaded += (s, e) => this.IsLoaded = false;
		}

		/// <summary>
		/// Shows pack with particular id.
		/// </summary>
		/// <param name="packId"></param>
		public void ShowPack(Guid packId)
		{
			var pack = this.GameData.Packs.FirstOrDefault(p => p.Pack.Id == packId);
			if (pack != null)
			{
				Logger.Debug("Showing pack '{0}'({1}).", pack.Pack.Name.DefaultTranslation, packId);
				this.ItemsContainer.SelectZoomedInElement(this.IsLoaded, pack);
			}
			else
			{
				Logger.Debug("Cannot show pack with id {0} - pack not found.", packId);
			}
		}

		protected internal override void OnNavigatedTo(Type sourcePageType)
		{
			Logger.Trace("Navigated to the GameList.");

			this.ItemsContainer.DataContext = this.GameData.Packs;
		}

		protected internal override void OnNavigatedFrom(Type destinationPageType)
		{
			Logger.Trace("Navigated from the GameList.");

			this.ItemsContainer.DataContext = null;
		}

		private void GoBack(object sender, RoutedEventArgs e)
		{
			this.UIManager.NavigateBack();
		}

		private void RunGame(object sender, ItemClickEventArgs e)
		{
			Validate.Debug(() => e.ClickedItem, v => v.NotNull().IsOfType(typeof(GameBoard)));

			var board = (GameBoard)e.ClickedItem;
			Logger.Info("Running game for board '{0}'({1}) from pack {2}.", board.Data.Name.DefaultTranslation, board.Data.Id, board.Data.PackId);
			this.GameManager.SelectModeAndStartGame(board.Data);
		}

		private void OnPackSelectionUpdated(object sender, object e)
		{
			if (sender == this.ItemsContainer)
			{
				this.Sounds.PlaySound(Sound.PageChanged);
				this.ZoomedOut.SelectedItem = null;
			}

			this.ManageSelection.Visibility = this.CurrentlySelectedPack != null ? Visibility.Visible : Visibility.Collapsed;
			this.RemovePackButton.Visibility = this.CurrentlySelectedPack != null &&
				(this.CurrentlySelectedPack.Type == Core.UserData.PackType.CustomInstallation || this.CurrentlySelectedPack.Type == Core.UserData.PackType.Linked)
				? Visibility.Visible : Visibility.Collapsed;

			if (this.CurrentlySelectedPack != null && !this.ItemsContainer.IsZoomedInViewActive)
				this.BottomAppBar.IsOpen = true;

			this.OnBoardPointerExited(null, null);
		}

		public void OnBoardPointerEntered(object sender, PointerRoutedEventArgs  e)
		{
            var g = (Grid)sender;
            this.LastHighlightedBoard = g;
            this.BoardStatistics.Show((GameBoard)g.Tag, g);
        }

		public void OnBoardPointerExited(object sender, PointerRoutedEventArgs e)
		{
            var g = (Grid)sender;
            if (this.LastHighlightedBoard == g)
            {
                this.LastHighlightedBoard = null;
                this.BoardStatistics.Hide();
            }
		}

		private void ShowPackInfo(object sender, RoutedEventArgs e)
		{
			this.BottomAppBar.IsOpen = false;
			this.PackInfo.DataContext = this.CurrentlySelectedPack;

			if (sender != null)
				this.PackInfo.OpenAndNavigateToDefault();
			else
				this.PackInfo.IsOpen = true;
		}

		private void RemovePack(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull().That(p => p.Type,
				v2 => v2.IsIn(new Core.UserData.PackType[] { Core.UserData.PackType.CustomInstallation, Core.UserData.PackType.Linked })));
			this.BottomAppBar.IsOpen = false;

			var pack = this.CurrentlySelectedPack.Pack;
			Logger.Trace("Deleting pack '{0}'({1}).", pack.Name.DefaultTranslation, pack.Id);
			if (this.CurrentlySelectedPack.Type == Core.UserData.PackType.CustomInstallation)
				this.CustomRepository.Remove(pack);
			else
				this.LinkedRepository.Remove(pack.Id);
		}

		private async void AddPack(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Trying to add pack.");
			this.BottomAppBar.IsOpen = false;
			if (!await this.FeatureProvider.TestForFeature(f => f.RequestFeature(Feature.PackInstall), this.Notifications, Logger))
				return;

			var pack = await Helpers.UserDataHelper.AddSinglePack(s => this.CustomRepository.Add(s), this.Notifications, Logger);
			if (pack != null)
			{
				var gamePack = this.GameData.Packs.FirstOrDefault(p => p.Pack == pack);
				if (gamePack != null)
					this.ItemsContainer.SelectZoomedInElement(true, gamePack);
				else
					this.ItemsContainer.IsZoomedInViewActive = false;

				Logger.Debug("Pack '{0}'({1}) added.", pack.Name.DefaultTranslation, pack.Id);
			}
		}
	}

	sealed class GameBoardGridViewItemStyleSelector
		: StyleSelector
	{
		private readonly Style FinishedStyle = null;
		private readonly Style NotFinishedStyle = null;

		private readonly Dictionary<GameBoard, GridViewItem> ItemToContainer = new Dictionary<GameBoard, GridViewItem>();

		public GameBoardGridViewItemStyleSelector()
		{
			var res = Application.Current.Resources;
			this.FinishedStyle = (Style)res["FinishedGameBoardGridViewItemStyle"];
			this.NotFinishedStyle = (Style)res["NotFinishedGameBoardGridViewItemStyle"];
		}

		protected override Style SelectStyleCore(object item, DependencyObject container)
		{
			// This hack enables to dynamically update style of the GridViewItem from NotFinished to Finished
			// We have to distinguish between the two styles because of the load performance.
			var gb = (GameBoard)item;
			if (!gb.IsFinished)
			{
				if (!this.ItemToContainer.ContainsKey(gb))
				{
                    var gvi = (GridViewItem)container;
					this.ItemToContainer.Add(gb, gvi);
					gb.PropertyChanged += this.OnGameBoardPropertyChanged;
				}
				return this.NotFinishedStyle;
			}
			return this.FinishedStyle;
		}

        private void OnGameBoardPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var gb = (GameBoard)sender;

			if (e.PropertyName == gb.NameOf(_ => _.IsFinished) && gb.IsFinished)
			{
				this.ItemToContainer[gb].Style = this.FinishedStyle;
				this.ItemToContainer.Remove(gb);
				gb.PropertyChanged -= this.OnGameBoardPropertyChanged;
			}
		}
	}
}
