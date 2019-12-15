using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.Online
{
	/// <summary>
	/// Signed in user.
	/// </summary>
	public sealed class User
		: INotifyPropertyChanged
	{
		private Web.Api.Public.User _ApiUser = null;
		private readonly string _UserId = null;

		/// <summary>
		/// Gets the user that is returned by the API.
		/// </summary>
		public Web.Api.Public.User ApiUser
		{
			get { return this._ApiUser; }
			internal set
			{
				if (value != this._ApiUser)
				{
					this._ApiUser = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}


		/// <summary>
		/// Gets the internal user id.
		/// </summary>
		public string UserId
		{
			get { return this._UserId; }
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="apiUser"></param>
		/// <param name="userId"></param>
		public User(Web.Api.Public.User apiUser, string userId)
		{
			Validate.Debug(() => apiUser, v => v.NotNull());
			Validate.Debug(() => userId, v => v.NotNullAndNotWhiteSpace());

			this._ApiUser = apiUser;
			this._UserId = userId;
		}
	}

	/// <summary>
	/// Blocks signing the user out.
	/// </summary>
	public sealed class SignOutBlockade
		: IDisposable
	{
		private readonly CountdownEvent Event = null;

		/// <summary>
		/// Gets the token that notifies
		/// </summary>
		public readonly CancellationToken CancellationToken;

		internal SignOutBlockade(CountdownEvent @event, CancellationToken cancellationToken)
		{
			this.Event = @event;
			this.CancellationToken = cancellationToken;
		}

		/// <summary>
		/// Releases lock for this thread.
		/// </summary>
		public void Dispose()
		{
			this.Event.Signal();
		}
	}

	/// <summary>
	/// Manages user account.
	/// </summary>
	/// <remarks>
	/// For now, we support only one provider.
	/// </remarks>
	/// TODO: when more sophisticated user system will be introduced, IAccountProvider should not provide user id.
	public sealed class AccountManager
		: INotifyPropertyChanged
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.AccountManager");

		private readonly Web.Api.Public.Client Client = null;
		private readonly IAccountProvider Provider = null;
		private readonly IFeatureProvider Features = null;

		private User _User = null;
		private bool _IsProcessing = false;

		private SemaphoreSlim SigningLock = new SemaphoreSlim(1);
		private CountdownEvent BlockadeEvent = null;
		private CancellationTokenSource TokenSource = null;

		/// <summary>
		/// Gets the currently signed in user.
		/// May be null, if user is not logged in.
		/// </summary>
		public User User
		{
			get { return this._User; }
			private set
			{
				if (value != this._User)
				{
					this._User = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <summary>
		/// Indicates whether the user can be signed out.
		/// </summary>
		public bool CanSignOut
		{
			get { return this.Provider.CanSignOut; }
		}

		/// <summary>
		/// Gets the API client that is authorized for current user.
		/// </summary>
		public Web.Api.Public.Client AuthorizedClient
		{
			get
			{
				Validate.Debug(() => this.User, v => v.NotNull());
				return this.Client;
			}
		}

		/// <summary>
		/// The event is raised when user is signed in.
		/// </summary>
		public event Action<AccountManager, User> UserSignedIn;

		/// <summary>
		/// The event is raised when user is signed out.
		/// </summary>
		public event Action<AccountManager, User> UserSignedOut;

		/// <summary>
		/// Gets value that indicates whether the manager is processing some action.
		/// </summary>
		public bool IsProcessing
		{
			get { return this._IsProcessing; }
			private set
			{
				if (value != this._IsProcessing)
				{
					this._IsProcessing = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="client">This client will be used for authorization.</param>
		public AccountManager(Web.Api.Public.Client client, IAccountProvider provider, IFeatureProvider features)
		{
			Validate.Debug(() => client, v => v.NotNull());
			Validate.Debug(() => provider, v => v.NotNull());

			this.Client = client;
			this.Provider = provider;
			this.Features = features;
		}

		/// <summary>
		/// Signs user in using selected provider.
		/// </summary>
		/// <param name="silent">If true, user will not be prompted for credentials and signing in is more likely to fail. Effectivly this enables silent auto sign-in.</param>
		public async Task<bool> SignIn(bool silent)
		{
			User userToRaise = null;
			try
			{
				await this.SigningLock.WaitAsync();
				if (this.User != null)
					return true;

				this.IsProcessing = true;
				Logger.Trace("Signing user in.");
				try
				{
					if (!await this.Provider.SignIn(silent))
						return false;
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot sign user in.", ex);
					throw;
				}

				try
				{
					var apiUser = await this.TrySignIn() ?? await this.RegisterNewUser();
					this.User = new User(apiUser, this.Provider.UserId);
					userToRaise = this.User;
				}
				catch
				{
					Logger.Debug("Sign-in process failed, reseting provider.");
					this.Provider.Reset();
					throw;
				}
			}
			finally
			{
				this.IsProcessing = false;
				this.SigningLock.Release();
			}

			// This needs to be raised outside of the lock
			if (userToRaise != null)
			{
				this.UserSignedIn.Raise(this, userToRaise);
				this.UpdateUserInformationIfNeeded();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Signs user out.
		/// </summary>
		public async Task SignOut()
		{
			Validate.Debug(() => this.CanSignOut, v => v.True());

			User userToRaise = null;
			try
			{
				await this.SigningLock.WaitAsync();
				if (this.User == null)
					return;

				this.IsProcessing = true;
				if (this.BlockadeEvent != null && !this.BlockadeEvent.IsSet)
				{
					if (!this.TokenSource.IsCancellationRequested)
						this.TokenSource.Cancel();
					this.BlockadeEvent.Wait();
				}

				this.BlockadeEvent = null;
				this.TokenSource = null;
				userToRaise = this.User;
				this.User = null;

				this.Client.SignOut();
				await this.Provider.SignOut();
			}
			finally
			{
				this.IsProcessing = false;
				this.SigningLock.Release();
			}

			// Same as in SignIn - needs to be raised outside of the lock
			if (userToRaise != null)
				this.UserSignedOut.Raise(this, userToRaise);
		}

		/// <summary>
		/// Refreshes information about the user.
		/// </summary>
		/// <remarks>
		/// This operation will not throw exception under any circumstances.
		/// </remarks>
		/// <returns>True, if user was refreshed, otherwise false.</returns>
		public async Task<bool> RefreshUser()
		{
			if (this.IsProcessing)
				return false;

			using (var blockade = await this.BlockSignOut())
			{
				if (blockade == null)
					return false;

				try
				{
					this.IsProcessing = true;
					await this.Client.RefreshUser();
					this.User.ApiUser = this.Client.User;
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot refresh user. Ignoring.", ex);
					return false;
				}
				finally
				{
					this.IsProcessing = false;
				}
			}
			return true;
		}

		/// <summary>
		/// Blocks signing user out.
		/// </summary>
		/// <remarks>
		/// May return null, when user is signed out during the initial call to the method.
		/// </remarks>
		/// <returns></returns>
		public async Task<SignOutBlockade> BlockSignOut()
		{
			try
			{
				await this.SigningLock.WaitAsync();
				if (this.User == null)
					return null;

				if (this.BlockadeEvent == null || !this.BlockadeEvent.TryAddCount())
				{
					this.BlockadeEvent = new CountdownEvent(1);
					this.TokenSource = new CancellationTokenSource();
				}

				return new SignOutBlockade(this.BlockadeEvent, this.TokenSource.Token);
			}
			finally
			{
				this.SigningLock.Release();
			}
		}

		private async Task<Web.Api.Public.User> TrySignIn()
		{
			try
			{
				return await this.Client.SignIn(this.Provider.AuthToken);
			}
			catch (Web.Api.Public.UserDoesNotExistException)
			{
				Logger.Info("User does not exist. Registering");
				return null;
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot authenticate user in API.", ex);
				throw;
			}
		}

		private async Task<Web.Api.Public.User> RegisterNewUser()
		{
			try
			{
				var appReceipt = await this.Features.BuyProduct(Product.FullApp, 0);
				var userInfo = await this.Provider.GetUserInformation();
				var registeredUser = await this.Client.Register(userInfo, this.Provider.AuthToken);
				//if (appReceipt != null)
				//{
				//	var msg = await this.Client.PostAsync("account/purchase", new Web.Api.Public.PurchaseRequest
				//	{
				//		Store = Web.Api.Public.Store.WindowsStore,
				//		ProductType = Web.Api.Public.ProductType.FullApplication,
				//		Receipt = appReceipt
				//	}, true);
				//	if (msg.IsSuccessStatusCode)
				//	{
				//		await this.Client.RefreshUser();
				//		registeredUser = this.Client.User;
				//	}
				//}
				return registeredUser;
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot register new user.", ex);
				throw;
			}
		}

		private async void UpdateUserInformationIfNeeded()
		{
			using (var blockade = await this.BlockSignOut())
			{
				if (blockade == null)
					return;

				bool needsUpdate = false;
				try
				{
					var oldInfo = this.User.ApiUser;
					var newInfo = await this.Provider.GetUserInformation();
					// 'Null' means not provided
					needsUpdate =
						(newInfo.FirstName != null && oldInfo.FirstName != newInfo.FirstName) ||
						(newInfo.LastName != null && oldInfo.LastName != newInfo.LastName) ||
						(newInfo.Address != null && oldInfo.Address != newInfo.Address);
					if (needsUpdate)
					{
						this.IsProcessing = true;
						//(await this.Client.PatchAsync("account", newInfo, true)).EnsureSuccessStatusCode();
						await this.Client.RefreshUser();
						this.User.ApiUser = this.Client.User;
					}
				}
				catch (Exception ex)
				{
					Logger.Warn("Cannot get user info from provider.", ex);
					return;
				}
				finally
				{
					if (needsUpdate)
						this.IsProcessing = false;
				}
			}
		}
	}
}
