using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Animation;

namespace FLib.UI.Controls
{
	/// <summary>
	/// Message dialog that supports mulitple pages and animations between them.
	/// </summary>
	/// <remarks>
	/// <see cref="MessageDialog"/> suffers the same problem as <see cref="SettingsPanel"/>:
	/// 
	/// Controls in <see cref="MessageDialog.Items"/> cannot be found using <see cref="FrameworkElement.FindName"/> on dialog or dialog's parent because
	/// <see cref="MessageDialog"/> uses custom collection that is not attached to any element as a host and therefore name scope doesn't contain any
	/// element from <see cref="MessageDialog.Items"/>.
	/// </remarks>
	[TemplatePart(Name = ContainerControlPartName, Type = typeof(UIElement)),
	 TemplatePart(Name = PresenterPartName, Type = typeof(ContentPresenter))]
	[ContentProperty(Name = "Items")]
	public sealed class MessageDialog
		: Control
	{
		private const string ContainerControlPartName = "PART_Container";
		private const string PresenterPartName = "PART_Presenter";
		private static readonly Duration DefaultDuration = new Duration(TimeSpan.FromSeconds(0.2));

		private static readonly DependencyProperty _SelectedIndexProperty =
			DependencyProperty.Register("SelectedIndex", typeof(int), typeof(MessageDialog), new PropertyMetadata(0, OnSelectedIndexChanged));
		private static readonly DependencyProperty _IsOpenProperty =
			DependencyProperty.Register("IsOpen", typeof(bool), typeof(MessageDialog), new PropertyMetadata(false, OnIsOpenChanged));

		private readonly ObservableCollection<FrameworkElement> _Items = new ObservableCollection<FrameworkElement>();

		private UIElement ContainerControl = null;
		private ContentPresenter Presenter = null;

		private readonly Storyboard FadeOut = new Storyboard();
		private readonly Storyboard FadeIn = new Storyboard();
		private readonly DoubleAnimation FadeOutAnimation = null;
		private readonly DoubleAnimation FadeInAnimation = null;

		private readonly Storyboard FadeElementOut = new Storyboard();
		private readonly Storyboard FadeElementIn = new Storyboard();
		private readonly Storyboard ResizePresenter = new Storyboard();
		private readonly DoubleAnimation FadeElementOutAnimation = null;
		private readonly DoubleAnimation FadeElementInAnimation = null;
		private readonly DoubleAnimation ResizePresenterAnimation = null;

		private readonly SemaphoreSlim AnimationLock = new SemaphoreSlim(1);
		private bool DoNotAnimate = false;

		/// <summary>
		/// Gets the collection of items that are being displayed in the 
		/// </summary>
		public IList<FrameworkElement> Items
		{
			get { return this._Items; }
		}

		/// <summary>
		/// Identifies the <see cref="SelectedIndex"/> property.
		/// </summary>
		public static DependencyProperty SelectedIndexProperty
		{
			get { return _SelectedIndexProperty; }
		}

		/// <summary>
		/// Gets or sets currently selected element's index from the <see cref="Items"/> collection.
		/// </summary>
		public int SelectedIndex
		{
			get { return (int)this.GetValue(_SelectedIndexProperty); }
			set { this.SetValue(_SelectedIndexProperty, value); }
		}

		/// <summary>
		/// Prevents the dialog from closing when background is clicked.
		/// </summary>
		public bool PreventClose { get; set; }

		/// <summary>
		/// Gets or sets the duration of all animations.
		/// Defaults to 0.2s.
		/// </summary>
		public Duration AnimationDuration
		{
			get { return this.FadeOutAnimation.Duration; }
			set
			{
				this.FadeOutAnimation.Duration = value;
				this.FadeInAnimation.Duration = value;
				this.FadeElementOutAnimation.Duration = value;
				this.FadeElementInAnimation.Duration = value;
				this.ResizePresenterAnimation.Duration = value;
			}
		}

		/// <summary>
		/// Identifies the <see cref="IsOpen"/> property.
		/// </summary>
		public static DependencyProperty IsOpenProperty
		{
			get { return _IsOpenProperty; }
		}

		/// <summary>
		/// Gets or sets whether the dialog is currently displayed on the screen.
		/// </summary>
		public bool IsOpen
		{
			get { return (bool)this.GetValue(_IsOpenProperty); }
			set { this.SetValue(_IsOpenProperty, value); }
		}

		/// <summary>
		/// Fires when the IsOpen property is set to false.
		/// </summary>
		public event EventHandler<object> Closed;

		/// <summary>
		/// Fires when the IsOpen property is set to true.
		/// </summary>
		public event EventHandler<object> Opened;

		/// <summary>
		/// Initializes the dialog.
		/// </summary>
		public MessageDialog()
		{
			this.DefaultStyleKey = typeof(MessageDialog);

			this.Visibility = Visibility.Collapsed;
			this.Opacity = 0;

			this._Items.CollectionChanged += this.OnItemsCollectionChanged;

			this.FadeOutAnimation = new DoubleAnimation { Duration = DefaultDuration, To = 0 };
			this.FadeInAnimation = new DoubleAnimation { Duration = DefaultDuration, To = 1 };
			Storyboard.SetTargetProperty(this.FadeOutAnimation, "Opacity");
			Storyboard.SetTargetProperty(this.FadeInAnimation, "Opacity");
			Storyboard.SetTarget(this.FadeOutAnimation, this);
			Storyboard.SetTarget(this.FadeInAnimation, this);

			this.FadeOut.Children.Add(this.FadeOutAnimation);
			this.FadeIn.Children.Add(this.FadeInAnimation);

			this.FadeElementOutAnimation = new DoubleAnimation { Duration = DefaultDuration, From = 1, To = 0 };
			this.FadeElementInAnimation = new DoubleAnimation { Duration = DefaultDuration, From = 0, To = 1 };
			this.ResizePresenterAnimation = new DoubleAnimation { Duration = DefaultDuration, EnableDependentAnimation = true };

			Storyboard.SetTargetProperty(this.FadeElementOutAnimation, "Opacity");
			Storyboard.SetTargetProperty(this.FadeElementInAnimation, "Opacity");
			Storyboard.SetTargetProperty(this.ResizePresenterAnimation, "Height");

			this.FadeElementIn.Children.Add(this.FadeElementInAnimation);
			this.FadeElementOut.Children.Add(this.FadeElementOutAnimation);
			this.ResizePresenter.Children.Add(this.ResizePresenterAnimation);
		}

		/// <summary>
		/// Selects specified index returns <see cref="IAsyncAction"/> that completes when new element is displayed.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IAsyncAction SelectIndexAsync(int index)
		{
			this.DoNotAnimate = true;
			this.SelectedIndex = index;
			this.DoNotAnimate = false;
			return this.SelectIndex(index, null).AsAsyncAction();
		}

		/// <summary>
		/// Opens the dialog and returns <see cref="IAsyncAction"/> that completes when open animation completes.
		/// </summary>
		/// <returns></returns>
		public IAsyncAction OpenAsync()
		{
			return this.OpenAsyncInternal().AsAsyncAction();
		}

		/// <summary>
		/// Opens the dialog.
		/// </summary>
		public void Open()
		{
			var ignored = this.OpenAsync();
		}

		/// <summary>
		/// Closes the dialog and returns <see cref="IAsyncAction"/> that completes when close animation completes.
		/// </summary>
		/// <returns></returns>
		public IAsyncAction CloseAsync()
		{
			return this.CloseAsyncInternal().AsAsyncAction();
		}

		/// <summary>
		/// Closes the dialog.
		/// </summary>
		public void Close()
		{
			var ignored = this.CloseAsync();
		}

		/// <inheritdoc />
		protected override void OnApplyTemplate()
		{
			if (this.ContainerControl != null)
				this.ContainerControl.PointerPressed -= this.SuppressClose;
			this.ContainerControl = this.GetTemplateChild(ContainerControlPartName) as UIElement;
			this.Presenter = this.GetTemplateChild(PresenterPartName) as ContentPresenter;

			if (this.Presenter != null)
			{
				Storyboard.SetTarget(this.FadeElementOutAnimation, this.Presenter);
				Storyboard.SetTarget(this.FadeElementInAnimation, this.Presenter);
				Storyboard.SetTarget(this.ResizePresenterAnimation, this.Presenter);
			}
			this.SelectIndex(this.SelectedIndex, true);

			if (this.ContainerControl != null)
				this.ContainerControl.PointerPressed += this.SuppressClose;
		}

		/// <inheritdoc />
		protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			base.OnPointerPressed(e);
			if (!e.Handled && !this.PreventClose)
				this.Close();
		}

		private void SuppressClose(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{
			e.Handled = true;
		}

		private static void OnSelectedIndexChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			MessageDialog dlg = (MessageDialog)sender;
			if (!dlg.DoNotAnimate)
				dlg.SelectIndex((int)e.NewValue, null);
		}

		private static void OnIsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			MessageDialog dlg = (MessageDialog)sender;
			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;

			if (!oldValue && newValue && dlg.Visibility == Visibility.Collapsed)
				dlg.Open();
			else if (oldValue && !newValue && dlg.Visibility == Visibility.Visible)
				dlg.Close();
		}

		private void OnItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (this.SelectedIndex >= this._Items.Count)
				this.SelectedIndex = Math.Max(0, this._Items.Count - 1);
			else if (
				(e.OldStartingIndex != -1 && this.SelectedIndex >= e.OldStartingIndex && this.SelectedIndex <= e.OldStartingIndex + e.OldItems.Count) ||
				(e.NewStartingIndex != -1 && this.SelectedIndex >= e.NewStartingIndex && this.SelectedIndex <= e.NewStartingIndex + e.NewItems.Count) ||
				e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset
				)
				this.SelectIndex(this.SelectedIndex, null);
		}

		private Task SelectIndex(int idx, bool? disableAnimation)
		{
			if (!disableAnimation.HasValue)
				disableAnimation = this.Visibility == Visibility.Collapsed;

			if (idx > -1 && idx < this._Items.Count)
				return this.TransitionTo(this._Items.Count > 0 ? this._Items[idx] : null, disableAnimation.Value);
			else if (idx == 0 && this._Items.Count == 0)
				return this.TransitionTo(null, disableAnimation.Value);
			else
				throw new IndexOutOfRangeException("The value of SelectedIndex is out of range.");
		}

		private async Task TransitionTo(FrameworkElement newElement, bool disableAnimation)
		{
			if (this.Presenter == null)
				return;

			if (disableAnimation)
			{
				this.Presenter.Height = await this.SetAndMeasureElement(newElement);
			}
			else
			{
				await this.AnimationLock.WaitAsync();

				this.FadeElementOut.Begin();
				await WaitFor.Event(h => this.FadeElementOut.Completed += h, h => this.FadeElementOut.Completed -= h);

				double newHeight = await this.SetAndMeasureElement(newElement);
				this.ResizePresenterAnimation.To = newHeight;
				this.ResizePresenter.Begin();
				await WaitFor.Event(h => this.ResizePresenter.Completed += h, h => this.ResizePresenter.Completed -= h);
				this.Presenter.Height = newHeight;

				this.FadeElementIn.Begin();
				await WaitFor.Event(h => this.FadeElementIn.Completed += h, h => this.FadeElementIn.Completed -= h);

				this.AnimationLock.Release();
			}
		}

		private async Task<double> SetAndMeasureElement(FrameworkElement element)
		{
			if (element == null)
				return 0.0;

			double height = element.ActualHeight;
			if (height == 0.0 || double.IsNaN(height) || double.IsInfinity(height))
			{
				double oldHeight = this.Presenter.Height;
				this.Presenter.Height = double.NaN;
				this.Presenter.Content = element;

				SizeChangedEventHandler h2 = null;
				await WaitFor.Event<SizeChangedEventArgs>(h => { h2 = (s, e) => h(s, e); element.SizeChanged += h2; }, h => element.SizeChanged -= h2, null);
				height = element.ActualHeight;

				this.Presenter.Height = oldHeight;
			}
			else
				this.Presenter.Content = element;
			return height;
		}

		private async Task OpenAsyncInternal()
		{
			await this.AnimationLock.WaitAsync();
			if (this.Visibility == Visibility.Collapsed)
			{
				this.Visibility = Visibility.Visible;
				this.FadeIn.Begin();
				await WaitFor.Event<object>(h => this.FadeIn.Completed += h, h => this.FadeIn.Completed -= h, null);

				this.IsOpen = true;
				this.Opened.Raise(this, null);
			}
			this.AnimationLock.Release();
		}

		private async Task CloseAsyncInternal()
		{
			await this.AnimationLock.WaitAsync();
			if (this.Visibility == Visibility.Visible)
			{
				this.FadeOut.Begin();
				await WaitFor.Event<object>(h => this.FadeOut.Completed += h, h => this.FadeOut.Completed -= h, null);
				this.Visibility = Visibility.Collapsed;

				this.IsOpen = false;
				this.Closed.Raise(this, null);
			}
			this.AnimationLock.Release();
		}
	}
}
