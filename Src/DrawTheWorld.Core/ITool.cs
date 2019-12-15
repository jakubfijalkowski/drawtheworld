using System.Collections.Generic;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Tool performs actions over <see cref="IGame"/> and <see cref="IBoard.Fields"/>.
	/// It is the only thing that is allowed to do this.
	/// </summary>
	public interface ITool
	{
		/// <summary>
		/// Gets the value that indicates whether the tool should be used normally or like "fire and forget".
		/// </summary>
		bool IsSingleAction { get; }

		/// <summary>
		/// Performs action on <see cref="IGame"/>.
		/// </summary>
		/// <remarks>
		/// Tool is allowed to perform action only on specified fields.
		/// </remarks>
		/// <param name="on">Game to be affected.</param>
		/// <param name="userData">Data passed by user.</param>
		/// <param name="fields">Fields that can be affected.</param>
		void Perform(IGame on, object userData, IEnumerable<Field> fields);
	}
}
