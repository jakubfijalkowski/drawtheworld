using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData.Repositories;
using DrawTheWorld.Game.Utilities;
using DrawTheWorld.Web.Api.Public;
using FLib;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Page with online store.
	/// </summary>
	sealed partial class StorePage
		: UIPage
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.StorePage");

		private readonly AccountManager Account = null;
		private readonly UserAccountManager UserAccount = null;
		private readonly Notifications Notifications = null;
		private readonly IUIManager UIManager = null;
		private readonly ISoundManager Sounds = null;

		private readonly PackList PackList = null;
		private readonly OnlineRepository Repository = null;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public StorePage(AccountManager account, UserAccountManager userAccount, Notifications notifications, IUIManager uiManager, ISoundManager sounds, PackList packs, OnlineRepository repo)
		{
			this.Account = account;
			this.UserAccount = userAccount;
			this.Notifications = notifications;
			this.UIManager = uiManager;
			this.Sounds = sounds;

			this.PackList = packs;
			this.Repository = repo;

			this.InitializeComponent();
		}

		protected internal override async void OnNavigatedTo(Type sourcePageType)
		{
			Logger.Trace("Navigated to the Store page.");

			if (this.PackList.RealCount == -1 || this.PackList.Count == 0)
			{
				Logger.Debug("Online packs were not loaded, updating.");

				LoadMoreItemsResult result;
				FilteredPackList list = null;
				try
				{
					result = await this.PackList.LoadMoreItemsAsync((uint)this.PacksView.DataFetchSize * 3);
					list = await Task.Run(() => new FilteredPackList(this.PackList, this.Repository));
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot load packs.", ex);
					this.Notifications.Notify(new Core.UI.Messages.CannotAccessApiMessage());
					return;
				}

				Logger.Info("{0} packs loaded, rest will be loaded by UI.", result.Count);

				this.LoadingIndicator.Visibility = Visibility.Collapsed;
				this.PacksView.Visibility = Visibility.Visible;
				this.PacksView.ItemsSource = list;
			}
		}

		private void GoBack(object sender, RoutedEventArgs e)
		{
			this.UIManager.NavigateBack();
		}

		private async void PurchasePack(object sender, RoutedEventArgs e)
		{
			var pack = (OnlinePack)((Control)sender).DataContext;
			Logger.Trace("Purchasing pack '{0}'({1}).", pack.Pack.Name, pack.Pack.Id);
			this.Sounds.PlaySound(Sound.PageChanged);

			try
			{
				pack.DuringPurchase = true;
				using (var blockade = await this.Account.EnsureSignedIn(this.Notifications))
				{
					if (blockade == null)
					{
						Logger.Debug("User not logged in, aborting.");
						return;
					}

					bool tryAgain = false;
					do
					{
						bool buyMoreMoney = false;
						tryAgain = false;
						try
						{
							Logger.Info("Purchasing pack {0}.", pack.Pack.Id);
							await this.UserAccount.PurchasePack(pack.Pack.Id);
						}
						catch (ApiException ex)
						{
							Logger.Warn("Purchase failed. Aborting.", ex);
							this.Notifications.Notify(new Core.UI.Messages.CannotAccessApiMessage());
						}
						catch (Core.UserData.PackLoadException ex)
						{
							Logger.Warn("Cannot parse pack. Purchase succeeded, but pack is not available.", ex);
							this.Notifications.Notify(new Core.UI.Messages.PackDataErrorMessage());
						}
						catch (PaymentRequiredException)
						{
							Logger.Warn("Purchase failed. Aborting.");
							buyMoreMoney = true;
						}

						if (buyMoreMoney)
						{
							var msg = new Core.UI.Messages.NotEnoughMoneyMessage();
							await this.Notifications.NotifyAsync(msg);
							tryAgain = msg.Result;
						}
					} while (tryAgain);
				}
			}
			finally
			{
				pack.DuringPurchase = false;
			}
		}

		private async void OpenPack(object sender, RoutedEventArgs e)
		{
			var pack = (OnlinePack)((Control)sender).DataContext;
			var id = pack.Pack.Id;

			if (!await this.Repository.IsDownloaded(id))
			{
				Logger.Info("Pack {0} purchased and not downloaded. Trying to reload.", id);
				try
				{
					await this.Repository.Download(id);
				}
				catch (System.IO.IOException ex)
				{
					Logger.Warn("Cannot save pack.", ex);
					this.Notifications.Notify(new Core.UI.Messages.StorageProblemMessage());
					return;
				}
				catch (Core.UserData.PackLoadException ex)
				{
					Logger.Warn("Cannot load pack.", ex);
					this.Notifications.Notify(new Core.UI.Messages.PackDataErrorMessage());
					return;
				}
			}
			Logger.Info("Opening pack {0}.", pack.Pack.Id);
			this.UIManager.NavigateTo<Pages.GameList>().ShowPack(pack.Pack.Id);
		}
	}

	sealed class OnlinePack
		: INotifyPropertyChanged
	{
		private const string ResourceKey = "Pages_StorePage_BoardInfo_";

		private bool _IsPurchased = false;
		private bool _DuringPurchase = false;

		public Pack Pack { get; private set; }

		public bool IsPurchased
		{
			get { return this._IsPurchased && !this._DuringPurchase; }
			set
			{
				if (value != this._IsPurchased)
				{
					this._IsPurchased = value;
					this.PropertyChanged.Raise(this);
					this.PropertyChanged.Raise(this, _ => _.IsNotPurchased);
				}
			}
		}

		public bool IsNotPurchased
		{
			get { return !this._IsPurchased && !this._DuringPurchase; }
		}

		public bool DuringPurchase
		{
			get { return this._DuringPurchase; }
			set
			{
				if (value != this._DuringPurchase)
				{
					this._DuringPurchase = value;
					this.PropertyChanged.Raise(this);
					this.PropertyChanged.Raise(this, _ => _.IsPurchased);
					this.PropertyChanged.Raise(this, _ => _.IsNotPurchased);
				}
			}
		}

		public string Information { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public OnlinePack(Pack pack, bool isPurchased)
		{
			this.Pack = pack;
			this.IsPurchased = isPurchased;

			var noun1 = LanguageHelper.GetNoun(pack.BoardsNumber, ResourceKey + "Boards");
			var noun2 = LanguageHelper.GetNoun(pack.BoardsNumber, ResourceKey + "CreatedBy");
			this.Information = pack.BoardsNumber.ToString() + " " + noun1 + " " + noun2 + " " + pack.Author;
		}
	}

	sealed class FilteredPackList
		: ObservableCollection<OnlinePack>, ISupportIncrementalLoading
	{
		private readonly PackList Inner = null;
		private readonly OnlineRepository Repository = null;

		public bool HasMoreItems
		{
			get { return this.Inner.HasMoreItems; }
		}

		public FilteredPackList(PackList inner, OnlineRepository repo)
		{
			this.Inner = inner;
			this.Repository = repo;

			foreach (var p in inner)
				this.Add(new OnlinePack(p, this.Repository.PurchasedPacks.Contains(p.Id)));

			this.Inner.CollectionChanged += this.OnInnerCollectionChanged;
			this.Repository.PurchasedPacks.CollectionChanged += this.OnPurchasedPacksChanged;
		}

		public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
		{
			return this.Inner.LoadMoreItemsAsync(count);
		}

		private void OnInnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var p in e.NewItems.Cast<Pack>())
						this.Add(new OnlinePack(p, this.Repository.PurchasedPacks.Contains(p.Id)));
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void OnPurchasedPacksChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					this.ChangePurchased(e.NewItems.Cast<Guid>(), true);
					break;

				case NotifyCollectionChangedAction.Remove:
					this.ChangePurchased(e.OldItems.Cast<Guid>(), true);
					break;

				case NotifyCollectionChangedAction.Reset:
					this.ForEach(p => p.IsPurchased = false);
					break;
			}
		}

		private void ChangePurchased(IEnumerable<Guid> collection, bool value)
		{
			foreach (var g in collection)
			{
				var element = this.FirstOrDefault(p => p.Pack.Id == g);
				if (element != null)
					element.IsPurchased = value;
			}
		}
    }
}
