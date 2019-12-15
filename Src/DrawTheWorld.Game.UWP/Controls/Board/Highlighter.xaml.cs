using System;
using System.Linq;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Supports highlighting current row/column.
	/// </summary>
	sealed partial class Highlighter
		: Canvas
	{
		private static readonly UISettings UISettings = App.Current.UISettings;
		private static readonly IGameManager GameManager = App.Current.GameManager;

		public static readonly DependencyProperty ColumnDescriptionOffsetProperty =
			DependencyProperty.Register("ColumnDescriptionOffset", typeof(double), typeof(Highlighter), new PropertyMetadata(0.0, OnOffsetPropertyChanged));

		public static readonly DependencyProperty RowDescriptionOffsetProperty =
			DependencyProperty.Register("RowDescriptionOffset", typeof(double), typeof(Highlighter), new PropertyMetadata(0.0, OnOffsetPropertyChanged));

		private Point PreviouslyHighlighted = Point.Invalid;

		/// <summary>
		/// Specifies size of the "column descriptions" row.
		/// </summary>
		public double ColumnDescriptionOffset
		{
			get { return (double)this.GetValue(ColumnDescriptionOffsetProperty); }
			set { this.SetValue(ColumnDescriptionOffsetProperty, value); }
		}

		/// <summary>
		/// Specified size of the "row descriptions" column
		/// </summary>
		public double RowDescriptionOffset
		{
			get { return (double)this.GetValue(RowDescriptionOffsetProperty); }
			set { this.SetValue(RowDescriptionOffsetProperty, value); }
		}

		public Highlighter()
		{
			this.DataContext = this;
			this.InitializeComponent();

			this.SizeChanged += this.UpdateSize;
		}

		/// <summary>
		/// Must be called when pointer enters the "highlighter" area.
		/// </summary>
		/// <param name="location"></param>
		public void OnPointerEntered(Windows.Foundation.Point location)
		{
			var pt = this.Convert(location);

			this.MoveColumn.Stop();
			this.MoveRow.Stop();

			this.HandleRowHighlighter(pt);
			this.HandleColumnHighlighter(pt);
		}

		/// <summary>
		/// Should be called when pointer moves around in "highlighter" area.
		/// </summary>
		/// <param name="location"></param>
		public void OnPointerMoved(Windows.Foundation.Point location)
		{
			var pt = this.Convert(location);

			this.HandleRowHighlighter(pt);
			this.HandleColumnHighlighter(pt);
		}

		/// <summary>
		/// Should be called when pointer exits the "highlighter" area.
		/// </summary>
		/// <param name="location"></param>
		public void OnPointerExited(Windows.Foundation.Point location)
		{
			this.Disable();
		}

		/// <summary>
		/// Disables the highlighter(it does not need to be re-enabled to work properly again).
		/// </summary>
		public void Disable()
		{
			if (this.PreviouslyHighlighted.X != -1)
				this.HideColumn.Begin();
			if (this.PreviouslyHighlighted.Y != -1)
				this.HideRow.Begin();

			this.PreviouslyHighlighted = Point.Invalid;
		}

		private static void OnOffsetPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (Math.Abs((double)e.NewValue - (double)e.OldValue) > 0.0001)
				((Highlighter)sender).UpdateSize(null, null);
		}

		private void UpdateSize(object sender, object e)
		{
			this.RowHighlighter.Width = (this.ActualWidth - this.RowDescriptionOffset).Clamp(0, double.MaxValue);
			this.ColumnHighlighter.Height = (this.ActualHeight - this.ColumnDescriptionOffset).Clamp(0, double.MaxValue);
		}

		private Point Convert(Windows.Foundation.Point pt)
		{
			pt.X -= this.RowDescriptionOffset;
			pt.Y -= this.ColumnDescriptionOffset;
			var p = new Point(
				pt.X > 0 ? (int)(pt.X / UISettings.FieldSize) : -1,
				pt.Y > 0 ? (int)(pt.Y / UISettings.FieldSize) : -1
				);
			if (p.X >= GameManager.Game.Board.BoardInfo.Size.Width)
				p.X = -1;
			if (p.Y >= GameManager.Game.Board.BoardInfo.Size.Height)
				p.Y = -1;
			return p;
		}

		private void HandleRowHighlighter(Point pt)
		{
			var targetY = pt.Y * UISettings.FieldSize + this.ColumnDescriptionOffset;

			if (this.PreviouslyHighlighted.Y == -1 && pt.Y != -1)
			{
				Canvas.SetTop(this.RowHighlighter, targetY);
				Canvas.SetTop(this.RowDescriptionHighlighter, targetY);

				this.ShowRow.Begin();
			}
			else if (this.PreviouslyHighlighted.Y != -1 && pt.Y == -1)
			{
				this.HideRow.Begin();
			}
			else if (this.PreviouslyHighlighted.Y != pt.Y)
			{
				this.MoveRow.SkipToFill();
				this.MoveRow.Children.Cast<DoubleAnimation>().ForEach(c => c.To = targetY);
				this.MoveRow.Begin();
			}

			this.PreviouslyHighlighted.Y = pt.Y;
		}

		private void HandleColumnHighlighter(Point pt)
		{
			var targetX = pt.X * UISettings.FieldSize + this.RowDescriptionOffset;

			if (this.PreviouslyHighlighted.X == -1 && pt.X != -1)
			{
				Canvas.SetLeft(this.ColumnHighlighter, targetX);
				Canvas.SetLeft(this.ColumnDescriptionHighlighter, targetX);

				this.ShowColumn.Begin();
			}
			else if (this.PreviouslyHighlighted.X != -1 && pt.X == -1)
			{
				this.HideColumn.Begin();
			}
			else if (this.PreviouslyHighlighted.X != pt.X)
			{
				this.MoveColumn.SkipToFill();
				this.MoveColumn.Children.Cast<DoubleAnimation>().ForEach(c => c.To = targetX);
				this.MoveColumn.Begin();
			}

			this.PreviouslyHighlighted.X = pt.X;
		}
	}
}
