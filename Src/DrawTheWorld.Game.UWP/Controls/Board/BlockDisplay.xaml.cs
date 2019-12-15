using System.ComponentModel;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Single <see cref="Block"/>.
	/// </summary>
	sealed partial class BlockDisplay
		: UserControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("UI.Board.BlockDisplay");

		private static readonly UISettings UISettings = App.Current.UISettings;
		private static readonly IGameManager GameManager = App.Current.GameManager;
		private readonly ToolData Brush = null;
		private Block _AssignedBlock = null;

		/// <summary>
		/// Assigned block.
		/// </summary>
		public Block AssignedBlock
		{
			get { return (Block)this.GetValue(AssignedBlockProperty); }
			set { this.SetValue(AssignedBlockProperty, value); }
		}

		public static readonly DependencyProperty AssignedBlockProperty =
			DependencyProperty.Register("AssignedBlock", typeof(Block), typeof(BlockDisplay), new PropertyMetadata(null, OnAssignedBlockPropertyChanged));

		/// <summary>
		/// Initializes the control.
		/// </summary>
		public BlockDisplay()
		{
			this.InitializeComponent();

			//TODO: this is for tutorial boards only, and this is hack, resolve it in next version
			if (GameManager.GameData != null)
				this.Brush = GameManager.GameData.GetTool(PredefinedTool.Brush);
		}

		/// <summary>
		/// Selects block's color as brush data.
		/// </summary>
		public void SelectColor()
		{
			Logger.Trace("Selecting color: {0}.", this._AssignedBlock.Color);
			this.Brush.Data = this._AssignedBlock.Color;
			GameManager.GameData.SelectedTool = this.Brush;
		}

		/// <summary>
		/// Inverses block state(if applicable).
		/// </summary>
		public void InverseState()
		{
			if (GameManager.Game.Mode.IsManualBlockUpdateEnabled)
			{
				Logger.Trace("Inversing block state.");
				this._AssignedBlock.IsFinished = !this._AssignedBlock.IsFinished;
			}
		}

		private static void OnAssignedBlockPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var bb = (BlockDisplay)sender;
			Validate.Debug(() => e.NewValue, v => v.NotNull());
			Validate.Debug(() => bb._AssignedBlock, v => v.Null());

			bb._AssignedBlock = (Block)e.NewValue;
			bb.AssignBlock();
		}

		private void AssignBlock()
		{
			this._AssignedBlock.PropertyChanged += this.OnBlockPropertyChanged;

			var brush = new SolidColorBrush(this._AssignedBlock.Color);
			this.Count.Text = this._AssignedBlock.Count.ToString();
			this.Count.Foreground = brush;
			this.FinishedMark.Stroke = brush;

			this.Count.FontSize = UISettings.CalculateBoardTextSize(this._AssignedBlock.Count);

			this.UpdateState();
		}

		private void UpdateState()
		{
			VisualStateManager.GoToState(this, this._AssignedBlock.IsFinished ? "Finished" : "NotFinished", true);
		}

		private void OnBlockPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsFinished")
				this.UpdateState();
		}
	}
}
