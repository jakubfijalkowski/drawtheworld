using System;
using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;

namespace DrawTheWorld.Core.GameModes
{
	/// <summary>
	/// Free mode.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item>
	///     User can freely paint on the board without any fine.
	///   </item>
	///   <item>
	///     The board needs to be correctly filled - only then user sees any change.
	///   </item>
	///   <item>
	///     If user tries to fill empty field or field with bad color - we allow it and no fine is charged.
	///   </item>
	/// </list>
	/// </remarks>
	[Priority(1)]
	public class FreeMode
		: IGameMode
	{
		private static readonly ITool[] _AvailableTools = new ITool[]
		{
			new Tools.FreeBrush(),
			new Tools.FreeExcluder(),
			new Tools.Eraser(),
			new Tools.CounterTool(),
			new Tools.EraseCounterTool(),
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
			get { return true; }
		}

		/// <inheritdoc />
		public Tuple<int, IEnumerable<Field>> OnAction(IGame game, ITool tool, object userData, IEnumerable<Field> fields, int fineNumber)
		{
			return null;
		}

		/// <summary>
		/// Not used in this mode.
		/// </summary>
		/// <param name="board"></param>
		/// <param name="tool"></param>
		/// <param name="userData"></param>
		/// <param name="changedFields"></param>
		public void UpdateDescriptions(IBoard board, ITool tool, object userData, IEnumerable<Field> changedFields)
		{ }

		/// <inheritdoc />
		public ITool GetTool(PredefinedTool tool)
		{
			switch (tool)
			{
				case PredefinedTool.Brush:
					return this.AvailableTools[0];

				case PredefinedTool.Excluder:
					return this.AvailableTools[1];

				case PredefinedTool.Eraser:
					return this.AvailableTools[2];
			}
			return null;
		}
	}
}
