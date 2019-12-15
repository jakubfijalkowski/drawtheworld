using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FLib.UI.Controls
{
	/// <summary>
	/// Panel that arranges items like <see cref="Windows.UI.Xaml.Controls.WrapGrid"/> but uses biggest item's size as item size.
	/// </summary>
	public sealed class BiggestItemSizedWrapGrid
		: Panel
	{
		#region Private Fields
		private double ComputedWidth = 0;
		private double ComputedHeight = 0;
		#endregion

		#region Dependency Properties
		private static readonly DependencyProperty _OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(BiggestItemSizedWrapGrid), new PropertyMetadata(Orientation.Vertical));

		/// <summary>
		/// Identifies the <see cref="Orientation"/> property.
		/// </summary>
		public static DependencyProperty OrientationProperty
		{
			get { return BiggestItemSizedWrapGrid._OrientationProperty; }
		}

		/// <summary>
		/// Gets or sets orientation of this panel.
		/// </summary>
		/// <remarks>
		/// <see cref="Windows.UI.Xaml.Controls.Orientation.Vertical"/> means that height is constant and items are placed in rows at first and
		/// <see cref="Windows.UI.Xaml.Controls.Orientation.Horizontal"/> that width is constant and items are placed in columns.
		/// </remarks>
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(OrientationProperty); }
			set { this.SetValue(OrientationProperty, value); }
		}
		#endregion

		#region Measure/Override
		/// <inheritdoc />
		protected override Size MeasureOverride(Size availableSize)
		{
			this.ComputedWidth = this.ComputedHeight = 1;
			foreach (var c in this.Children)
			{
				c.Measure(availableSize);
				if (c.DesiredSize.Width > this.ComputedWidth)
					this.ComputedWidth = c.DesiredSize.Width;
				if (c.DesiredSize.Height > this.ComputedHeight)
					this.ComputedHeight = c.DesiredSize.Height;
			}

			// We need more space despite the number of items
			if (availableSize.Width < this.ComputedWidth)
				availableSize.Width = this.ComputedWidth;
			if (availableSize.Height < this.ComputedHeight)
				availableSize.Height = this.ComputedHeight;

			int rows = 0, columns = 0;
			switch (this.Orientation)
			{
				case Orientation.Horizontal:
					columns = (int)(availableSize.Width / this.ComputedWidth);
					rows = (int)Math.Ceiling(this.Children.Count / (double)columns);
					break;
				case Orientation.Vertical:
					rows = (int)(availableSize.Height / this.ComputedHeight);
					columns = (int)Math.Ceiling(this.Children.Count / (double)rows);
					break;
			}
			return new Size(columns * this.ComputedWidth, rows * this.ComputedHeight);
		}

		/// <inheritdoc />
		protected override Size ArrangeOverride(Size finalSize)
		{
			var orientation = this.Orientation;

			Rect itemRect = new Rect(0, 0, this.ComputedWidth, this.ComputedHeight);
			foreach (var c in this.Children)
			{
				c.Arrange(itemRect);
				if (c.Visibility != Visibility.Collapsed)
				{
					switch (orientation)
					{
						case Orientation.Horizontal:
							itemRect.X += this.ComputedWidth;
							if ((itemRect.X + this.ComputedWidth) >= finalSize.Width)
							{
								itemRect.X = 0;
								itemRect.Y += this.ComputedHeight;
							}
							break;
						case Orientation.Vertical:
							itemRect.Y += this.ComputedHeight;
							if ((itemRect.Y + this.ComputedHeight) >= finalSize.Height)
							{
								itemRect.Y = 0;
								itemRect.X += this.ComputedWidth;
							}
							break;
					}
				}
			}
			return finalSize;
		}
		#endregion
	}
}
