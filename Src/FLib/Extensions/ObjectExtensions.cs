using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace FLib
{
	/// <summary>
	/// Extensions of <see cref="System.Object"/>.
	/// </summary>
	/// <remarks>
	/// Based on http://blog.decarufel.net/2010/03/how-to-use-strongly-typed-name-with.html and http://www.minddriven.de/index.php/technology/dot-net/c-sharp/efficient-expression-values.
	/// </remarks>
	public static class ObjectExtensions
	{
		#region NameOf
		/// <summary>
		/// Gets name of the property inside lambda expression.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="K"></typeparam>
		/// <param name="target"></param>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static string NameOf<T, K>(this T target, Expression<Func<T, K>> expression)
		{
			MemberExpression body = null;
			body = expression.Body is UnaryExpression
				 ? (expression.Body as UnaryExpression).Operand as MemberExpression
				 : expression.Body as MemberExpression;
			if (body == null)
				throw new ArgumentException("Must be member expression.", "expression");

			return body.Member.Name;
		}

		/// <summary>
		/// Gets name of the property inside lambda expression.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="target"></param>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static string NameOf<T>(this object target, Expression<Func<T>> expression)
		{
			MemberExpression body = null;
			body = expression.Body is UnaryExpression
				 ? (expression.Body as UnaryExpression).Operand as MemberExpression
				 : expression.Body as MemberExpression;
			if (body == null)
				throw new ArgumentException("Must be member expression.", "expression");

			return body.Member.Name;
		}
		#endregion

		#region NameAndValueOf
		/// <summary>
		/// Gets the name(path) and the value of specified expression.
		/// </summary>
		/// <remarks>
		/// Supports only fields and properties, indexers and other expressions are not supported.
		/// Calls may be chained and there is no upper limit. Consider using <see cref="SimpleNameAndValueOf{T}(Expression{Func{T}})"/>
		/// when expression is consisted of at most two calls(obj or obj.Field/Property).
		/// 
		/// Samples:
		/// <code>
		/// string value = "string";
		/// NameAndValueOf(() => value); // ("value", "string")
		/// NameAndValueOf(() => value.Length); // ("value.Length", 6)
		/// 
		/// Exception obj = new Exception("msg");
		/// NameAndValueOf(() => obj.Message.Length); // ("obj.Message.Length", 3)
		/// </code>
		/// 
		/// Can be used like
		/// <code>value.NameAndValueOf(() => value)</code>
		/// using <see cref="NameAndValueOf{T}(object, Expression{Func{T}})"/>.
		/// </remarks>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <returns>Return tuple, where the first element is name(path) and the other one is retrived value.</returns>
		public static Tuple<string, T> NameAndValueOf<T>(Expression<Func<T>> expression)
		{
			if (!(expression.Body is MemberExpression))
				throw new ArgumentException("Expression must be field or property access.", "expression");

			Stack<MemberExpression> expressions = new Stack<MemberExpression>(2);
			expressions.Push((MemberExpression)expression.Body);
			string name = string.Empty;

			MemberExpression current = null;
			while ((current = expressions.Peek()).Expression is MemberExpression)
				expressions.Push((MemberExpression)current.Expression);

			if (current.Expression != null && !(current.Expression is ConstantExpression))
				throw new ArgumentException("Expression may not contain {0}.".FormatWith(current.Expression.GetType().Name), "expression");

			object value = current.Expression != null ? ((ConstantExpression)current.Expression).Value : null;
			while (expressions.Count > 0)
			{
				current = expressions.Pop();
				name += '.' + current.Member.Name;
				value = current.Member.GetValue(value);
			}
			return Tuple.Create(name.Remove(0, 1), (T)value);
		}

		/// <summary>
		/// Gets the name and the value of specified chained field or property access.
		/// </summary>
		/// <remarks>
		/// Supports only fields and properties, indexers and other expressions are not supported.
		/// Calls may be chained and there is no upper limit. Consider using <see cref="SimpleNameAndValueOf{K, T}(K, Expression{Func{K, T}})"/>
		/// when expression is simple field/property access(eg. root.Field/Property).
		/// Root object's name is ommitted, but the root object has to be used.
		/// 
		/// Samples:
		/// <code>
		/// string value = "string";
		/// NameAndValueOf(value, v => v.Length); // ("Length", 6)
		/// 
		/// Exception obj = new Exception("msg");
		/// NameAndValueOf(obj, o => o.Message.Length); // ("Message.Length", 3)
		/// </code>
		/// </remarks>
		/// <typeparam name="K">Root object's type.</typeparam>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="obj">Root object.</param>
		/// <param name="expression">Expression.</param>
		/// <returns>Return tuple, where the first element is name(path) and the other one is retrived value.</returns>
		public static Tuple<string, T> NameAndValueOf<K, T>(this K obj, Expression<Func<K, T>> expression)
		{
			if (!(expression.Body is MemberExpression))
				throw new ArgumentException("Expression must be field or property access.", "expression");

			Stack<MemberExpression> expressions = new Stack<MemberExpression>(2);
			expressions.Push((MemberExpression)expression.Body);
			string name = string.Empty;

			MemberExpression current = null;
			while ((current = expressions.Peek()).Expression is MemberExpression)
				expressions.Push((MemberExpression)current.Expression);

			if (!(current.Expression is ParameterExpression))
				throw new ArgumentException("Expression may not contain {0} and must use root object.".FormatWith(current.Expression.GetType().Name), "expression");

			object value = obj;
			while (expressions.Count > 0)
			{
				current = expressions.Pop();
				name += '.' + current.Member.Name;
				value = current.Member.GetValue(value);
			}
			return Tuple.Create(name.Remove(0, 1), (T)value);
		}
		#endregion

		#region SimpleNameAndValueOf
		/// <summary>
		/// Gets the name(path) and the value of specified simple expression.
		/// </summary>
		/// <remarks>
		/// Supports only fields and properties, indexers and other expressions are not supported.
		/// Calls may be chained to at most second level(eg. obj.Property, but not obj.Property.Field).
		/// If you want name and value of more complicated expression, use <see cref="NameAndValueOf{T}(Expression{Func{T}})"/>.
		/// 
		/// Samples:
		/// <code>
		/// string value = "string";
		/// SimpleNameAndValueOf(() => value); // ("value", "string")
		/// SimpleNameAndValueOf(() => value.Length); // ("value.Length", 6)
		/// 
		/// Exception obj = new Exception("msg");
		/// SimpleNameAndValueOf(() => obj.Message.Length); // Throws ArgumentException
		/// </code>
		/// </remarks>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="expression">Expression.</param>
		/// <returns>Return tuple, where the first element is name(path) and the other one is retrived value.</returns>
		public static Tuple<string, T> SimpleNameAndValueOf<T>(Expression<Func<T>> expression)
		{
			if (!(expression.Body is MemberExpression))
				throw new ArgumentException("Expression must be field or property access.", "expression");

			var body = (MemberExpression)expression.Body;
			string name = string.Empty;
			object value = null;

			if (body.Expression is MemberExpression)
			{
				var innerBody = (MemberExpression)body.Expression;
				if (!(innerBody.Expression is ConstantExpression))
					throw new ArgumentException("Expression may not contain {0}.".FormatWith(innerBody.Expression.GetType().Name), "expression");

				value = ((ConstantExpression)innerBody.Expression).Value;
				value = innerBody.Member.GetValue(value);
				name = innerBody.Member.Name + '.';
			}
			else if (body.Expression is ConstantExpression)
				value = ((ConstantExpression)body.Expression).Value;
			else if (body.Expression == null)
				value = null;
			else
				throw new ArgumentException("Expression may not contain {0}.".FormatWith(body.Expression.GetType().Name), "expression");

			name += body.Member.Name;
			value = body.Member.GetValue(value);
			return Tuple.Create(name, (T)value);
		}

		/// <summary>
		/// Gets the name and the value of specified field or property of specified object.
		/// </summary>
		/// <remarks>
		/// Supports only one level access to field or property(eg. s => s.Property).
		/// Root object's name is ommitted, but the root object has to be used.
		/// 
		/// Samples:
		/// <code>
		/// string s = "abc";
		/// SimpleNameAndValueOf(s, obj => obj.Length); // ("Length", 3)
		/// </code>
		/// </remarks>
		/// <typeparam name="K">Root object's type.</typeparam>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="obj">Root object</param>
		/// <param name="expression">Expression.</param>
		/// <returns>Return tuple, where the first element is name(path) and the other one is retrived value.</returns>
		public static Tuple<string, T> SimpleNameAndValueOf<K, T>(this K obj, Expression<Func<K, T>> expression)
		{
			var body = expression.Body as MemberExpression;
			if (body == null || !(body.Expression is ParameterExpression))
				throw new ArgumentException("Expression must be simple access to field or property that uses passed argument.", "expression");
			
			return Tuple.Create(body.Member.Name, (T)body.Member.GetValue(obj));
		}
		#endregion

		#region Shorthands
		/// <summary>
		/// Shorthand for <see cref="NameAndValueOf{T}(Expression{Func{T}})"/>.
		/// See <see cref="NameAndValueOf{T}(Expression{Func{T}})"/> for more details.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="this">Ignored.</param>
		/// <param name="expression">Expression.</param>
		/// <returns></returns>
		public static Tuple<string, T> NameAndValueOf<T>(this object @this, Expression<Func<T>> expression)
		{
			return NameAndValueOf(expression);
		}

		/// <summary>
		/// Shorthand for <see cref="SimpleNameAndValueOf{T}(Expression{Func{T}})"/>.
		/// See <see cref="SimpleNameAndValueOf{T}(Expression{Func{T}})"/> for more details.
		/// </summary>
		/// <typeparam name="T">Value type.</typeparam>
		/// <param name="this">Ignored.</param>
		/// <param name="expression">Expression.</param>
		/// <returns></returns>
		public static Tuple<string, T> SimpleNameAndValueOf<T>(this object @this, Expression<Func<T>> expression)
		{
			return SimpleNameAndValueOf(expression);
		}
		#endregion
	}
}
