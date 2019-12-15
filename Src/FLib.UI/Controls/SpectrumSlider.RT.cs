using System;
using System.ComponentModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace FLib.UI.Controls
{
	[TemplatePart(Name = VerticalThumbPartName, Type = typeof(Thumb))]
	[TemplatePart(Name = HorizontalThumbPartName, Type = typeof(Thumb))]
	[TemplatePart(Name = VerticalSliderPartName, Type = typeof(Canvas))]
	[TemplatePart(Name = HorizontalSliderPartName, Type = typeof(Canvas))]
	public sealed partial class SpectrumSlider
		: Control, INotifyPropertyChanged
	{
		#region Config
		private const string VerticalThumbPartName = "PART_VerticalThumb";
		private const string HorizontalThumbPartName = "PART_HorizontalThumb";

		private const string VerticalSliderPartName = "PART_VerticalSlider";
		private const string HorizontalSliderPartName = "PART_HorizontalSlider";
		#endregion

		#region Private Fields
		private Color _RGBColor = Colors.Red;

		private Thumb VerticalThumb = null;
		private Thumb HorizontalThumb = null;
		private Canvas VerticalSlider = null;
		private Canvas HorizontalSlider = null;

		private double SliderValue = 0.0;
		#endregion

		#region Dependency Properties
		private static readonly DependencyProperty _ColorProperty =
			DependencyProperty.Register("Color", typeof(HsvColor), typeof(SpectrumSlider), new PropertyMetadata(HsvColors.Red, OnColorChanged));

		private static readonly DependencyProperty _OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SpectrumSlider), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

		/// <summary>
		/// Identifies the <see cref="Color"/> property.
		/// </summary>
		public static DependencyProperty ColorProperty
		{
			get { return _ColorProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="Orientation"/> proeprty.
		/// </summary>
		public static DependencyProperty OrientationProperty
		{
			get { return SpectrumSlider._OrientationProperty; }
		}

		/// <summary>
		/// Orientation of the slider.
		/// </summary>
		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}
		#endregion

		#region Dependency Properties' Events
		private static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var oldValue = (Orientation)e.OldValue;
			var newValue = (Orientation)e.NewValue;
			if (oldValue != newValue)
				((SpectrumSlider)sender).UpdateOrientation(newValue);
		}
		#endregion

		#region Properties
		/// <summary>
		/// RGB representation of <see cref="Color"/>.
		/// </summary>
		public Color RGBColor
		{
			get { return this._RGBColor; }
			private set
			{
				if (value != this.RGBColor)
				{
					this._RGBColor = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Occurs when <see cref="Color"/> changes.
		/// </summary>
		public event EventHandler<HsvColorChanged> ColorChanged;
		#endregion

		#region INotifyPropertyChanged Members
		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region Constructors
		static SpectrumSlider()
		{
			InitializeGradients();
		}

		/// <summary>
		/// Initializes control.
		/// </summary>
		public SpectrumSlider()
		{
			this.DefaultStyleKey = typeof(SpectrumSlider);
		}
		#endregion

		#region Overrides
		/// <summary>
		/// Updates parts.
		/// </summary>
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			#region Vertical Slider
			if (this.VerticalThumb != null)
				this.VerticalThumb.DragDelta += VerticalThumb_DragDelta;
			this.VerticalThumb = this.GetTemplateChild(VerticalThumbPartName) as Thumb;
			if (this.VerticalThumb != null)
				this.VerticalThumb.DragDelta += VerticalThumb_DragDelta;

			if (this.VerticalSlider != null)
			{
				this.VerticalSlider.PointerPressed -= VerticalSlider_PointerPressed;
				this.VerticalSlider.SizeChanged -= this.VerticalSlider_SizeChanged;
			}
			this.VerticalSlider = this.GetTemplateChild(VerticalSliderPartName) as Canvas;
			if (this.VerticalSlider != null)
			{
				this.VerticalSlider.Background = new SolidColorBrush(Colors.Transparent);
				this.VerticalSlider.PointerPressed += VerticalSlider_PointerPressed;
				this.VerticalSlider.SizeChanged += this.VerticalSlider_SizeChanged;
			}
			#endregion

			#region Horizontal Slider
			if (this.HorizontalThumb != null)
				this.HorizontalThumb.DragDelta += HorizontalThumb_DragDelta;
			this.HorizontalThumb = this.GetTemplateChild(HorizontalThumbPartName) as Thumb;
			if (this.HorizontalThumb != null)
				this.HorizontalThumb.DragDelta += HorizontalThumb_DragDelta;

			if (this.HorizontalSlider != null)
			{
				this.HorizontalSlider.PointerPressed -= HorizontalSlider_PointerPressed;
				this.HorizontalSlider.SizeChanged -= this.HorizontalSlider_SizeChanged;
			}
			this.HorizontalSlider = this.GetTemplateChild(HorizontalSliderPartName) as Canvas;
			if (this.HorizontalSlider != null)
			{
				this.HorizontalSlider.Background = new SolidColorBrush(Colors.Transparent);
				this.HorizontalSlider.PointerPressed += HorizontalSlider_PointerPressed;
				this.HorizontalSlider.SizeChanged += this.HorizontalSlider_SizeChanged;
			}
			#endregion

			if (this.SpectrumRect != null)
				this.SpectrumRect.Fill = null;
			this.SpectrumRect = this.GetTemplateChild(SpectrumPartName) as Rectangle;

			this.UpdateOrientation(this.Orientation);
		}
		#endregion

		#region Slider Implementation
		private void UpdateOrientation(Orientation newValue)
		{
			if (newValue == Windows.UI.Xaml.Controls.Orientation.Vertical)
			{
				if (this.VerticalSlider != null)
					this.VerticalSlider.Visibility = Visibility.Visible;
				if (this.HorizontalSlider != null)
					this.HorizontalSlider.Visibility = Visibility.Collapsed;
				if (this.SpectrumRect != null)
					this.SpectrumRect.Fill = VerticalSpectrumGradient;

				this.MinWidth = 15;
				this.MinHeight = 100;
			}
			else
			{
				if (this.VerticalSlider != null)
					this.VerticalSlider.Visibility = Visibility.Collapsed;
				if (this.HorizontalSlider != null)
					this.HorizontalSlider.Visibility = Visibility.Visible;
				if (this.SpectrumRect != null)
					this.SpectrumRect.Fill = HorizontalSpectrumGradient;

				this.MinWidth = 100;
				this.MinHeight = 15;
			}

			this.MoveSlider();
		}

		#region Vertical Slider
		private void VerticalSlider_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.VerticalThumb != null)
			{
				this.VerticalThumb.Width = this.VerticalSlider.ActualWidth;
				this.MoveSlider();
			}
		}

		private void VerticalSlider_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			var pt = e.GetCurrentPoint(this.VerticalSlider).Position;
			this.SliderValue = pt.Y / this.VerticalSlider.ActualHeight;

			this.MoveSlider();
			this.UpdateHueFromSlider();
		}

		private void VerticalThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (this.VerticalSlider == null)
				return;

			this.SliderValue += e.VerticalChange / this.VerticalSlider.ActualHeight;
			this.MoveSlider();
			this.UpdateHueFromSlider();
		}
		#endregion

		#region Horizontal Slider
		private void HorizontalSlider_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.HorizontalThumb != null)
			{
				this.HorizontalThumb.Height = this.HorizontalSlider.ActualHeight;
				this.MoveSlider();
			}
		}

		private void HorizontalSlider_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			var pt = e.GetCurrentPoint(this.HorizontalSlider).Position;
			this.SliderValue = 1.0 - pt.X / this.HorizontalSlider.ActualWidth;

			this.MoveSlider();
			this.UpdateHueFromSlider();
		}

		private void HorizontalThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (this.HorizontalSlider == null)
				return;

			this.SliderValue -= e.HorizontalChange / this.HorizontalSlider.ActualWidth;
			this.MoveSlider();
			this.UpdateHueFromSlider();
		}
		#endregion

		private void MoveSlider()
		{
			if (this.Orientation == Orientation.Vertical && this.VerticalSlider != null)
			{
				var newMargin = this.SliderValue.Clamp(0.0, 1.0) * this.VerticalSlider.ActualHeight - this.VerticalThumb.ActualHeight / 2;
				Canvas.SetTop(this.VerticalThumb, newMargin);
			}
			else if (this.HorizontalSlider != null)
			{
				var newMargin = (1.0 - this.SliderValue.Clamp(0.0, 1.0)) * this.HorizontalSlider.ActualWidth - this.HorizontalThumb.ActualWidth / 2;
				Canvas.SetLeft(this.HorizontalThumb, newMargin);
			}
		}

		private void UpdateHueFromSlider()
		{
			this.SuppressEvents = true;
			this.Color = new HsvColor { Hue = 360.0 - this.SliderValue.Clamp(0.0, 1.0) * 360.0, Saturation = 1.0, Value = 1.0 };
			this.SuppressEvents = false;
		}
		#endregion

		#region Implementation-specific
		private void RaiseColorChanged(HsvColor oldValue, HsvColor newValue)
		{
			if (this.ColorChanged != null)
				this.ColorChanged(this, new HsvColorChanged(oldValue, newValue));
		}

		private void UpdateHue(double newHueValue)
		{
			this.SliderValue = 1.0 - newHueValue / 360.0;
			this.MoveSlider();
		}
		#endregion
	}
}
