using System;
using System.Collections.Generic;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Reason why game has been finished.
	/// </summary>
	public enum FinishReason
		: byte
	{
		/// <summary>
		/// Board has been correctly filled.
		/// </summary>
		Correct,

		/// <summary>
		/// Fine has exceeded upper limit.
		/// </summary>
		TooMuchFine,

		/// <summary>
		/// User wanted to end game.
		/// </summary>
		User,

		/// <summary>
		/// Game is not finished yet.
		/// </summary>
		NotFinished
	}

	/// <summary>
	/// Handler for	<see cref="IGame.GameFinished"/>.
	/// </summary>
	/// <param name="game"></param>
	/// <param name="reason"></param>
	public delegate void GameFinishedHandler(IGame game, FinishReason reason);

	/// <summary>
	/// Game.
	/// </summary>
	public interface IGame
	{
		/// <summary>
		/// Current mode.
		/// </summary>
		IGameMode Mode { get; }

		/// <summary>
		/// Current board.
		/// </summary>
		IBoard Board { get; }

		/// <summary>
		/// Pallete of colors used in board.
		/// </summary>
		Color[] Palette { get; }

		/// <summary>
		/// Gets the start date of the game.
		/// </summary>
		DateTime StartDate { get; }

		/// <summary>
		/// Gets the finisha date of the game.
		/// </summary>
		/// <remarks>
		/// May throw expcetion when used during the game.
		/// </remarks>
		DateTime FinishDate { get; }

		/// <summary>
		/// Finish reason.
		/// </summary>
		/// <remarks>
		/// May throw expcetion when used during the game.
		/// </remarks>
		FinishReason FinishReason { get; }

		/// <summary>
		/// Fine that the user has already
		/// </summary>
		int Fine { get; }

		/// <summary>
		/// Raised when game finishes.
		/// </summary>
		event GameFinishedHandler GameFinished;

		/// <summary>
		/// Starts game.
		/// </summary>
		void Start();

		/// <summary>
		/// Performs action on this game(current board).
		/// </summary>
		/// <param name="tool">Tool.</param>
		/// <param name="userData">User data passed to <see cref="ITool.Perform"/>.</param>
		/// <param name="locations">Locations of the fields that should be affected.</param>
		/// <returns>Returns information about fine -or- null, if fine was not charged.</returns>
		Tuple<int, IEnumerable<Point>> PerformAction(ITool tool, object userData, IEnumerable<Point> locations);

		/// <summary>
		/// Finishes game because user wants this.
		/// </summary>
		void Finish();

		/// <summary>
		/// Check if the game is finished(doesn't matter if correctly or not), and if it is, fire event.
		/// </summary>
		void CheckIfFinished();
	}
}
