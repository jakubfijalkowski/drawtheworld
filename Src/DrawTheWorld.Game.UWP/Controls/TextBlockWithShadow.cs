using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Text block with shadow.
	/// </summary>
	[ContentProperty(Name = "Text")]
	sealed class TextBlockWithShadow
		: ContentControl
	{
		private readonly TextBlock Main = null;
		private readonly new TextBlock Shadow = null;

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(TextBlockWithShadow), new PropertyMetadata(string.Empty, OnTextChanged));
		public static readonly DependencyProperty ShadowBrushProperty =
			DependencyProperty.Register("ShadowBrush", typeof(Brush), typeof(TextBlockWithShadow), PropertyMetadata.Create(() => new SolidColorBrush(Colors.Black), OnShadowBrushChanged));
		public static readonly DependencyProperty ShadowOffsetProperty =
			DependencyProperty.Register("ShadowOffset", typeof(double), typeof(TextBlockWithShadow), new PropertyMetadata(1.0, OnShadowOffsetChanged));
		public static readonly DependencyProperty TextAlignmentProperty =
			DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(TextBlockWithShadow), new PropertyMetadata(TextAlignment.Left, OnTextAlignmentChanged));
		public static readonly DependencyProperty TextHorizontalAlignmentProperty =
			DependencyProperty.Register("TextHorizontalAlignment", typeof(HorizontalAlignment), typeof(TextBlockWithShadow), new PropertyMetadata(HorizontalAlignment.Left, OnTextVHAlignmentChanged));
		public static readonly DependencyProperty TextVerticalAlignmentProperty =
			DependencyProperty.Register("TextVerticalAlignment", typeof(VerticalAlignment), typeof(TextBlockWithShadow), new PropertyMetadata(VerticalAlignment.Top, OnTextVHAlignmentChanged));
		public static readonly DependencyProperty TextWrappingProperty =
			DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextBlockWithShadow), new PropertyMetadata(TextWrapping.Wrap, OnTextWrappingChanged));
		public static readonly DependencyProperty TextTrimmingProperty =
			DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(TextBlockWithShadow), new PropertyMetadata(TextTrimming.WordEllipsis, OnTextTrimmingChanged));

		/// <summary>
		/// Text to display.
		/// </summary>
		public string Text
		{
			get { return (string)this.GetValue(TextProperty); }
			set { this.SetValue(TextProperty, value); }
		}

		/// <summary>
		/// Shadow brush.
		/// </summary>
		public Brush ShadowBrush
		{
			get { return (Brush)this.GetValue(ShadowBrushProperty); }
			set { this.SetValue(ShadowBrushProperty, value); }
		}

		/// <summary>
		/// Gets or sets the vertical offset of the shadow.
		/// </summary>
		public double ShadowOffset
		{
			get { return (double)this.GetValue(ShadowOffsetProperty); }
			set { this.SetValue(ShadowOffsetProperty, value); }
		}

		/// <summary>
		/// Text alignment within TextBlocks.
		/// </summary>
		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)this.GetValue(TextAlignmentProperty); }
			set { this.SetValue(TextAlignmentProperty, value); }
		}

		/// <summary>
		/// Vertical alignment of the text.
		/// </summary>
		public VerticalAlignment TextVerticalAlignment
		{
			get { return (VerticalAlignment)this.GetValue(TextVerticalAlignmentProperty); }
			set { this.SetValue(TextVerticalAlignmentProperty, value); }
		}

		/// <summary>
		/// Horizontal alignment of the text.
		/// </summary>
		public HorizontalAlignment TextHorizontalAlignment
		{
			get { return (HorizontalAlignment)this.GetValue(TextHorizontalAlignmentProperty); }
			set { this.SetValue(TextHorizontalAlignmentProperty, value); }
		}

		/// <summary>
		/// Gets or sets how <see cref="TextBlockWithShadow"/> wraps text.
		/// </summary>
		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)this.GetValue(TextWrappingProperty); }
			set { this.SetValue(TextWrappingProperty, value); }
		}

		/// <summary>
		/// Gets or sets how <see cref="TextBlockWithShadow"/> trims text.
		/// </summary>
		public TextTrimming TextTrimming
		{
			get { return (TextTrimming)this.GetValue(TextTrimmingProperty); }
			set { this.SetValue(TextTrimmingProperty, value); }
		}

		private static void OnTextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			TextBlockWithShadow s = (TextBlockWithShadow)sender;
			s.Main.Text = s.Shadow.Text = (e.NewValue as string) ?? string.Empty;
		}

		private static void OnShadowOffsetChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			TextBlockWithShadow s = (TextBlockWithShadow)sender;
			s.Shadow.Margin = new Thickness(0, (double)e.NewValue, 0, 0);
		}

		private static void OnShadowBrushChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			TextBlockWithShadow s = (TextBlockWithShadow)sender;
			s.Shadow.Foreground = (Brush)e.NewValue;
		}

		private static void OnTextAlignmentChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			TextBlockWithShadow s = (TextBlockWithShadow)sender;
			s.Main.TextAlignment = s.Shadow.TextAlignment = (TextAlignment)e.NewValue;
		}

		private static void OnTextVHAlignmentChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!e.OldValue.Equals(e.NewValue))
			{
				((TextBlockWithShadow)sender).UpdateAlignment();
			}
		}

		private static void OnTextWrappingChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var tb = (TextBlockWithShadow)sender;
			var oldValue = (TextWrapping)e.OldValue;
			var newValue=(TextWrapping)e.NewValue;

			if (oldValue != newValue)
				tb.Main.TextWrapping = tb.Shadow.TextWrapping = newValue;
		}

		private static void OnTextTrimmingChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var tb = (TextBlockWithShadow)sender;
			var oldValue = (TextTrimming)e.OldValue;
			var newValue = (TextTrimming)e.NewValue;

			if (oldValue != newValue)
				tb.Main.TextTrimming = tb.Shadow.TextTrimming = newValue;
		}

		/// <summary>
		/// Initializes control.
		/// </summary>
		public TextBlockWithShadow()
		{
			this.Main = new TextBlock
			{
				TextWrapping = TextWrapping.Wrap,
				TextTrimming = TextTrimming.WordEllipsis
			};
			this.Shadow = new TextBlock
			{
				Foreground = this.ShadowBrush,
				Margin = new Thickness(0, this.ShadowOffset, 0, 0),
				TextWrapping = TextWrapping.Wrap,
				TextTrimming = TextTrimming.WordEllipsis
			};

			var grid = new Grid();
			grid.Children.Add(this.Shadow);
			grid.Children.Add(this.Main);

			this.Content = grid;
		}

		private void UpdateAlignment()
		{
			Thickness mainMargin = new Thickness();
			Thickness shadowMargin = new Thickness(0, this.ShadowOffset, 0, 0);

			if (this.TextVerticalAlignment == VerticalAlignment.Bottom)
			{
				mainMargin.Bottom = shadowMargin.Top;
				shadowMargin.Top = 0;
			}
			if (this.TextHorizontalAlignment == HorizontalAlignment.Right)
			{
				mainMargin.Right = shadowMargin.Left;
				shadowMargin.Left = 0;
			}

			this.Main.Margin = mainMargin;
			this.Shadow.Margin = shadowMargin;
			this.Main.VerticalAlignment = this.Shadow.VerticalAlignment = this.TextVerticalAlignment;
			this.Main.HorizontalAlignment = this.Shadow.HorizontalAlignment = this.TextHorizontalAlignment;
		}
	}
}
