using System.ComponentModel;
using DrawTheWorld.Game.Helpers;
using FLib;
using FLib.UI;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Controls.ColorPicker
{
	/// <summary>
	/// Settings dialog for colors in designer's color picker settings.
	/// </summary>
	/// <remarks>
	/// The picker will set the color to transparent, if user wants to remove it from the palette.
	/// </remarks>
	sealed partial class SettingsPicker
		: UserControl
	{
		private PaletteEntry SelectedEntry = null;

		/// <summary>
		/// Initializes the control.
		/// </summary>
		public SettingsPicker()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Opens the picker near the <paramref name="element"/>.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="element"></param>
		public void Open(PaletteEntry entry, FrameworkElement element)
		{
			Validate.Debug(() => entry, v => v.NotNull());
			Validate.Debug(() => element, v => v.NotNull());

			this.SelectedEntry = entry;

			var parentRect = UIHelper.GetElementRect((FrameworkElement)this.Parent);
			var rect = UIHelper.GetElementRect(element, (UIElement)this.Parent);

			double centerY = rect.Top + element.ActualHeight / 2;
			double arrowMargin = element.ActualHeight / 2;
			double popupY = centerY - arrowMargin;
			if (popupY + this.MainPopup.Height > parentRect.Height)
			{
				popupY = parentRect.Height - this.MainPopup.Height;
				arrowMargin = centerY - popupY;
			}

			this.MainPopup.HorizontalOffset = rect.Left - this.MainPopup.Width;
			this.MainPopup.VerticalOffset = popupY;
			this.Arrow.Margin = new Thickness(0, arrowMargin, 0, 0);

			this.MainPopup.IsOpen = true;
		}

		private void OnCanvasLoaded(object sender, object e)
		{
			if (this.SelectedEntry.Color.A != 255)
			{
				this.ColorCanvas.RGBColor = Colors.White;
				this.SelectedEntry.Color = Colors.White;
			}
			else
			{
				this.ColorCanvas.RGBColor = this.SelectedEntry.Color;
			}
		}

		private void UpdateColor(object sender, FLib.UI.Controls.HsvColorChanged e)
		{
			this.SelectedEntry.Color = e.NewValue.ToRGB();
		}

		private void RemoveColor(object sender, RoutedEventArgs e)
		{
			this.ColorCanvas.RGBColor = Colors.White;
			this.SelectedEntry.Color = Colors.Transparent;
		}
	}

	/// <summary>
	/// Describes entry in settings.
	/// </summary>
	sealed class PaletteEntry
		: INotifyPropertyChanged
	{
		private readonly SolidColorBrush Background = null;

		/// <summary>
		/// Gets or sets color of the entry.
		/// </summary>
		public Color Color
		{
			get { return this.Background.Color; }
			set
			{
				if (this.Background.Color != value)
				{
					bool aDiffers = this.Background.Color.A != value.A;
					this.Background.Color = value;
					if (aDiffers)
					{
						this.PropertyChanged.Raise(this, _ => _.IsColorVisible);
						this.PropertyChanged.Raise(this, _ => _.IsColorVisibleInverse);
					}
				}
			}
		}

		/// <summary>
		/// Visibility of color(alpha == 255 - visible, otherwise invisible).
		/// </summary>
		public Visibility IsColorVisible
		{
			get { return this.Color.A == 255 ? Visibility.Visible : Visibility.Collapsed; }
		}

		/// <summary>
		/// Inverse of <see cref="IsColorVisible"/>.
		/// </summary>
		public Visibility IsColorVisibleInverse
		{
			get { return this.Color.A == 0 ? Visibility.Visible : Visibility.Collapsed; }
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		public PaletteEntry(SolidColorBrush background)
		{
			Validate.Debug(() => background, v => v.NotNull());
			this.Background = background;
		}
	}
}
