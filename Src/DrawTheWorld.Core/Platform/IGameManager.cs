using System.Threading.Tasks;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Core.UserData;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Manages "current game" and game-related data.
	/// </summary>
	/// <remarks>
	/// Allows to start new game and cleans after it and performs various in-game actions that are not related to other subsystems.
	/// </remarks>
	public interface IGameManager
	{
		/// <summary>
		/// Gets current game.
		/// May be null if not in game.
		/// </summary>
		IGame Game { get; }

		/// <summary>
		/// Gets current game data.
		/// May be null if not in game.
		/// </summary>
		GameData GameData { get; }

		/// <summary>
		/// Gets value that indicates whether app is "in-game" or not.
		/// </summary>
		bool IsInGame { get; }

		/// <summary>
		/// Asks user to select mode and starts new game using <paramref name="board"/>.
		/// </summary>
		/// <param name="board"></param>
		/// <returns>Task returns true, when game was starter, otherwise returns false.</returns>
		Task<bool> SelectModeAndStartGame(BoardData board);

		/// <summary>
		/// Starts designer for specified <paramref name="board"/>.
		/// </summary>
		/// <param name="board"></param>
		/// <returns></returns>
		Task StartDesigner(MutableBoardData board);
	}
}
