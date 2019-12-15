using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FLib.UI.Controls
{
	/// <summary>
	/// Uniform grid, same as in WPF.
	/// </summary>
	/// <remarks>
	/// The only difference between this and WPF implementations is that, when <see cref="Rows"/> and <see cref="Columns"/> are zero,
	/// this implementation favors columns instead of rows(grid is wider) and grid could be not square.
	/// </remarks>
	public sealed class UniformGrid
		: Panel
	{
		#region Private Fieldss
		private int _FirstColumn = 0;
		private int _Columns = 0;
		private int _Rows = 0;

		private int ComputedColumns = 0;
		private int ComputedRows = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the number of leading blank cells in first row of the grid.
		/// </summary>
		public int FirstColumn
		{
			get { return this._FirstColumn; }
			set
			{
				Validate.Debug(() => value, v => v.Min(0));
				this._FirstColumn = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of columns that are in the grid.
		/// </summary>
		public int Columns
		{
			get { return this._Columns; }
			set
			{
				Validate.Debug(() => value, v => v.Min(0));
				this._Columns = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of rows that are in the grid.
		/// </summary>
		public int Rows
		{
			get { return this._Rows; }
			set
			{
				Validate.Debug(() => value, v => v.Min(0));
				this._Rows = value;
			}
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Adapted from WPF.
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size constraint)
		{
			this.UpdateComputedValues();

			Size availableSize = new Size(constraint.Width / this.ComputedColumns, constraint.Height / this.ComputedRows);
			double maxWidth = 0.0;
			double maxHeight = 0.0;
			foreach (var c in base.Children)
			{
				c.Measure(availableSize);
				var desiredSize = c.DesiredSize;
				if (desiredSize.Width > maxWidth)
					maxWidth = desiredSize.Width;
				if (desiredSize.Height > maxHeight)
					maxHeight = desiredSize.Height;
			}
			return new Size(maxWidth * this.ComputedColumns, maxHeight * this.ComputedRows);
		}

		/// <summary>
		/// Adapted from WPF.
		/// </summary>
		/// <param name="arrangeSize"></param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size arrangeSize)
		{
			Rect finalRect = new Rect(0.0, 0.0, arrangeSize.Width / this.ComputedColumns, arrangeSize.Height / (double)this.ComputedRows);
			double width = finalRect.Width;
			double num = arrangeSize.Width - 1.0;
			finalRect.X += finalRect.Width * this.FirstColumn;
			foreach (UIElement uIElement in base.Children)
			{
				uIElement.Arrange(finalRect);
				if (uIElement.Visibility != Visibility.Collapsed)
				{
					finalRect.X += width;
					if (finalRect.X >= num)
					{
						finalRect.Y += finalRect.Height;
						finalRect.X = 0.0;
					}
				}
			}
			return arrangeSize;
		}
		#endregion

		#region Auxiliary Methods
		/// <summary>
		/// Adapted from WPF source.
		/// </summary>
		private void UpdateComputedValues()
		{
			this.ComputedColumns = this.Columns;
			this.ComputedRows = this.Rows;
			if (this.FirstColumn >= this.ComputedColumns)
				this.FirstColumn = 0;

			if (this.ComputedRows == 0 || this.ComputedColumns == 0)
			{
				int num = 0;
				for (int i = 0; i < base.Children.Count; i++)
				{
					if (base.Children[i].Visibility != Visibility.Collapsed)
						++num;
				}
				if (num == 0)
					num = 1;
				if (this.ComputedRows == 0)
				{
					if (this.ComputedColumns > 0)
					{
						this.ComputedRows = (num + this.FirstColumn + (this.ComputedColumns - 1)) / this.ComputedColumns;
					}
					else
					{
						this.ComputedColumns = (int)Math.Sqrt((double)num);
						this.ComputedRows = this.ComputedColumns;
						if (this.ComputedColumns * this.ComputedRows < num)
						{
							this.ComputedColumns++;
							if (this.ComputedColumns * this.ComputedRows < num)
								this.ComputedRows++;
						}
					}
				}
				else if (this.ComputedColumns == 0)
					this.ComputedColumns = (num + (this.ComputedRows - 1)) / this.ComputedRows;
			}
		}
		#endregion
	}
}
