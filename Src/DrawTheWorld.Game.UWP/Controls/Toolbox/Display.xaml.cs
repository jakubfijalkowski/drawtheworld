using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Controls.Toolbox
{
	/// <summary>
	/// Displays toolbox and available tools.
	/// </summary>
	sealed partial class Display
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.Toolbox.Display");

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(Display), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

		private Board.Display _BoardDisplay = null;
		private SelectedToolGuard Guard = null;

		/// <summary>
		/// Gets or sets the orientation of the display.
		/// </summary>
		public Orientation Orientation
		{
			get { return (Orientation)this.GetValue(OrientationProperty); }
			set { this.SetValue(OrientationProperty, value); }
		}

		/// <summary>
		/// Gets or sets the <see cref="Board.Display"/> used along with the toolbox.
		/// </summary>
		public Board.Display BoardDisplay
		{
			get { return this._BoardDisplay; }
			set
			{
				Validate.Debug(() => value, v => v.NotNull());
				Validate.Debug(() => this._BoardDisplay, v => v.Null());

				this._BoardDisplay = value;
				this.Guard = new SelectedToolGuard(this.Items, this._BoardDisplay);
			}
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public Display()
		{
			this.InitializeComponent();

			this.UpdateOrientation();
		}

		/// <summary>
		/// Forces the toolbox to reload.
		/// </summary>
		public void ReloadToolbox()
		{
			this.Guard.Reload();
		}

		private static void OnOrientationChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((Orientation)e.OldValue != (Orientation)e.NewValue)
				((Display)sender).UpdateOrientation();
		}

		private void UpdateOrientation()
		{
			switch (this.Orientation)
			{
				case Orientation.Horizontal:

					this.Items.ItemContainerStyle = this.HorizontalItemStyle;

					this.Items.HorizontalAlignment = HorizontalAlignment.Center;
					this.Items.HorizontalContentAlignment = HorizontalAlignment.Center;

					this.Items.VerticalAlignment = VerticalAlignment.Top;
					this.Items.VerticalContentAlignment = VerticalAlignment.Top;

					this.ListTransition.Edge = EdgeTransitionLocation.Top;

					ScrollViewer.SetHorizontalScrollMode(this.Items, ScrollMode.Auto);
					ScrollViewer.SetHorizontalScrollBarVisibility(this.Items, ScrollBarVisibility.Auto);

					ScrollViewer.SetVerticalScrollMode(this.Items, ScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(this.Items, ScrollBarVisibility.Hidden);

					break;

				case Orientation.Vertical:

					this.Items.ItemContainerStyle = this.VerticalItemStyle;

					this.Items.HorizontalAlignment = HorizontalAlignment.Left;
					this.Items.HorizontalContentAlignment = HorizontalAlignment.Left;

					this.Items.VerticalAlignment = VerticalAlignment.Center;
					this.Items.VerticalContentAlignment = VerticalAlignment.Center;

					this.ListTransition.Edge = EdgeTransitionLocation.Left;

					ScrollViewer.SetHorizontalScrollMode(this.Items, ScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(this.Items, ScrollBarVisibility.Hidden);

					ScrollViewer.SetVerticalScrollMode(this.Items, ScrollMode.Auto);
					ScrollViewer.SetVerticalScrollBarVisibility(this.Items, ScrollBarVisibility.Auto);

					break;
			}
		}
	}
}
