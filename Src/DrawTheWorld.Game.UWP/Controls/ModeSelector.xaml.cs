using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Attributes;
using DrawTheWorld.Core.Platform;
using FLib;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Mode selector displayed on top of the main UI.
	/// Can be used only by <see cref="Subsystems.UIManager"/>, because it needs to be integrated into the UI.
	/// </summary>
	sealed partial class ModeSelector
		: UserControl
	{
		private const string ModeNameResourceKey = "Controls_ModeSelector_Mode_";

		private readonly Utilities.SelectCommand<IGameMode> _SelectModeCommand = new Utilities.SelectCommand<IGameMode>();

		public Utilities.SelectCommand<IGameMode> SelectModeCommand
		{
			get { return this._SelectModeCommand; }
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="partsProvider"></param>
		public ModeSelector(IPartsProvider partsProvider)
		{
			Validate.Debug(() => partsProvider, v => v.NotNull());

			this.InitializeComponent();

			var modes = from m in partsProvider.AvailableGameModes
						let type = m.GetType()
						let name = Strings.Get(ModeNameResourceKey + type.Name)
						let priority = PriorityAttribute.GetPriority(type)
						orderby priority
						select new { Name = name, Instance = m };

            ((ItemsControl)((Border)this.Popup.Items[0]).Child).ItemsSource = modes.ToArray();
        }

		/// <summary>
		/// Selects mode.
		/// </summary>
		/// <returns></returns>
		public Task<IGameMode> SelectMode()
		{
			this.SelectModeCommand.SelectedItem = null;
            this.Popup.Open();
            return WaitFor.Event<IGameMode, object>(
                h => this.Popup.Closed += h, h => this.Popup.Closed -= h,
                () => this.SelectModeCommand.SelectedItem);
        }

		private void ClosePopup(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
            this.Popup.Close();
        }
	}
}
