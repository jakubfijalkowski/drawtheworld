using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Fixes invalid visual state after clicking the button.
	/// </summary>
	public sealed class ButtonFix
	{
		public static readonly DependencyProperty FixButtonStateProperty =
			DependencyProperty.RegisterAttached("FixButtonState", typeof(bool), typeof(ButtonFix), new PropertyMetadata(false, OnFixButtonStateChanged));

		public static bool GetFixButtonState(ButtonBase obj)
		{
			return (bool)obj.GetValue(FixButtonStateProperty);
		}

		public static void SetFixButtonState(ButtonBase obj, bool value)
		{
			obj.SetValue(FixButtonStateProperty, value);
		}

		private static void OnFixButtonStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;
			var btn = (ButtonBase)sender;
			if (oldValue != newValue)
			{
				if (newValue)
					btn.Loaded += FixState;
				else
					btn.Loaded -= FixState;
			}
		}

		private static void FixState(object sender, RoutedEventArgs e)
		{
			var btn = (ButtonBase)sender;
			VisualStateManager.GoToState(btn, "Unfocused", false);
			VisualStateManager.GoToState(btn, "Normal", false);
		}
	}
}
