using System;
using System.Collections.Generic;
using System.Linq;
using DrawTheWorld.Core;
using DrawTheWorld.Core.Helpers;
using DrawTheWorld.Core.UserData;
using FLib;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DrawTheWorld.Game.Controls
{
	/// <summary>
	/// Special board control that can only show things.
	/// </summary>
	public sealed partial class TutorialBoard
		: UserControl
	{
		private IBoard _AssignedBoard = null;

		private readonly DispatcherTimer Timer = null;
		private int CurrentStep = -1;
		private TutorialData.Step[] AnimationSteps = null;

		/// <summary>
		/// Assigned board name to load from <see cref="TutorialData"/>.
		/// </summary>
		public string AssignedBoardName
		{
			get { return string.Empty; }
			set
			{
				Validate.Debug(() => this._AssignedBoard, v => v.Null());

				this._AssignedBoard = TutorialData.Boards[value];
				TutorialData.Animations.TryGetValue(value, out this.AnimationSteps);
			}
		}

		/// <summary>
		/// Initializes the control.
		/// </summary>
		public TutorialBoard()
		{
			this.InitializeComponent();

			this.Timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1)
			};
			this.Timer.Tick += this.PerformAnimationStep;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			if (this.AnimationSteps != null && this.Timer.IsEnabled)
			{
				this.Timer.Stop();

				for (int i = this.CurrentStep + 1; i < this.AnimationSteps.Length; ++i)
					this.PerformSingleStep(this.AnimationSteps[i], false);

				this.EndAnimation();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.BoardDisplay.ReloadBoard(this._AssignedBoard, true);
			this.AnimationControl.Visibility = this.AnimationSteps != null ? Visibility.Visible : Visibility.Collapsed;
		}

		private void PlayAnimation(object sender, RoutedEventArgs e)
		{
			if (this.AnimationSteps != null && !this.Timer.IsEnabled)
			{
				this.Timer.Start();
				this.AnimationControl.Visibility = Visibility.Collapsed;
			}
		}

		private void PerformAnimationStep(object sender, object e)
		{
			var step = this.AnimationSteps[++this.CurrentStep];

			this.PerformSingleStep(step);

			if (this.CurrentStep == this.AnimationSteps.Length - 1)
				this.EndAnimation();
		}

		private void PerformSingleStep(TutorialData.Step step, bool updateUI = true)
		{
			this._AssignedBoard.GetFields(step.Fields).ForEach(step.PerformAction);
			if (updateUI)
				this.BoardDisplay.UpdateFields(step.Fields);
		}

		private void EndAnimation()
		{
			this.Timer.Stop();
			this.CurrentStep = -1;
			this.AnimationControl.Visibility = Visibility.Visible;
		}
	}

	public static class TutorialData
	{
		public static readonly Dictionary<string, IBoard> Boards = new Dictionary<string, IBoard>();
		/// <summary>
		/// Last step in list should always restore board to initial state
		/// </summary>
		public static readonly Dictionary<string, Step[]> Animations = new Dictionary<string, Step[]>();

		static TutorialData()
		{
			#region TutorialPage1
			{
				var cA = Color.FromArgb(255, 120, 49, 1);
				var cB = Color.FromArgb(255, 255, 228, 123);
				var cC = Color.FromArgb(255, 66, 222, 0);
				var cD = Colors.Black;

				Boards.Add(
					"TutorialPage1",
					new TutorialBoard(
						new Size(5, 6),
						new Color?[5 * 6]
						{
							null, cA, cA, cA, null,
							null, cA, cB, cB, null,
							null, cB, cB, cB, null,
							cC, cC, cC, cC, cC,
							cB, cC, cC, cC, cB,
							null, cD, cD, cD, null
						},
						fillFields: false
						)
					);
			}
			#endregion

			#region TutorialPage2
			{
				Boards.Add(
					"TutorialPage2",
					new TutorialBoard(
						new Size(5, 4),
						new Color?[5 * 4]
						{
							Colors.Red, null, Colors.Green, null, Colors.Black,
							null, null, Colors.Red, Colors.Green, Colors.Black,
							Colors.Red, Colors.Green, null, Colors.Black, null,
							Colors.Goldenrod, null, Colors.Goldenrod, null, Colors.Goldenrod
						},
						hideColumns: true
						)
					);
			}
			#endregion

			#region ExcluderSample
			{
				var excluderSampleBoard = new TutorialBoard(
					new Size(2, 5),
					new Color?[2 * 5]
					{
						Colors.Black, null,
						Colors.Black, Colors.Black,
						Colors.Black, Colors.Black,
						Colors.Black, null,
						Colors.Black, null
					},
					fillFields: false);
				Boards.Add("ExcluderSample", excluderSampleBoard);

				Animations.Add(
					"ExcluderSample",
					new Step[]
					{
						new Step(f => f.Fill = f.CorrectFill, true, WholeColumn(excluderSampleBoard, 0).ToArray()),
						new Step(f => f.Excluded = true, true, new Point(1, 4), new Point(1, 3)),
						new Step(f => f.Excluded = true, true, new Point(1, 0)),
						new Step(f => f.Fill = f.CorrectFill, true, new Point(1, 1), new Point(1, 2)),
						new Step(f =>
							{
								f.Fill = null;
								f.Excluded = false;
							}, true, WholeBoard(excluderSampleBoard).ToArray())
				});
			}
			#endregion

			#region CounterSample
			{
				var counterSampleBoard = new TutorialBoard(
					new Size(2, 5),
					new Color?[2 * 5]
					{
						Colors.Black, null,
						Colors.Black, Colors.Black,
						Colors.Black, Colors.Black,
						Colors.Black, null,
						Colors.Black, null
					},
					fillFields: false);
				Boards.Add("CounterSample", counterSampleBoard);

				Animations.Add(
					"CounterSample",
					new Step[]
					{
						new Step(f => f.Counter = 1, false, WholeColumn(counterSampleBoard, 0).ToArray()),
						
						new Step(f => f.Counter = f.Location.X == 1 ? 1 : 2, false, new Point(0, 1), new Point(1, 1)),
						new Step(f => f.Fill = f.CorrectFill, true, new Point(0, 1)),
						
						new Step(f => f.Counter = f.Location.X == 1 ? 1 : 2, false, new Point(0, 2), new Point(1, 2)),
						new Step(f => f.Fill = f.CorrectFill, true, new Point(0, 2)),

						new Step(f =>
							{
								f.Counter = 0;
								f.Fill = null;
							}, true,
							new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3), new Point(0, 4), new Point(1, 1), new Point(1, 2))
					});
			}
			#endregion
		}

		private static IEnumerable<Point> WholeBoard(IBoard board)
		{
			for (int x = 0; x < board.BoardInfo.Size.Width; x++)
			{
				for (int y = 0; y < board.BoardInfo.Size.Height; y++)
					yield return new Point(x, y);
			}
		}

		private static IEnumerable<Point> WholeColumn(IBoard board, int x)
		{
			for (int y = 0; y < board.BoardInfo.Size.Height; y++)
				yield return new Point(x, y);
		}

		private sealed class TutorialBoard
			: IBoard
		{
			private readonly MutableBoardData _Description = null;
			private readonly Field[] _Fields = null;
			private readonly IList<Block>[] _Rows = null;
			private readonly IList<Block>[] _Columns = null;

			/// <inheritdoc />
			public IBoardInfoProvider BoardInfo
			{
				get { return this._Description; }
			}

			/// <inheritdoc />
			public Field[] Fields
			{
				get { return this._Fields; }
			}

			/// <inheritdoc />
			public IEnumerable<Block>[] Rows
			{
				get { return this._Rows; }
			}

			/// <inheritdoc />
			public IEnumerable<Block>[] Columns
			{
				get { return this._Columns; }
			}

			public TutorialBoard(Size size, Color?[] data, bool fillFields = true, bool hideColumns = false)
			{
				Validate.Debug(() => size, v => v.That(s => s.Width > 0 && s.Height > 0, "Size > 0"));
				Validate.Debug(() => data, v => v.NotNull().CountEquals(size.Width * size.Height));

				this._Description = new MutableBoardData(Guid.NewGuid());
				this._Description.Size = size;
				this._Description.Data = data;

				this._Fields = data.Select((c, i) => new Field(c, new Point(i % size.Width, i / size.Width)) { Fill = fillFields ? c : null }).ToArray();

				Core.Helpers.BoardDescriptionHelper.BuildDescriptions(this, out this._Rows, out this._Columns, false, () => new List<Block>());
				if (hideColumns)
				{
					this._Columns = new IList<Block>[size.Width];
					for (int i = 0; i < size.Width; i++)
						this._Columns[i] = new List<Block>();
				}
			}
		}

		public sealed class Step
		{
			public readonly Point[] Fields = null;
			public readonly Action<Field> PerformAction = null;
			public readonly bool UsesFullData = false;

			public Step(Action<Field> performAction, bool usesFullData, params Point[] fields)
			{
				Validate.Debug(() => performAction, v => v.NotNull());
				Validate.Debug(() => fields, v => v.NotNullAndNotEmpty());
				this.Fields = fields;
				this.PerformAction = performAction;
				this.UsesFullData = usesFullData;
			}
		}
	}
}
