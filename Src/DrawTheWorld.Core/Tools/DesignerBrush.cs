using System.Collections.Generic;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.ToolBehaviors;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core.Tools
{
	/// <summary>
	/// Special brush, that fills all fields with specified color.
	/// Should be used only in design mode.
	/// </summary>
	[BindBehavior(typeof(LineBehavior)), ToolName("Brush")]
	public class DesignerBrush
		: BaseTool<Color>
	{
		/// <inheritdoc />
		protected override void Perform(IGame on, Color userData, IEnumerable<Field> fields)
		{
			foreach (var field in fields)
			{
				field.Excluded = false;
				field.Fill = userData;
			}
		}
	}
}
