using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.UserData;
using FLib;

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Combines <see cref="Pack"/> and <see cref="PackStatistics"/> into one class.
	/// </summary>
	public sealed class GamePack
	{
		private readonly Pack _Pack = null;
		private readonly PackType _Type = PackType.Demo;
		private readonly PackStatistics _Statistics = null;
		private readonly List<GameBoard> _Boards = null;

		/// <summary>
		/// Gets the pack.
		/// </summary>
		public Pack Pack
		{
			get { return this._Pack; }
		}

		/// <summary>
		/// Gets the type of the pack.
		/// </summary>
		public PackType Type
		{
			get { return this._Type; }
		}

		/// <summary>
		/// Gets the statistics of the pack.
		/// </summary>
		public PackStatistics Statistics
		{
			get { return this._Statistics; }
		}

		/// <summary>
		/// Gets the statistics of the particular board.
		/// </summary>
		public IReadOnlyList<GameBoard> Boards
		{
			get { return this._Boards; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="pack"></param>
		/// <param name="type"></param>
		/// <param name="statistics"></param>
		public GamePack(Pack pack, PackType type, PackStatistics statistics)
		{
			Validate.Debug(() => pack, v => v.NotNull());
			Validate.Debug(() => statistics, v => v.NotNull().Nest(s => s.Boards).Nest(s => s.Keys).That(c => pack.Boards.Select(b => b.Id).Except(c).Count() == 0, "Statistics must contain keys for all boards."));

			this._Pack = pack;
			this._Type = type;
			this._Statistics = statistics;

			this._Boards = new List<GameBoard>(this._Pack.Boards.Count);
			for (int i = 0; i < this._Pack.Boards.Count; i++)
				this._Boards.Add(new GameBoard(this._Pack.Boards[i], this._Statistics.Boards[this._Pack.Boards[i].Id]));
		}
	}
}
