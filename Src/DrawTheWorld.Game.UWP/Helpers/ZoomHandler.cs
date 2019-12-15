using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Helpers
{
	/// <summary>
	/// Wraps displaying zoom menu for changing <see cref="UISettings.Factor"/>.
	/// </summary>
	sealed class ZoomHandler
	{
		private readonly IUIManager UIManager = null;
		private readonly UISettings UISettings = null;
		private readonly AppBar AppBar = null;
		private readonly Controls.MainGame GameControl = null;

		private readonly PopupMenu ZoomMenu = new PopupMenu();

		/// <summary>
		/// Initializes the menu and assings it to existing control.
		/// </summary>
		public ZoomHandler(IUIManager uiManager, UISettings settings, ButtonBase showButton, AppBar appBar, Controls.MainGame gameControl)
		{
			this.UIManager = uiManager;
			this.UISettings = settings;
			this.AppBar = appBar;
			this.GameControl = gameControl;

			this.ZoomMenu.Commands.Add(new UICommand("1.0", this.ZoomBoard, 1.0));
			this.ZoomMenu.Commands.Add(new UICommand("0.9", this.ZoomBoard, 0.9));
			this.ZoomMenu.Commands.Add(new UICommand("0.8", this.ZoomBoard, 0.8));
			this.ZoomMenu.Commands.Add(new UICommand("0.7", this.ZoomBoard, 0.7));
			this.ZoomMenu.Commands.Add(new UICommand("0.6", this.ZoomBoard, 0.6));
			this.ZoomMenu.Commands.Add(new UICommand("0.5", this.ZoomBoard, 0.5));

			showButton.Click += this.ShowZoomMenu;
		}

		public void Reset()
		{
			this.UISettings.Factor = 1;
		}

		private void ShowZoomMenu(object sender, RoutedEventArgs e)
		{
			var ignored = this.ZoomMenu.ShowForSelectionAsync(UIHelper.GetElementRect((FrameworkElement)sender));
		}

		private async void ZoomBoard(IUICommand uiCommand)
		{
			this.AppBar.IsOpen = false;
			await UIManager.ShowLoading();
			await Task.Delay(100);
			this.UISettings.Factor = (double)uiCommand.Id;
			this.GameControl.ReloadBoard();
			this.UIManager.HideLoadingPopup();
		}
	}
}
