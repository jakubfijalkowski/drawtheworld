using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;
using FLib;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Counter tool.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("CounterTool")]
	public sealed class CounterTool
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
			fields.ForEach(f => f.Counter++);
		}
	}
}
