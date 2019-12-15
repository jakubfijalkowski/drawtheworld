using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using WPoint = Windows.Foundation.Point;
using WSize = Windows.Foundation.Size;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Displays board.
	/// </summary>
	sealed partial class Display
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.Board.Display");

		private static readonly UISettings UISettings = App.Current.UISettings;
		private static readonly IGameManager GameManager = App.Current.GameManager;

		private Core.IBoard AssignedBoard = null;

		private readonly EventManager EventManager = null;
		private readonly ElementsCollection<Rectangle> FillCollection = null;
		private readonly ElementsCollection<UIElement> ExcludedCollection = null;
		private readonly ElementsCollection<TextBlockWithShadow> CounterCollection = null;

		private bool _IsSnapped = false;

		/// <summary>
		/// Indicates whether the board needs to be displayed as "snapped" or not.
		/// </summary>
		public bool IsSnapped
		{
			get { return this._IsSnapped; }
			set
			{
				if (!this._IsSnapped && value)
					this.Snap();
				else if (this._IsSnapped && !value)
					this.Unsnap();
				this._IsSnapped = value;
			}
		}

		/// <summary>
		/// Gets or sets the visibility of the game logo. Should be changed only in TutorialBoard.
		/// </summary>
		public Visibility LogoVisibility
		{
			get { return this.Logo.Visibility; }
			set { this.Logo.Visibility = value; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public Display()
		{
			this.InitializeComponent();

			this.ActionLayer.Display = this;
			this.Timer.SnappedTimer = this.SnappedTimer;

			this.EventManager = new EventManager(this, this.Container, this.Highlighter, this.ActionLayer);

			this.FillCollection = new ElementsCollection<Rectangle>(this.Fields);
			this.ExcludedCollection = new ElementsCollection<UIElement>(this.ExcludedMarks);
			this.CounterCollection = new ElementsCollection<TextBlockWithShadow>(this.TextMarks);
		}

		/// <summary>
		/// Reloads board. Have to be called when board or it size changes
		/// Uses <see cref="App.GameManager"/> to select board.
		/// </summary>
		/// <param name="customBoard">Specifies custom board(not the one from <see cref="App.GameManager"/>).</param>
		/// <param name="forceHideTimer">Forcibly hides timer. Used only in TutorialBoard.</param>
		public void ReloadBoard(Core.IBoard customBoard = null, bool forceHideTimer = false)
		{
			Logger.Info("Reloading board.");

			this.MinimizeBoard.Stop();
			this.EventManager.IsEnabled = true;
			this.Clean();

			this.AssignedBoard = customBoard ?? GameManager.Game.Board;

			this.CreateGridAndDecor();
			this.CreateDescriptions();
			this.CreateFields();
			this.Timer.Reset(forceHideTimer);
			this.RegisterSizeChanged();
			this.UpdateSize();

			this.ActionLayer.Reload();
		}

		/// <summary>
		/// Updates fields after applying action.
		/// </summary>
		/// <param name="locations"></param>
		public void UpdateFields(IEnumerable<Core.Point> locations)
		{
			Logger.Trace("Updating fields.");
			var fields = this.AssignedBoard.GetFields(locations);
			foreach (var f in fields)
			{
				if (!f.Fill.HasValue)
				{
					this.FillCollection.Remove(f.Location, true);

					if (f.Excluded)
						this.ExcludeField(f, true);
				}
				else
					this.FillField(f, true);

				if (!f.Excluded)
					this.ExcludedCollection.Remove(f.Location, true);

				if (f.Counter == 0)
					this.CounterCollection.Remove(f.Location, true);
				else
					this.SetFieldCounter(f, true);
			}
		}

		/// <summary>
		/// Shows "fine charged" marks and updates timer.
		/// </summary>
		/// <param name="fine"></param>
		public void NotifyOfFine(Tuple<int, IEnumerable<Core.Point>> fine)
		{
			Logger.Trace("Fine charged, showing marks.");
			foreach (var f in fine.Item2)
			{
				TextBlockWithShadow tb = new TextBlockWithShadow
				{
					Width = UISettings.FieldSize,
					Height = UISettings.FieldSize,
					Style = this.ChargedMarkStyle
				};
				Canvas.SetLeft(tb, f.X * UISettings.FieldSize);
				Canvas.SetTop(tb, f.Y * UISettings.FieldSize);
				var storyboard = new Storyboard
				{
					AutoReverse = true
				};
				storyboard.Completed += (s, e) => this.TextMarks.Children.Remove(tb);
				var animation = new DoubleAnimation
				{
					Duration = new Windows.UI.Xaml.Duration(TimeSpan.FromSeconds(0.1)),
					To = 1
				};
				storyboard.Children.Add(animation);
				Storyboard.SetTarget(animation, tb);
				Storyboard.SetTargetProperty(animation, "Opacity");
				this.TextMarks.Children.Add(tb);
				storyboard.Begin();
			}
			Logger.Trace("Notifying timer.");
			this.Timer.ShowFineAlert(fine.Item1);
		}

		/// <summary>
		/// Clears counter layer.
		/// </summary>
		public void ClearCounterLayer()
		{
			this.CounterCollection.Clear();
		}

		/// <summary>
		/// Disables the board functionality and minimizes the UI to fit available space.
		/// </summary>
		/// <param name="availableSize">The size that is available for the board.</param>
		/// <returns>The exact size that the minimized board uses.</returns>
		public WSize DisableAndMinimize(WSize availableSize)
		{
			Logger.Debug("Disabling functionality.");

			this.EventManager.IsEnabled = false;
			this.Highlighter.Disable();
			this.ActionLayer.Disable();
			this.Timer.Disable();

			Logger.Debug("Animating.");
			//Ensure size is no greater than GameScrollViewer's viewport size
			double viewportWidth = Math.Min(availableSize.Width, this.GameScrollViewer.ViewportWidth - this.Container.Margin.Left - this.Container.Margin.Right);
			double viewportHeight = Math.Min(availableSize.Height, this.GameScrollViewer.ViewportHeight - this.Container.Margin.Top - this.Container.Margin.Bottom);

			double scale = Math.Min(
				viewportWidth / (this.AssignedBoard.BoardInfo.Size.Width * UISettings.FieldSize + 1),
				viewportHeight / (this.AssignedBoard.BoardInfo.Size.Height * UISettings.FieldSize + 1)
				)
				.Clamp(0, 1);

			double width = (this.AssignedBoard.BoardInfo.Size.Width * UISettings.FieldSize + 1) * scale,
				   height = (this.AssignedBoard.BoardInfo.Size.Height * UISettings.FieldSize + 1) * scale;

			this.MinimizeBoardScaleX.To = this.MinimizeBoardScaleY.To = scale;

			this.MinimizeBoardContainerWidth.To = this.MinimizeBoardWidth.To = width;
			this.MinimizeBoardContainerHeight.To = this.MinimizeBoardHeight.To = height;

			this.MinimizeBoard.Begin();

			return new WSize(width, height);
		}

		private void Clean()
		{
			Logger.Trace("Cleaning content.");

			this.ColumnDescriptions.Children.Clear();
			this.RowDescriptions.Children.Clear();

			this.FillCollection.Clear();
			this.ExcludedCollection.Clear();

			this.OuterDecor.Children.Clear();
			this.InnerDecor.Children.Clear();
			this.InnerGrid.Data = null;

			this.TextMarks.Children.Clear();

			if (this.AssignedBoard != null)
				this.UnregisterSizeChanged();
		}

		private void CreateGridAndDecor()
		{
			Logger.Trace("Creating grid.");
			var fieldSize = UISettings.FieldSize;
			double w = this.AssignedBoard.BoardInfo.Size.Width * fieldSize, h = this.AssignedBoard.BoardInfo.Size.Height * fieldSize;

			this.OuterGrid.Width = w + 1;
			this.OuterGrid.Height = h + 1;
			var gg = new GeometryGroup();
			for (double x = fieldSize; x < w; x += fieldSize)
				gg.Children.Add(new LineGeometry { StartPoint = new WPoint(x, 0), EndPoint = new WPoint(x, h) });
			for (double y = fieldSize; y < h; y += fieldSize)
				gg.Children.Add(new LineGeometry { StartPoint = new WPoint(0, y), EndPoint = new WPoint(w, y) });
			this.InnerGrid.Data = gg;

			Logger.Trace("Creating decoration.");
			for (int i = 0; i < 2; i++)
			{
				var verticalRect = new Rectangle
				{
					Height = h,
					Style = this.VerticalDecorStyle
				};
				var horizontalRect = new Rectangle
				{
					Width = w,
					Style = this.HorizontalDecorStyle,
				};
				Canvas.SetLeft(verticalRect, w * i - 2);
				Canvas.SetTop(horizontalRect, h * i - 2);
				this.OuterDecor.Children.Add(verticalRect);
				this.OuterDecor.Children.Add(horizontalRect);
			}
			this.OuterDecor.Clip = new RectangleGeometry { Rect = new Windows.Foundation.Rect(0, 0, w, h) };

			for (double x = fieldSize; x < w; x += fieldSize)
			{
				var r = new Rectangle
				{
					Height = h,
					Style = this.VerticalDecorStyle
				};
				Canvas.SetLeft(r, x - 2);
				this.InnerDecor.Children.Add(r);
			}

			for (double y = fieldSize; y < h; y += fieldSize)
			{
				var r = new Rectangle
				{
					Width = w,
					Style = this.HorizontalDecorStyle
				};
				Canvas.SetTop(r, y - 2);
				this.InnerDecor.Children.Add(r);
			}
		}

		private void CreateDescriptions()
		{
			Logger.Trace("Creating descriptions.");
			foreach (var d in this.AssignedBoard.Rows)
			{
				var divider = new Rectangle { Style = this.HorizontalDescriptionDividerStyle };
				var container = new ItemsControl { ItemsSource = d, Style = this.HorizontalDescriptionContainerStyle, Height = UISettings.BlockSize };
				this.RowDescriptions.Children.Add(divider);
				this.RowDescriptions.Children.Add(container);
			}
			this.RowDescriptions.Children.Add(new Rectangle { Style = this.HorizontalDescriptionDividerStyle });

			foreach (var d in this.AssignedBoard.Columns)
			{
				var divider = new Rectangle { Style = this.VerticalDescriptionDividerStyle };
				var container = new ItemsControl { ItemsSource = d, Style = this.VerticalDescriptionContainerStyle, Width = UISettings.BlockSize };
				this.ColumnDescriptions.Children.Add(divider);
				this.ColumnDescriptions.Children.Add(container);
			}
			this.ColumnDescriptions.Children.Add(new Rectangle { Style = this.VerticalDescriptionDividerStyle });
		}

		private void CreateFields()
		{
			foreach (var f in this.AssignedBoard.Fields)
			{
				if (f.Fill.HasValue)
					this.FillField(f, false);
				else if (f.Excluded)
					this.ExcludeField(f, false);
				if (f.Counter > 0)
					this.SetFieldCounter(f, false);
			}
		}

		private void UpdateSize()
		{
			this.UpdateBoardWidth(null);
			this.UpdateBoardHeight(null);
		}

		/// <summary>
		/// If <paramref name="newRowCount"/> is null, size is reset.
		/// </summary>
		/// <param name="newRowCount"></param>
		private void UpdateBoardWidth(int? newRowCount)
		{
			if (!newRowCount.HasValue)
			{
				newRowCount = this.AssignedBoard.Rows.Max(x => x.Count());
				this.RowDescriptions.Width = 0;
			}

			double fieldWidth = this.AssignedBoard.BoardInfo.Size.Width * UISettings.FieldSize + 1;
			double rowDescSize = Math.Max(UISettings.MinDescriptionSize, newRowCount.Value * UISettings.BlockSize);
			rowDescSize = Math.Max(this.RowDescriptions.Width, rowDescSize);

			this.RowDescriptions.Width = rowDescSize;
			this.Container.Width = fieldWidth + rowDescSize;
			this.Highlighter.RowDescriptionOffset = this.RowDescriptions.Width;
		}

		/// <summary>
		/// If <paramref name="newColumnCount"/> is null, size is reset.
		/// </summary>
		/// <param name="newColumnCount"></param>
		private void UpdateBoardHeight(int? newColumnCount)
		{
			if (!newColumnCount.HasValue)
			{
				newColumnCount = this.AssignedBoard.Columns.Max(x => x.Count());
				this.ColumnDescriptions.Height = 0;
			}

			double fieldHeight = this.AssignedBoard.BoardInfo.Size.Height * UISettings.FieldSize + 1;
			double columnDescSize = Math.Max(UISettings.MinDescriptionSize, newColumnCount.Value * UISettings.BlockSize);
			columnDescSize = Math.Max(this.ColumnDescriptions.Height, columnDescSize);

			this.ColumnDescriptions.Height = columnDescSize;
			this.Container.Height = fieldHeight + columnDescSize;
			this.Highlighter.ColumnDescriptionOffset = this.ColumnDescriptions.Height;
		}

		private void OnRowSizeChanged(object sender, PropertyChangedEventArgs e)
		{
			ICollection<Core.Block> collection = (ICollection<Core.Block>)sender;
			if (e.PropertyName == collection.NameOf(_ => _.Count))
				this.UpdateBoardWidth(collection.Count);
		}

		private void OnColumnSizeChanged(object sender, PropertyChangedEventArgs e)
		{
			ICollection<Core.Block> collection = (ICollection<Core.Block>)sender;
			if (e.PropertyName == collection.NameOf(_ => _.Count))
				this.UpdateBoardHeight(collection.Count);
		}

		private void FillField(Core.Field field, bool animate)
		{
			var fillColor = field.Fill.Value;

			Rectangle rect = null;
			if (!this.FillCollection.TryGetValue(field.Location, out rect))
			{
				this.ExcludedCollection.Remove(field.Location, animate);
				rect = new Rectangle
				{
					Width = UISettings.FieldSize,
					Height = UISettings.FieldSize,
					Fill = new SolidColorBrush(Color.FromArgb(255, fillColor.R, fillColor.G, fillColor.B))
				};
				Canvas.SetLeft(rect, field.Location.X * UISettings.FieldSize);
				Canvas.SetTop(rect, field.Location.Y * UISettings.FieldSize);

				this.FillCollection.Add(field.Location, rect, animate);
			}
			else if (animate)
			{
				var storyboard = new Storyboard();
				var colorAnimation = new ColorAnimation
				{
					Duration = ElementsCollection<Rectangle>.AnimationDuration,
					To = field.Fill.Value
				};
				Storyboard.SetTarget(colorAnimation, rect);
				Storyboard.SetTargetProperty(colorAnimation, "(Rectangle.Fill).(SolidColorBrush.Color)");
				storyboard.Children.Add(colorAnimation);
				storyboard.Begin();
			}
			else
				((SolidColorBrush)rect.Fill).Color = field.Fill.Value;
		}

		private void ExcludeField(Core.Field field, bool animate)
		{
			UIElement mark = null;
			if (!this.ExcludedCollection.TryGetValue(field.Location, out mark))
			{
				this.FillCollection.Remove(field.Location, animate);
				mark = (UIElement)this.ExcludedMarkContainerTemplate.LoadContent();
				Canvas.SetLeft(mark, field.Location.X * UISettings.FieldSize);
				Canvas.SetTop(mark, field.Location.Y * UISettings.FieldSize);

				this.ExcludedCollection.Add(field.Location, mark, animate);
			}
		}

		private void SetFieldCounter(Core.Field field, bool animate)
		{
			if (field.Counter == 0)
			{
				this.CounterCollection.Remove(field.Location, animate);
				return;
			}

			TextBlockWithShadow counter = null;
			if (this.CounterCollection.TryGetValue(field.Location, out counter))
			{
				if (!animate)
				{
					counter.Text = field.Counter.ToString();
					return;
				}
				this.CounterCollection.Remove(field.Location, animate);
			}

			counter = new TextBlockWithShadow
			{
				Width = UISettings.FieldSize,
				Height = UISettings.FieldSize,
				Style = this.CounterTextStyle,
				Text = field.Counter.ToString()
			};
			counter.FontSize *= UISettings.Factor;
			Canvas.SetLeft(counter, field.Location.X * UISettings.FieldSize);
			Canvas.SetTop(counter, field.Location.Y * UISettings.FieldSize);

			this.CounterCollection.Add(field.Location, counter, animate);
		}

		private async void Snap()
		{
			this.FullGame.Visibility = Visibility.Collapsed;
			this.SnappedGame.Visibility = Visibility.Visible;

			this.BoardThumbnail.Source = await Core.Helpers.BoardDrawer.DrawThumbnail(this.AssignedBoard, this.BoardThumbnail.Source as WriteableBitmap);
		}

		private void Unsnap()
		{
			this.FullGame.Visibility = Visibility.Visible;
			this.SnappedGame.Visibility = Visibility.Collapsed;
		}

		private void RegisterSizeChanged()
		{
			if (this.AssignedBoard.Rows.Any(r => r is INotifyPropertyChanged))
			{
				foreach (var r in this.AssignedBoard.Rows.Cast<INotifyPropertyChanged>())
					r.PropertyChanged += this.OnRowSizeChanged;
				foreach (var c in this.AssignedBoard.Columns.Cast<INotifyPropertyChanged>())
					c.PropertyChanged += this.OnColumnSizeChanged;
			}
		}

		private void UnregisterSizeChanged()
		{
			if (this.AssignedBoard.Rows.Any(r => r is INotifyPropertyChanged))
			{
				foreach (var r in this.AssignedBoard.Rows.Cast<INotifyPropertyChanged>())
					r.PropertyChanged -= this.OnRowSizeChanged;
				foreach (var c in this.AssignedBoard.Columns.Cast<INotifyPropertyChanged>())
					c.PropertyChanged -= this.OnColumnSizeChanged;
			}
		}
	}
}
