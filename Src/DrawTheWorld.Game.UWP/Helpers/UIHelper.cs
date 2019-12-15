using FLib;
using FLib.UI.Controls;
using Windows.Foundation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DrawTheWorld.Game.Helpers
{
	/// <summary>
	/// Contains methods related to UI but used in many places.
	/// </summary>
	static class UIHelper
	{
		/// <summary>
		/// Gets rect that contains the element, relative to screen.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="relativeTo"></param>
		/// <returns></returns>
		public static Rect GetElementRect(FrameworkElement element, UIElement relativeTo = null)
		{
			var point = element.TransformToVisual(relativeTo).TransformPoint(new Point());
			return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
		}

		/// <summary>
		/// After pressing back button on panel, shows Settings Charm and deregisters handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void OpenDefaultSettings(SettingsPanel sender, RoutedEventArgs args)
		{
			sender.BackButtonPressed -= OpenDefaultSettings;
		}

		/// <summary>
		/// Opens <see cref="SettingsPanel"/> and navigates to <see cref="SettingsPane"/> when it is closed.
		/// </summary>
		/// <param name="panel"></param>
		public static void OpenAndNavigateToDefault(this SettingsPanel panel)
		{
			panel.BackButtonPressed += OpenDefaultSettings;
			panel.IsOpen = true;
		}

		/// <summary>
		/// Shows zoomed in view and selects particular item in it even if the control is not loaded yet.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="isLoaded"></param>
		/// <param name="selection"></param>
		public static void SelectZoomedInElement(this SemanticZoom container, bool isLoaded, object selection)
		{
			Validate.Debug(() => container, v => v.NotNull().Nest(c => c.ZoomedInView).NotNull().IsOfType(typeof(Selector)));

			var zoomedIn = (Selector)container.ZoomedInView;
			container.IsZoomedInViewActive = true;

			//Hack to make things work before zoomed in control gets loaded.
			if (!isLoaded)
			{
				RoutedEventHandler bringIntoView = null;
				bringIntoView = (_, __) =>
				{
					zoomedIn.SelectedItem = selection;
					zoomedIn.Loaded -= bringIntoView;
				};
				zoomedIn.Loaded += bringIntoView;
			}
			else
			{
				zoomedIn.SelectedItem = selection;
			}
		}
	}
}
