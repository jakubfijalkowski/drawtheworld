using System;
using System.Globalization;
#if WINRT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Foundation;

using ChannelType = System.Int16;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using ChannelType = System.Byte;
#endif

namespace FLib.UI.Controls
{
	/// <summary>
	/// Color canvas.
	/// </summary>
	[TemplatePart(Name = SpectrumSliderPartName, Type = typeof(SpectrumSlider))]
	[TemplatePart(Name = ColorShadingCanvasPartName, Type = typeof(Canvas))]
	[TemplatePart(Name = ColorSelectorPartName, Type = typeof(Canvas))]
	public sealed partial class ColorCanvas
		: Control
	{
		#region Config
		/// <summary>
		/// Name of spectrum slider part.
		/// </summary>
		private const string SpectrumSliderPartName = "PART_SpectrumSlider";

		/// <summary>
		/// Name of color shading part.
		/// </summary>
		private const string ColorShadingCanvasPartName = "PART_ColorShadingCanvas";

		/// <summary>
		/// Name of color selection part.
		/// </summary>
		private const string ColorSelectorPartName = "PART_ColorSelector";
		#endregion

		#region Private Fields
		private bool DuringColorUpdate = false;

		private TranslateTransform SelectorTranslation = new TranslateTransform();

		private SpectrumSlider SpectrumSlider = null;
		private Canvas ColorShadingCanvas = null;
		private Canvas ColorSelector = null;
		#endregion

		#region Dependency Properties
		/// <summary>
		/// Selected color.
		/// </summary>
		public HsvColor Color
		{
			get { return (HsvColor)this.GetValue(ColorProperty); }
			set { this.SetValue(ColorProperty, value); }
		}

		/// <summary>
		/// Selected color as RGB.
		/// </summary>
		public Color RGBColor
		{
			get { return (Color)this.GetValue(RGBColorProperty); }
			set { this.SetValue(RGBColorProperty, value); }
		}

		/// <summary>
		/// Hexadecimal representation of selected color(ARGB).
		/// </summary>
		public string HexString
		{
			get { return (string)this.GetValue(HexStringProperty); }
			set { this.SetValue(HexStringProperty, value); }
		}

		/// <summary>
		/// Red component of selected color.
		/// </summary>
		public byte R
		{
			get { return (byte)(ChannelType)this.GetValue(RProperty); }
			set { this.SetValue(RProperty, (ChannelType)value); }
		}

		/// <summary>
		/// Green component of selected color.
		/// </summary>
		public byte G
		{
			get { return (byte)(ChannelType)this.GetValue(GProperty); }
			set { this.SetValue(GProperty, (ChannelType)value); }
		}

		/// <summary>
		/// Blue component of selected color.
		/// </summary>
		public byte B
		{
			get { return (byte)(ChannelType)this.GetValue(BProperty); }
			set { this.SetValue(BProperty, (ChannelType)value); }
		}
		#endregion

		#region Dependency Properties Events
		/// <summary>
		/// Synchronizes <see cref="HexString"/> with <see cref="Color"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnHexStringChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (!ValidateHexString(e.NewValue))
				throw new ArgumentException("Invalid data for HexString.");

			var oldValue = ((string)e.OldValue).ToLower();
			var newValue = ((string)e.NewValue).ToLower();

			var ctrl = (ColorCanvas)sender;
			if (!ctrl.DuringColorUpdate && oldValue != newValue)
			{
				byte r = byte.Parse(newValue.Substring(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				byte g = byte.Parse(newValue.Substring(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
				byte b = byte.Parse(newValue.Substring(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

				ctrl.Color = HsvColorHelper.FromRGB(r, g, b);
			}
		}

		/// <summary>
		/// Synchronizes <see cref="RGBColor"/> with <see cref="Color"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnRGBColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var oldValue = (Color)e.OldValue;
			var newValue = (Color)e.NewValue;
			var ctrl = (ColorCanvas)sender;
			if (!ctrl.DuringColorUpdate && oldValue != newValue)
				ctrl.Color = newValue.ToHSV();
		}

		/// <summary>
		/// Synchronizes all properties with <see cref="Color"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (ColorCanvas)sender;
			var newValue = (HsvColor)e.NewValue;
			var oldValue = (HsvColor)e.OldValue;
			if (!HsvColorHelper.Equals(oldValue, newValue))
			{
				ctrl.DuringColorUpdate = true;
				var c = newValue.ToRGB();
				ctrl.RGBColor = c;
				ctrl.HexString = "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
				ctrl.R = c.R;
				ctrl.G = c.G;
				ctrl.B = c.B;
				ctrl.DuringColorUpdate = false;

				ctrl.OnColorChanged(oldValue, newValue);
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Called when <see cref="Color"/> changes.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		private void OnColorChanged(HsvColor oldValue, HsvColor newValue)
		{
			this.UpdatePositionFromColor(newValue);
			this.UpdateSpectrum(newValue);

			this.RaiseColorChangedEvent(oldValue, newValue);
		}
		#endregion

		#region Common Parts' Events
		private void ColorShadingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.UpdatePositionFromColor(this.Color);
		}
		#endregion

		#region Auxiliary Methods
		private void UpdatePositionFromColor(HsvColor color)
		{
			if (this.ColorShadingCanvas == null || this.ColorSelector == null)
				return;

			this.SelectorTranslation.X = color.Saturation * this.ColorShadingCanvas.ActualWidth - this.ColorSelector.Width / 2;
			this.SelectorTranslation.Y = (1.0 - color.Value) * this.ColorShadingCanvas.ActualHeight - this.ColorSelector.Height / 2;
		}

		private void UpdatePosition(Point pt)
		{
			if (this.ColorShadingCanvas == null || this.ColorSelector == null)
				return;

			this.SelectorTranslation.X = pt.X - this.ColorSelector.Width / 2;
			this.SelectorTranslation.Y = pt.Y - this.ColorSelector.Height / 2;
		}

		private void UpdateColorFromCanvas(Point pos)
		{
			if (this.ColorShadingCanvas == null || this.ColorSelector == null || this.SpectrumSlider == null)
				return;

			var hsv = this.SpectrumSlider.Color;
			hsv.Saturation = pos.X / this.ColorShadingCanvas.ActualWidth;
			hsv.Value = 1.0 - pos.Y / this.ColorShadingCanvas.ActualHeight;
				
			this.Color = hsv;
		}

		private void UpdateSpectrum(HsvColor color)
		{
			if (this.SpectrumSlider != null)
				this.SpectrumSlider.Color = color;
		}
		#endregion
	}
}
