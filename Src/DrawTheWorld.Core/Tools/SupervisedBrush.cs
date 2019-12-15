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
	/// Brush for <see cref="Modes.SupervisedMode"/>.
	/// </summary>
	/// <remarks>
	/// * each field is filled with color, if user selected it correctly
	/// * empty fields are excluded
	/// * fields with different color are left untouched
	/// </remarks>
	[BindBehavior(typeof(LineBehavior)), ToolName("Brush")]
	public class SupervisedBrush
		: BaseTool<Color>
	{
		/// <inheritdoc />
		protected override void Perform(IGame on, Color userData, IEnumerable<Field> fields)
		{
			foreach (var field in fields)
			{
				if (field.Excluded)
					field.Excluded = false;
				else if (field.CorrectFill == userData)
					field.Fill = userData;
				else if (!field.CorrectFill.HasValue)
					field.Excluded = true;
			}
		}
	}
}
