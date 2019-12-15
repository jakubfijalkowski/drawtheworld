using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.UserData.Repositories
{
	/// <summary>
	/// Repository with custom packs.
	/// </summary>
	public sealed class CustomPackRepository
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.CustomPackRepository");
		private readonly PacksCollection _Packs = new PacksCollection();
		private readonly IPackStore Store = null;

		/// <summary>
		/// Gets the provider of packs for this repository.
		/// </summary>
		public IPackProvider Provider
		{
			get { return this._Packs; }
		}

		/// <summary>
		/// Initializes the repository.
		/// </summary>
		/// <param name="store"></param>
		public CustomPackRepository(ICustomPackStore store)
		{
			Validate.Debug(() => store, v => v.NotNull());
			this.Store = store;
		}

		/// <summary>
		/// Loads pack from <paramref name="input"/>, assigns id and adds to the underlying repository.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async Task<Pack> Add(Stream input)
		{
			Validate.Debug(() => input, v => v.NotNull().That(s => s.CanRead, v2 => v2.True()));

			Pack pack;
			try
			{
				Logger.Info("Loading new pack from the stream.");
				pack = await Task.Run(() => PackLoader.Load(input));
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot load pack. Aborting.", ex);
				throw;
			}
			await this.AddOrReplace(pack);
			return pack;
		}

		/// <summary>
		/// Adds new pack to the repository or replaces existing one.
		/// </summary>
		/// <param name="pack"></param>
		/// <returns></returns>
		public async Task AddOrReplace(Pack pack)
		{
			Validate.Debug(() => pack, v => v.NotNull().That(p => p.Id, v2 => v2.NotEqual(Guid.Empty)));
			var existingId = this._Packs.IndexOf(pack.Id);

			if (existingId != -1)
			{
				var existing = this._Packs[existingId];
				Logger.Info("Replacing pack '{0}'({1}) with new version(name: '{2}').", existing.Name.DefaultTranslation, pack.Id, pack.Name.DefaultTranslation);
			}
			else
				Logger.Info("Adding new pack '{0}'({1}).", pack.Name.DefaultTranslation, pack.Id);

			try
			{
				Logger.Trace("Saving pack locally.");
				await this.Store.SavePack(pack.Id, s => PackLoader.Save(pack, s));
				Logger.Trace("Saved. Updating local collection.");
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot save pack locally. Aborting.", ex);
				throw new IOException("Cannot save pack.", ex);
			}

			if (existingId != -1)
				this._Packs.RemoveAt(existingId);
			this._Packs.Add(pack);
			Logger.Info("Pack {0}({1}) added successfully.", pack.Name.DefaultTranslation, pack.Id);
		}

		/// <summary>
		/// Tries to update pack that is already in repository.
		/// </summary>
		/// <param name="pack"></param>
		/// <returns>True if the pack was updated, otherwise false.</returns>
		public bool TryUpdate(Pack pack)
		{
			if (this._Packs.Remove(pack.Id))
			{
				this._Packs.Add(pack);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Checks if repository contains pack with specified id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool Contains(Guid id)
		{
			return this._Packs.IndexOf(id) != -1;
		}

		/// <summary>
		/// Removes <paramref name="pack"/> from the repository.
		/// </summary>
		/// <param name="pack"></param>
		public void Remove(Pack pack)
		{
			Validate.Debug(() => pack, v => v.NotNull().IsIn(this._Packs));

			Logger.Info("Removing pack {0}({1}).", pack.Name.DefaultTranslation, pack.Id);
			this._Packs.Remove(pack);
			this.Store.RemovePack(pack.Id);
		}

		/// <summary>
		/// Loads installed packs from the store.
		/// </summary>
		/// <remarks>
		/// The repository will be cleared before packs will be loaded.
		/// </remarks>
		/// <returns></returns>
		public async Task Load()
		{
			Logger.Info("Loading packs from the store.");
			this._Packs.Clear();

			await this.Store.LoadPacks(async (s, id) =>
			{
				Logger.Trace("Loading pack {0}.", id);
				var pack = await Task.Run(() => PackLoader.Load(s, id));
				this._Packs.Add(pack);
				Logger.Info("Pack {0}({1}) loaded.", pack.Name.DefaultTranslation, pack.Id);
			});
		}

		private sealed class PacksCollection
			: ObservableCollection<Pack>, IPackProvider
		{
			public PackType PackType
			{
				get { return PackType.CustomInstallation; }
			}

			public int IndexOf(Guid id)
			{
				for (int i = 0; i < this.Items.Count; i++)
				{
					if (this.Items[i].Id == id)
						return i;
				}
				return -1;
			}

			public bool Remove(Guid id)
			{
				var idx = this.IndexOf(id);
				if (idx != -1)
				{
					this.RemoveAt(idx);
					return true;
				}
				return false;
			}
		}
	}
}
