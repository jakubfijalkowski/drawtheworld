using System;
using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Special-case tool to enable panning of the board using default ScrollViewer and one finger.
	/// When this tool is selected, <see cref="ToolActionBridge"/> should not allow to start new selection.
	/// </summary>
	[BindBehavior(typeof(ToolBehaviors.LineBehavior)), ToolName("PanTool")]
	public sealed class PanTool
		: ITool
	{
		/// <inheritdoc />
		public bool IsSingleAction
		{
			get { return false; }
		}

		/// <summary>
		/// Not usable.
		/// </summary>
		/// <param name="on"></param>
		/// <param name="userData"></param>
		/// <param name="fields"></param>
		public void Perform(IGame on, object userData, IEnumerable<Field> fields)
		{
			throw new InvalidOperationException();
		}

		/// <summary>
		/// Not usable.
		/// </summary>
		/// <param name="on"></param>
		/// <param name="userData"></param>
		/// <param name="fields"></param>
		public void PerformOnInvalid(IGame on, object userData, IEnumerable<Field> fields)
		{
			throw new InvalidOperationException();
		}
	}
}
