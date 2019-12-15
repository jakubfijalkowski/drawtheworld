using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.UserData;
using FLib;

namespace DrawTheWorld.Core.Internals
{
	/// <summary>
	/// Special implementation of <see cref="IBoard"/> for use in <see cref="Designer"/>.
	/// </summary>
	sealed class DesignerBoard
		: IBoard, INotifyPropertyChanged
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.DesignerBoard");

		internal readonly MutableBoardData Data = null;

		private Field[] _Fields = null;
		private IList<Block>[] _Rows = null;
		private IList<Block>[] _Columns = null;

		/// <inheritdoc />
		public IBoardInfoProvider BoardInfo
		{
			get { return this.Data; }
		}

		/// <inheritdoc />
		public Field[] Fields
		{
			get { return this._Fields; }
		}

		/// <inheritdoc />
		public IEnumerable<Block>[] Rows
		{
			get { return this._Rows; }
		}

		/// <inheritdoc />
		public IEnumerable<Block>[] Columns
		{
			get { return this._Columns; }
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		public DesignerBoard(MutableBoardData data)
		{
			Validate.Debug(() => data, v => v.NotNull());
			Logger.Info("Creating designer board.");

			this.Data = data;
			this.Data.PropertyChanged += this.OnSizeUpdated;

			this.Initialize();
		}

		private void OnSizeUpdated(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Data.NameOf(_ => _.Size))
				this.UpdateSize();
		}

		/// <summary>
		/// Should be called by <see cref="Designer"/> to update particular fields.
		/// </summary>
		/// <param name="locations"></param>
		internal void UpdateBoard(IEnumerable<Point> locations)
		{
			Logger.Trace(() => "Updating board data.");

			locations
				.Select(p => p.Y)
				.Distinct()
				.ForEach(i =>
					{
						var row = (LineDescription)this._Rows[i];
						BoardDescriptionHelper.BuildDescriptionForRow(this, row, i, true);
						row.Reset();
					});

			locations
				.Select(p => p.X)
				.Distinct()
				.ForEach(i =>
					{
						var column = (LineDescription)this._Columns[i];
						BoardDescriptionHelper.BuildDescriptionForColumn(this, column, i, true);
						column.Reset();
					});
		}

		/// <summary>
		/// Should be called by <see cref="Designer"/> when user finishes editing board.
		/// </summary>
		internal void FinishGame()
		{
			Logger.Trace("Finishing.");
			this.Data.PropertyChanged -= this.OnSizeUpdated;
		}

		/// <summary>
		/// Initializes board.
		/// </summary>
		private void Initialize()
		{
			this._Fields = new Field[this.BoardInfo.Size.Width * this.BoardInfo.Size.Height];
			Logger.Info("Initializing {0} fields.", this._Fields.Length);
			for (int y = 0, i = 0; y < this.BoardInfo.Size.Height; y++)
			{
				for (int x = 0; x < this.BoardInfo.Size.Width; x++, i++)
				{
					this._Fields[i] = new Field(null, new Point(x, y)) { Fill = this.Data.Data[i] };
				}
			}

			Logger.Trace("Building descriptions.");
			BoardDescriptionHelper.BuildDescriptions(this, out this._Rows, out this._Columns, true, () => new LineDescription());
		}

		/// <summary>
		/// Updates size of the board.
		/// </summary>
		private void UpdateSize()
		{
			// Hacky, but it's clearer than tracking old size inside MutableBoardData
			// I know we can do it locally, but this is easier ;)
			int oldW = this._Columns.Length;
			int oldH = this._Rows.Length;

			Logger.Info("Changing size from {0}x{1} to {2}x{3}.", oldW, oldH, this.BoardInfo.Size.Width, this.BoardInfo.Size.Height);

			var newFields = new Field[this.BoardInfo.Size.Width * this.BoardInfo.Size.Height];
			Logger.Trace("Initializing {0} fields.", newFields.Length);
			for (int x = 0; x < this.BoardInfo.Size.Width; x++)
			{
				for (int y = 0; y < this.BoardInfo.Size.Height; y++)
				{
					int i = x + y * this.BoardInfo.Size.Width;
					if (x < oldW && y < oldH)
						newFields[i] = this._Fields[x + y * oldW]; //Location is the same in the old, and in the new field
					else
						newFields[i] = new Field(null, new Point(x, y));
				}
			}
			this._Fields = newFields;

			Logger.Trace("Rebuilding descriptions.");
			BoardDescriptionHelper.BuildDescriptions(this, out this._Rows, out this._Columns, true, () => new LineDescription());

			this.PropertyChanged.Raise(this, _ => _.Rows);
			this.PropertyChanged.Raise(this, _ => _.Columns);
			this.PropertyChanged.Raise(this, _ => _.Fields);
		}

		private sealed class LineDescription
			: Collection<Block>, INotifyCollectionChanged, INotifyPropertyChanged
		{
			public event NotifyCollectionChangedEventHandler CollectionChanged;
			public event PropertyChangedEventHandler PropertyChanged;

			/// <summary>
			/// Raises <see cref="CollectionChanged"/> with <see cref="NotifyCollectionChangedAction.Reset"/>.
			/// </summary>
			public void Reset()
			{
				this.CollectionChanged.Raise(this, NotifyCollectionChangedAction.Reset);
				this.PropertyChanged.Raise(this, _ => _.Count);
			}
		}
	}
}
