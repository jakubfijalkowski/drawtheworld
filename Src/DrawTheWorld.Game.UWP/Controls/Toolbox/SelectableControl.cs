using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DrawTheWorld.Game.Controls.Toolbox
{
	/// <summary>
	/// Substitute for SelectorItem.
	/// </summary>
	sealed class SelectableControl
		: Control
	{
		public static readonly DependencyProperty IsSelectedProperty =
			DependencyProperty.Register("IsSelected", typeof(bool), typeof(SelectableControl), new PropertyMetadata(false, OnIsSelectedChanged));

		/// <summary>
		/// Indicates whether the control is selected or isn't.
		/// </summary>
		public bool IsSelected
		{
			get { return (bool)this.GetValue(IsSelectedProperty); }
			set { this.SetValue(IsSelectedProperty, value); }
		}

		public SelectableControl()
		{
			this.Loaded += (s, e) =>
			{
				var res = VisualStateManager.GoToState(this, "Normal", true);
				var res2 = VisualStateManager.GoToState(this, this.IsSelected ? "Selected" : "Unselected", true);
			};
		}

		private static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var newValue = (bool)e.NewValue;
			var ctrl = (SelectableControl)sender;
			var res = VisualStateManager.GoToState(ctrl, newValue ? "Selected" : "Unselected", true);
		}

		protected override void OnPointerEntered(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
            var res = VisualStateManager.GoToState(this, "PointerOver", true);
            base.OnPointerEntered(e);
		}

		protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
            var res = VisualStateManager.GoToState(this, "Normal", true);
            base.OnPointerExited(e);
		}

		protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
            var res = VisualStateManager.GoToState(this, "Pressed", true);
            base.OnPointerPressed(e);
		}

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            var res = VisualStateManager.GoToState(this, "Normal", true);
            base.OnPointerReleased(e);
        }
    }
}
