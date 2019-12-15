using System;
using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.Attributes;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core.GameModes
{
	/// <summary>
	/// Supervised mode.
	/// </summary>
	/// <remarks>
	/// * If field is filled correctly, it cannot be changed(without fine)
	/// * If user tries to fill empty field or tries to fill field with wrong color, fine will be 1 for first field and other fields will be ignored
	/// * All tools are available
	/// </remarks>
	[Priority(0)]
	public class SupervisedMode
		: IGameMode
	{
		private static readonly ITool[] _AvailableTools = new ITool[]
		{
			new Tools.SupervisedBrush(),
			new Tools.SupervisedExcluder(),
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
			get { return Config.MaxFineForSupervisedMode; }
		}

		/// <inheritdoc />
		public bool IsManualBlockUpdateEnabled
		{
			get { return false; }
		}

		/// <inheritdoc />
		public Tuple<int, IEnumerable<Field>> OnAction(IGame game, ITool tool, object userData, IEnumerable<Field> fields, int fineNumber)
		{
			if (tool == this.GetTool(PredefinedTool.Brush))
			{
				var color = (Color)userData;
				var invalidField = fields.Where(f => f.Fill == null && !f.Excluded && f.CorrectFill != color).FirstOrDefault();
				if (invalidField != null)
					return Tuple.Create(1 * 1 << fineNumber, Enumerable.Repeat(invalidField, 1));
			}
			return null;
		}

		/// <inheritdoc />
		public void UpdateDescriptions(IBoard board, ITool tool, object userData, IEnumerable<Field> changedFields)
		{
			foreach (var line in changedFields.Select(f => board.Columns[f.Location.X]).Concat(changedFields.Select(f => board.Rows[f.Location.Y])))
			{
				foreach (var block in line)
				{
					if (!block.IsFinished)
						block.IsFinished = block.AssignedFields.All(b => b.Fill == b.CorrectFill);
				}
			}
		}

		/// <inheritdoc />
		public ITool GetTool(PredefinedTool tool)
		{
			switch (tool)
			{
				case PredefinedTool.Brush:
					return _AvailableTools[0];

				case PredefinedTool.Excluder:
					return _AvailableTools[1];
			}
			return null;
		}
	}
}
