using System;
using System.ComponentModel;
using FLib;
using Windows.UI.Xaml;

namespace DrawTheWorld.Game
{
	/// <summary>
	/// Settings of the UI that can be changed at runtime.
	/// </summary>
	public sealed class UISettings
		: INotifyPropertyChanged
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.UISettings");

		private double DefaultFieldSize = 0;
		private double BlockMargin = 0;
		private double DescriptionsNumber = 0;

		private double BoardLineEntryTextSizeVariant1 = 0;
		private double BoardLineEntryTextSizeVariant2 = 0;
		private double BoardLineEntryTextSizeVariant3 = 0;

		private double _Factor = 0;

		/// <summary>
		/// Gets or sets the factor which is used to calculate properties.
		/// </summary>
		public double Factor
		{
			get { return this._Factor; }
			set
			{
				if (Math.Abs(value - this._Factor) > 0.01)
					this.RecalculateProperties(value);
			}
		}

		/// <summary>
		/// Gets the field size.
		/// </summary>
		public double FieldSize { get; private set; }

		/// <summary>
		/// Gets the size of "block" for current <see cref="FieldSize"/>.
		/// </summary>
		public double BlockSize { get; private set; }

		/// <summary>
		/// Gets the minimum description size for current <see cref="FieldSize"/>.
		/// </summary>
		public double MinDescriptionSize { get; private set; }

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raised when all setings are updated.
		/// </summary>
		public event EventHandler<object> SettingsUpdated;

		/// <summary>
		/// Initializes the settings loading default values from app resources.
		/// </summary>
		public void Initialize()
		{
			var appRes = Application.Current.Resources;

			this._Factor = 1;
			this.DefaultFieldSize = (double)appRes[this.NameOf(() => this.DefaultFieldSize)];
			this.BlockMargin = (double)appRes[this.NameOf(() => this.BlockMargin)];
			this.DescriptionsNumber = (double)appRes[this.NameOf(() => this.DescriptionsNumber)];

			this.BoardLineEntryTextSizeVariant1 = (double)appRes[this.NameOf(() => this.BoardLineEntryTextSizeVariant1)];
			this.BoardLineEntryTextSizeVariant2 = (double)appRes[this.NameOf(() => this.BoardLineEntryTextSizeVariant2)];
			this.BoardLineEntryTextSizeVariant3 = (double)appRes[this.NameOf(() => this.BoardLineEntryTextSizeVariant3)];

			this.RecalculateProperties(1);
		}

		/// <summary>
		/// Calculates size of the number that will be used in row/column descriptions.
		/// </summary>
		/// <param name="cnt"></param>
		/// <returns></returns>
		public double CalculateBoardTextSize(int cnt)
		{
			double baseSize = BoardLineEntryTextSizeVariant1;
			if (cnt >= 100)
				baseSize = BoardLineEntryTextSizeVariant3;
			else if (cnt >= 20)
				baseSize = BoardLineEntryTextSizeVariant2;

			return baseSize * this.Factor;
		}

		private void RecalculateProperties(double factor)
		{
			this._Factor = factor;
			this.FieldSize = Math.Round(this.DefaultFieldSize * factor);
			this.BlockSize = this.FieldSize - this.BlockMargin;
			this.MinDescriptionSize = this.FieldSize * this.DescriptionsNumber;

			this.PropertyChanged.Raise(this, t => t.Factor);
			this.PropertyChanged.Raise(this, t => t.FieldSize);
			this.PropertyChanged.Raise(this, t => t.BlockSize);
			this.PropertyChanged.Raise(this, t => t.MinDescriptionSize);

			this.SettingsUpdated.Raise(this, null);

			Logger.Info("UISettings properties recalculated. Current values: ");
			Logger.Info("Factor: {0}, field: {1}, block: {2}, description: {3}", this.Factor, this.FieldSize, this.BlockSize, this.MinDescriptionSize);
		}
	}
}
