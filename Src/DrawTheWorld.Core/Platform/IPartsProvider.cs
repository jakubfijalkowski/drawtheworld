using System;
using System.Collections.Generic;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Provides parts of the game.
	/// </summary>
	public interface IPartsProvider
	{
		/// <summary>
		/// Gets the list of available game modes.
		/// </summary>
		IReadOnlyList<IGameMode> AvailableGameModes { get; }

		/// <summary>
		/// Gets the <see cref="IToolBehavior"/> for specified <paramref name="tool"/>.
		/// </summary>
		/// <param name="tool"></param>
		/// <returns></returns>
		IToolBehavior GetBehavior(ITool tool);

		/// <summary>
		/// Gets the translated name(<see cref="ToolData.Name"/>) for specified <paramref name="tool"/>.
		/// </summary>
		/// <param name="tool"></param>
		/// <returns></returns>
		string GetName(ITool tool);

		/// <summary>
		/// Gets the look address(<see cref="ToolData.Look"/>) for specified <paramref name="tool"/>.
		/// </summary>
		/// <param name="tool"></param>
		/// <returns></returns>
		Uri GetLook(ITool tool);
	}
}
