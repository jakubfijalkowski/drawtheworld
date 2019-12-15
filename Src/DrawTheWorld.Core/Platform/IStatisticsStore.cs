using System;
using System.Threading.Tasks;
using DrawTheWorld.Core.UserData;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Interface that provides access to the persistent statistics store.
	/// </summary>
	public interface IStatisticsStore
	{
		/// <summary>
		/// Gets or sets the callback that should be called when roaming statistics are synchronized.
		/// </summary>
		Action<BoardStatistics> RoamingStatisticsAdded { get; set; }

		/// <summary>
		/// Saves single pack statistics to the store.
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		Task Save(BoardStatistics board);

		/// <summary>
		/// Loads all saved statistics from the store.
		/// </summary>
		/// <param name="loadCallback">Method that should be called to load single stats.</param>
		/// <returns></returns>
		Task Load(Action<BoardStatistics> loadCallback);
	}
}
