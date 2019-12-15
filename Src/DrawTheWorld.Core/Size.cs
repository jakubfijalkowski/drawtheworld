namespace DrawTheWorld.Core
{
	/// <summary>
	/// Size.
	/// </summary>
	public struct Size
	{
		/// <summary>
		/// Represents empty size.
		/// </summary>
		public static readonly Size Empty = new Size { Width = -1, Height = -1 };

		private int _Width;
		private int _Height;

		/// <summary>
		/// Width.
		/// </summary>
		public int Width
		{
			get { return this._Width; }
			set { this._Width = value; }
		}

		/// <summary>
		/// Height.
		/// </summary>
		public int Height
		{
			get { return this._Height; }
			set { this._Height = value; }
		}
		
		/// <summary>
		/// Initializes the size with specified values.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public Size(int width, int height)
		{
			this._Width = width;
			this._Height = height;
		}

		/// <summary>
		/// Compares this object with specified.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is Size)
			{
				var b = (Size)obj;
				return this.Width == b.Width && this.Height == b.Height;
			}
			return false;
		}

		/// <summary>
		/// Gets hash code of current size.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.Width.GetHashCode() ^ this.Height.GetHashCode();
		}

		/// <summary>
		/// Compares two <see cref="Size"/>s for equality.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Size a, Size b)
		{
			return a.Width == b.Width && a.Height == b.Height;
		}

		/// <summary>
		/// Compares two <see cref="Size"/>s for inequality.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Size a, Size b)
		{
			return a.Width != b.Width || a.Height != b.Height;
		}
	}
}
