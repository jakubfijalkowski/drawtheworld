using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.UserData.Repositories
{
	/// <summary>
	/// Pack is not valid for linkage.
	/// </summary>
	public sealed class InvalidPackException
		: Exception
	{ }

	/// <summary>
	/// Repository with linked packs.
	/// </summary>
	public sealed class LinkedPackRepository
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.LinkedPacksRepository");

		private readonly ILinkedPackStore Store = null;
		private readonly DesignerRepository DesignerRepository = null;

		private readonly PackCollection _Packs = new PackCollection();
		private HashSet<Guid> LinkedPacks = null;

		/// <summary>
		/// Gets the provider of packs for this repository.
		/// </summary>
		public IPackProvider Provider
		{
			get { return this._Packs; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="store"></param>
		/// <param name="repository"></param>
		public LinkedPackRepository(ILinkedPackStore store, DesignerRepository repository)
		{
			this.Store = store;
			this.DesignerRepository = repository;
		}

		/// <summary>
		/// Loads saved packs.
		/// </summary>
		/// <returns></returns>
		public async Task Load()
		{
			var loaded = await Task.Run(() =>
			{
				var ids = new HashSet<Guid>(this.Store.Load());
				var candidates = new HashSet<Guid>();
				List<Pack> packs = new List<Pack>();
				foreach (var id in ids)
				{
					var pack = this.DesignerRepository.Packs.FirstOrDefault(p => p.Id == id);
					if (pack == null || !pack.Boards.Any(BoardHelper.FilterEmpty))
					{
						Logger.Warn("Linked pack with id {0} cannot be found or is not suitable for linkage.", id);
						this.Store.Remove(id);
					}
					else
					{
						packs.Add(pack.ToReadOnly());
						candidates.Add(id);
					}
				}
				return Tuple.Create(candidates, packs);
			});
			this.LinkedPacks = loaded.Item1;
			this._Packs.AddRange(loaded.Item2);
			this.DesignerRepository.ChangesCommited += this.OnChangesCommited;
			this.DesignerRepository.Packs.CollectionChanged += this.OnDesignerPacksChanged;
			Logger.Info("Loaded {0} linked packs.", this._Packs.Count);
		}

		/// <summary>
		/// Adds packs as linked.
		/// </summary>
		/// <param name="packToLink"></param>
		/// <exception cref="InvalidPackException">Thrown when pack is not suitable for linkage.</exception>
		public async Task<Pack> Add(MutablePack packToLink)
		{
			Validate.Debug(() => packToLink, v => v.NotNull().That(CheckIfSuitableForLinking).Nest(p => p.Id).IsNotIn(this.LinkedPacks).IsIn(this.DesignerRepository.Packs.Select(p => p.Id)));

			Logger.Info("Linking pack '{0}'({1}).", packToLink.Name.DefaultTranslation, packToLink.Id);
			var roPack = await Task.Run(() => packToLink.ToReadOnly(BoardHelper.FilterEmpty));

			this.Store.Add(packToLink.Id);
			this.LinkedPacks.Add(packToLink.Id);
			this._Packs.Add(roPack);
			return roPack;
		}

		/// <summary>
		/// Removes linking of the pack.
		/// </summary>
		/// <param name="packId"></param>
		public void Remove(Guid packId)
		{
			Validate.Debug(() => packId, v => v.IsIn(this.LinkedPacks));

			Logger.Info("De-linking pack with id {0}.", packId);
			this.Store.Remove(packId);
			this.LinkedPacks.Remove(packId);
			this._Packs.Remove(packId);
		}

		/// <summary>
		/// Checks if pack with particular <paramref name="id"/> is linked.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool CheckIfLinked(Guid id)
		{
			return this.LinkedPacks.Contains(id);
		}

		/// <summary>
		/// Checks if pack is suitable for linking.
		/// </summary>
		/// <param name="pack"></param>
		/// <returns></returns>
		public bool CheckIfSuitableForLinking(MutablePack pack)
		{
			return pack.Boards.Count(BoardHelper.FilterEmpty) != 0;
		}

		private void OnChangesCommited(DesignerRepository repository, MutablePack pack)
		{
			if (this.CheckIfLinked(pack.Id))
			{
				if (this.CheckIfSuitableForLinking(pack))
				{
					Logger.Info("Linked pack '{0}'({1}) saved. Updating.", pack.Name.DefaultTranslation, pack.Id);
					var roPack = pack.ToReadOnly(BoardHelper.FilterEmpty);
					this._Packs.Replace(roPack);
				}
				else
				{
					Logger.Info("Linked pack '{0}'({1}) is empty. Ignore update.", pack.Name.DefaultTranslation, pack.Id);
				}
			}
		}

		private void OnDesignerPacksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				foreach (var id in e.OldItems.Cast<MutablePack>().Select(p => p.Id).Intersect(this.LinkedPacks))
				{
					Logger.Info("Pack with id {0} removed from designer. Removing linkage");
					this.Remove(id);
				}
			}
		}

		private sealed class PackCollection
			: ObservableCollection<Pack>, IPackProvider
		{
			public PackType PackType
			{
				get { return PackType.Linked; }
			}

			public void AddRange(IEnumerable<Pack> collection)
			{
				int oldCount = this.Count;
				collection.ForEach(base.Items.Add);
				this.OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, collection.ToList(), oldCount));
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
				if (idx > -1)
				{
					this.RemoveAt(idx);
					return true;
				}
				return false;
			}

			/// <remarks>
			/// Assumes that the pack exists in the collection.
			/// </remarks>
			public void Replace(Pack pack)
			{
				var idx = this.IndexOf(pack.Id);
				this.SetItem(idx, pack);
			}
		}
	}
}
