namespace DrawTheWorld.Core
{
	/// <summary>
	/// Point.
	/// </summary>
	public struct Point
	{
		/// <summary>
		/// Represents invalid point.
		/// </summary>
		public static readonly Point Invalid = new Point(true);

		/// <summary>
		/// X-coordinate.
		/// </summary>
		public int X;

		/// <summary>
		/// Y-coordinate.
		/// </summary>
		public int Y;

		/// <summary>
		/// Initializes point with specified coordinates.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public Point(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}

		private Point(bool invalid)
		{
			this.X = this.Y = -1;
		}

		/// <summary>
		/// Compares this object with specified.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is Point)
			{
				var b = (Point)obj;
				return this.X == b.X && this.Y == b.Y;
			}
			return false;
		}

		/// <summary>
		/// Gets hash code of current point.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		/// <summary>
		/// Compares two <see cref="Point"/>s for equality.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Point a, Point b)
		{
			return a.X == b.X && a.Y == b.Y;
		}

		/// <summary>
		/// Compares two <see cref="Point"/>s for inequality.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Point a, Point b)
		{
			return a.X != b.X || a.Y != b.Y;
		}
	}
}
