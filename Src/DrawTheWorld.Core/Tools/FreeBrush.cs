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
	/// Brush for <see cref="Modes.FreeMode"/>.
	/// </summary>
	/// <remarks>
	/// * fills every field with selected color.
	/// * if field is excluded - deexcludes it and does not set fill.
	/// </remarks>
	[BindBehavior(typeof(LineBehavior)), ToolName("Brush")]
	public class FreeBrush
		: BaseTool<Color>
	{
		/// <inheritdoc />
		protected override void Perform(IGame on, Color userData, IEnumerable<Field> fields)
		{
			foreach (var f in fields)
			{
				if (f.Excluded)
					f.Excluded = false;
				else
					f.Fill = userData;
			}
		}
	}
}
