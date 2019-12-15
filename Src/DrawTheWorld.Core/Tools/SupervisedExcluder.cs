using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Special brush, that excludes only fields that can be excluded.
	/// Should be used only in <see cref="Modes.SupervisedMode"/>.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("Excluder")]
	public class SupervisedExcluder
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
				if (field.Fill == null)
					field.Excluded = action;
		}
	}
}
