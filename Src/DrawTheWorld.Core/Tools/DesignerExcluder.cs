using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Special brush, that excludes all fields.
	/// Should be used only in design mode.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("Excluder")]
	public class DesignerExcluder
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
			foreach (var field in fields)
			{
				field.Fill = null;
				field.Excluded = true;
			}
		}
	}
}
