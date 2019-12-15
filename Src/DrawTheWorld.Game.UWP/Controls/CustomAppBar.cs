using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// AppBar that does not allow to open when application is in <see cref="ApplicationViewState.Snapped"/> state.
	/// </summary>
	class CustomAppBar
		: AppBar
	{
		protected override void OnOpened(object e)
		{
			if (ApplicationView.Value == ApplicationViewState.Snapped)
				this.IsOpen = false;
			else
				base.OnOpened(e);
		}
	}
}
