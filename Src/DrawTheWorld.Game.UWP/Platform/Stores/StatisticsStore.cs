using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Game.Helpers;
using FLib;
using Windows.Storage;
using Windows.UI.Core;

namespace DrawTheWorld.Game.Platform.Stores
{
	/// <summary>
	/// Implements <see cref="IStatisticsStore"/> that saves statistics in user data.
	/// </summary>
	/// <remarks>
	/// It is not compatibile with v1.
	/// </remarks>
	sealed class StatisticsStore
		: IStatisticsStore
	{
		private const string StatsContainer = "Statistics";

		internal static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.StatisticsStore");

		private CoreDispatcher Dispatcher = null;
		private ulong CurrentId = 0;

		private ThreadLocal<ApplicationDataContainer> Container = new ThreadLocal<ApplicationDataContainer>(() =>
			ApplicationData.Current.RoamingSettings.CreateContainer(StatsContainer, ApplicationDataCreateDisposition.Always), false);

		/// <inheritdoc />
		public Action<BoardStatistics> RoamingStatisticsAdded { get; set; }

		/// <inheritdoc />
		public Task Save(BoardStatistics board)
		{
			Logger.Debug("Adding statistics for board {0}.", board.BoardId);

			var stats = board.Dump();
			this.Container.Value.Values.Add((++this.CurrentId).ToString(), stats);
			return Task.FromResult<object>(null);
		}

		/// <inheritdoc />
		public Task Load(Action<BoardStatistics> loadCallback)
		{
			this.Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
			Logger.Info("Loading statistics.");
			return Task.Run(() =>
			{
				var list = this.Container.Value.Values.ToStatisticsList();
				foreach (var entry in list)
					loadCallback(entry.Item2);
				if (list.Count > 0)
					this.CurrentId = list.Last().Item1;
				ApplicationData.Current.DataChanged += OnRoamingDataSynced;
			});
		}

		private async void OnRoamingDataSynced(ApplicationData sender, object args)
		{
			if (this.RoamingStatisticsAdded == null)
				return;

			Logger.Info("Syncing roaming statistics.");
			await Task.Run(() =>
			{
				var list = this.Container.Value.Values.ToStatisticsList();
				foreach (var entry in list.SkipWhile(e => e.Item1 <= this.CurrentId))
				{
					var ignored = this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => this.RoamingStatisticsAdded(entry.Item2));
				}
				if (list.Count > 0)
					this.CurrentId = list.Last().Item1;
			});
		}
	}

	static class StatisticsStoreExtension
	{
		public static List<Tuple<ulong, BoardStatistics>> ToStatisticsList(this Windows.Foundation.Collections.IPropertySet set)
		{
			return set.Select(k => DecodeEntry(k)).Where(k => k != null).OrderBy(k => k.Item1).ToList();
		}

		private static Tuple<ulong, BoardStatistics> DecodeEntry(KeyValuePair<string, object> entry)
		{
			try
			{
				ulong id = ulong.Parse(entry.Key);
				var stats = (ApplicationDataCompositeValue)entry.Value;
				return Tuple.Create(id, stats.RestoreStatistics());
			}
			catch (Exception ex)
			{
				StatisticsStore.Logger.Error("Cannot decode entry with key {0}. Ignoring.".FormatWith(entry.Key), ex);
			}
			return null;
		}
	}
}
