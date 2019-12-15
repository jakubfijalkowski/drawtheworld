using System.Collections.Generic;
using System.ComponentModel;
using FLib;

#if WINRT
using Windows.UI;
#else
using System.Windows.Media;
#endif

namespace DrawTheWorld.Core
{
	/// <summary>
	/// Describes one block.
	/// Used in columns and rows descriptions.
	/// </summary>
	public sealed class Block
		: INotifyPropertyChanged
	{
		private readonly Color _Color = default(Color);
		private bool _IsFinished = false;

		/// <summary>
		/// Color of the block.
		/// </summary>
		public Color Color
		{
			get { return this._Color; }
		}

		/// <summary>
		/// Number of fields inside block.
		/// </summary>
		public int Count { get; internal set; }

		/// <summary>
		/// Indicates whether block is finished or not.
		/// </summary>
		public bool IsFinished
		{
			get { return this._IsFinished; }
			set
			{
				if (this._IsFinished != value)
				{
					this._IsFinished = value;
					this.PropertyChanged.Raise(this);
				}
			}
		}

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Available only in game, not in designer.
		/// Used to perform fast check, whether the block is finished or not.
		/// </summary>
		internal List<Field> AssignedFields = null;

		/// <summary>
		/// Initializes block.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="count"></param>
		public Block(Color color, int count)
		{
			this._Color = color;
			this.Count = count;
		}
	}
}
