using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.UserData
{
	using PackDictionary = Dictionary<Guid, IBoardStatisticsList>;

	/// <summary>
	/// User statistics.
	/// </summary>
	public sealed class UserStatistics
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.Statistics");
		private readonly Dictionary<Guid, PackStatistics> Statistics = new Dictionary<Guid, PackStatistics>();
		private readonly IStatisticsStore Store = null;

		/// <summary>
		/// Gets the statistics for particular pack.
		/// </summary>
		/// <param name="packId"></param>
		/// <returns>Statistics for particular pack -or- null, if statistics are not found.</returns>
		public PackStatistics this[Guid packId]
		{
			get
			{
				Validate.Debug(() => packId, v => v.NotEqual(Guid.Empty));
				PackStatistics stats = null;
				this.Statistics.TryGetValue(packId, out stats);
				return stats;
			}
		}

		/// <summary>
		/// Raised when new statistics gets added.
		/// </summary>
		public event Action<PackStatistics, BoardStatistics> StatsAdded;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="store"></param>
		public UserStatistics(IStatisticsStore store)
		{
			Validate.Debug(() => store, v => v.NotNull());

			this.Store = store;
			this.Store.RoamingStatisticsAdded = b => this.AddStatisticsInternal(b);
		}

		/// <summary>
		/// Adds particular statistics.
		/// </summary>
		/// <param name="statistics"></param>
		/// <returns></returns>
		public async void AddStatistics(BoardStatistics statistics)
		{
			Validate.Debug(() => statistics, v => v.NotNull());
			this.AddStatisticsInternal(statistics);
			await this.Store.Save(statistics);
		}

		/// <summary>
		/// Loads all stats from the store.
		/// </summary>
		/// <returns></returns>
		public async Task Load()
		{
			this.Statistics.Clear();

			int total = 0;
			await this.Store.Load((s) =>
			{
				this.GetStats(this.GetStats(s.PackId), s.BoardId).Add(s);
				++total;
			});
			Logger.Info("Stats for {0} packs loaded({1} stats in total).", this.Statistics.Count, total);
		}

		/// <summary>
		/// Synces statistics of the pack and the pack(adds missing boards to the stats).
		/// </summary>
		/// <param name="pack"></param>
		/// <returns></returns>
		public PackStatistics Sync(Pack pack)
		{
			Validate.Debug(() => pack, v => v.NotNull());

			Logger.Info("Syncing stats for pack '{0}'({1}).", pack.Name.DefaultTranslation, pack.Id);
			var stats = this.GetStats(pack.Id);
			foreach (var b in pack.Boards)
				this.GetStats(stats, b.Id); //GetStats handles adding empty list to the PackStatistics
			return stats;
		}

		private PackStatistics GetStats(Guid id)
		{
			PackStatistics pack = null;
			if (!this.Statistics.TryGetValue(id, out pack))
				this.Statistics.Add(id, pack = new PackStatistics(id, new PackDictionary()));
			return pack;
		}

		private BoardStatisticsList GetStats(PackStatistics pack, Guid boardId)
		{
			IBoardStatisticsList stats = null;
			if (!pack.Boards.TryGetValue(boardId, out stats))
				((PackDictionary)pack.Boards).Add(boardId, stats = new BoardStatisticsList());

			return (BoardStatisticsList)stats;
		}

		private void AddStatisticsInternal(BoardStatistics statistics)
		{
			var pack = this.GetStats(statistics.PackId);

			Logger.Info("Adding statistics for board {0}.", statistics.BoardId);
			Logger.Trace("Result: {0}, date: {1}, time: {2}.", statistics.Result, statistics.Date, statistics.Time);
			this.GetStats(pack, statistics.BoardId).Add(statistics);
			this.StatsAdded.Raise(pack, statistics);
		}

		private class BoardStatisticsList
			: ObservableCollection<BoardStatistics>, IBoardStatisticsList
		{ }
	}
}
