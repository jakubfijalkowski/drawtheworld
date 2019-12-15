using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using FLib;
using DrawTheWorld.Core.UserData;

#if WINRT
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows.Media.Imaging;
#endif

namespace DrawTheWorld.Core.UI
{
	/// <summary>
	/// Combines <see cref="BoardData"/> and <see cref="BoardStatistics"/> into one class.
	/// </summary>
	public sealed class GameBoard
		: IHaveThumbnail, INotifyPropertyChanged
	{
		private readonly BoardData _Data = null;
		private readonly IBoardStatisticsList _Statistics = null;
		private BitmapSource _Thumbnail = null;

		/// <summary>
		/// Gets the data of the board.
		/// </summary>
		public BoardData Data
		{
			get { return this._Data; }
		}

		/// <summary>
		/// Gets all the statistics.
		/// </summary>
		public IBoardStatisticsList Statistics
		{
			get { return this._Statistics; }
		}

		/// <summary>
		/// Indicates if whether the board was correctly finished.
		/// </summary>
		public bool IsFinished { get; private set; }

		/// <summary>
		/// Negates <see cref="IsFinished"/>.
		/// </summary>
		public bool IsNotFinished
		{
			get { return !this.IsFinished; }
		}

		/// <summary>
		/// Gets the number of wins.
		/// </summary>
		public int Wins { get; private set; }

		/// <summary>
		/// Gets the number of lost.
		/// </summary>
		public int Lost { get; private set; }

		/// <summary>
		/// Gets the number of aborts.
		/// </summary>
		public int Aborts { get; private set; }

		/// <summary>
		/// Gets the best time or returns <see cref="TimeSpan.Zero"/> if the game <see cref="IsNotFinished"/>.
		/// </summary>
		public TimeSpan BestTime { get; private set; }

		/// <inheritdoc />
		public BitmapSource Thumbnail
		{
			get { return this._Thumbnail; }
			internal set
			{
				if (value != this._Thumbnail)
				{
					this._Thumbnail = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="stats"></param>
		public GameBoard(BoardData data, IBoardStatisticsList stats)
		{
			Validate.Debug(() => data, v => v.NotNull());
			Validate.Debug(() => stats, v => v.NotNull());

			this._Data = data;
			this._Statistics = stats;

			this._Statistics.CollectionChanged += this.OnStatisticsChanged;
			this.OnStatisticsChanged(null, null);
		}

		public void OnStatisticsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// We can assume that statistics can only be added
			
			this.Wins = this.Statistics.Count(s => s.Result == FinishReason.Correct);
			this.Lost = this.Statistics.Count(s => s.Result == FinishReason.TooMuchFine);
			this.Aborts = this.Statistics.Count(s => s.Result == FinishReason.User);

			this.IsFinished = this.Wins > 0;

			this.BestTime = this.IsFinished ?
				TimeSpan.FromSeconds(
					this.Statistics
						.Where(s => s.Result == FinishReason.Correct)
						.Select(s => s.Time)
						.OrderBy(s => s)
						.FirstOrDefault()
				)
				: TimeSpan.Zero;

			this.PropertyChanged.Raise(this, _ => _.Wins);
			this.PropertyChanged.Raise(this, _ => _.Lost);
			this.PropertyChanged.Raise(this, _ => _.Aborts);

			this.PropertyChanged.Raise(this, _ => _.IsFinished);
			this.PropertyChanged.Raise(this, _ => _.IsNotFinished);

			this.PropertyChanged.Raise(this, _ => _.BestTime);
		}
	}
}
