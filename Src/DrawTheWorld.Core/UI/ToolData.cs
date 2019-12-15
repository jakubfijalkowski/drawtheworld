using System;
using System.ComponentModel;
using FLib;

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Tool data.
	/// </summary>
	public sealed class ToolData
		: INotifyPropertyChanged
	{
		private static readonly MetroLog.ILogger Logger = MetroLog.LogManagerFactory.DefaultLogManager.GetLogger("Core.ToolData");

		private readonly Uri _Look = null;
		private readonly ITool _Tool = null;
		private readonly string _Name = string.Empty;
		private readonly IToolBehavior _Behavior = null;
		private object _Data = null;

		/// <summary>
		/// Gets the actual <see cref="ITool"/>.
		/// </summary>
		public ITool Tool
		{
			get { return this._Tool; }
		}

		/// <summary>
		/// Gets the look address for this tool.
		/// </summary>
		public Uri Look
		{
			get { return this._Look; }
		}

		/// <summary>
		/// Gets the localized name.
		/// </summary>
		public string Name
		{
			get { return this._Name; }
		}

		/// <summary>
		/// Gets the <see cref="IToolBehavior"/> that this tool uses.
		/// </summary>
		public IToolBehavior Behavior
		{
			get { return this._Behavior; }
		}

		/// <summary>
		/// Gets or sets the data for the tool.
		/// </summary>
		public object Data
		{
			get { return this._Data; }
			set
			{
				if ((this._Data != null && !this._Data.Equals(value)) || (this._Data == null && value != null))
				{
					this._Data = value;
					this.PropertyChanged.Raise(this);
					Logger.Debug("Data for tool '{0}' changed to '{1}'.", this.Tool.GetType().Name, this.Data);
				}
			}
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="tool"></param>
		/// <param name="look"></param>
		/// <param name="name"></param>
		/// <param name="behavior"></param>
		public ToolData(ITool tool, Uri look, string name, IToolBehavior behavior)
		{
			Validate.Debug(() => tool, v => v.NotNull());
			Validate.Debug(() => look, v => v.NotNull());
			Validate.Debug(() => name, v => v.NotNullAndNotWhiteSpace());
			Validate.Debug(() => behavior, v => v.NotNull());

			this._Tool = tool;
			this._Look = look;
			this._Name = name;
			this._Behavior = behavior;
			Logger.Debug("Tool data for {0} created.", tool.GetType().Name);
		}
	}
}
