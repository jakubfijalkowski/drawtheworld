using System.Collections.Generic;

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Defines behavior for selecting fields.
	/// </summary>
	public interface IToolBehavior
	{
		/// <summary>
		/// Gets temporary list of fields that should be altered by tool.
		/// </summary>
		IReadOnlyList<Point> Fields { get; }

		/// <summary>
		/// Called when user starts selecting fields.
		/// </summary>
		/// <param name="first"></param>
		void Start(Point first);

		/// <summary>
		/// Called when user moves over next field.
		/// </summary>
		/// <remarks>
		/// One field can be "selected"(<see cref="Over"/> called) multiple times.
		/// </remarks>
		/// <param name="field"></param>
		void Over(Point field);

		/// <summary>
		/// Selection is over.
		/// </summary>
		/// <remarks>
		/// Should clear <see cref="Fields"/> list.
		/// </remarks>
		void Finish();
	}
}
