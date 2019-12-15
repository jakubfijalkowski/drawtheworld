using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Deals with correct event distribution.
	/// </summary>
	sealed class EventManager
	{
		private readonly Control Focusable = null;
		private readonly UIElement RootElement = null;
		private readonly Highlighter Highlighter = null;
		private readonly ActionLayer ActionLayer = null;

		private bool _IsEnabled = true;

		/// <summary>
		/// Indicates whether the events will or will not be ignored.
		/// </summary>
		public bool IsEnabled
		{
			get { return this._IsEnabled; }
			set
			{
				this._IsEnabled = value;
				if (!value)
					this.RootElement.ReleasePointerCaptures();
			}
		}

		public EventManager(Control focusable, UIElement root, Highlighter highlighter, ActionLayer actionLayer)
		{
			Validate.Debug(() => focusable, v => v.NotNull());
			Validate.Debug(() => root, v => v.NotNull());
			Validate.Debug(() => highlighter, v => v.NotNull());
			Validate.Debug(() => actionLayer, v => v.NotNull());

			this.Focusable = focusable;
			this.RootElement = root;
			this.Highlighter = highlighter;
			this.ActionLayer = actionLayer;

			root.PointerEntered += this.OnPointerEntered;
			root.PointerMoved += this.OnPointerMoved;
			root.PointerPressed += this.OnPointerPressed;
			root.PointerReleased += this.OnPointerReleased;
			root.PointerExited += this.OnPointerExited;

			root.KeyDown += this.OnKeyDown;
			root.Tapped += this.OnTapped;
			root.DoubleTapped += this.OnDoubleTapped;

			this.Highlighter.IsHitTestVisible = false;
			this.ActionLayer.IsHitTestVisible = false;
		}

		private void OnPointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			this.Highlighter.OnPointerEntered(e.GetCurrentPoint(this.RootElement).Position);
		}

		private void OnPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			this.Highlighter.OnPointerMoved(e.GetCurrentPoint(this.RootElement).Position);
			this.ActionLayer.OnPointerMoved(e);
		}

		private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			var state = this.ActionLayer.OnPointerPressed(e);
			if (state == PointerCaptureState.Capture)
				this.CapturePointer(e.Pointer);
			else if (state == PointerCaptureState.Release)
				this.ReleasePointer();
		}

		private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			if (this.ActionLayer.OnPointerReleased(e) == PointerCaptureState.Release)
				this.ReleasePointer();
		}

		private void OnPointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			this.Highlighter.OnPointerExited(e.GetCurrentPoint(this.RootElement).Position);
		}

		private void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			if (this.ActionLayer.OnKeyDown(e) == PointerCaptureState.Release)
				this.ReleasePointer();
		}

		private void OnTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			var block = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource) as BlockDisplay;
			if (block != null)
				block.SelectColor();
		}

		private void OnDoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			var block = VisualTreeHelper.GetParent((DependencyObject)e.OriginalSource) as BlockDisplay;
			if (block != null)
				block.InverseState();
		}

		private void CapturePointer(Windows.UI.Xaml.Input.Pointer pointer)
		{
			this.Focusable.Focus(FocusState.Programmatic);
			this.RootElement.CapturePointer(pointer);
			this.RootElement.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
		}

		private void ReleasePointer()
		{
			this.RootElement.ReleasePointerCaptures();
			this.RootElement.ManipulationMode = ManipulationModes.System;
		}
	}
}
