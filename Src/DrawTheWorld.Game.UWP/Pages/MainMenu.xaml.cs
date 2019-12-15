using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UI;
using DrawTheWorld.Game.Controls;
using DrawTheWorld.Game.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Main menu page.
	/// </summary>
	sealed partial class MainMenu
		: Utilities.UIPage
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.Pages.MainMenu");
		
		private readonly IUIManager UIManager = null;
		private readonly IFeatureProvider FeatureProvider = null;
		private readonly Notifications Notifications = null;
        private readonly SettingsAndAccount appSettings;
        private readonly AppInfo appInfo;

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="uiManager"></param>
        /// <param name="featureProvider"></param>
        public MainMenu(IUIManager uiManager, IFeatureProvider featureProvider, Notifications notifications,  SettingsAndAccount appSettings, AppInfo appInfo)
        {
            this.UIManager = uiManager;
            this.FeatureProvider = featureProvider;
            this.Notifications = notifications;

            this.InitializeComponent();

            this.PageBackground = new ImageBrush
            {
                ImageSource = Utilities.GameBackgroundChooser.SelectedBackground,
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
            this.appSettings = appSettings;
            this.appInfo = appInfo;
        }

        private void GoToGameList(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Navigating to the game list.");
			this.UIManager.NavigateTo<Pages.GameList>();
		}

		private async void GoToDesignerList(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Navigating to the designer list.");
			if (await this.FeatureProvider.TestForFeature(f => f.RequestFeature(Feature.Designer), this.Notifications, Logger))
				this.UIManager.NavigateTo<Pages.DesignerList>();
		}

		private void GoToStore(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Navigating to the store.");
			this.UIManager.NavigateTo<Pages.StorePage>();
		}

		private void GoToTutorial(object sender, RoutedEventArgs e)
		{
			Logger.Trace("Navigating to the tutorial.");
			this.UIManager.NavigateTo<Pages.TutorialPage>();
		}

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            appSettings.Open();
        }

        private void OpenInfo(object sender, RoutedEventArgs e)
        {
            appInfo.Open();
        }
    }
}
