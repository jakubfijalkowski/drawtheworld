using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.UserData.Repositories;
using DrawTheWorld.Game.Controls;
using DrawTheWorld.Game.Helpers;
using DrawTheWorld.Game.Utilities;
using FLib;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Displays list with packs.
	/// </summary>
	sealed partial class DesignerList
		: UIPage
	{
		internal const string ResourceKey = "Pages_DesignerList_";

		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.DesignerList");

		private const int AddExistingCommandId = 1;
		private const int AddNewCommandId = 2;

		private readonly IGameManager GameManager = null;
		private readonly ISettingsProvider Settings = null;
		private readonly DesignerRepository DesignerRepository = null;
		private readonly LinkedPackRepository LinkedRepository = null;
		private readonly Core.UI.Notifications Notifications = null;
		private readonly IUIManager UIManager = null;
		private readonly ISoundManager Sounds = null;
		private readonly IFeatureProvider Features = null;

		private readonly Selector ZoomedIn = null;
		private readonly Selector ZoomedOut = null;

		private readonly PopupMenu AddMenu = null;

		private new bool IsLoaded = false;

		private Selector CurrentBoardSelector = null;

		private MutablePack CurrentlySelectedPack
		{
			get
			{
				return (this.ItemsContainer.IsZoomedInViewActive ? this.ZoomedIn.SelectedItem : this.ZoomedOut.SelectedItem) as MutablePack;
			}
		}

		private MutableBoardData CurrentlySelectedBoard
		{
			get { return this.CurrentBoardSelector != null && this.ItemsContainer.IsZoomedInViewActive ? this.CurrentBoardSelector.SelectedItem as MutableBoardData : null; }
		}

		/// <summary>
		/// Initializes the control.
		/// </summary>
		public DesignerList(IGameManager gameManager, ISettingsProvider settings, DesignerRepository designerRepo, Core.UI.Notifications notifications, IUIManager uiManager,
			ISoundManager sounds, IFeatureProvider features, LinkedPackRepository linkedRepo)
		{
			this.GameManager = gameManager;
			this.Settings = settings;
			this.DesignerRepository = designerRepo;
			this.LinkedRepository = linkedRepo;
			this.Notifications = notifications;
			this.UIManager = uiManager;
			this.Sounds = sounds;
			this.Features = features;

			this.InitializeComponent();

			this.ZoomedIn = (Selector)this.ItemsContainer.ZoomedInView;
			this.ZoomedOut = (Selector)this.ItemsContainer.ZoomedOutView;

			this.AddMenu = new PopupMenu();
			this.AddMenu.Commands.Add(new UICommand(Strings.Get(ResourceKey + "AppBar_Common_AddPack_Existing"), s => this.AddPack(s, null), AddExistingCommandId));
			this.AddMenu.Commands.Add(new UICommand(Strings.Get(ResourceKey + "AppBar_Common_AddPack_New"), s => this.AddPack(s, null), AddNewCommandId));

			this.ItemsContainer.DataContext = designerRepo.Packs;

			this.Loaded += (s, e) => this.IsLoaded = true;
			this.Unloaded += (s, e) => this.IsLoaded = false;
		}

		/// <summary>
		/// Shows pack with particular id.
		/// </summary>
		/// <param name="packId"></param>
		public void ShowPack(Guid packId)
		{
			var pack = this.DesignerRepository.Packs.FirstOrDefault(p => p.Id == packId);
			if (pack != null)
			{
				Logger.Debug("Showing pack '{0}'({1}).", pack.Name.DefaultTranslation, packId);
				this.ItemsContainer.SelectZoomedInElement(this.IsLoaded, pack);
			}
			else
			{
				Logger.Debug("Cannot show pack with id {0} - pack not found.", packId);
			}
		}

		protected internal override void OnNavigatedTo(Type sourcePageType)
		{
			Logger.Trace("Navigated to the DesignerList, updating state.");
			//SettingsPane.GetForCurrentView().CommandsRequested += this.FillSettingsPane;

			if (sourcePageType != typeof(GameList))
			{
				this.ItemsContainer.IsZoomedInViewActive = false;
				this.ZoomedIn.SelectedItem = this.ZoomedOut.SelectedItem = null;
			}

			this.DesignerRepository.Packs.CollectionChanged += this.OnDesignerRepositoryPacksChanged;
			this.OnDesignerRepositoryPacksChanged(null, null);
		}

		protected internal override void OnNavigatedFrom(Type destinationPageType)
		{
			Logger.Trace("Navigated from the DesignedList, removing DesignerRepository changes tracker.");

			this.DesignerRepository.Packs.CollectionChanged -= this.OnDesignerRepositoryPacksChanged;
		}

		private void GoBack(object sender, RoutedEventArgs e)
		{
			this.UIManager.NavigateBack();
		}

		private void RunDesigner(object sender, ItemClickEventArgs e)
		{
			Validate.Debug(() => e.ClickedItem, v => v.NotNull().IsOfType(typeof(MutableBoardData)));

			var board = (MutableBoardData)e.ClickedItem;
			Logger.Info("Opening designer for board '{0}'({1}) from pack {2}.", board.Name.DefaultTranslation, board.Id, board.PackId);
			this.GameManager.StartDesigner(board);
		}

		private void ShowPackSettings(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			Logger.Trace("Showing settings panel for pack '{0}'({1}).", this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);
			this.PackSettings.DataContext = this.CurrentlySelectedPack;

			if (sender != null)
				this.PackSettings.OpenAndNavigateToDefault();
			else
				this.PackSettings.IsOpen = true;
		}

		private void ShowBoardSettings(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedBoard, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			Logger.Trace("Showing settings panel for board '{0}'({1}).", this.CurrentlySelectedBoard.Name.DefaultTranslation, this.CurrentlySelectedBoard.Id);
			this.BoardSettings.DataContext = this.CurrentlySelectedBoard;

			if (sender != null)
				this.BoardSettings.OpenAndNavigateToDefault();
			else
				this.BoardSettings.IsOpen = true;
		}

		private void DeletePack(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			Logger.Trace("Removing pack '{0}'({1}).", this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);
			this.DesignerRepository.Remove(this.CurrentlySelectedPack);
		}

		private void AddBoard(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			Logger.Info("Adding new board to the pack '{0}'({1}).", this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);

			var board = MutableBoardData.CreateDefault();
			Helpers.UserDataHelper.AdjustNewBoardForDesigner(board, this.Settings);
			this.CurrentlySelectedPack.Boards.Add(board);
			this.ItemsContainer.SelectZoomedInElement(true, this.CurrentlySelectedPack);

			this.BoardSettings.DataContext = board;
			this.BoardSettings.IsOpen = true;
		}

		private async void ExportPack(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			try
			{
				var pack = this.CurrentlySelectedPack.ToReadOnly();
				await Helpers.UserDataHelper.ExportPack(pack);
			}
			catch (IOException ex)
			{
				Logger.Warn("Cannot export pack '{0}'({1}) because of IO error.".FormatWith(this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id), ex);
				this.Notifications.Notify(new Core.UI.Messages.StorageProblemMessage());
			}
			catch
			{
				Logger.Debug("Pack '{0}'({1}) is not valid for export.", this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);
				this.Notifications.Notify(new Core.UI.Messages.PackNotValidMessage { Pack = this.CurrentlySelectedPack });
			}
		}

		private async void ImportImages(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			IEnumerable<MutableBoardData> boards = null;

			Logger.Trace("Importing images.");
			try
			{
				boards = await Helpers.UserDataHelper.Import();
			}
			catch (Helpers.AggregateExceptionWithPartialResult<IEnumerable<MutableBoardData>> ex)
			{
				Logger.Warn("Cannot import some images.", ex);
				boards = ex.Result;
				this.Notifications.Notify(new Core.UI.Messages.BoardImportErrorMessage { ErrorCount = ex.InnerExceptions.Count });
			}

			if (boards.Any())
			{
				Logger.Info("{0} images loaded. Adding them to the pack '{1}'({2}).", boards.Count(), this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);
				boards.ForEach(this.CurrentlySelectedPack.Boards.Add);

				try
				{
					await this.DesignerRepository.CommitChanges(this.CurrentlySelectedPack);
				}
				catch (IOException ex)
				{
					Logger.Warn("Cannot save pack.", ex);
					this.Notifications.Notify(new Core.UI.Messages.StorageProblemMessage());
				}
			}
		}

		private void DeleteSelectedBoard(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());
			Validate.Debug(() => this.CurrentlySelectedBoard, v => v.NotNull());

			this.BottomAppBar.IsOpen = false;

			Logger.Info("Removing board '{0}'({1}) from pack '{2}'({3}).", this.CurrentlySelectedBoard.Name.DefaultTranslation, this.CurrentlySelectedBoard.Id,
				this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);

			this.CurrentlySelectedPack.Boards.Remove(this.CurrentlySelectedBoard);
			this.SaveCurrentPack();
		}

		private async void AddPack(object sender, RoutedEventArgs e)
		{
			if (sender is IUICommand)
			{
				var cmd = (IUICommand)sender;
				MutablePack pack = null;
				switch ((int)cmd.Id)
				{
					case AddExistingCommandId:
						if ((pack = await Helpers.UserDataHelper.AddSinglePack(s => this.DesignerRepository.Add(s), this.Notifications, Logger)) == null)
							return;
						break;

					case AddNewCommandId:
						Logger.Trace("Adding new pack.");
						pack = this.DesignerRepository.AddNew();
						Helpers.UserDataHelper.AdjustNewPackForDesigner(pack, this.Settings);
						break;
				}

				this.ItemsContainer.SelectZoomedInElement(true, pack);

				this.PackSettings.DataContext = pack;
				this.PackSettings.IsOpen = true;

				Logger.Debug("Pack '{0}'({1}) added.", pack.Name.DefaultTranslation, pack.Id);
			}
			else
			{
				Logger.Trace("Showing 'add pack' menu.");
				var ignored = this.AddMenu.ShowForSelectionAsync(UIHelper.GetElementRect((FrameworkElement)sender));
			}
		}

		private async void LinkPackWithGame(object sender, RoutedEventArgs e)
		{
			Validate.Debug(() => this.CurrentlySelectedPack, v => v.NotNull());
			var selectedPack = this.CurrentlySelectedPack;
			this.BottomAppBar.IsOpen = false;

			Logger.Trace("Linking pack.");
			if (await this.Features.TestForFeature(f => f.RequestFeature(Feature.PackInstall), this.Notifications, Logger))
			{
				if (!this.LinkedRepository.CheckIfLinked(selectedPack.Id))
				{
					Logger.Trace("Pack not linked, trying to link.");
					if (!this.LinkedRepository.CheckIfSuitableForLinking(selectedPack))
					{
						Logger.Debug("Not valid for linkage.");
						this.Notifications.Notify(new Core.UI.Messages.PackIsEmptyMessage());
					}
					await this.LinkedRepository.Add(selectedPack);
				}
				Logger.Trace("Navigating to GameList.");
				this.UIManager.NavigateTo<Pages.GameList>().ShowPack(selectedPack.Id);
			}
		}

		private void OnPackInfoChanged(FLib.UI.Controls.SettingsPanel sender, object args)
		{
			this.SaveCurrentPack();
		}

		private void OnPackSelectionUpdated(object sender, object e)
		{
			if (sender == this.ItemsContainer)
				this.Sounds.PlaySound(Sound.PageChanged);

			var packRelatedVisibility = this.CurrentlySelectedPack != null ? Visibility.Visible : Visibility.Collapsed;
			this.ManagePackSelection.Visibility = this.AddBoardButton.Visibility = packRelatedVisibility;
			this.BottomAppBar.IsOpen = this.CurrentlySelectedPack != null && !this.ItemsContainer.IsZoomedInViewActive;

			this.OnBoardSelectionUpdated(null, null);
		}

		private void OnBoardSelectionUpdated(object sender, SelectionChangedEventArgs e)
		{
			if (this.CurrentBoardSelector != null)
				this.CurrentBoardSelector.SelectedItem = null;

			this.CurrentBoardSelector = sender as Selector;

			if (this.CurrentlySelectedBoard == null)
			{
				this.CurrentBoardSelector = null;
				this.ManageBoardSelection.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.ManageBoardSelection.Visibility = Visibility.Visible;
				this.BottomAppBar.IsOpen = true;
			}
		}

		private void OnDesignerRepositoryPacksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (this.DesignerRepository.Packs.Count == 0)
			{
				this.NoPacksInformation.Visibility = Visibility.Visible;
				this.BottomAppBar.IsOpen = true;
			}
			else
			{
				this.NoPacksInformation.Visibility = Visibility.Collapsed;
			}
		}

		private async void SaveCurrentPack()
		{
			Logger.Debug("Commiting changes to the pack '{0}'({1}).", this.CurrentlySelectedPack.Name.DefaultTranslation, this.CurrentlySelectedPack.Id);
			try
			{
				await this.DesignerRepository.CommitChanges(this.CurrentlySelectedPack);
			}
			catch
			{
				this.Notifications.Notify(new Core.UI.Messages.PackSaveErrorMessage { Pack = this.CurrentlySelectedPack });
			}
		}
	}

	sealed class DesignerListBoardDisplay
		: Grid
	{
		private static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register("ItemsSource", typeof(object), typeof(DesignerListBoardDisplay), new PropertyMetadata(null, OnItemsSourcePropertyChanged));

		private readonly TextBlockWithShadow AlternateText = null;
		private readonly GridView Items = null;

		/// <summary>
		/// Gets or sets the source of items for this control.
		/// </summary>
		public IReadOnlyObservableList<MutableBoardData> ItemsSource
		{
			get { return (IReadOnlyObservableList<MutableBoardData>)this.GetValue(ItemsSourceProperty); }
			set { this.SetValue(ItemsSourceProperty, value); }
		}

		/// <summary>
		/// See <see cref="GridView.ItemClick"/>.
		/// </summary>
		public event ItemClickEventHandler ItemClick
		{
			add { this.Items.ItemClick += value; }
			remove { this.Items.ItemClick -= value; }
		}

		/// <summary>
		/// See <see cref="GridView.SelectionChanged"/>.
		/// </summary>
		public event SelectionChangedEventHandler SelectionChanged
		{
			add { this.Items.SelectionChanged += value; }
			remove { this.Items.SelectionChanged -= value; }
		}

		public DesignerListBoardDisplay()
		{
			this.AlternateText = new TextBlockWithShadow
			{
				IsTabStop = false,
				Text = Strings.Get(DesignerList.ResourceKey + "NoBoardsInformation"),
				Visibility = Visibility.Visible,
				Style = (Style)Application.Current.Resources["FullScreenTextStyle"]
			};

			this.Items = new GridView
			{
				ItemContainerStyle = (Style)Application.Current.Resources["DesignerBoardGridViewItemStyle"],
				Margin = new Thickness(0, 10, 0, 10),
				IsItemClickEnabled = true,
				SelectionMode = ListViewSelectionMode.Single,
				ItemsPanel = (ItemsPanelTemplate)Application.Current.Resources["HorizontalWrapGrid"],
				Visibility = Visibility.Collapsed,
				CanReorderItems = true,
				AllowDrop = true
			};

			ScrollViewer.SetHorizontalScrollMode(this.Items, ScrollMode.Disabled);
			ScrollViewer.SetHorizontalScrollBarVisibility(this.Items, ScrollBarVisibility.Hidden);
			ScrollViewer.SetVerticalScrollMode(this.Items, ScrollMode.Enabled);
			ScrollViewer.SetVerticalScrollBarVisibility(this.Items, ScrollBarVisibility.Auto);
			ScrollViewer.SetIsHorizontalRailEnabled(this.Items, true);
			ScrollViewer.SetIsVerticalRailEnabled(this.Items, true);

			this.Children.Add(this.AlternateText);
			this.Children.Add(this.Items);
		}

		private static void OnItemsSourcePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var list = (DesignerListBoardDisplay)sender;
			var oldValue = (IReadOnlyObservableList<MutableBoardData>)e.OldValue;
			var newValue = (IReadOnlyObservableList<MutableBoardData>)e.NewValue;

			if (oldValue != null)
				oldValue.CollectionChanged -= list.OnItemsSourceCollectionChanged;
			if (newValue != null)
				newValue.CollectionChanged += list.OnItemsSourceCollectionChanged;

			list.OnItemsSourceCollectionChanged(null, null);
			list.Items.ItemsSource = newValue;
		}

		private void OnItemsSourceCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (this.ItemsSource.Count == 0)
			{
				this.AlternateText.Visibility = Visibility.Visible;
				this.Items.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.AlternateText.Visibility = Visibility.Collapsed;
				this.Items.Visibility = Visibility.Visible;
			}
		}
	}
}
