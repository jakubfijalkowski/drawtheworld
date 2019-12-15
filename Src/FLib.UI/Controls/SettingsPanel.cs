using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace FLib.UI.Controls
{
	/// <summary>
	/// Dimension of <see cref="SettingsPanel"/> imposed by guidelines.
	/// </summary>
	public enum SettingsDimension
		: int
	{
		/// <summary>
		/// 346 pixels wide.
		/// </summary>
		Narrow = 346,

		/// <summary>
		/// 646 pixels wide.
		/// </summary>
		Wide = 646
	}

	/// <summary>
	/// Settings panel.
	/// </summary>
	/// <remarks>
	/// Controls in <see cref="SettingsPanel.Content"/> cannot be found using <see cref="FrameworkElement.FindName"/> on panel or panel's parent because
	/// <see cref="SettingsPanel"/> uses <see cref="Popup"/> that is not attached to any element as a host and therefore name scope doesn't contain any
	/// element from <see cref="SettingsPanel.Content"/>.
	/// </remarks>
	[ContentProperty(Name = "Content")]
	public sealed class SettingsPanel
		: FrameworkElement
	{
		#region Private Fields
		private readonly Popup HostPopup = null;

		private readonly SettingsPanelContentControl ContentHost = null;
		#endregion

		#region Dependency Properties
		private static readonly DependencyProperty _DimensionProperty =
			DependencyProperty.Register("Dimension", typeof(SettingsDimension), typeof(SettingsPanel), new PropertyMetadata(SettingsDimension.Narrow, OnDimensionChanged));

		private static readonly DependencyProperty _IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof(bool), typeof(SettingsPanel), new PropertyMetadata(false, OnIsOpenChanged));

		private static readonly DependencyProperty _SettingsNameProperty =
			DependencyProperty.Register("SettingsName", typeof(string), typeof(SettingsPanel), new PropertyMetadata("Settings"));

		private static readonly DependencyProperty _ContentStyleProperty =
			DependencyProperty.Register("ContentStyle", typeof(Style), typeof(SettingsPanel), new PropertyMetadata(null, OnContentStyleChanged));

		/// <summary>
		/// Identifies the <see cref="Dimension"/> property.
		/// </summary>
		public static DependencyProperty DimensionProperty
		{
			get { return SettingsPanel._DimensionProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="IsOpen"/> property.
		/// </summary>
		public static DependencyProperty IsOpenProperty
		{
			get { return SettingsPanel._IsOpenProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="SettingsName"/> property.
		/// </summary>
		public static DependencyProperty SettingsNameProperty
		{
			get { return SettingsPanel._SettingsNameProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="ContentStyle"/> property.
		/// </summary>
		public static DependencyProperty ContentStyleProperty
		{
			get { return SettingsPanel._ContentStyleProperty; }
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Width of the <see cref="SettingsPanel"/>.
		/// </summary>
		public SettingsDimension Dimension
		{
			get { return (SettingsDimension)this.GetValue(DimensionProperty); }
			set { this.SetValue(DimensionProperty, value); }
		}

		/// <summary>
		/// Indicates whether settings panel is opened or is closed.
		/// </summary>
		public bool IsOpen
		{
			get { return (bool)this.GetValue(IsOpenProperty); }
			set { this.SetValue(IsOpenProperty, value); }
		}

		/// <summary>
		/// Settings name.
		/// </summary>
		public string SettingsName
		{
			get { return (string)this.GetValue(SettingsNameProperty); }
			set { this.SetValue(SettingsNameProperty, value); }
		}

		/// <summary>
		/// Content of settings.
		/// </summary>
		public UIElement Content
		{
			get { return (UIElement)this.ContentHost.Content; }
			set { this.ContentHost.Content = value; }
		}

		/// <summary>
		/// Custom <see cref="SettingsPanelContentControl"/> style.
		/// </summary>
		public Style ContentStyle
		{
			get { return (Style)GetValue(ContentStyleProperty); }
			set { SetValue(ContentStyleProperty, value); }
		}
		#endregion

		#region Dependency Properties Events
		private static void OnDimensionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			int oldValue = (int)(SettingsDimension)e.OldValue;
			int newValue = (int)(SettingsDimension)e.NewValue;

			if (oldValue != newValue)
			{
				var settings = (SettingsPanel)sender;
				settings.ContentHost.Width = settings.HostPopup.Width = newValue;
				settings.AlignPopup();
			}
		}

		private static void OnIsOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;

			if (oldValue != newValue)
			{
				var settings = (SettingsPanel)sender;
				settings.HostPopup.IsOpen = newValue;

				if (newValue)
					settings.Opened.Raise(settings, null);
				else
					settings.Closed.Raise(settings, null);
			}
		}

		private static void OnContentStyleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			Style oldValue = (Style)e.OldValue;
			Style newValue = (Style)e.NewValue;
			if (oldValue != newValue)
			{
				var settings = (SettingsPanel)sender;
				settings.ContentHost.Style = newValue;
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Fires when the <see cref="IsOpen"/> property is set to true.
		/// </summary>
		public event TypedEventHandler<SettingsPanel, object> Opened;

		/// <summary>
		/// Fires when the <see cref="IsOpen"/> property is set to false.
		/// </summary>
		public event TypedEventHandler<SettingsPanel, object> Closed;

		/// <summary>
		/// Firest when back button of <see cref="SettingsPanelContentControl"/> is pressed.
		/// </summary>
		public event TypedEventHandler<SettingsPanel, RoutedEventArgs> BackButtonPressed;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes panel.
		/// </summary>
		public SettingsPanel()
		{
			this.Width = this.Height = 0;
			this.Visibility = Visibility.Collapsed;

			double width = (int)SettingsDimension.Narrow;
			double height = Window.Current.Bounds.Height;

			this.ContentHost = new SettingsPanelContentControl(this)
			{
				Width = width,
				Height = height
			};

			this.ContentHost.SetBinding(SettingsPanelContentControl.DataContextProperty, new Binding
			{
				Source = this,
				Path = new PropertyPath("DataContext"),
				Mode = BindingMode.OneWay
			});

			this.HostPopup = new Popup
			{
				IsLightDismissEnabled = true,
				IsOpen = this.IsOpen,
				Child = this.ContentHost,
				VerticalOffset = 0,
				Width = width,
				Height = height
			};

			this.HostPopup.ChildTransitions = new TransitionCollection();
			this.HostPopup.ChildTransitions.Add(new EdgeUIThemeTransition
				{
					Edge = EdgeTransitionLocation.Right
				});

			this.HostPopup.Closed += this.HostPopup_Closed;

			this.AlignPopup();

			this.Loaded += (s, e) => Window.Current.SizeChanged += this.OnWindowSizeChanged;
			this.Unloaded += (s, e) => Window.Current.SizeChanged -= this.OnWindowSizeChanged;
		}
		#endregion

		#region Event Handlers
		private void HostPopup_Closed(object sender, object e)
		{
			this.IsOpen = false;
		}

		private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			this.ContentHost.Height = e.Size.Height;
			this.HostPopup.Height = e.Size.Height;

			this.AlignPopup();
		}
		#endregion

		#region Internals
		internal void OnBackButtonPressed(RoutedEventArgs e)
		{
			if (this.BackButtonPressed != null)
				this.BackButtonPressed(this, e);
			this.IsOpen = false;
		}
		#endregion

		#region Auxiliary Methods
		private void AlignPopup()
		{
			var windowWidth = Window.Current.Bounds.Width;
			var popupWidth = (int)this.Dimension;

			this.HostPopup.HorizontalOffset = windowWidth - popupWidth;
		}
		#endregion
	}

	/// <summary>
	/// Content control for <see cref="SettingsPanel"/>.
	/// </summary>
	[TemplatePart(Name = BackButtonPartName, Type = typeof(Button))]
	public sealed class SettingsPanelContentControl
		: ContentControl
	{
		#region Config
		private const string BackButtonPartName = "PART_BACKBUTTON";
		#endregion

		#region Private Fields
		private readonly SettingsPanel _Owner = null;

		private Button BackButton = null;
		#endregion

		#region Dependency Properties
		private static readonly DependencyProperty _ThemeBrushProperty =
			DependencyProperty.Register("ThemeBrush", typeof(Brush), typeof(SettingsPanelContentControl), new PropertyMetadata(null));

		private static readonly DependencyProperty _HeaderForegroundProperty =
			DependencyProperty.Register("HeaderForeground", typeof(Brush), typeof(SettingsPanelContentControl), new PropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="ThemeBrush"/> property.
		/// </summary>
		public static DependencyProperty ThemeBrushProperty
		{
			get { return SettingsPanelContentControl._ThemeBrushProperty; }
		}

		/// <summary>
		/// Identifies the <see cref="HeaderForeground"/> property.
		/// </summary>
		public static DependencyProperty HeaderForegroundProperty
		{
			get { return SettingsPanelContentControl._HeaderForegroundProperty; }
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Owner of this <see cref="SettingsPanelContentControl"/>.
		/// </summary>
		public SettingsPanel Owner
		{
			get { return this._Owner; }
		}

		/// <summary>
		/// Theme brush.
		/// </summary>
		public Brush ThemeBrush
		{
			get { return (Brush)this.GetValue(ThemeBrushProperty); }
			set { this.SetValue(ThemeBrushProperty, value); }
		}

		/// <summary>
		/// Header foreground.
		/// </summary>
		public Brush HeaderForeground
		{
			get { return (Brush)GetValue(HeaderForegroundProperty); }
			set { SetValue(HeaderForegroundProperty, value); }
		}
		#endregion

		#region Constructors
		internal SettingsPanelContentControl(SettingsPanel owner)
		{
			this._Owner = owner;
			this.DefaultStyleKey = typeof(SettingsPanelContentControl);
		}
		#endregion

		#region Overrrides
		/// <summary>
		/// Retrives <see cref="BackButton"/> from template.
		/// </summary>
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (this.BackButton != null)
				this.BackButton.Click -= this.BackButton_Click;
			this.BackButton = this.GetTemplateChild(BackButtonPartName) as Button;
			if (this.BackButton != null)
				this.BackButton.Click += this.BackButton_Click;
		}
		#endregion

		#region Events
		private void BackButton_Click(object sender, RoutedEventArgs e)
		{
			this.Owner.OnBackButtonPressed(e);
		}
		#endregion
	}
}
