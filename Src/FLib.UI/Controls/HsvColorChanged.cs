namespace FLib.UI.Controls
{
	/// <summary>
	/// Event args for <see cref="ColorCanvas.ColorChanged"/> and <see cref="SpectrumSlider.ColorChanged"/>.
	/// </summary>
	public sealed class HsvColorChanged
#if !WINRT
		: System.EventArgs
#endif
	{
		/// <summary>
		/// Old value.
		/// </summary>
		public HsvColor OldValue { get; set; }

		/// <summary>
		/// New value.
		/// </summary>
		public HsvColor NewValue { get; set; }

		/// <summary>
		/// Initializes event args.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		public HsvColorChanged(HsvColor oldValue, HsvColor newValue)
		{
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}
	}
}
