using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FLib.UI.Controls
{
	public sealed partial class ColorCanvas
	{
		#region Dependency Properties
		private static readonly DependencyProperty _ColorProperty =
			DependencyProperty.Register("Color", typeof(HsvColor), typeof(ColorCanvas), new PropertyMetadata(HsvColors.Red, OnColorChanged));

		private static readonly DependencyProperty _RGBColorProperty =
			DependencyProperty.Register("RGBColor", typeof(Color), typeof(ColorCanvas), new PropertyMetadata(Colors.Red, OnRGBColorChanged));


		private static readonly DependencyProperty _HexStringProperty =
			DependencyProperty.Register("HexString", typeof(string), typeof(ColorCanvas), new PropertyMetadata("#FFFFFF", OnHexStringChanged));


		private static readonly DependencyProperty _RProperty =
			DependencyProperty.Register("R", typeof(short), typeof(ColorCanvas), new PropertyMetadata((short)255, OnChannelChanged));


		private static readonly DependencyProperty _GProperty =
			DependencyProperty.Register("G", typeof(short), typeof(ColorCanvas), new PropertyMetadata((short)255, OnChannelChanged));


		private static readonly DependencyProperty _BProperty =
			DependencyProperty.Register("B", typeof(short), typeof(ColorCanvas), new PropertyMetadata((short)255, OnChannelChanged));

		/// <summary>
		/// Identifies the <see cref="Color"/> property.
		/// </summary>
		public static DependencyProperty ColorProperty
		{
			get { return ColorCanvas._ColorProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="RGBColor"/> property.
		/// </summary>
		public static DependencyProperty RGBColorProperty
		{
			get { return ColorCanvas._RGBColorProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="HexString"/> property.
		/// </summary>
		public static DependencyProperty HexStringProperty
		{
			get { return ColorCanvas._HexStringProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="R"/> property.
		/// </summary>
		public static DependencyProperty RProperty
		{
			get { return ColorCanvas._RProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="G"/> property.
		/// </summary>
		public static DependencyProperty GProperty
		{
			get { return ColorCanvas._GProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="B"/> property.
		/// </summary>
		public static DependencyProperty BProperty
		{
			get { return ColorCanvas._BProperty; }
		}
		#endregion

		#region Dependency Properties' Events
		/// <summary>
		/// Synchronizes ARGB values with <see cref="Color"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnChannelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var oldValue = (short)e.OldValue;
			var newValue = (short)e.NewValue;
			var ctrl = (ColorCanvas)sender;
			if (!ctrl.DuringColorUpdate && oldValue != newValue)
			{
				//Coercion
				if (newValue < 0 || newValue > 255)
				{
					sender.SetValue(e.Property, newValue < 0 ? 0 : 255);
					return;
				}
				ctrl.Color = HsvColorHelper.FromRGB(ctrl.R, ctrl.G, ctrl.B);
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Occurs when <see cref="Color"/> changes.
		/// </summary>
		public event EventHandler<HsvColorChanged> ColorChanged;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes new control.
		/// </summary>
		public ColorCanvas()
		{
			this.DefaultStyleKey = typeof(ColorCanvas);
		}
		#endregion

		#region Template
		/// <inheritdoc />
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (this.ColorShadingCanvas != null)
			{
				this.ColorShadingCanvas.PointerPressed -= this.ColorShadingCanvas_PointerPressed;
				this.ColorShadingCanvas.PointerMoved -= this.ColorShadingCanvas_PointerMoved;
				this.ColorShadingCanvas.PointerReleased -= this.ColorShadingCanvas_PointerReleased;
				this.ColorShadingCanvas.PointerCanceled -= this.ColorShadingCanvas_PointerReleased;

				this.ColorShadingCanvas.SizeChanged -= this.ColorShadingCanvas_SizeChanged;
			}
			this.ColorShadingCanvas = this.GetTemplateChild(ColorShadingCanvasPartName) as Canvas;
			if (this.ColorShadingCanvas != null)
			{
				this.ColorShadingCanvas.PointerPressed += this.ColorShadingCanvas_PointerPressed;
				this.ColorShadingCanvas.PointerMoved += this.ColorShadingCanvas_PointerMoved;
				this.ColorShadingCanvas.PointerReleased += this.ColorShadingCanvas_PointerReleased;
				this.ColorShadingCanvas.PointerCanceled += this.ColorShadingCanvas_PointerReleased;

				this.ColorShadingCanvas.SizeChanged += this.ColorShadingCanvas_SizeChanged;
			}

			if (this.ColorSelector != null)
				this.ColorSelector.RenderTransform = null;
			this.ColorSelector = this.GetTemplateChild(ColorSelectorPartName) as Canvas;
			if (this.ColorSelector != null)
				this.ColorSelector.RenderTransform = this.SelectorTranslation;



			if (this.SpectrumSlider != null)
				this.SpectrumSlider.ColorChanged -= this.SpectrumSlider_SelectedColorChanged;
			this.SpectrumSlider = this.GetTemplateChild(SpectrumSliderPartName) as SpectrumSlider;

			var c = this.Color;
			this.UpdateSpectrum(c);
			this.UpdatePositionFromColor(c);

			if (this.SpectrumSlider != null)
				this.SpectrumSlider.ColorChanged += this.SpectrumSlider_SelectedColorChanged;
		}
		#endregion

		#region Parts' Events
		private void SpectrumSlider_SelectedColorChanged(object sender, HsvColorChanged e)
		{
			if (this.ColorSelector != null)
				this.UpdateColorFromCanvas(new Point(
					this.SelectorTranslation.X + this.ColorSelector.Width / 2,
					this.SelectorTranslation.Y + this.ColorSelector.Height / 2
					));
		}

		void ColorShadingCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
		{
			var pt = e.GetCurrentPoint(this.ColorShadingCanvas).Position
				.Clamp(0, this.ColorShadingCanvas.ActualWidth, 0, this.ColorShadingCanvas.ActualHeight);
			this.UpdateColorFromCanvas(pt);
			this.UpdatePosition(pt);

			this.ColorShadingCanvas.CapturePointer(e.Pointer);
		}

		void ColorShadingCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			var contact = e.GetCurrentPoint(this.ColorShadingCanvas);
			if (contact.IsInContact)
			{
				var pt = e.GetCurrentPoint(this.ColorShadingCanvas).Position
					.Clamp(0, this.ColorShadingCanvas.ActualWidth, 0, this.ColorShadingCanvas.ActualHeight);
				this.UpdateColorFromCanvas(pt);
				this.UpdatePosition(pt);
			}
		}

		void ColorShadingCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
		{
			this.ColorShadingCanvas.ReleasePointerCapture(e.Pointer);
		}
		#endregion

		#region Implementation-specific
		private void RaiseColorChangedEvent(HsvColor oldValue, HsvColor newValue)
		{
			if (this.ColorChanged != null)
				this.ColorChanged(this, new HsvColorChanged(oldValue, newValue));
		}

		/// <summary>
		/// Validates <see cref="HexString"/> to match specified pattern.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		internal static bool ValidateHexString(object data)
		{
			string hex = ((string)data).Trim().ToLower();
			return hex.Length == 7 && hex[0] == '#' && hex.Cast<char>().Skip(1).All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
		}
		#endregion
	}
}
