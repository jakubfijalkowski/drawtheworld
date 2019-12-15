using System;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Game.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Displays statistics of board.
	/// </summary>
	sealed partial class BoardStatistics
		: UserControl
	{
		private FrameworkElement Control = null;

		public BoardStatistics()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Shows popup near specified control.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="control"></param>
		public void Show(GameBoard data, FrameworkElement control)
		{
			if (this.MainPopup.IsOpen)
				this.Hide();

			this.MainPopup.DataContext = data;
			this.Control = control;
			this.PositionPopup();
			this.MainPopup.IsOpen = true;
		}

		/// <summary>
		/// Hides popup.
		/// </summary>
		public void Hide()
		{
			if (this.Control != null)
			{
				this.MainPopup.IsOpen = false;
				this.Control = null;
			}
		}

		private void PositionPopup()
		{
			var rect = UIHelper.GetElementRect(this.Control, (UIElement)this.Parent);
			if (Math.Abs(this.MainPopup.VerticalOffset - rect.Top) >= 1)
				this.MainPopup.VerticalOffset = rect.Top;

			var horizontal = rect.Right;
			if (horizontal + this.MainPopup.Width >= Window.Current.Bounds.Right)
			{
				this.LeftArrow.Visibility = Visibility.Collapsed;
				this.RightArrow.Visibility = Visibility.Visible;
				horizontal = rect.Left - this.MainPopup.Width;
			}
			else
			{
				this.LeftArrow.Visibility = Visibility.Visible;
				this.RightArrow.Visibility = Visibility.Collapsed;
			}

			if (Math.Abs(this.MainPopup.HorizontalOffset - horizontal) >= 1)
				this.MainPopup.HorizontalOffset = horizontal;
		}
	}
}
