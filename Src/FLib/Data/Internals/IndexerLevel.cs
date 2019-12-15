using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FLib.Data.Internals
{
	/// <summary>
	/// Single level(indexer) in <see cref="PropertyPath"/>.
	/// </summary>
	internal sealed class IndexerLevel
		: IPropertyLevel
	{
		#region Private fields
		private PropertyInfo Indexer = null;
		private object[] Indecies = null;
		#endregion

		#region IPropertyLevel Members
		/// <inheritdoc />
		public MemberInfo TargetMember { get { return this.Indexer; } }

		/// <inheritdoc />
		public int Level { get; private set; }

		/// <inheritdoc />
		public string Name { get; private set; }

		/// <inheritdoc />
		public Type Type { get { return this.Indexer.PropertyType; } }

		/// <inheritdoc />
		public object Value { get; private set; }

		/// <inheritdoc />
		public Action<int> ValueChanged { get; private set; }

		/// <inheritdoc />
		public void UpdateValue(object root)
		{
			this.Value = this.Indexer.GetValue(root, this.Indecies);
		}

		/// <inheritdoc />
		public void SetValue(object to, object value)
		{
			this.Indexer.SetValue(to, value, this.Indecies);
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
			return Converters.Utilities.GetTypeConverter(this.Indexer);
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes new object.
		/// </summary>
		/// <param name="rootType">Parent type..</param>
		/// <param name="indecies">Indecies to parse.</param>
		/// <param name="level">Level - internal.</param>
		/// <param name="valueChanged">Invoked when value changes.</param>
		public IndexerLevel(Type rootType, string indecies, int level, Action<int> valueChanged)
		{
			this.Level = level;
			this.ValueChanged = valueChanged;
			this.Indexer = rootType.GetProperty("Item");
			if (this.Indexer == null)
			{
				throw new ArgumentException("Cannot find indexer in type {0}".FormatWith(rootType.Name));
			}

			this.Name = "Item";
			this.ParseIndecies(indecies);
			foreach (var idx in this.Indecies)
			{
				this.Name += "." + idx.ToString();
			}
		}
		#endregion

		#region Private methods
		private void OnValueChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == this.Name || e.PropertyName == "Item")
			{
				this.ValueChanged(this.Level);
			}
		}

		private void ParseIndecies(string indecies)
		{
			var parameters = this.Indexer.GetIndexParameters();
			this.Indecies = new object[parameters.Length];

			//We need to handle string inside ' and "
			#region Parsing
			StringCollection s = new StringCollection();
			StringBuilder quoteBuilder = new StringBuilder();
			char isQuote = '\0';

			for (int i = 0; i < indecies.Length; ++i)
			{
				if (indecies[i] == '"' || indecies[i] == '\'') //We have to skip commas
				{
					if (indecies[i] == isQuote) //End of scope
					{
						quoteBuilder.Append(indecies.Substring(0, i));
						indecies = indecies.Remove(0, i + 1);
						i = -1;
						isQuote = '\0';
					}
					else if (isQuote == '\0') //New scope
					{
						isQuote = indecies[i];
						quoteBuilder.Append(indecies.Substring(0, i).Trim());
						indecies = indecies.Remove(0, i + 1);
						i = 0;
					}
				}
				else if (isQuote == '\0' && indecies[i] == ',')
				{
					s.Add(quoteBuilder.ToString() + indecies.Substring(0, i).Trim());
					indecies = indecies.Remove(0, i + 1);
					quoteBuilder.Clear();
					i = -1;
				}
			}
			s.Add(quoteBuilder.ToString() + indecies.Trim());
			#endregion

			for (int i = 0; i < parameters.Length; i++)
			{
				if (s.Count > i)
				{
					var converter = TypeDescriptor.GetConverter(parameters[i]);
					if (converter.CanConvertFrom(typeof(string)))
					{
						this.Indecies[i] = converter.ConvertFrom(s[i]);
					}
					else
					{
						this.Indecies[i] = Convert.ChangeType(s[i], parameters[i].ParameterType, CultureInfo.InvariantCulture);
					}
				}
				else if (parameters[i].DefaultValue != DBNull.Value)
				{
					this.Indecies[i] = parameters[i].DefaultValue;
				}
				else
				{
					throw new ArgumentException("Insufficient parameters");
				}
			}
		}
		#endregion
	}
}
