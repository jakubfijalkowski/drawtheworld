using System;
using System.Collections.Generic;
using Autofac;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DrawTheWorld.Game.Utilities
{
	/// <summary>
	/// Custom navigation UI.
	/// </summary>
	public sealed class UIContainer
		: ContentControl
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.UIContainer");

		private readonly Dictionary<Type, WeakReference<UIPage>> Cache = new Dictionary<Type, WeakReference<UIPage>>();

        private readonly IComponentContext Container;
		private readonly Stack<UIPage> Breadcrumbs = new Stack<UIPage>();

		/// <summary>
		/// Returns current page.
		/// </summary>
		public UIPage CurrentPage
		{
			get { return this.Content as UIPage; }
		}

		/// <summary>
		/// Initializes the container.
		/// </summary>
		/// <param name="container"></param>
		public UIContainer(IComponentContext container)
		{
            Validate.Debug(() => container, v => v.NotNull());
            Container = container;

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Stretch;

			this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
			this.VerticalContentAlignment = VerticalAlignment.Stretch;
		}

		/// <summary>
		/// Navigates to specified page.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Navigate<T>()
			where T : class, IPage
		{
			Logger.Info("Navigating to {0}.", typeof(T).Name);
			
			var page = this.GetPage<T>();
			this.NavigateTo(page);
			this.Breadcrumbs.Push(page);
			return page as T;
		}

		/// <summary>
		/// Navigates back in the page list.
		/// </summary>
		/// <returns></returns>
		public UIPage NavigateBack()
		{
			Validate.Debug(() => this.Breadcrumbs, v => v.MinCount(2));

			this.Breadcrumbs.Pop();
			var page = this.Breadcrumbs.Peek();
			this.NavigateTo(page);
			return page;
		}

		private void NavigateTo(UIPage page)
		{
			var current = this.Content as UIPage;
			if (current != null)
				current.OnNavigatedFrom(page != null ? page.GetType() : null);
			if (page != null)
				page.OnNavigatedTo(current != null ? current.GetType() : null);
			this.Content = page;
		}

		private UIPage GetPage<T>()
			where T : class, IPage
		{
			WeakReference<UIPage> reference = null;
			if (!this.Cache.TryGetValue(typeof(T), out reference))
				this.Cache.Add(typeof(T), reference = new WeakReference<UIPage>(null));

			UIPage page = null;
			if (!reference.TryGetTarget(out page))
			{
				Logger.Debug("Page needs to be freshly created.");
				try
				{
                    page = this.Container.Resolve<T>() as UIPage;
                }
				catch (Exception ex)
				{
					Logger.Fatal("Cannot create page {0}.".FormatWith(typeof(T).Name), ex);
					throw;
				}
				reference.SetTarget(page);
			}
			else
				Logger.Debug("Page retrived from cache.");
			return page;
		}
	}

	/// <summary>
	/// Custom page for <see cref="UIContainer"/>.
	/// </summary>
	[TemplateVisualState(GroupName = "ViewStates", Name = "FullScreenLandscape")]
	[TemplateVisualState(GroupName = "ViewStates", Name = "Filled")]
	[TemplateVisualState(GroupName = "ViewStates", Name = "Snapped")]
	[TemplateVisualState(GroupName = "ViewStates", Name = "FullScreenPortrait")]
	public class UIPage
		: Page, IPage
	{
		/// <summary>
		/// Background of the page.
		/// </summary>
		public Brush PageBackground { get; set; }

		/// <summary>
		/// Initializes the page.
		/// </summary>
		public UIPage()
		{
			this.Loaded += (s, e) =>
			{
				Window.Current.SizeChanged += this.OnWindowSizeChanged;
				this.OnViewStateChanged(ApplicationView.Value);
			};
			this.Unloaded += (s, e) => Window.Current.SizeChanged -= this.OnWindowSizeChanged;
		}

		protected internal virtual void OnNavigatedTo(Type sourcePageType)
		{
		}

		protected internal virtual void OnNavigatedFrom(Type destinationPageType)
		{
		}

		protected internal virtual void OnViewStateChanged(ApplicationViewState state)
		{
			VisualStateManager.GoToState(this, state.ToString(), true);
		}

		private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
		{
			this.OnViewStateChanged(ApplicationView.Value);
		}
	}
}
