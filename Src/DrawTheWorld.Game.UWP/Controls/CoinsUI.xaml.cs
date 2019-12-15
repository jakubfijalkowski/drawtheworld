using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Displays popup with available coin packs on top of actual UI.
	/// Can be used only by <see cref="Subsystems.UIManager"/>, because it needs to be integrated into the UI.
	/// </summary>
	sealed partial class CoinsUI
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.CoinsUI");

		private const string CoinTierImageUri = "ms-appx:///Assets/Images/InApp/CoinTier{0}.png";

		private const int TiersList = 0;
		private const int PurchaseProgress = 1;
		private const int PackPurchased = 2;
		private const int NoMoreConsumablesError = 3;
		private const int ApiError = 4;
		private const int OtherError = 5;
		private const int OtherErrorWithReceipt = 6;

		private readonly IFeatureProvider FeatureProvider = null;
		private readonly UserAccountManager UserAccount = null;
		private readonly Notifications Notifications = null;

		private readonly Utilities.SelectCommand<int?> _SelectTierCommand = new Utilities.SelectCommand<int?>();
		private readonly DialogStatus Status = new DialogStatus();

		public Utilities.SelectCommand<int?> SelectTierCommand
		{
			get { return this._SelectTierCommand; }
		}

		/// <summary>
		/// Initializes the UI.
		/// </summary>
		/// <param name="featureProvider"></param>
		public CoinsUI(IFeatureProvider featureProvider, UserAccountManager userAccount, Notifications notifications)
		{
			this.DataContext = this.Status;
			this.InitializeComponent();

			this.FeatureProvider = featureProvider;
			this.UserAccount = userAccount;
			this.Notifications = notifications;

			Logger.Debug("Loading tiers.");
			var tiersList = (ItemsControl)((StackPanel)((Border)this.Popup.Items[0]).Child).Children[1];
			tiersList.ItemsSource = this.FeatureProvider.CoinTiers
				.Select(t => new { Info = t, Coins = t.CoinsAmount, Image = new Uri(CoinTierImageUri.FormatWith(t.Tier)) })
				.ToList();
		}

		/// <summary>
		/// Shows the UI and waits for user action.
		/// </summary>
		/// <param name="showHint"></param>
		/// <returns></returns>
		public async Task<bool> Show(bool showHint)
		{
			Logger.Trace("Showing UI.");

			this.Status.ShowHint = showHint;
			await this.Popup.SelectIndexAsync(TiersList);
			await this.Popup.OpenAsync();

			var tier = await
				WaitFor.Action<int?, int?>(
				h => this.SelectTierCommand.ItemSelected += h,
				h => this.SelectTierCommand.ItemSelected -= h,
				() => this.SelectTierCommand.SelectedItem);

			string receipt = null;
			int screen = 0;
			try
			{
				if (!tier.HasValue)
					return false;

				this.Popup.PreventClose = true;
				await this.Popup.SelectIndexAsync(PurchaseProgress);

				Logger.Debug("Buying coins tier {0}.", tier.Value);

				receipt = await this.FeatureProvider.BuyProduct(Product.CoinPack, tier.Value);
				if (receipt != null)
				{
					Logger.Info("Product purchased in WinStore.");
					await this.UserAccount.Purchase(receipt, Web.Api.Public.Store.WindowsStore, Web.Api.Public.ProductType.CoinPack, tier.Value);
					receipt = null;
					Logger.Info("Product purchase fulfilled.");
					this.Status.CoinsAmount = this.UserAccount.AccountManager.User.ApiUser.Coins;
					screen = PackPurchased;
				}
				else
				{
					Logger.Warn("Product cannot be purchased. Receipt not included.");
					screen = OtherError;
				}
			}
			catch (NoMoreConsumablesException ex)
			{
				Logger.Warn("Cannot buy product because of WinStore error.", ex);
				screen = NoMoreConsumablesError;
			}
			catch (ApiException ex)
			{
				Logger.Error("Cannot access DtW API. The receipt will be saved for future use.", ex);
				screen = ApiError;
			}
			catch (InvalidReceiptException ex)
			{
				Logger.Error("The receipt was invalid.", ex);
				screen = OtherError;
				receipt = null;
			}
			catch (Exception ex)
			{
				Logger.Error("Coin purchase cannot be finalized because of error.", ex);
				screen = receipt != null ? OtherError : OtherErrorWithReceipt;
			}
			finally
			{
				this.Popup.PreventClose = false;
			}

			if (screen != PackPurchased && receipt != null)
				this.FeatureProvider.SaveUnfulfilledReceipt(Product.CoinPack, tier.Value, receipt);

			await this.Popup.SelectIndexAsync(screen);
			await WaitFor.Event<object>(h => this.Popup.Closed += h, h => this.Popup.Closed -= h);
			return screen == PackPurchased;
		}

		private void OnPopupClosed(object sender, object e)
		{
			this.SelectTierCommand.Execute(null);
		}

		private sealed class DialogStatus
			: INotifyPropertyChanged
		{
			private bool _ShowHint = false;
			private long _CoinsAmount = 0;

			public bool ShowHint
			{
				get { return this._ShowHint; }
				set
				{
					if (value != this._ShowHint)
					{
						this._ShowHint = value;
						this.PropertyChanged.Raise(this);
					}
				}
			}

			public long CoinsAmount
			{
				get { return this._CoinsAmount; }
				set
				{
					if (value != this._CoinsAmount)
					{
						this._CoinsAmount = value;
						this.PropertyChanged.Raise(this);
					}
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}
	}
}
