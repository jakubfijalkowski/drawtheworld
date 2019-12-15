using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// <see cref="FlipView"/> that implements <see cref="ISemanticZoomInformation"/>.
	/// </summary>
	sealed class SemanticFlipView
		: FlipView, ISemanticZoomInformation
	{
		public bool IsActiveView { get; set; }

		public bool IsZoomedInView { get; set; }

		public SemanticZoom SemanticZoomOwner { get; set; }

		public void InitializeViewChange()
		{ }

		public void MakeVisible(SemanticZoomLocation item)
		{
			if (item.Item != null)
				this.SelectedItem = item.Item;
		}

		public void CompleteViewChange()
		{ }

		public void StartViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
		{
			source.Item = this.SelectedItem;
		}

		public void CompleteViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
		{ }

		public void StartViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
		{
			if (source.Item != null)
				this.SelectedItem = source.Item;
			else if (destination.Item != null)
				this.SelectedItem = source.Item;
		}

		public void CompleteViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
		{ }
	}
}
