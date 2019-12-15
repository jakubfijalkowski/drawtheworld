using System;
using System.ComponentModel;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Game.Helpers;
using FLib;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Manages user's account and displays information about it in the settings.
	/// </summary>
	sealed partial class SettingsAndAccount
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.AccountControl");

		private const string ResourceKey = "Controls_SettingsAndAccount_";

		private readonly AccountManager Manager = null;
		private readonly IUIManager UIManager = null;
		private readonly Status CurrentStatus = null;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public SettingsAndAccount(AccountManager manager, IUIManager uiManager, ISettingsProvider settings)
		{
			this.Manager = manager;
			this.UIManager = uiManager;

			this.InitializeComponent();

			this.Manager.UserSignedIn += this.OnUserSignedIn;
			this.Manager.UserSignedOut += this.OnUserSignedOut;

			this.SettingsPopup.DataContext = this.CurrentStatus = new Status(manager, settings);
		}

        public void Open()
        {
            this.SettingsPopup.IsOpen = true;
        }

		private void OnAccountInfoOpened(FLib.UI.Controls.SettingsPanel sender, object args)
		{
			this.CurrentStatus.HasErrors = false;
		}

		private async void OnSignButtonPressed(object sender, RoutedEventArgs e)
		{
			bool signIn = (string)((Button)sender).Tag == "In";

			try
			{
				Logger.Info("Signing user {0}.", signIn ? "in" : "out");
				if (signIn)
					await this.Manager.SignIn(false);
				else
					await this.Manager.SignOut();
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot sign user {0}.".FormatWith(signIn ? "in" : "out"), ex);
				this.CurrentStatus.HasErrors = true;
			}
		}

		private void OnUserSignedIn(AccountManager manager, User user)
		{
			Logger.Debug("User signed in, updating UI.");
			this.CurrentStatus.SignedIn = true;
			this.CurrentStatus.SignedAs = user.ApiUser.FirstName + ' ' + user.ApiUser.LastName;
			this.CurrentStatus.CoinsAmount = this.Manager.User.ApiUser.Coins;
			this.CurrentStatus.CanSignOut = manager.CanSignOut;
		}

		private void OnUserSignedOut(AccountManager manager, User user)
		{
			Logger.Debug("User signed out, updating UI.");
			this.CurrentStatus.SignedIn = false;
			this.CurrentStatus.SignedAs = string.Empty;
			this.CurrentStatus.CoinsAmount = 0;
			this.CurrentStatus.CanSignOut = manager.CanSignOut;
		}

		private async void BuyMoreCoins(object sender, RoutedEventArgs e)
		{
			this.SettingsPopup.IsOpen = false;
			if (await this.UIManager.ShowCoinsUI(false))
				this.CurrentStatus.CoinsAmount = this.Manager.User.ApiUser.Coins;
		}

		private sealed class Status
			: INotifyPropertyChanged
		{
			private readonly AccountManager Manager = null;
			private readonly ISettingsProvider _Settings = null;

			private string _SignedAs = string.Empty;
			private bool _HasErrors = false;
			private long _CoinsAmount = 0;

			private bool _CanSignOut = false;
			private bool _SignedIn = false;

			public string SignedAs
			{
				get { return this._SignedAs; }
				set
				{
					if (value != this._SignedAs)
					{
						this._SignedAs = value;
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

			public bool HasErrors
			{
				get { return this._HasErrors; }
				set
				{
					if (value != this._HasErrors)
					{
						this._HasErrors = value;
						this.PropertyChanged.Raise(this);
					}
				}
			}

			public bool CanSignOut
			{
				get { return this._CanSignOut; }
				set
				{
					if (value != this._CanSignOut)
					{
						this._CanSignOut = value;
						this.RaiseStatusChange();
					}
				}
			}

			public bool SignedIn
			{
				get { return this._SignedIn; }
				set
				{
					if (value != this._SignedIn)
					{
						this._SignedIn = value;
						this.RaiseStatusChange();
					}
				}
			}

			public bool DuringProcess
			{
				get { return this.Manager.IsProcessing; }
			}

			public bool EffectiveSignIn
			{
				get { return !this.SignedIn && !this.Manager.IsProcessing; }
			}

			public bool EffectiveSignOut
			{
				get { return this.SignedIn && !this.Manager.IsProcessing; }
			}

			public bool EffectiveSignOutButton
			{
				get { return this.EffectiveSignOut && this.CanSignOut; }
			}

			public ISettingsProvider Settings
			{
				get { return this._Settings; }
			}

			public event PropertyChangedEventHandler PropertyChanged;

			private void RaiseStatusChange()
			{
				this.PropertyChanged.Raise(this, _ => _.DuringProcess);
				this.PropertyChanged.Raise(this, _ => _.EffectiveSignIn);
				this.PropertyChanged.Raise(this, _ => _.EffectiveSignOut);
				this.PropertyChanged.Raise(this, _ => _.EffectiveSignOutButton);
			}

			public Status(AccountManager manager, ISettingsProvider settings)
			{
				this.Manager = manager;
				this._Settings = settings;
				this.Manager.PropertyChanged += this.OnManagerPropertyChanged;
			}

			private void OnManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == this.Manager.NameOf(_ => _.IsProcessing))
					this.RaiseStatusChange();
			}
		}
	}
}
