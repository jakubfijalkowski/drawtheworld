using System.Collections.Generic;
using Windows.UI.Xaml;

namespace FLib.UI.Utilities
{
    /// <summary>
    /// Utility to make settings 
    /// </summary>
    public static class StyleHelper
    {
        #region NestedStyles
        private static readonly DependencyProperty _NestedStylesProperty =
            DependencyProperty.RegisterAttached("NestedStyles", typeof(StylesCollection), typeof(StyleHelper), new PropertyMetadata(null, OnNestesStylesChanged));

        /// <summary>
        /// Allows specifying styles that should be attached to control's <see cref="FrameworkElement.Resources"/> as default ones.
        /// </summary>
        public static DependencyProperty NestedStylesProperty
        {
            get { return StyleHelper._NestedStylesProperty; }
        }

        /// <summary>
        /// Sets <see cref="NestedStylesProperty"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static StylesCollection GetNestedStyles(FrameworkElement sender)
        {
            return (StylesCollection)sender.GetValue(NestedStylesProperty);
        }

        /// <summary>
        /// Gets <see cref="NestedStylesProperty"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        public static void SetNestedStyles(FrameworkElement sender, StylesCollection value)
        {
            sender.SetValue(NestedStylesProperty, value);
        }

        private static void OnNestesStylesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (FrameworkElement)sender;
            var newVal = e.NewValue as StylesCollection;
            var oldVal = e.OldValue as StylesCollection;
            var appRes = Application.Current.Resources;
            if (oldVal != null)
            {
                foreach (var styleKey in oldVal)
                {
                    var style = appRes[styleKey] as Style;
                    ctrl.Resources.Remove(style.TargetType);
                }
            }

            if (newVal != null)
            {
                foreach (var styleKey in newVal)
                {
                    var style = appRes[styleKey] as Style;
                    ctrl.Resources.Add(style.TargetType,
                        new Style(style.TargetType)
                        {
                            BasedOn = style
                        });
                }
            }

        }
        #endregion
    }

    /// <summary>
    /// Collection of <see cref="Style"/>s.
    /// </summary>
    public sealed class StylesCollection : List<string> { }
}
