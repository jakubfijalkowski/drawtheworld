using System.Collections.Generic;

namespace DrawTheWorld.Core.ToolBehaviors
{
	/// <summary>
	/// Base class for behaviors.
	/// </summary>
	public abstract class BehaviorBase
		: IToolBehavior
	{
		private readonly List<Point> _Fields = new List<Point>();

		/// <summary>
		/// Start field(first field selected by user).
		/// </summary>
		protected Point StartField { get; private set; }

		/// <summary>
		/// End field(last field selected by user).
		/// </summary>
		protected Point EndField { get; private set; }

		/// <inheritdoc />
		public IReadOnlyList<Point> Fields
		{
			get { return this._Fields; }
		}

		/// <inheritdoc />
		public void Start(Point first)
		{
			this.StartField = this.EndField = first;
			this.PrepareList();
		}

		/// <inheritdoc />
		public void Over(Point field)
		{
			if (this.EndField != field)
			{
				this.EndField = field;
				this.PrepareList();
			}
		}

		/// <inheritdoc />
		public void Finish()
		{
			this.StartField = this.EndField = Point.Invalid;

			this.PrepareList();
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public BehaviorBase()
		{
			this.StartField = this.EndField = Point.Invalid;
		}

		/// <summary>
		/// Prepares list of fields.
		/// </summary>
		/// <param name="list"></param>
		protected abstract void PrepareFields(List<Point> list);

		private void PrepareList()
		{
			this._Fields.Clear();

			if (this.StartField != Point.Invalid)
				this.PrepareFields(this._Fields);
		}
	}
}
