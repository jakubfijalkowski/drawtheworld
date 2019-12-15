using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FLib;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.UserData;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Special-case game - designer.
	/// </summary>
	public sealed class Designer
		: IGame, INotifyPropertyChanged
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.Designer");

		private readonly GameModes.DesignerMode _Mode = new GameModes.DesignerMode();

		private Internals.DesignerBoard _Board = null;
		private Color[] _Palette = Config.DefaultPalette;
		private FinishReason _FinishReason = FinishReason.NotFinished;

		/// <inheritdoc />
		public IGameMode Mode
		{
			get { return this._Mode; }
		}

		/// <inheritdoc />
		public IBoard Board
		{
			get { return this._Board; }
		}

		/// <summary>
		/// Pallete of colors used in board.
		/// Can be changed at runtime.
		/// </summary>
		public Color[] Palette
		{
			get { return this._Palette; }
			set
			{
				this._Palette = value != null && value.Length > 0 && value.Length <= Config.MaxPaletteSize ? value : Config.DefaultPalette;
				Logger.Trace(() => "Palette changed to: " + string.Join(", ", this._Palette.Select(c => c.ToString())) + ".");
				this.PropertyChanged.Raise(this);
			}
		}

		/// <inheritdoc />
		public DateTime StartDate
		{
			get { return DateTime.MinValue; }
		}

		/// <inheritdoc />
		public DateTime FinishDate
		{
			get { return DateTime.MaxValue; }
		}

		/// <inheritdoc />
		public FinishReason FinishReason
		{
			get { return this._FinishReason; }
		}

		/// <inheritdoc />
		public int Fine
		{
			get { return 0; }
		}

		/// <inhertidoc />
		public event GameFinishedHandler GameFinished;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes designer with specified board.
		/// </summary>
		/// <param name="description"></param>
		public Designer(MutableBoardData data)
		{
			Validate.Debug(() => data, v => v.NotNull());
			Logger.Info("Creating designer for board '{0}'(id: {1}) from pack: {2}.", data.Name.DefaultTranslation, data.Id, data.PackId);
			Logger.Info("Size: {0}x{1}", data.Size.Width, data.Size.Height);

			this._Board = new Internals.DesignerBoard(data);

			var palette = data.Data
				.Distinct()
				.Where(c => c.HasValue)
				.Select(c => c.Value);

			this._Palette = palette.Any() ? palette.ToArray() : Config.DefaultPalette;
			if (this._Palette == Config.DefaultPalette)
				Logger.Trace("Default palette used.");
			else
				Logger.Trace("Palette: " + string.Join(", ", this._Palette.Select(c => c.ToString())));
		}

		/// <inheritdoc />
		public void Start()
		{ }

		/// <inheritdoc />
		public Tuple<int, IEnumerable<Point>> PerformAction(ITool tool, object userData, IEnumerable<Point> locations)
		{
			Validate.Debug(() => tool, v => v.NotNull());
			Validate.Debug(() => this.Board, v => v.NotNull());

			var fields = tool.IsSingleAction ? this.Board.Fields : this.Board.GetFields(locations).ToArray();
			tool.Perform(this, userData, fields);
			this._Board.UpdateBoard(locations);
			return null;
		}

		/// <inhertidoc />
		public void Finish()
		{
			Logger.Info("Finishing designer.");
			this._FinishReason = FinishReason.Correct;
			this.GameFinished.Raise(this, FinishReason.Correct);
			this._Board.FinishGame();
		}

		/// <inheritdoc />
		public void CheckIfFinished()
		{ }

		/// <summary>
		/// Saves changes made in board to <see cref="BoardDescription"/>.
		/// </summary>
		public void SaveChanges()
		{
			Logger.Info("Saving changes.");
			this._Board.Data.Data = this._Board.Fields
					.Select(f => f.Fill)
					.ToArray();
			
		}
	}
}
