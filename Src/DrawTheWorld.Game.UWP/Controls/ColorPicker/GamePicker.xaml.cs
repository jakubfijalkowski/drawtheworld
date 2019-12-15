using System;
using System.Collections.ObjectModel;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Controls.ColorPicker
{
	/// <summary>
	/// Color picker that should be used inside game, not for settings.
	/// </summary>
	sealed partial class GamePicker
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.ColorPicker.Game");

		private static readonly IGameManager GameManager = App.Current.GameManager;
		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GamePicker), new PropertyMetadata(Orientation.Vertical, OnOrientationPropertyChanged));

		private Core.IGame Game = null;
		private GameData GameData = null;
		private ToolData Brush = null;
		private ObservableCollection<Color> Entries = null;

		/// <summary>
		/// Gets or sets the orientation of the color picker.
		/// </summary>
		public Orientation Orientation
		{
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public GamePicker()
		{
			this.InitializeComponent();
			this.UpdateOrientation();
		}

		/// <summary>
		/// Reloads color picker.
		/// Should be called only if game object has been changed.
		/// </summary>
		public void ReloadPicker()
		{
			this.Clean();

			this.Game = GameManager.Game;
			this.GameData = GameManager.GameData;

			this.Brush = this.GameData.GetTool(Core.PredefinedTool.Brush);
			this.Entries = new ObservableCollection<Color>();
			this.RefreshPalette();

			if (this.Game is Core.Designer)
				((Core.Designer)this.Game).PropertyChanged += this.OnGamePropertyChanged;
			this.Brush.PropertyChanged += this.OnBrushPropertyChanged;
		}

		private void Clean()
		{
			if (this.Game is Core.Designer)
				((Core.Designer)this.Game).PropertyChanged -= this.OnGamePropertyChanged;

			if (this.Brush != null)
				this.Brush.PropertyChanged -= this.OnBrushPropertyChanged;

			this.Game = null;
			this.GameData = null;
			this.Brush = null;
			this.Entries = null;
		}

		private void UpdateOrientation()
		{
			switch (this.Orientation)
			{
				case Orientation.Horizontal:

					this.ColorsList.ItemContainerStyle = this.HorizontalColorPickerEntryStyle;

					this.ColorsList.HorizontalAlignment = HorizontalAlignment.Center;
					this.ColorsList.HorizontalContentAlignment = HorizontalAlignment.Center;

					this.ColorsList.VerticalAlignment = VerticalAlignment.Bottom;
					this.ColorsList.VerticalContentAlignment = VerticalAlignment.Bottom;

					this.ListTransition.Edge = EdgeTransitionLocation.Bottom;

					ScrollViewer.SetHorizontalScrollMode(this.ColorsList, ScrollMode.Auto);
					ScrollViewer.SetHorizontalScrollBarVisibility(this.ColorsList, ScrollBarVisibility.Auto);

					ScrollViewer.SetVerticalScrollMode(this.ColorsList, ScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(this.ColorsList, ScrollBarVisibility.Hidden);

					break;

				case Orientation.Vertical:

					this.ColorsList.ItemContainerStyle = this.VerticalColorPickerEntryStyle;

					this.ColorsList.HorizontalAlignment = HorizontalAlignment.Right;
					this.ColorsList.HorizontalContentAlignment = HorizontalAlignment.Right;

					this.ColorsList.VerticalAlignment = VerticalAlignment.Center;
					this.ColorsList.VerticalContentAlignment = VerticalAlignment.Center;

					this.ListTransition.Edge = EdgeTransitionLocation.Right;

					ScrollViewer.SetHorizontalScrollMode(this.ColorsList, ScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(this.ColorsList, ScrollBarVisibility.Hidden);

					ScrollViewer.SetVerticalScrollMode(this.ColorsList, ScrollMode.Auto);
					ScrollViewer.SetVerticalScrollBarVisibility(this.ColorsList, ScrollBarVisibility.Auto);

					break;
			}
		}

		private void RefreshPalette()
		{
			var savedData = (Color)this.Brush.Data;
			var palette = this.Game.Palette;

			for (int i = 0; i < this.Entries.Count; i++)
			{
				if (Array.IndexOf(palette, this.Entries[i]) == -1)
					this.Entries.RemoveAt(i--);
			}

			for (int i = 0, j = 0; i < palette.Length; j++, i++)
			{
				if (this.Entries.Count <= j)
					this.Entries.Add(palette[i]);
				else if (this.Entries[j] != palette[i])
					this.Entries.Insert(j, palette[i]);
			}

			var selectedColor = Array.IndexOf(palette, savedData) != -1 ? savedData : this.Entries[0];
			this.Brush.Data = selectedColor;
			this.ColorsList.ItemsSource = this.Entries;
			this.ColorsList.SelectedItem = selectedColor;
		}

		private static void OnOrientationPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((Orientation)e.NewValue != (Orientation)e.OldValue)
				((GamePicker)sender).UpdateOrientation();
		}

		private void OnGamePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Game.NameOf(g => g.Palette))
				this.RefreshPalette();
		}

		private void OnBrushPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Brush.NameOf(b => b.Data))
				this.ColorsList.SelectedItem = this.Brush.Data;
		}

		private void UpdateSelectedColor(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 0)
				this.ColorsList.SelectedItem = e.RemovedItems[0];
			else
			{
				this.GameData.SelectedTool = this.Brush;
				this.Brush.Data = e.AddedItems[0];
			}
		}
	}
}
