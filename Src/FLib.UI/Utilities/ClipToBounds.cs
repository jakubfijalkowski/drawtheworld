using Windows.UI.Xaml;

namespace FLib.UI.Utilities
{
	/// <summary>
	/// Utility that acts like ClipToBounds from WPF.
	/// </summary>
	public static class ClipToBounds
	{
		#region Attached Property
		private static readonly DependencyProperty _ClipProperty = DependencyProperty.RegisterAttached("Clip", typeof(bool), typeof(ClipToBounds), new PropertyMetadata(false, OnClipChanged));

		/// <summary>
		/// Identifies the clip property.
		/// </summary>
		public static DependencyProperty ClipProperty
		{
			get { return _ClipProperty; }
		}

		/// <summary>
		/// Gets value of <see cref="ClipProperty"/>.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static bool GetClip(FrameworkElement d)
		{
			return (bool)d.GetValue(ClipProperty);
		}

		/// <summary>
		/// Sets value of <see cref="ClipProperty"/>.
		/// </summary>
		/// <param name="d"></param>
		/// <param name="value"></param>
		public static void SetClip(FrameworkElement d, bool value)
		{
			d.SetValue(ClipProperty, value);
		}
		#endregion

		#region Mechanism
		private static void OnClipChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			var newValue = (bool)e.NewValue;

			var ctrl = sender as FrameworkElement;
			if (newValue && !oldValue) //Attach
			{
				Clip(ctrl);

				ctrl.SizeChanged += ControlSizeChanged;
				ctrl.Loaded += ControlLoaded;
			}
			else if (oldValue && !newValue) //Detach
			{
				ctrl.Clip = null;

				ctrl.SizeChanged -= ControlSizeChanged;
				ctrl.Loaded -= ControlLoaded;
			}
		}

		private static void Clip(FrameworkElement ctrl)
		{
			ctrl.Clip = new Windows.UI.Xaml.Media.RectangleGeometry()
			{
				Rect = new Windows.Foundation.Rect(0, 0, ctrl.ActualWidth, ctrl.ActualHeight)
			};
		}

		static void ControlLoaded(object sender, RoutedEventArgs e)
		{
			Clip((FrameworkElement)sender);
		}

		static void ControlSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Clip((FrameworkElement)sender);
		}
		#endregion
	}
}
