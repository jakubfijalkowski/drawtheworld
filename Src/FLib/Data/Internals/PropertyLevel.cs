using System;
using System.ComponentModel;
using System.Reflection;

namespace FLib.Data.Internals
{
	/// <summary>
	/// Single level(field/value) in PropertyPath.
	/// </summary>
	internal sealed class PropertyLevel
		: IPropertyLevel
	{
		#region Private fields
		private MemberInfo Member = null;
		#endregion

		#region IPropertyLevel Members
		/// <inheritdoc />
		public MemberInfo TargetMember { get { return this.Member; } }

		/// <inheritdoc />
		public int Level { get; private set; }

		/// <inheritdoc />
		public string Name { get { return this.Member.Name; } }

		/// <inheritdoc />
		public Type Type
		{
			get
			{
				return this.Member is PropertyInfo ?
					(this.Member as PropertyInfo).PropertyType :
					(this.Member as FieldInfo).FieldType;
			}
		}

		/// <inheritdoc />
		public object Value { get; private set; }

		/// <inheritdoc />
		public Action<int> ValueChanged { get; private set; }

		/// <inheritdoc />
		public void UpdateValue(object root)
		{
			if (this.Member is PropertyInfo)
			{
				this.Value = (this.Member as PropertyInfo).GetValue(root, null);
			}
			else
			{
				this.Value = (this.Member as FieldInfo).GetValue(root);
			}
		}

		/// <inheritdoc />
		public void SetValue(object to, object value)
		{
			if (this.Member is PropertyInfo)
			{
				(this.Member as PropertyInfo).SetValue(to, value, null);
			}
			else
			{
				(this.Member as FieldInfo).SetValue(to, value);
			}
			this.Value = value;
		}

		/// <inheritdoc />
		public void RegisterPropertyChanged(object root)
		{
			var npc = root as INotifyPropertyChanged;
			if (npc != null)
				npc.PropertyChanged += new PropertyChangedEventHandler(OnValueChanged);
		}

		/// <inheritdoc />
		public void UnregisterPropertyChanged(object root)
		{
			var npc = root as INotifyPropertyChanged;
			if (npc != null)
				npc.PropertyChanged -= new PropertyChangedEventHandler(OnValueChanged);
		}

		/// <inheritdoc />
		public TypeConverter GetTypeConverter()
		{
			return Converters.Utilities.GetTypeConverter(this.Member);
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes new object.
		/// </summary>
		/// <param name="rootType">Parent type.</param>
		/// <param name="name">Name.</param>
		/// <param name="level">Level - internal.</param>
		/// <param name="valueChanged">Invoked when value changes on this level.</param>
		public PropertyLevel(Type rootType, string name, int level, Action<int> valueChanged)
		{
			this.Level = level;
			this.ValueChanged = valueChanged;

			this.Member = rootType.GetProperty(name);
			if (this.Member == null)
			{
				this.Member = rootType.GetField(name);
				if (this.Member == null)
				{
					throw new ArgumentException("Cannot find property nor field named {0} in type {1}.".FormatWith(name, rootType.Name));
				}
			}
		}
		#endregion

		#region Private methods
		private void OnValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Name)
			{
				this.ValueChanged(this.Level);
			}
		}
		#endregion
	}
}
