using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Excluder for <see cref="Modes.FreeMode"/>.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("Excluder")]
	public class FreeExcluder
		: ITool
	{
		/// <inheritdoc />
		public bool IsSingleAction
		{
			get { return false; }
		}

		/// <inheritdoc />
		public void Perform(IGame on, object userData, IEnumerable<Field> fields)
		{
			bool action = !fields.First().Excluded;
			foreach (var field in fields)
			{
				if (!field.Fill.HasValue)
					field.Excluded = action;
			}
		}
	}
}
