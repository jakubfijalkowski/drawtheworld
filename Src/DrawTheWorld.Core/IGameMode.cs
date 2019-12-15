using System;
using System.Collections.Generic;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// List of predefined tools that should be supported by each <see cref="IGameMode"/>.
	/// </summary>
	public enum PredefinedTool
	{
		/// <summary>
		/// Default brush.
		/// </summary>
		Brush,

		/// <summary>
		/// Default excluder.
		/// </summary>
		Excluder,

		/// <summary>
		/// Default eraser
		/// </summary>
		Eraser
	}

	/// <summary>
	/// Game mode.
	/// </summary>
	/// <remarks>
	/// Game mode is used to tell which tool may be used and if the usage is correct. It does not filter invalid
	/// fields(as it was in V1), but allows tool to perform the same action on every field. 
	/// </remarks>
	public interface IGameMode
	{
		/// <summary>
		/// Tools that are available in this mode.
		/// </summary>
		ITool[] AvailableTools { get; }

		/// <summary>
		/// Maximum fine that user can have before game ends.
		/// </summary>
		int MaximumFine { get; }

		/// <summary>
		/// If true, user can update the <see cref="Block.IsFinished"/>.
		/// </summary>
		bool IsManualBlockUpdateEnabled { get; }

		/// <summary>
		/// Tests, if the action is valid or not.
		/// Returns value that indicates how much fine we should apply(1st item) and which fields have taken part in the calculation.
		/// </summary>
		/// <param name="game">The game which the tool needs to perform action on.</param>
		/// <param name="tool">The tool used to the action.</param>
		/// <param name="userData">The data user has assigned to the tool.</param>
		/// <param name="fields">The fields the user wants to perform the action on.</param>
		/// <param name="fineNumber">The number of fines that the user already has.</param>
		/// <returns></returns>
		Tuple<int, IEnumerable<Field>> OnAction(IGame game, ITool tool, object userData, IEnumerable<Field> fields, int fineNumber);

		/// <summary>
		/// Updates descriptions of the board after action.
		/// </summary>
		/// <param name="game"></param>
		/// <param name="board"></param>
		/// <param name="tool"></param>
		/// <param name="userData"></param>
		/// <param name="changedFields"></param>
		void UpdateDescriptions(IBoard board, ITool tool, object userData, IEnumerable<Field> changedFields);

		/// <summary>
		/// Gets one of the predefined tools from <see cref="AvailableTools"/>.
		/// </summary>
		/// <param name="tool">Tool.</param>
		/// <returns></returns>
		ITool GetTool(PredefinedTool tool);
	}
}
