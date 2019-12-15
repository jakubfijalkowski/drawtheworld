using System;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Field inside the board.
	/// </summary>
	public sealed class Field
	{
		private readonly Color? _CorrectFill = null;
		private readonly Point _Location;

		private bool _Excluded = false;
		private Color? _Fill = null;

		/// <summary>
		/// Field is excluded.
		/// </summary>
		public bool Excluded
		{
			get { return this._Excluded; }
			set
			{
				if (value && this._Fill.HasValue)
					throw new InvalidOperationException();

				this._Excluded = value;
			}
		}

		/// <summary>
		/// Field is filled.
		/// </summary>
		public Color? Fill
		{
			get { return this._Fill; }
			set
			{
				if (value != null && this._Excluded)
					throw new InvalidOperationException();

				this._Fill = value;
			}
		}

		/// <summary>
		/// Correct value of <see cref="Fill"/>.
		/// </summary>
		public Color? CorrectFill
		{
			get { return this._CorrectFill; }
		}

		/// <summary>
		/// Gets location of the field inside the board.
		/// </summary>
		public Point Location
		{
			get { return this._Location; }
		}

		/// <summary>
		/// Counter.
		/// </summary>
		public int Counter { get; set; }

		/// <summary>
		/// Initializes empty field.
		/// </summary>
		/// <param name="correctFill">See <see cref="CorrectFill"/>.</param>
		/// <param name="location">Location.</param>
		public Field(Color? correctFill, Point location)
		{
			this._CorrectFill = correctFill;
			this._Location = location;
		}
	}
}
