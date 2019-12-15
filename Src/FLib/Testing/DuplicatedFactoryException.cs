using System;

namespace FLib.Testing
{
	/// <summary>
	/// Factory was duplicated.
	/// </summary>
	public class DuplicatedFactoryException
		: Exception
	{
		/// <summary>
		/// Base type.
		/// </summary>
		public Type Type { get; private set; }

		/// <summary>
		/// Variation or empty.
		/// </summary>
		public string Variation { get; private set; }

		/// <summary>
		/// Initializes exception.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="variation"></param>
		/// <param name="message"></param>
		public DuplicatedFactoryException(Type type, string variation, string message = "")
			: base(message)
		{
			this.Type = type;
			this.Variation = variation;
		}
	}
}
