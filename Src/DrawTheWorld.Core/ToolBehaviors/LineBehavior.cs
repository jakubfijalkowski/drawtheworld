using System;
using System.Collections.Generic;

namespace DrawTheWorld.Core.ToolBehaviors
{
	/// <summary>
	/// This behavior specifies that user can select only whole line(row/column), not particular fields.
	/// </summary>
	public sealed class LineBehavior
		: BehaviorBase
	{
		/// <inheritdoc />
		protected override void PrepareFields(List<Point> list)
		{
			if (Math.Abs(this.StartField.X - this.EndField.X) > Math.Abs(this.StartField.Y - this.EndField.Y))
			{
				if (this.StartField.X < this.EndField.X)
				{
					for (int x = this.StartField.X; x <= this.EndField.X; x++)
						list.Add(new Point(x, this.StartField.Y));
				}
				else
				{
					for (int x = this.StartField.X; x >= this.EndField.X; x--)
						list.Add(new Point(x, this.StartField.Y));
				}
			}
			else
			{
				if (this.StartField.Y < this.EndField.Y)
				{
					for (int y = this.StartField.Y; y <= this.EndField.Y; y++)
						list.Add(new Point(this.StartField.X, y));
				}
				else
				{
					for (int y = this.StartField.Y; y >= this.EndField.Y; y--)
						list.Add(new Point(this.StartField.X, y));
				}
			}
		}
	}
}
