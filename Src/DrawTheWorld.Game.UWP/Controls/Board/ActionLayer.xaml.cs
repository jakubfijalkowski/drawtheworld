using System.Linq;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Indicates what to do with pointer capture.
	/// </summary>
	enum PointerCaptureState
	{
		Capture,
		Release,
		DoNotChange
	}

	/// <summary>
	/// The layer which user performs actions on.
	/// </summary>
	sealed partial class ActionLayer
		: Canvas
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.Board.ActionLayer");

		private static readonly UISettings UISettings = App.Current.UISettings;
		private static readonly IGameManager GameManager = App.Current.GameManager;

		private Display _Display = null;

		private Core.IGame Game = null;
		private Core.UI.GameData GameData = null;

		private bool IsDuringSelection = false;
		private uint PointerId = 0;
		private ToolData CurrentTool = null;

		private Core.Point LastLabel = Core.Point.Invalid;
		private int LastLabelCount = -1;

		public Display Display
		{
			get { return this._Display; }
			set
			{
				Validate.Debug(() => value, v => v.NotNull());
				Validate.Debug(() => this._Display, v => v.Null());
				this._Display = value;
			}
		}

		/// <summary>
		/// Initializes the layer.
		/// </summary>
		public ActionLayer()
		{
			this.InitializeComponent();
		}

		/// <summary>
		/// Reloads the layer.
		/// </summary>
		public void Reload()
		{
			if (this.IsDuringSelection)
				this.Interrupt();

			this.Game = GameManager.Game;
			this.GameData = GameManager.GameData;
		}

		/// <summary>
		/// Should be called when user presses the pointer anywhere on the board.
		/// Deals with starting selection and interrupting it with second finger.
		/// </summary>
		/// <param name="e"></param>
		/// <returns>Returns value indicating whether pointer should be captured or not.</returns>
		public PointerCaptureState OnPointerPressed(PointerRoutedEventArgs e)
		{
			Validate.Debug(() => this.Game, v => v.NotNull());

			//Interrupt if needed
			if (this.IsDuringSelection && e.Pointer.PointerId != this.PointerId)
			{
				Logger.Trace("Second finger pressed the board, interrupting selection.");
				this.Interrupt();
				return PointerCaptureState.Release;
			}

			var pt = e.GetCurrentPoint(this);
			if (!this.IsInside(pt.Position))
				return PointerCaptureState.DoNotChange;

			if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse && !pt.Properties.IsLeftButtonPressed)
				return PointerCaptureState.DoNotChange;

			if (pt.IsInContact && !(this.GameData.SelectedTool.Tool is Core.Tools.PanTool))
			{
				Logger.Trace("Starting selection.");
				this.Start(this.Convert(pt.Position), e.Pointer.PointerId);
				return PointerCaptureState.Capture;
			}

			return PointerCaptureState.DoNotChange;
		}

		/// <summary>
		/// Should be called on every pointer move.
		/// </summary>
		/// <param name="e"></param>
		public void OnPointerMoved(PointerRoutedEventArgs e)
		{
			Validate.Debug(() => this.Game, v => v.NotNull());

			if (this.IsDuringSelection && e.Pointer.PointerId == this.PointerId)
			{
				var pt = this.Convert(e.GetCurrentPoint(this).Position);
				pt.X = pt.X.Clamp(0, this.Game.Board.BoardInfo.Size.Width - 1);
				pt.Y = pt.Y.Clamp(0, this.Game.Board.BoardInfo.Size.Height - 1);
				this.Move(pt);
			}
		}

		/// <summary>
		/// Should be called when user presses the key.
		/// Allows interrupting a selection with Space.
		/// </summary>
		/// <param name="e"></param>
		public PointerCaptureState OnKeyDown(KeyRoutedEventArgs e)
		{
			Validate.Debug(() => this.Game, v => v.NotNull());

			if (this.IsDuringSelection && e.Key == Windows.System.VirtualKey.Space)
			{
				Logger.Trace("Space pressed, interrupting selection.");
				this.Interrupt();
				return PointerCaptureState.Release;
			}
			return PointerCaptureState.DoNotChange;
		}

		/// <summary>
		/// Should be called when pointer gets released.
		/// Finishes selection.
		/// </summary>
		/// <param name="e"></param>
		/// <returns>Returns value indicating whether pointer should be released or not.</returns>
		public PointerCaptureState OnPointerReleased(PointerRoutedEventArgs e)
		{
			Validate.Debug(() => this.Game, v => v.NotNull());

			if (this.IsDuringSelection && this.PointerId == e.Pointer.PointerId)
			{
				Logger.Trace("Pointer released, finishing selection.");
				this.Finish();
				return PointerCaptureState.Release;
			}
			return PointerCaptureState.DoNotChange;
		}

		/// <summary>
		/// Disables the action layer(use <see cref="Reload"/> to reenable).
		/// </summary>
		public void Disable()
		{
			Logger.Trace("Disabling action layer.");
			if (this.IsDuringSelection)
				this.Interrupt();

			this.Game = null;
		}

		private bool IsInside(Windows.Foundation.Point pt)
		{
			return pt.X >= 0 && pt.Y >= 0 && pt.X < this.ActualWidth && pt.Y < this.ActualHeight;
		}

		private Core.Point Convert(Windows.Foundation.Point pt)
		{
			return new Core.Point(
				(int)(pt.X / UISettings.FieldSize),
				(int)(pt.Y / UISettings.FieldSize)
				);
		}

		private void Start(Core.Point pt, uint pointerId)
		{
			this.IsDuringSelection = true;
			this.PointerId = pointerId;
			this.CurrentTool = this.GameData.SelectedTool;
			this.CurrentTool.Behavior.Start(pt);
			this.UpdateLabels();

			Logger.Debug("Starting selection on point {0}x{1} using tool {2}.", pt.X, pt.Y, this.CurrentTool.GetType().Name);
		}

		private void Move(Core.Point pt)
		{
			this.CurrentTool.Behavior.Over(pt);
			this.UpdateLabels();
		}

		private void Interrupt()
		{
			this.IsDuringSelection = false;
			this.PointerId = 0;
			this.CurrentTool.Behavior.Finish();
			this.CurrentTool = null;

			this.LastLabel = Core.Point.Invalid;
			this.LastLabelCount = -1;

			this.Children.Clear();
			Logger.Trace("Selection interrupted.");
		}

		private void Finish()
		{
			Logger.Debug("Finishing selection, action will be performed on {0} fields.", this.CurrentTool.Behavior.Fields.Count);
			var fine = this.Game.PerformAction(this.CurrentTool.Tool, this.CurrentTool.Data, this.CurrentTool.Behavior.Fields);
			this.Display.UpdateFields(this.CurrentTool.Behavior.Fields);
			if (fine != null)
			{
				Logger.Trace("Fine was charged, notifying display.");
				this.Display.NotifyOfFine(fine);
			}
			BoardSounds.PlaySoundOnAction(this.GameData, this.CurrentTool, fine != null);
			this.Interrupt();

			this.Game.CheckIfFinished();
		}

		private void UpdateLabels()
		{
			if (this.CurrentTool.Behavior is Core.ToolBehaviors.LineBehavior)
			{
				Core.Point currentMax = this.CurrentTool.Behavior.Fields.Last();
				int diffX = currentMax.X - this.LastLabel.X,
					diffY = currentMax.Y - this.LastLabel.Y;

				if (diffX != 0 && diffY != 0)
				{
					this.RegenerateLabels();
				}
				else
				{
					int diff = this.CurrentTool.Behavior.Fields.Count - this.LastLabelCount;
					while (diff++ < 0)
						this.Children.RemoveAt(this.Children.Count - 1);
					--diff;
					while (diff-- > 0)
						this.CreateLabel(this.Children.Count);
				}
				this.LastLabelCount = this.CurrentTool.Behavior.Fields.Count;
				this.LastLabel = currentMax;
			}
			else
				this.RegenerateLabels();
		}

		private void RegenerateLabels()
		{
			this.Children.Clear();
			for (int i = 0; i < this.CurrentTool.Behavior.Fields.Count; i++)
				this.CreateLabel(i);
		}

		private void CreateLabel(int i)
		{
			var tb = new TextBlockWithShadow
			{
				Width = UISettings.BlockSize,
				Height = UISettings.BlockSize,
				Style = this.NumberStyle,
				Text = (i + 1).ToString()
			};
			tb.FontSize *= UISettings.Factor;
			Canvas.SetLeft(tb, this.CurrentTool.Behavior.Fields[i].X * UISettings.FieldSize);
			Canvas.SetTop(tb, this.CurrentTool.Behavior.Fields[i].Y * UISettings.FieldSize);
			this.Children.Add(tb);
		}
	}
}
