using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.UserData.Repositories
{
	/// <summary>
	/// Repository of designer's packs.
	/// </summary>
	public sealed class DesignerRepository
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.Repository.Designer");

		private readonly MutablePackObservableList _Packs = new MutablePackObservableList();
		private readonly IDesignerStore Store = null;

		/// <summary>
		/// Raised when pack is successfully commited.
		/// </summary>
		public event Action<DesignerRepository, MutablePack> ChangesCommited;

		/// <summary>
		/// Packs available in designer.
		/// </summary>
		public IReadOnlyObservableList<MutablePack> Packs
		{
			get { return this._Packs; }
		}

		/// <summary>
		/// Initializes the repository.
		/// </summary>
		/// <param name="store"></param>
		public DesignerRepository(IDesignerStore store)
		{
			Validate.Debug(() => store, v => v.NotNull());
			this.Store = store;
		}

		/// <summary>
		/// Adds new pack to the repository.
		/// </summary>
		/// <remarks>
		/// Use <see cref="CommitChanges"/> to save the pack to the store.
		/// </remarks>
		/// <returns></returns>
		public MutablePack AddNew()
		{
			var pack = new MutablePack(Guid.NewGuid());
			this._Packs.Add(pack);
			Logger.Info("Empty pack {0} added.", pack.Id);
			return pack;
		}

		/// <summary>
		/// Adds pack from existing stream.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public async Task<MutablePack> Add(Stream input)
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
			var id = this._Packs.Contains(pack.Id) ? Guid.NewGuid() : pack.Id;
			try
			{
				Logger.Trace("Pack loaded, saving locally.");
				await this.Store.SavePack(id, s => PackLoader.Save(pack, s));
				Logger.Trace("Saved.");
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot save pack locally. Aborting.", ex);
				throw new IOException("Cannot save pack.", ex);
			}

			var mutablePack = await Task.Run(() => new MutablePack(pack, id));
			Logger.Info("Pack '{0}'({1}) loaded successfully.", pack.Name.DefaultTranslation, id);
			this._Packs.Add(mutablePack);

			return mutablePack;
		}

		/// <summary>
		/// Removes <paramref name="pack"/> from the repository.
		/// </summary>
		/// <param name="pack"></param>
		public void Remove(MutablePack pack)
		{
			Validate.Debug(() => pack, v => v.NotNull().IsIn(this._Packs));

			Logger.Info("Removing pack {0}.", pack.Id);
			this._Packs.Remove(pack);
			this.Store.RemovePack(pack.Id);
		}

		/// <summary>
		/// Commits changes made to the pack(save pack to the store).
		/// </summary>
		/// <param name="pack"></param>
		public async Task CommitChanges(MutablePack pack)
		{
			Validate.Debug(() => pack, v => v.NotNull().IsIn(this._Packs));

			Logger.Trace("Saving pack {0}.", pack.Id);
			try
			{
				var copy = pack.ToReadOnly();
				await this.Store.SavePack(copy.Id, s => PackLoader.Save(copy, s));
				this.ChangesCommited.Raise(this, pack);
				Logger.Info("Pack {0} saved.", copy.Id);
			}
			catch (Exception ex)
			{
				Logger.Warn("Cannot save pack '{0}'({1}).".FormatWith(pack.Name.DefaultTranslation, pack.Id), ex);
				throw new IOException("Cannot save pack.", ex);
			}
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
				var pack = await Task.Run(() => new MutablePack(PackLoader.Load(s, id)));
				this._Packs.Add(pack);
				Logger.Info("Pack {0} loaded.", pack.Id);
			});
		}

		private sealed class MutablePackObservableList
			: ObservableCollection<MutablePack>, IReadOnlyObservableList<MutablePack>
		{
			public bool Contains(Guid id)
			{
				for (int i = 0; i < this.Items.Count; i++)
				{
					if (this.Items[i].Id == id)
						return true;
				}
				return false;
			}
		}
	}
}
