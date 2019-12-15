using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using FLib.Interfaces.Data;

namespace FLib.Data
{
	/// <summary>
	/// Ścieżka do właściwości lub pola.
	/// </summary>
	/// <remarks>
	/// Po utworzeniu(po ustawieniu na wartości poprawne Path i Root) nie można zmieniać typu(i tylko jego) wartości głównej.
	/// Wspiera ścieżkę dostępu przez pola, właściwości oraz indeksery(z jednym parametrem typu int).
	/// </remarks>
	/// <seealso cref="FLib.Interfaces.Data.IPropertyPath"/>
	public sealed class PropertyPath
		: IPropertyPath
	{
		#region Private fields
		private object _Root = null;
		private Type _RootType = null;

		private List<Internals.IPropertyLevel> Levels = new List<Internals.IPropertyLevel>();
		#endregion

		#region IPropertyPath Members
		/// <inheritdoc />
		public MemberInfo TargetMember
		{
			get { return this.Levels[this.Levels.Count - 1].TargetMember; }
		}

		/// <inheritdoc />
		public string Path { get; private set; }

		/// <inheritdoc />
		public object Root
		{
			get { return this._Root; }
			set
			{
				if (this.RootType != null && !this.RootType.IsInstanceOfType(value))
				{
					throw new ArgumentException("Value must be of type {0}".FormatWith(this.RootType.Name), "value");
				}
				this.UnregisterListeners();
				this._Root = value;
				this.RegisterListeners();
				this.Evaluate();
				this.PropertyChanged.Raise(this, () => Value);
			}
		}

		/// <inheritdoc />
		public Type RootType
		{
			get { return this._RootType; }
			set
			{
				if (this.Initialized)
				{
					throw new NotSupportedException("Already initialized");
				}
				this._RootType = value;
			}
		}

		/// <inheritdoc />
		public Type ValueType { get; private set; }

		/// <inheritdoc />
		public TypeConverter ValueConverter { get; private set; }

		/// <inheritdoc />
		public object Value
		{
			get
			{
				if (!this.Initialized)
				{
					throw new InvalidOperationException("Initialize first");
				}
				return this.Levels[this.Levels.Count - 1].Value;
			}
			set
			{
				if (!this.Initialized)
				{
					throw new InvalidOperationException("Initialize first");
				}
				if (value != null && !this.ValueType.IsInstanceOfType(value) && this.ValueConverter.CanConvertFrom(value.GetType()))
				{
					value = this.ValueConverter.ConvertFrom(value);
				}
				this.Levels[this.Levels.Count - 1].SetValue(
					this.Levels.Count == 1 ? this.Root : this.Levels[this.Levels.Count - 2].Value
				, value);
			}
		}

		/// <inheritdoc />
		public bool Initialized { get; private set; }
		#endregion

		#region INotifyPropertyChanged Members
		/// <summary>
		/// Invoked when any value in path gets changed.
		/// </summary>
		public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region ISupportInitialize Members
		/// <summary>
		/// Unused.
		/// </summary>
		public void BeginInit()
		{ }

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <remarks>
		/// Sets RootType if it was not set previously.
		/// Parses path.
		/// Registers OnPropertyChanged events.
		/// Calculates value if it is possible.
		/// </remarks>
		public void EndInit()
		{
			if (this.RootType == null && this.Root != null)
			{
				this.RootType = this.Root.GetType();
			}
			else if (this.RootType == null)
			{
				throw new InvalidOperationException("RootType must be set.");
			}

			//Parsujemy ścieżkę
			var parts = this.Path.Split('.');
			var lastType = this.RootType;
			for (int i = 0, j = 0; i < parts.Length; ++i, ++j)
			{
				parts[i] = parts[i].Trim();
				int openBracket = parts[i].IndexOf('[');
				if (openBracket > -1)
				{
					int closeBracket = parts[i].IndexOf(']');
					if (closeBracket < openBracket)
					{
						throw new ArgumentException("Invalid format", "Path");
					}
					Internals.IPropertyLevel lvl = new Internals.PropertyLevel(lastType, parts[i].Substring(0, openBracket), j, this.ValueChanged);
					this.Levels.Add(lvl);
					lastType = this.HandleType(lvl, (i + 1) == parts.Length);
					++j;

					lvl = new Internals.IndexerLevel(lastType, parts[i].Substring(openBracket + 1, closeBracket - openBracket - 1), j, this.ValueChanged);
					this.Levels.Add(lvl);
					lastType = this.HandleType(lvl, (i + 1) == parts.Length);
				}
				else
				{
					var lvl = new Internals.PropertyLevel(lastType, parts[i], j, this.ValueChanged);
					this.Levels.Add(lvl);
					lastType = this.HandleType(lvl, (i + 1) == parts.Length);
				}
			}
			this.Evaluate();
			this.ValueType = this.Levels[this.Levels.Count - 1].Type;
			this.ValueConverter = this.Levels[this.Levels.Count - 1].GetTypeConverter();

			//Dodajemy zdarzenia PropertyChanged tam gdzie się da
			this.RegisterListeners();
			this.Initialized = true;
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="path">Path.</param>
		public PropertyPath(string path)
		{
			Validate.All(() => path, v => v.NotNullAndNotWhiteSpace());
			this.Path = path;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="rootType">Root type.</param>
		public PropertyPath(string path, Type rootType)
			: this(path)
		{
			Validate.All(() => path, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => rootType, v => v.NotNull());
			this.RootType = rootType;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="rootObject">Root object.</param>
		public PropertyPath(string path, object rootObject)
			: this(path)
		{
			Validate.All(() => path, v => v.NotNullAndNotWhiteSpace());
			Validate.All(() => rootObject, v => v.NotNull());
			this.RootType = rootObject.GetType();
			this.Root = rootObject;
		}
		#endregion

		#region Destructors
		/// <summary>
		/// Disposes.
		/// </summary>
		~PropertyPath()
		{
			this.Dispose();
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Some value inside path changed.
		/// </summary>
		/// <param name="idx"></param>
		private void ValueChanged(int idx)
		{
			//We need to move PropertyChanged events from old objects to new ones.

			object prevObject = (idx == 0 ? this.Root : this.Levels[idx].Value);
			for (int i = idx; i < this.Levels.Count; i++)
			{
				this.Levels[i].UnregisterPropertyChanged(prevObject);
				prevObject = this.Levels[i].Value;
			}
			this.Evaluate(idx);

			prevObject = (idx == 0 ? this.Root : this.Levels[idx].Value);
			for (int i = idx; i < this.Levels.Count; i++)
			{
				this.Levels[i].RegisterPropertyChanged(prevObject);
				prevObject = this.Levels[i].Value;
			}

			this.PropertyChanged.Raise(this, () => Value);
		}

		/// <summary>
		/// Evaluates values.
		/// </summary>
		/// <param name="startAt">Start element.</param>
		private void Evaluate(int startAt = 0)
		{
			object lastObj = (startAt == 0 ? this.Root : this.Levels[startAt - 1].Value);
			for (int i = startAt; i < this.Levels.Count; i++)
			{
				if (lastObj == null)
				{
					break;
				}
				this.Levels[i].UpdateValue(lastObj);
				lastObj = this.Levels[i].Value;
			}
		}

		/// <summary>
		/// Gets type from level.
		/// </summary>
		/// <param name="lvl"></param>
		/// <param name="isLast"></param>
		/// <returns></returns>
		private Type HandleType(Internals.IPropertyLevel lvl, bool isLast)
		{
			if (lvl.Type == typeof(object))
			{
				this.Evaluate();
				if (this.Levels[this.Levels.Count - 1].Value == null && !isLast)
				{
					throw new InvalidOperationException("Cannot evaluate full path");
				}
				else if (!isLast)
				{
					return this.Levels[this.Levels.Count - 1].Value.GetType();
				}
			}
			return lvl.Type;
		}

		private void RegisterListeners()
		{
			object lastObj = this.Root;
			foreach (var lvl in this.Levels)
			{
				lvl.RegisterPropertyChanged(lastObj);
				lastObj = lvl.Value;
			}
		}

		private void UnregisterListeners()
		{
			object lastObj = this.Root;
			foreach (var lvl in this.Levels)
			{
				lvl.UnregisterPropertyChanged(lastObj);
				lastObj = lvl.Value;
			}
		}
		#endregion

		#region IDisposable Members
		/// <inheritdoc />
		public void Dispose()
		{
			object prevObject = this.Root;
			for (int i = 0; i < this.Levels.Count; i++)
			{
				this.Levels[i].UnregisterPropertyChanged(prevObject);
				prevObject = this.Levels[i].Value;
			}
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
