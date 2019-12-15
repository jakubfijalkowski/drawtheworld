using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Eraser - sets <see cref="Field.Excluded"/> to false and <see cref="Field.Fill"/> to null.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("Eraser")]
	public sealed class Eraser
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
			foreach (var f in fields)
			{
				f.Excluded = false;
				f.Fill = null;
			}
		}
	}
}
