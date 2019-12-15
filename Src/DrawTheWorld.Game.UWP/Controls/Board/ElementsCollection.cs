using System;
using System.Collections.Generic;
using FLib;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace DrawTheWorld.Game.Controls.Board
{
	/// <summary>
	/// Caches board elements.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	sealed class ElementsCollection<T>
		: Dictionary<Core.Point, T>
		where T : UIElement
	{
		public static readonly Duration AnimationDuration = new Duration(TimeSpan.FromSeconds(0.25));

		private readonly Panel Container = null;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="container"></param>
		public ElementsCollection(Panel container)
		{
			Validate.Debug(() => container, v => v.NotNull());
			this.Container = container;
		}

		/// <summary>
		/// Removes element with specified key from internal collection AND from the container.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(Core.Point key, bool animate)
		{
			Validate.Debug(() => key, v => v.NotEqual(Core.Point.Invalid));

			T element;
			if (base.TryGetValue(key, out element))
			{
				base.Remove(key);
				if (animate)
					this.UseStoryboard(element, 0, (s, e) => this.Container.Children.Remove(element));
				else
					this.Container.Children.Remove(element);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Adds element to the internal collection AND the container.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="element"></param>
		public void Add(Core.Point key, T element, bool animate)
		{
			Validate.Debug(() => key, v => v.NotEqual(Core.Point.Invalid));
			Validate.Debug(() => element, v => v.NotNull());

			base.Add(key, element);
			this.Container.Children.Add(element);
			if (animate)
			{
				element.Opacity = 0;
				this.UseStoryboard(element, 1, null);
			}
		}

		/// <summary>
		/// Clears internal collection AND container.
		/// </summary>
		public new void Clear()
		{
			base.Clear();
			this.Container.Children.Clear();
		}

		[Obsolete("Use the other overload(with animation).", true)]
		new public bool Remove(Core.Point key)
		{
			return false;
		}

		[Obsolete("Use the other overload(with animation).", true)]
		new public void Add(Core.Point key, T element)
		{ }

		private void UseStoryboard(T element, double opacity, EventHandler<object> completedAction)
		{
			Storyboard storyboard = new Storyboard();
			var animation = new DoubleAnimation
			{
				To = opacity,
				Duration = AnimationDuration
			};
			storyboard.Children.Add(animation);
			Storyboard.SetTargetProperty(animation, "Opacity");
			Storyboard.SetTarget(animation, element);

			if (completedAction != null)
				storyboard.Completed += completedAction;
			storyboard.Begin();
		}
	}
}
