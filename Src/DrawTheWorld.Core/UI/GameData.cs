using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DrawTheWorld.Core.Platform;
using FLib;

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Game data that is used INSIDE game.
	/// </summary>
	public sealed class GameData
		:INotifyPropertyChanged
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.GameData");

		private readonly IGame Game = null;
		private readonly ToolData[] _Tools = null;
		private ToolData _SelectedTool = null;

		/// <summary>
		/// Currently selected tool.
		/// </summary>
		/// <remarks>
		/// "Single action tools" have to be handled outside <see cref="GameData"/>.
		/// </remarks>
		public ToolData SelectedTool
		{
			get { return this._SelectedTool; }
			set
			{
				Validate.Debug(() => value, v => v.That(t => t == null || this._Tools.Contains(t)));

				if (this._SelectedTool != value)
				{
					this._SelectedTool = value;
					this.PropertyChanged.Raise(this);
					Logger.Debug("Tool '{0}' selected.", this._SelectedTool != null ? this._SelectedTool.Tool.GetType().Name : "null");
				}
			}
		}

		/// <summary>
		/// Gets all tools with their data.
		/// </summary>
		public IReadOnlyList<ToolData> Tools
		{
			get { return this._Tools; }
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets one of the predefined tools from <see cref="Tools"/>(corresponds to current <see cref="IGameMode"/>).
		/// </summary>
		/// <param name="tool"></param>
		/// <returns></returns>
		public ToolData GetTool(PredefinedTool tool)
		{
			var predef = this.Game.Mode.GetTool(tool);
			return this.Tools.FirstOrDefault(t => t.Tool == predef);
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="game"></param>
		/// <param name="parts"></param>
		public GameData(IGame game, IPartsProvider parts)
		{
			Validate.Debug(() => game, v => v.NotNull());
			Validate.Debug(() => parts, v => v.NotNull());

			this.Game = game;
			
			this._Tools = game.Mode.AvailableTools
				.Select(t => new ToolData(t, parts.GetLook(t), parts.GetName(t), parts.GetBehavior(t)))
				.ToArray();

			this.SelectedTool = this.GetTool(PredefinedTool.Brush);
			if (this.SelectedTool != null)
				this.SelectedTool.Data = this.Game.Palette[0];
		}
	}
}
