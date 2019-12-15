using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using FLib;

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Combines game data and statistics into data that can be consumed by UI.
	/// </summary>
	public sealed class GameDataManager
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.GameDataManager");
		private readonly UIGamePackCollection _Packs = new UIGamePackCollection();

		private readonly UserStatistics Statistics = null;
		private readonly IThumbnailStore<GameBoard> Thumbnails = null;
		private readonly IPackProvider[] Providers = null;

		/// <summary>
		/// Gets the list of available packs.
		/// </summary>
		public IReadOnlyObservableList<GamePack> Packs
		{
			get { return this._Packs; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="stats"></param>
		/// <param name="thumbnails"></param>
		public GameDataManager(UserStatistics stats, IThumbnailStore<GameBoard> thumbnails, params IPackProvider[] providers)
		{
			Validate.Debug(() => stats, v => v.NotNull());
			Validate.Debug(() => thumbnails, v => v.NotNull());
			Validate.Debug(() => providers, v => v.NotNullAndNotEmpty());

			this.Statistics = stats;
			this.Thumbnails = thumbnails;
			this.Providers = providers;
		}

		/// <summary>
		/// Loads all packs from <see cref="CustomPackRepository"/> to the manager.
		/// </summary>
		public void Load()
		{
			foreach (var provider in this.Providers)
			{
				foreach (var p in provider)
				{
					var s = this.Statistics.Sync(p);
					var gameData = new GamePack(p, provider.PackType, s);
					this.GenerateThumbnails(gameData);
					this._Packs.Add(gameData);
				}

				provider.CollectionChanged += this.OnPackCollectionChanged;
			}
		}

		private void OnPackCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var provider = (IPackProvider)sender;
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					for (int i = 0; i < e.NewItems.Count; i++)
					{
						var pack = (Pack)e.NewItems[i];
						var stats = this.Statistics.Sync(pack);
						var gameData = new GamePack(pack, provider.PackType, stats);
						this.GenerateThumbnails(gameData);
						this._Packs.Add(gameData);
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						var idx = this._Packs.IndexOf((Pack)e.OldItems[i]);

						this.CleanThumbnails(this._Packs[idx]);
						this._Packs.RemoveAt(idx);
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					for (int i = 0; i < e.OldItems.Count; i++)
					{
						var oldIdx = this._Packs.IndexOf((Pack)e.OldItems[i]);

						var newPack = (Pack)e.NewItems[i];
						var stats = this.Statistics.Sync(newPack);
						var gameData = new GamePack(newPack, provider.PackType, stats);

						this._Packs.RemoveAt(oldIdx);
						this._Packs.Add(gameData);
						this.ForceGenerateThumbnails(gameData);
					}
					break;

				default:
					throw new NotSupportedException("Only add, remove and replace operations are supported.");
			}
		}

		private async void GenerateThumbnails(GamePack pack)
		{
			Logger.Debug("Generating thumbnails for pack '{0}'({1}).", pack.Pack.Name.DefaultTranslation, pack.Pack.Id);

			for (int i = 0; i < pack.Boards.Count; i++)
			{
				pack.Boards[i].Thumbnail = (await this.Thumbnails.GetThumbnail(pack.Boards[i])) ?? (await this.Thumbnails.RegenerateThumbnail(pack.Boards[i]));
			}
		}

		private async void ForceGenerateThumbnails(GamePack pack)
		{
			Logger.Debug("Forcibly generating thumbnails for pack '{0}'({1}).", pack.Pack.Name.DefaultTranslation, pack.Pack.Id);

			for (int i = 0; i < pack.Boards.Count; i++)
			{
				pack.Boards[i].Thumbnail = await this.Thumbnails.RegenerateThumbnail(pack.Boards[i]);
			}
		}

		private void CleanThumbnails(GamePack pack)
		{
			Logger.Debug("Cleaning thumbnails for pack '{0}'({1}).", pack.Pack.Name.DefaultTranslation, pack.Pack.Id);
			Task.Run(() => pack.Boards.ForEach(this.Thumbnails.Clean));
		}

		private class UIGamePackCollection
			: ObservableCollection<GamePack>, IReadOnlyObservableList<GamePack>
		{
			public int IndexOf(Pack pack)
			{
				for (int i = 0; i < this.Count; i++)
				{
					if (base[i].Pack == pack)
						return i;
				}
				return -1;
			}

			protected override void InsertItem(int index, GamePack item)
			{
				base.InsertItem(this.FindLocation(item), item);
			}

			private int FindLocation(GamePack pack)
			{
				//Binary search
				int min = 0, max = this.Count;
				while (min < max)
				{
					int mid = (min + max) / 2;

					if (ComparePacks(this.Items[mid], pack) < 0)
						min = mid + 1;
					else
						max = mid;
				}

				return min;
			}

			private static int ComparePacks(GamePack a, GamePack b)
			{
				int ret = a.Pack.Name.MainTranslation.CompareTo(b.Pack.Name.MainTranslation);
				if (ret == 0)
					ret = a.Pack.Id.CompareTo(b.Pack.Id);
				return ret;
			}
		}
	}
}
