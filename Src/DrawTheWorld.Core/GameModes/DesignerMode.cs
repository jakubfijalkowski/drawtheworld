using System;
using System.Collections.Generic;

namespace DrawTheWorld.Core.GameModes
{
	/// <summary>
	/// Special mode that is intended to be used inside board designer.
	/// </summary>
	public class DesignerMode
		: IGameMode
	{
		private static readonly ITool[] _AvailableTools = new ITool[]
		{
			new Tools.DesignerBrush(),
			new Tools.DesignerExcluder(),
			new Tools.Eraser(),
			new Tools.PanTool()
		};

		/// <inheritdoc />
		public ITool[] AvailableTools
		{
			get { return _AvailableTools; }
		}

		/// <inheritdoc />
		public int MaximumFine
		{
			get { return 0; }
		}

		/// <inheritdoc />
		public bool IsManualBlockUpdateEnabled
		{
			get { return false; }
		}

		/// <inheritdoc />
		public Tuple<int, IEnumerable<Field>> OnAction(IGame game, ITool tool, object userData, IEnumerable<Field> fields, int fineNumber)
		{
			return null;
		}

		/// <inheritdoc />
		/// <summary>
		/// Updating <see cref="IBoard.Rows"/> and <see cref="IBoard.Columns"/> is handled by the <see cref="Designer"/>.
		/// </summary>
		public void UpdateDescriptions(IBoard board, ITool tool, object userData, IEnumerable<Field> changedFields)
		{ }

		/// <inheritdoc />
		public ITool GetTool(PredefinedTool tool)
		{
			switch (tool)
			{
				case PredefinedTool.Brush:
					return _AvailableTools[0];

				case PredefinedTool.Excluder:
					return _AvailableTools[1];

				case PredefinedTool.Eraser:
					return _AvailableTools[2];
			}
			return null;
		}
	}
}
