using System;
using System.Linq;
using DrawTheWorld.Core.Platform;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Displays information about the app in the settings pane.
	/// </summary>
	sealed partial class AppInfo
		: Grid
	{
		private const string GameModeResourceKey = "Controls_AppInfo_GameModes_";

		/// <summary>
		/// Initializes the control.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="partsProvider"></param>
		public AppInfo(ISettingsProvider settings, IPartsProvider partsProvider)
		{
			this.InitializeComponent();

            var lst = from m in partsProvider.AvailableGameModes
					  let name = m.GetType().Name
					  let key = GameModeResourceKey + name + "_"
					  select new { Title = Strings.Get(key + "Title"), Description = Strings.Get(key + "Description") };
			this.ModesHelp.DataContext = lst.ToList();
		}

		public void Open()
		{
            this.Main.IsOpen = true;
		}

		private void OpenHelp(object sender, Windows.UI.Xaml.RoutedEventArgs args)
		{
			this.Help.IsOpen = true;
            this.Main.IsOpen = false;
		}

		private void OpenPackExportGuidelines(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			this.Help.IsOpen = false;
			this.BoardExportGuidelinesHelp.IsOpen = true;
            this.Main.IsOpen = false;
		}

		private void OpenImageImportHelp(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			this.Help.IsOpen = false;
			this.ImageImportHelp.IsOpen = true;
            this.Main.IsOpen = false;
		}

		private void OpenModesHelp(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			this.Help.IsOpen = false;
			this.ModesHelp.IsOpen = true;
            this.Main.IsOpen = false;
		}

		private void OpenTipsHelp(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			this.Help.IsOpen = false;
			this.TipsHelp.IsOpen = true;
            this.Main.IsOpen = false;
		}

		private void OpenCredits(object sender, Windows.UI.Xaml.RoutedEventArgs args)
		{
			this.Credits.IsOpen = true;
            this.Main.IsOpen = false;
		}

		private void OpenOFL(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
            this.Main.IsOpen = false;
			this.Credits.IsOpen = false;
			this.OFLLicense.IsOpen = true;
		}

        private void CloseMainSettings(FLib.UI.Controls.SettingsPanel sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            this.Main.IsOpen = false;
        }

        private async void ContactUsClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
                new Uri("mailto:?to=kuba@codinginfinity.me&subject=" + Strings.Get("Controls_AppInfo_ContactUs_EmailSubject")) );
        }

        private void BackToMainSettings(FLib.UI.Controls.SettingsPanel sender, Windows.UI.Xaml.RoutedEventArgs args)
        {
            //sender.IsOpen = false;
            this.Main.IsOpen = true;
        }
    }
}
