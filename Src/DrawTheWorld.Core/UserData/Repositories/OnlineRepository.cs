using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.Online;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;

namespace DrawTheWorld.Core.UserData.Repositories
{
	/// <summary>
	/// Repository with packs that the user purchased.
	/// </summary>
	public sealed class OnlineRepository
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.OnlineRepository");

		private readonly PacksCollection _Provider = new PacksCollection();

		private readonly Online.AccountManager AccountManager = null;
		private readonly Online.UserAccountManager UserAccount = null;
		private readonly IUserPackStore Store = null;
		private readonly Notifications Notifications = null;

		private readonly PurchasedPacksCollection _PurchasedPacks = new PurchasedPacksCollection();

		private readonly SemaphoreSlim SyncLock = new SemaphoreSlim(1);

		/// <summary>
		/// Gets the <see cref="IPackProvider"/> for this repository.
		/// </summary>
		public IPackProvider Provider
		{
			get { return this._Provider; }
		}

		/// <summary>
		/// Gets the list of pack ids that the user have.
		/// </summary>
		public IReadOnlyObservableList<Guid> PurchasedPacks
		{
			get { return this._PurchasedPacks; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="store"></param>
		/// <param name="manager"></param>
		/// <param name="userAccount"></param>
		/// <param name="notifications"></param>
		public OnlineRepository(IUserPackStore store, Online.AccountManager manager, Online.UserAccountManager userAccount, Notifications notifications)
		{
			Validate.Debug(() => store, v => v.NotNull());
			Validate.Debug(() => manager, v => v.NotNull());
			Validate.Debug(() => userAccount, v => v.NotNull());
			Validate.Debug(() => notifications, v => v.NotNull());

			this.Store = store;
			this.AccountManager = manager;
			this.UserAccount = userAccount;
			this.Notifications = notifications;

			this.AccountManager.UserSignedIn += this.OnUserSignedIn;
			this.AccountManager.UserSignedOut += this.OnUserSignedOut;
			this.UserAccount.PackPurchased += this.OnPackPurchased;


			if (this.AccountManager.User != null)
				this.OnUserSignedIn(this.AccountManager, this.AccountManager.User);
		}

		/// <summary>
		/// Checks if pack is downloaded. Assumes that the pack is purchased.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<bool> IsDownloaded(Guid id)
		{
			await this.SyncLock.WaitAsync();
			bool result = this._Provider.Contains(id);
			this.SyncLock.Release();
			return result;
		}

		/// <summary>
		/// Tries to download purchased pack that has not been downloaded.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task Download(Guid id)
		{
			Validate.Debug(() => id, v => v.IsIn(this.PurchasedPacks).IsNotIn(this.Provider.Select(p => p.Id)));

			await this.SyncLock.WaitAsync();
			try
			{
				//await this.DownloadAndAddPack(id, CancellationToken.None);
			}
			finally
			{
				this.SyncLock.Release();
			}
		}

		public async void OnPackPurchased(UserAccountManager userAccount, Guid packId, string packData)
		{
			await this.SyncLock.WaitAsync();
			try
			{
				this._PurchasedPacks.Add(packId);
				if (packData != null)
				{
					Pack pack = null;
					try
					{
						using (var s = new StringReader(packData))
							pack = await Task.Run(() => PackLoader.Load(s, packId));
					}
					catch (Exception ex)
					{
						Logger.Warn("Cannot parse pack with id {0}.".FormatWith(packId), ex);
						this.Notifications.Notify(new UI.Messages.PackDataErrorMessage());
						return;
					}

					try
					{
						await this.Store.SavePack(packId, s => PackLoader.Save(pack, s));
					}
					catch (Exception ex)
					{
						Logger.Warn("Cannot save pack {0} to local storage.".FormatWith(packId), ex);
						this.Notifications.Notify(new UI.Messages.StorageProblemMessage());
						return;
					}

					this._Provider.Add(pack);
					this.Notifications.Notify(new UI.Messages.PackPurchasedMessage { Pack = pack });
				}
			}
			finally
			{
				this.SyncLock.Release();
			}
		}

		private async void OnUserSignedIn(AccountManager manager, User user)
		{
			Validate.Debug("Store.UserId", this.Store.UserId, v => v.Null());

			Logger.Info("User signed in, loading installed packs for user {0}.", user.UserId);
			this.Store.UserId = user.UserId;
			await this.SyncLock.WaitAsync();
			await this.Store.LoadPacks(async (s, id) =>
			{
				Logger.Trace("Loading pack {0}.", id);
				var pack = await Task.Run(() => PackLoader.Load(s, id));
				this._Provider.Add(pack);
				Logger.Info("Pack '{0}'({1}) loaded.", pack.Name.DefaultTranslation, id);
			});
			this.SyncPacks();
			this.SyncLock.Release();
		}

		private async void OnUserSignedOut(AccountManager manager, User user)
		{
			Logger.Info("User signed out. Clearing store for user {0}.", user.UserId);

			await this.Store.Clear();
			this.Store.UserId = null;
			this._Provider.Clear();
			this._PurchasedPacks.Clear();
		}

		/// <summary>
		/// Does not acequires sync lock.
		/// </summary>
		private async void SyncPacks()
		{
			int successes = 0, failures = 0;
			using (var blockade = await this.AccountManager.BlockSignOut())
			{
				if (blockade == null)
					return;
				Logger.Info("Syncing packs.");

				try
				{
					//var purchases = await this.DownloadPurchases(blockade.CancellationToken);
					//purchases.ForEach(p => this._PurchasedPacks.Add(p.PackId));
				}
				catch (ApiException)
				{
					this.Notifications.Notify(new UI.Messages.CannotAccessApiMessage { IsBackground = true });
					return;
				}
				catch (OperationCanceledException)
				{
					Logger.Debug("Cancelled.");
					return;
				}

				for (int i = 0; i < this._PurchasedPacks.Count && !blockade.CancellationToken.IsCancellationRequested; i++)
				{
					Guid id = this._PurchasedPacks[i];
					if (!this._Provider.Contains(id))
					{
						try
						{
							//await this.DownloadAndAddPack(id, blockade.CancellationToken);
							++successes;
						}
						catch (OperationCanceledException)
						{
							Logger.Debug("Cancelled.");
							return;
						}
						catch
						{
							Logger.Warn("There is problem with api or pack is invalid(see previous logs). Ignoring.");
							++failures;
						}
					}
				}
			}
			if (successes > 0 || failures > 0)
				this.Notifications.Notify(new UI.Messages.PacksSynchronizedMessage { Successes = successes, Failures = failures });
		}

		//private async Task<IList<Web.Api.Public.PackPurchase>> DownloadPurchases(CancellationToken cancellationToken)
		//{
		//	Logger.Trace("Downloading purchases.");
		//	var purchases = await this.AccountManager.AuthorizedClient.Download(
		//		c => c.GetAsync<IList<Web.Api.Public.PackPurchase>>("account/packs", cancellationToken, true),
		//		ex => Logger.Warn("Cannot download purchases.", ex),
		//		cancellationToken);

		//	cancellationToken.ThrowIfCancellationRequested();

		//	if (purchases.Item1)
		//	{
		//		Logger.Error("Cannot download purchases. Aborted.");
		//		throw new ApiException("Cannot download purchases.");
		//	}
		//	return purchases.Item2;
		//}

		//private async Task DownloadAndAddPack(Guid id, CancellationToken cancellationToken)
		//{
		//	string packData = await this.DownloadPack(id, cancellationToken);
		//	var pack = await this.ParsePack(id, packData);
		//	cancellationToken.ThrowIfCancellationRequested();
		//	await this.Store.SavePack(id, s => PackLoader.Save(pack, s));
		//	this._Provider.Add(pack);
		//	Logger.Info("Pack '{0}'({1}) downloaded and added to the repository.", pack.Name.DefaultTranslation, pack.Id);
		//}

		//private async Task<string> DownloadPack(Guid id, CancellationToken cancellationToken)
		//{
		//	Logger.Info("Downloading pack with id {0}.", id);
		//	HttpResponseMessage downloadResponse = (await this.AccountManager.AuthorizedClient.Download(
		//		c => c.GetAsync("pack/{0}/download".FormatWith(id), cancellationToken, true),
		//		ex => Logger.Warn("Cannot download pack with id {0}.".FormatWith(id), ex),
		//		cancellationToken)).Item2;
		//	cancellationToken.ThrowIfCancellationRequested();

		//	if (downloadResponse == null)
		//	{
		//		Logger.Error("Cannot download pack with id {0}. Aborted.", id);
		//		throw new ApiException("Cannot download pack.");
		//	}
		//	else if (!downloadResponse.IsSuccessStatusCode)
		//	{
		//		Logger.Error("Cannot download pack with id {0}. Status code: {1}. Aborted.", id, downloadResponse.StatusCode);
		//		throw new ApiException("Cannot download pack.", downloadResponse.StatusCode);
		//	}
		//	return await this.AccountManager.AuthorizedClient.DeserializeString(downloadResponse);
		//}

		private async Task<Pack> ParsePack(Guid id, string packData)
		{
			Pack pack = null;
			try
			{
				using (var s = new StringReader(packData))
					pack = await Task.Run(() => PackLoader.Load(s, id));
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot parse pack with id {0}. Skipping.".FormatWith(id), ex);
				throw new PackLoadException("Cannot parse downloaded pack.", ex);
			}
			return pack;
		}

		private sealed class PacksCollection
			: ObservableCollection<Pack>, IPackProvider
		{
			public PackType PackType
			{
				get { return PackType.UserPurchase; }
			}

			public bool Contains(Guid id)
			{
				for (int i = 0; i < this.Items.Count; i++)
				{
					if (this.Items[i].Id == id)
						return true;
				}
				return false;
			}

			protected override void ClearItems()
			{
				this.CheckReentrancy();
				base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (System.Collections.IList)base.Items, 0));
				base.Items.Clear();
				base.OnPropertyChanged(new PropertyChangedEventArgs(this.NameOf(() => Count)));
				base.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
			}
		}

		private sealed class PurchasedPacksCollection
			: ObservableCollection<Guid>, IReadOnlyObservableList<Guid>
		{ }
	}
}
