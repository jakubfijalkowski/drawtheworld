using System;
using System.Collections.Generic;
using FLib;

namespace DrawTheWorld.Core.UserData
{
	/// <summary>
	/// Observable list of <see cref="BoardStatistics"/>.
	/// </summary>
	public interface IBoardStatisticsList
		: IReadOnlyObservableList<BoardStatistics>
	{ }

	/// <summary>
	/// Statistics from a single game.
	/// </summary>
	public sealed class BoardStatistics
	{
		private readonly Guid _PackId = default(Guid);
		private readonly Guid _BoardId = default(Guid);
		private readonly int _Time = 0;
		private readonly DateTime _Date = DateTime.MinValue;
		private readonly FinishReason _Result = FinishReason.NotFinished;

		/// <summary>
		/// Gets the unique id of the pack.
		/// </summary>
		public Guid PackId
		{
			get { return this._PackId; }
		}

		/// <summary>
		/// Gets the unique id of the board that the user played.
		/// </summary>
		public Guid BoardId
		{
			get { return this._BoardId; }
		}

		/// <summary>
		/// Gets time that the user has needed to solve board.
		/// </summary>
		public int Time
		{
			get { return _Time; }
		}

		/// <summary>
		/// Gets the game date.
		/// </summary>
		public DateTime Date
		{
			get { return _Date; }
		}

		/// <summary>
		/// Gets the result of the game.
		/// </summary>
		public FinishReason Result
		{
			get { return _Result; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="packId"></param>
		/// <param name="boardId"></param>
		/// <param name="time"></param>
		/// <param name="date"></param>
		/// <param name="result"></param>
		public BoardStatistics(Guid packId, Guid boardId, int time, DateTime date, FinishReason result)
		{
			Validate.Debug(() => packId, v => v.NotEqual(Guid.Empty));
			Validate.Debug(() => boardId, v => v.NotEqual(Guid.Empty));
			Validate.Debug(() => time, v => v.Min(1));
			Validate.Debug(() => date, v => v.That(d => d.Kind, v2 => v2.Equal(DateTimeKind.Utc)));
			Validate.Debug(() => result, v => v.NotEqual(FinishReason.NotFinished));

			this._PackId = packId;
			this._BoardId = boardId;

			this._Time = time;
			this._Date = date;
			this._Result = result;
		}
	}

	/// <summary>
	/// Statistics for the whole pack.
	/// </summary>
	public sealed class PackStatistics
	{
		private readonly Guid _Id = Guid.Empty;
		private readonly IReadOnlyDictionary<Guid, IBoardStatisticsList> _Boards = null;

		/// <summary>
		/// Empty statistics.
		/// </summary>
		public static readonly PackStatistics Empty = new PackStatistics(true);

		/// <summary>
		/// Gets the pack id.
		/// </summary>
		public Guid Id
		{
			get { return this._Id; }
		}

		/// <summary>
		/// Gets the statistics for particular boards.
		/// </summary>
		/// <remarks>
		/// Bear in mind, that some boards may not have statistics at all.
		/// </remarks>
		public IReadOnlyDictionary<Guid, IBoardStatisticsList> Boards
		{
			get { return this._Boards; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="boards"></param>
		public PackStatistics(Guid id, IReadOnlyDictionary<Guid, IBoardStatisticsList> boards)
		{
			Validate.Debug(() => id, v => v.NotNull());
			Validate.Debug(() => boards, v => v.NotNull());

			this._Id = id;
			this._Boards = boards;
		}

		private PackStatistics(bool empyt)
		{
			this._Id = Guid.Empty;
			this._Boards = new Dictionary<Guid, IBoardStatisticsList>();
		}
	}
}
