using System;
using System.Linq;
using FLib;
using DrawTheWorld.Core.UserData;
using DrawTheWorld.Core.Helpers;
using System.Collections.Generic;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Main game class implementation.
	/// </summary>
	public sealed class Game
		: IGame
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.Game");

		private readonly IGameMode _Mode = null;
		private readonly Internals.StaticBoard _Board = null;
		private readonly Color[] _Palette = null;

		private DateTime _StartDate = DateTime.MinValue;
		private DateTime _FinishDate = DateTime.MaxValue;
		private FinishReason _FinishReason = FinishReason.NotFinished;

		private int FineNo = 0;

		/// <inheritdoc />
		public IBoard Board
		{
			get { return this._Board; }
		}

		/// <inheritdoc />
		public IGameMode Mode
		{
			get { return this._Mode; }
		}

		/// <inheritdoc />
		public Color[] Palette
		{
			get { return this._Palette; }
		}

		/// <inheritdoc />
		public DateTime StartDate
		{
			get { return this._StartDate; }
		}

		/// <inheritdoc />
		public DateTime FinishDate
		{
			get { return this._FinishDate; }
		}

		/// <inhertidoc />
		public FinishReason FinishReason
		{
			get { return this._FinishReason; }
		}

		/// <inheritdoc />
		public int Fine { get; private set; }

		/// <inhertidoc />
		public event GameFinishedHandler GameFinished;

		/// <inheritdoc />
		public void Start()
		{
			Validate.Debug(() => this.StartDate, v => v.Equal(DateTime.MinValue));
			Logger.Info("Game started.");
			this._StartDate = DateTime.UtcNow;
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="mode"></param>
		public Game(BoardData board, IGameMode mode)
		{
			Validate.Debug(() => board, v => v.NotNull().That(b => b.Size.Width > 0 && b.Size.Height > 0 && b.Data.Count == b.Size.Width * b.Size.Height, "Board data or size is invalid."));
			Validate.Debug(() => mode, v => v.NotNull());
			Logger.Info("Creating game for '{0}'(id: {1}) from pack {2}.", board.Name.DefaultTranslation, board.Id, board.PackId);
			Logger.Trace("Size: {0}x{1}", board.Size.Width, board.Size.Height);
			Logger.Trace("Mode: " + mode.GetType().Name);

			this._Mode = mode;
			this._Board = new Internals.StaticBoard(board);
			this._Palette = board.Data
				.Where(c => c.HasValue)
				.Distinct()
				.Select(c => c.Value)
				.OrderByDescending(c => board.Data.Count(cb => c == cb))
				.ToArray();

			Logger.Trace(() => "Palette: " + string.Join(", ", this._Palette.Select(c => c.ToString())));
		}

		/// <inheritdoc />
		public Tuple<int, IEnumerable<Point>> PerformAction(ITool tool, object userData, IEnumerable<Point> locations)
		{
			Validate.Debug(() => tool, v => v.NotNull());
			Validate.Debug(() => this.StartDate, v => v.NotEqual(DateTime.MinValue));
			Validate.Debug(() => this.FinishDate, v => v.Equal(DateTime.MaxValue));

			var fields = tool.IsSingleAction ? this.Board.Fields : this.Board.GetFields(locations).ToArray();
			Logger.Trace(() => "Performing action with tool {0} on {1} fields.".FormatWith(tool.GetType().Name, fields.Length));

			var action = this.Mode.OnAction(this, tool, userData, fields, this.FineNo);

			Logger.Trace(() => "Performing action on {0} fields.".FormatWith(fields.Length));
			tool.Perform(this, userData, fields);
			this.Mode.UpdateDescriptions(this.Board, tool, userData, fields);

			if (action != null)
			{
				Logger.Trace("Charging fine of {0}.", action.Item1);

				this.FineNo++;
				this.Fine += action.Item1;
			}

			return action != null ? Tuple.Create(action.Item1, action.Item2.Select(f => f.Location)) : null;
		}

		/// <inhertidoc />
		public void Finish()
		{
			Validate.Debug(() => this.StartDate, v => v.NotEqual(DateTime.MinValue));
			Validate.Debug(() => this.FinishDate, v => v.Equal(DateTime.MaxValue));

			this.FinishGame(FinishReason.User);
		}

		/// <inheritdoc />
		public void CheckIfFinished()
		{
            // Changed: The play is more fun then
			//if (this.Fine > this.Mode.MaximumFine)
			//	this.FinishGame(FinishReason.TooMuchFine);

			if (this.Board.Fields.All(f => f.Fill == f.CorrectFill))
				this.FinishGame(FinishReason.Correct);
		}

		private void FinishGame(FinishReason reason)
		{
			this._FinishDate = DateTime.UtcNow;
			this._FinishReason = reason;
			Logger.Info("Game finished. Reason: {0}.".FormatWith(reason.ToString()));
			this.GameFinished.Raise(this, reason);
		}
	}
}
