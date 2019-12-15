using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;
using FLib;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Erases counter tool.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("EraseCounterTool")]
	public sealed class EraseCounterTool
		: ITool
	{
		/// <inheritdoc />
		public bool IsSingleAction
		{
			get { return true; }
		}

		/// <inheritdoc />
		public void Perform(IGame on, object userData, IEnumerable<Field> fields)
		{
			fields.ForEach(f => f.Counter = 0);
		}
	}
}
