using System.Threading.Tasks;

namespace DrawTheWorld.Core.Platform
{
	/// <summary>
	/// Marker interface that indicates "pages".
	/// </summary>
	public interface IPage { }

	/// <summary>
	/// Manages UI - allows displaying particular pages.
	/// </summary>
	public interface IUIManager
	{
		/// <summary>
		/// Gets currently displayed page as <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T GetCurrentPage<T>()
			where T : class, IPage;

		/// <summary>
		/// Navigates to particular page.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T NavigateTo<T>()
			where T : class, IPage;

		/// <summary>
		/// Navigates back in the page list.
		/// </summary>
		/// <returns></returns>
		IPage NavigateBack();

		/// <summary>
		/// Shows loading popup.
		/// </summary>
		/// <remarks>
		/// <c>await</c> for returned value to ensure that the popup is displayed.
		/// </remarks>
		/// <returns></returns>
		Task ShowLoading();

		/// <summary>
		/// Hides loading popup.
		/// </summary>
		void HideLoadingPopup();

		/// <summary>
		/// Asks user to select game mode.
		/// </summary>
		/// <returns></returns>
		Task<IGameMode> SelectGameMode();

		/// <summary>
		/// Shows UI that allows to buy coins.
		/// </summary>
		/// <param name="showHint">If true, the ui should show information that this action is required.</param>
		/// <returns></returns>
		Task<bool> ShowCoinsUI(bool showHint);
	}
}
