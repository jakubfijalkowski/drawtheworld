using DrawTheWorld.Core.Platform;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace DrawTheWorld.Game.Pages
{
	/// <summary>
	/// Tutorial page.
	/// </summary>
	public sealed partial class TutorialPage
		: Utilities.UIPage
	{
		private readonly IUIManager UIManager = null;

		/// <summary>
		/// Initializes page.
		/// </summary>
		public TutorialPage(IUIManager uiManager)
		{
			this.InitializeComponent();

			this.UIManager = uiManager;
		}

		private void GoBack(object sender, RoutedEventArgs e)
		{
			this.UIManager.NavigateBack();
		}
	}
}
