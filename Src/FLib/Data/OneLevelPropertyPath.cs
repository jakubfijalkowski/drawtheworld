using System;
using System.ComponentModel;
using System.Reflection;
using FLib.Interfaces.Data;

namespace FLib.Data
{
	/// <summary>
	/// One-level property path - root object + field/property, should be faster than normal <see cref="PropertyPath"/>.
	/// </summary>
	/// <seealso cref="FLib.Interfaces.Data.IPropertyPath"/>
	public sealed class OneLevelPropertyPath
		: IPropertyPath
	{
		#region Private fields
		private object _Root = null;
		private MemberInfo Member = null;
		private Type _RootType = null;
		private string MemberName = string.Empty;
		#endregion

		#region IPropertyPath Members
		/// <inheritdoc />
		public MemberInfo TargetMember { get { return this.Member; } }

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

				if (this.Initialized && this._Root is INotifyPropertyChanged)
				{
					(this.Root as INotifyPropertyChanged).PropertyChanged -= this.OnValueChanged;
				}
				this._Root = value;
				if (this.Initialized && this._Root is INotifyPropertyChanged)
				{
					(this.Root as INotifyPropertyChanged).PropertyChanged += this.OnValueChanged;
				}
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
		public Type ValueType
		{
			get
			{
				return (this.Member is PropertyInfo ? (this.Member as PropertyInfo).PropertyType : (this.Member as FieldInfo).FieldType);
			}
		}

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
				return (this.Member is PropertyInfo ? (this.Member as PropertyInfo).GetValue(this.Root, null) : (this.Member as FieldInfo).GetValue(this.Root));
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
				if (this.Member is PropertyInfo)
				{
					(this.Member as PropertyInfo).SetValue(this.Root, value, null);
				}
				else
				{
					(this.Member as FieldInfo).SetValue(this.Root, value);
				}
			}
		}

		/// <inheritdoc />
		public bool Initialized { get; private set; }
		#endregion

		#region INotifyPropertyChanged Members
		/// <summary>
		/// Invoked when <see cref="Value"/> changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region ISupportInitialize Members
		/// <summary>
		/// Unused.
		/// </summary>
		public void BeginInit()
		{ }

		/// <summary>
		/// Initializes pbject.
		/// </summary>
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
			if (this.Member == null)
			{
				this.Member = this.RootType.GetProperty(this.MemberName);
				if (this.Member == null)
				{
					this.Member = this.RootType.GetField(this.MemberName);
					if (this.Member == null)
					{
						throw new ArgumentException("Cannot find property nor field named {0} in type {1}".FormatWith(this.MemberName, this.RootType.Name));
					}
				}
			}

			this.ValueConverter = Converters.Utilities.GetTypeConverter(this.Member);

			if (this.Root is INotifyPropertyChanged)
			{
				(this.Root as INotifyPropertyChanged).PropertyChanged += this.OnValueChanged;
			}

			this.Initialized = true;
		}
		#endregion

		#region Constructors
		#region One parameter
		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="name">Name of the field/property.</param>
		public OneLevelPropertyPath(string name)
		{
			Validate.All(() => name, v => v.NotNullAndNotWhiteSpace());
			this.MemberName = name;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="property">Property.</param>
		public OneLevelPropertyPath(PropertyInfo property)
		{
			Validate.All(() => property, v => v.NotNull());
			this.Member = property;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="field">Field.</param>
		public OneLevelPropertyPath(FieldInfo field)
		{
			Validate.All(() => field, v => v.NotNull());
			this.Member = field;
		}
		#endregion

		#region With object
		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="name">Name of the field/property.</param>
		/// <param name="root">Root object.</param>
		public OneLevelPropertyPath(string name, object root)
			: this(name)
		{
			this.Root = root;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="root">Root object.</param>
		public OneLevelPropertyPath(PropertyInfo property, object root)
			: this(property)
		{
			this.Root = root;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="field">Field.</param>
		/// <param name="root">Root object.</param>
		public OneLevelPropertyPath(FieldInfo field, object root)
			: this(field)
		{
			this.Root = root;
		}
		#endregion

		#region With type
		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="name">Name of the field/property.</param>
		/// <param name="rootType">Root type.</param>
		public OneLevelPropertyPath(string name, Type rootType)
			: this(name)
		{
			this.RootType = rootType;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="rootType">Root type.</param>
		public OneLevelPropertyPath(PropertyInfo property, Type rootType)
			: this(property)
		{
			this.RootType = rootType;
		}

		/// <summary>
		/// Initializes object.
		/// </summary>
		/// <param name="field">Field.</param>
		/// <param name="rootType">Root type.</param>
		public OneLevelPropertyPath(FieldInfo field, Type rootType)
			: this(field)
		{
			this.RootType = rootType;
		}
		#endregion
		#endregion

		#region Private members
		private void OnValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Member.Name && this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(this.Member.Name));
			}
		}
		#endregion

		#region IDisposable Members
		/// <inheritdoc />
		public void Dispose()
		{
			if (this.Root is INotifyPropertyChanged)
			{
				(this._Root as INotifyPropertyChanged).PropertyChanged -= this.OnValueChanged;
			}
			this._Root = null;
			this.Member = null;
			this._RootType = null;
			//CONSIDER: GC.SuppressFinalize(this);
		}
		#endregion
	}
}
