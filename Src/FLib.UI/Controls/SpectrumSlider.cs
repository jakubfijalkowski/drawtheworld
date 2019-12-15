#if WINRT
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
#endif

namespace FLib.UI.Controls
{
	/// <summary>
	/// Spectrum slider.
	/// </summary>
	[TemplatePart(Name = SpectrumPartName, Type = typeof(Rectangle))]
	public sealed partial class SpectrumSlider
	{
		#region Configs
		/// <summary>
		/// Name of the spectrum part.
		/// </summary>
		private const string SpectrumPartName = "PART_Spectrum";

		private const double MinimumValue = 0.0;
		private const double MaximumValue = 360.0;
		private const double DefaultTickFrequency = 0.0001;
		#endregion

		#region Static Data
		private static LinearGradientBrush VerticalSpectrumGradient = null;
		private static LinearGradientBrush HorizontalSpectrumGradient = null;

		private static void InitializeGradients()
		{
			const int Steps = 30;
			const double StopIncrement = 1.0 / Steps;
			const double HueIncrement = 360.0 / Steps;

			VerticalSpectrumGradient = new LinearGradientBrush();
			VerticalSpectrumGradient.StartPoint = new Point(0.5, 1.0);
			VerticalSpectrumGradient.EndPoint = new Point(0.5, 0.0);
			VerticalSpectrumGradient.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

			HorizontalSpectrumGradient = new LinearGradientBrush();
			HorizontalSpectrumGradient.StartPoint = new Point(0.0, 0.5);
			HorizontalSpectrumGradient.EndPoint = new Point(1.0, 0.5);
			HorizontalSpectrumGradient.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;

			for (int i = 0; i < Steps - 1; i++)
			{
				VerticalSpectrumGradient.GradientStops.Add(new GradientStop { Color = new HsvColor { Hue = i * HueIncrement, Saturation = 1.0, Value = 1.0 }.ToRGB(), Offset = i * StopIncrement });
				HorizontalSpectrumGradient.GradientStops.Add(new GradientStop { Color = new HsvColor { Hue = i * HueIncrement, Saturation = 1.0, Value = 1.0 }.ToRGB(), Offset = i * StopIncrement });
			}
			VerticalSpectrumGradient.GradientStops.Add(new GradientStop { Color = new HsvColor { Hue = 360.0, Saturation = 1.0, Value = 1.0 }.ToRGB(), Offset = 1.0 });
			HorizontalSpectrumGradient.GradientStops.Add(new GradientStop { Color = new HsvColor { Hue = 360.0, Saturation = 1.0, Value = 1.0 }.ToRGB(), Offset = 1.0 });
		}
		#endregion

		#region Private Fields
		private Rectangle SpectrumRect = null;

		private bool SuppressEvents = false;
		#endregion

		#region Dependency Properties
		/// <summary>
		/// Currently selected color.
		/// </summary>
		public HsvColor Color
		{
			get { return (HsvColor)this.GetValue(ColorProperty); }
			set { this.SetValue(ColorProperty, value); }
		}
		#endregion

		#region Dependency Properties' Events
		private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var oldValue = (HsvColor)e.OldValue;
			var newValue = (HsvColor)e.NewValue;
			var ctrl = (SpectrumSlider)sender;

			// Constrains new value
			if (newValue.Saturation != 1.0 || newValue.Value != 1.0)
			{
				newValue.Saturation = newValue.Value = 1.0;
				ctrl.Color = newValue;
			}
			if (Math.Abs(oldValue.Hue - newValue.Hue) > HsvColorConfig.Epsilon)
				ctrl.OnColorChanged(oldValue, newValue);
		}
		#endregion

		#region Events
		/// <summary>
		/// Called when <see cref="Color"/> gets chagned.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		private void OnColorChanged(HsvColor oldValue, HsvColor newValue)
		{
			this.RGBColor = newValue.ToRGB();

			if (!this.SuppressEvents)
			{
				this.SuppressEvents = true;
				this.UpdateHue(newValue.Hue);
				this.SuppressEvents = false;
			}

			this.RaiseColorChanged(oldValue, newValue);
		}
		#endregion
	}
}
