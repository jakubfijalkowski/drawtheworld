using System.Collections.Generic;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.UserData;
using FLib;

namespace DrawTheWorld.Core.Internals
{
	/// <summary>
	/// Internal implementation of static board - used in "real" game.
	/// </summary>
	sealed class StaticBoard
		: IBoard
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.StaticBoard");

		private readonly BoardData Data = null;
		private readonly Field[] _Fields = null;

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

		public StaticBoard(BoardData data)
		{
			Validate.Debug(() => data, v => v.NotNull());

			Logger.Info("Creating static board.");
			this.Data = data;

			this._Fields = new Field[data.Data.Count];
			Logger.Trace("Initializing {0} fields.", this._Fields.Length);
			for (int y = 0, i = 0; y < data.Size.Height; y++)
			{
				for (int x = 0; x < data.Size.Width; x++, i++)
					this._Fields[i] = new Field(data.Data[i], new Point(x, y));
			}

			Logger.Trace("Building descriptions.");
			BoardDescriptionHelper.BuildDescriptions(this, out this._Rows, out this._Columns, false, () => new List<Block>());
		}
	}
}
