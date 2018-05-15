using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataPowerTools.Extensions.MiscUtil.Reflection;

namespace DataPowerTools.Extensions
{

    namespace MiscUtil.Reflection
    {
    }

    /// <summary>
    /// <see cref="object" /> extensions.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Copies all public, readable properties from the source object to the
        /// target. The target type does not have to have a parameterless constructor,
        /// as no new instance needs to be created.
        /// </summary>
        /// <remarks>Only the properties of the source and target types themselves
        /// are taken into account, regardless of the actual types of the arguments.</remarks>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="source">Source to copy properties from</param>
        /// <param name="strictMode">Whether properties are required to be one-to-one (will throw otherwise).</param>
        /// <param name="target">Target to copy properties to</param>
        /// <param name="propNames"></param>
        public static void CopyTo<TSource, TTarget>(this TSource source, TTarget target, string[] propNames, bool strictMode = false)
            where TSource : class
            where TTarget : class
        {
            new PropertyCopier<TSource, TTarget>(strictMode, propNames).Copy(source, target);
        }

        /// <summary>
        /// Copies all public, readable properties from the source object to the
        /// target. The target type does not have to have a parameterless constructor,
        /// as no new instance needs to be created.
        /// </summary>
        /// <remarks>Only the properties of the source and target types themselves
        /// are taken into account, regardless of the actual types of the arguments.</remarks>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="source">Source to copy properties from</param>
        /// <param name="strictMode">Whether properties are required to be one-to-one (will throw otherwise).</param>
        /// <param name="target">Target to copy properties to</param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        public static void CopyTo<TSource, TTarget>(this TSource source, TTarget target, bool ignoreNonStringReferenceTypes = true, bool strictMode = false)
            where TSource : class
            where TTarget : class
        {
            var fieldNames = typeof(TSource).GetColumnMemberNames(ignoreNonStringReferenceTypes);

            new PropertyCopier<TSource, TTarget>(strictMode, fieldNames).Copy(source, target);
        }
        
        /// <summary>
        /// Retrieves the name of a property referenced by a lambda expression.
        /// </summary>
        /// <typeparam name="TObject">The type of object containing the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="this">The object containing the property.</param>
        /// <param name="expression">A lambda expression selecting the property from the containing object.</param>
        /// <returns>The name of the property referenced by <paramref name="expression"/>.</returns>
        public static string GetPropertyName<TObject, TProperty>(this TObject @this, Expression<Func<TObject, TProperty>> expression)
        {
            // For more information on the technique used here, see these blog posts:
            //   http://themechanicalbride.blogspot.com/2007/03/symbols-on-steroids-in-c.html
            //   http://michaelsync.net/2009/04/09/silverlightwpf-implementing-propertychanged-with-expression-tree
            //   http://joshsmithonwpf.wordpress.com/2009/07/11/one-way-to-avoid-messy-propertychanged-event-handling/
            // Note that the following blog post:
            //   http://www.ingebrigtsen.info/post/2008/12/11/INotifyPropertyChanged-revisited.aspx
            // uses a similar technique, but must also account for implicit casts to object by checking for UnaryExpression.
            // Our solution uses generics, so this additional test is not necessary.
            return ((MemberExpression)expression.Body).Member.Name;
        }

        /// <summary>
        /// Reads an untyped property from an object.
        /// </summary>
        /// <param name="this">The object from which to read the property. May not be <c>null</c>.</param>
        /// <param name="name">The name of the property to read. This property must exist.</param>
        /// <returns>The value of the property for that object.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="this"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">The object <paramref name="this"/> does not have a property named <paramref name="name"/>.</exception>
        public static object GetProperty(this object @this, string name)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("@this", "Object passed to GetProperty cannot be null.");
            }

            var property = @this.GetType().GetProperty(name);
            if (property == null)
            {
                throw new KeyNotFoundException("The type " + @this.GetType().Name + " does not contain a property named " + name + ".");
            }

            return property.GetValue(@this, null);
        }

        /// <summary>
        /// Reads a property from an object.
        /// </summary>
        /// <typeparam name="T">The type of the property that is returned.</typeparam>
        /// <param name="this">The object from which to read the property. May not be <c>null</c>.</param>
        /// <param name="name">The name of the property to read. This property must exist.</param>
        /// <returns>The value of the property for that object.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="this"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">The object <paramref name="this"/> does not have a property named <paramref name="name"/>.</exception>
        /// <exception cref="InvalidCastException">The property was found, but is not of type <typeparamref name="T"/>.</exception>
        public static T GetProperty<T>(this object @this, string name)
        {
            var ret = @this.GetProperty(name);
            if (ret == null)
            {
                return default(T);
            }
            else if (ret is T)
            {
                return (T)ret;
            }
            else
            {
                throw new InvalidCastException("The type " + @this.GetType().Name + " does have a property named " + name + ", but it is of type " + ret.GetType().Name + ", not " + typeof(T).Name + ".");
            }
        }

        /// <summary>
        /// Converts the object to the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this object value)
        {
            return (T)value.ConvertTo(typeof(T));
        }

        /// <summary>Converts the given value to the given type. DbNull's will be converted to null.</summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="type">Type to convert the given value to.</param>
        /// <returns>Converted value.</returns>
        /// <exception cref="TypeConversionException">Thrown when an error occurs attempting to convert a value to an enum.</exception>
        /// <exception cref="TypeConversionException">Thrown when an error occurs attempting to convert a value to a type.</exception>
        public static object ConvertTo(this object value, Type type)
        {
            // Handle DBNull
            if (value == DBNull.Value)
                value = null;

            // Handle value type conversion of null to the values types default value
            if (value == null && type.IsValueType)
                return type.GetDefaultValue(); // Extension method internally handles caching

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            // Handle Enums
            if (underlyingType.IsEnum)
            {
                try
                {
                    // ReSharper disable once PossibleNullReferenceException // Because an enum and a nullable enum are both value types, it's actually not possible to reach the next line of code when the value variable is null
                    value = Enum.Parse(underlyingType, value.ToString(), true);
                }
                catch (Exception exception)
                {
                    throw new TypeConversionException(
                        $"An error occurred while attempting to convert the value '{value}' to an enum of type '{underlyingType}'", exception);
                }
            }

            try
            {
                // Handle Guids
                if (underlyingType == typeof(Guid))
                {
                    if (value is string)
                    {
                        value = new Guid(value as string);
                    }
                    if (value is byte[])
                    {
                        value = new Guid(value as byte[]);
                    }
                }

                var result = Convert.ChangeType(value, underlyingType);

                return result;
            }
            catch (Exception exception)
            {
                throw new TypeConversionException(
                    $"An error occurred while attempting to convert the value '{value}' to type '{underlyingType}'", exception);
            }
        }
        
        /// <summary>Converts the given object to an <see cref="int" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Int representation of the object.</returns>
        public static int ToInt(this object obj)
        {
            return ConvertTo<int>(obj);
        }

        /// <summary>Converts the given object to an <see cref="int" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Int representation of the object.</returns>
        public static int? ToNullableInt(this object obj)
        {
            return ConvertTo<int?>(obj);
        }

        /// <summary>Converts the given object to an <see cref="long" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Long representation of the object.</returns>
        public static long ToLong(this object obj)
        {
            return ConvertTo<long>(obj);
        }

        /// <summary>Converts the given object to an <see cref="long" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Long representation of the object.</returns>
        public static long? ToNullableLong(this object obj)
        {
            return ConvertTo<long?>(obj);
        }

        /// <summary>Converts the given object to an <see cref="double" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Double representation of the object.</returns>
        public static double ToDouble(this object obj)
        {
            return ConvertTo<double>(obj);
        }

        /// <summary>Converts the given object to an <see cref="double" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Double representation of the object.</returns>
        public static double? ToNullableDouble(this object obj)
        {
            return ConvertTo<double?>(obj);
        }

        /// <summary>Converts the given object to an <see cref="decimal" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Decimal representation of the object.</returns>
        public static decimal ToDecimal(this object obj)
        {
            return ConvertTo<decimal>(obj);
        }

        /// <summary>Converts the given object to an <see cref="decimal" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Decimal representation of the object.</returns>
        public static decimal? ToNullableDecimal(this object obj)
        {
            return ConvertTo<decimal?>(obj);
        }

        /// <summary>Converts the given object to an <see cref="DateTime" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>DateTime representation of the object.</returns>
        public static DateTime ToDateTime(this object obj)
        {
            return ConvertTo<DateTime>(obj);
        }

        /// <summary>Converts the given object to an <see cref="DateTime" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>DateTime representation of the object.</returns>
        public static DateTime? ToNullableDateTime(this object obj)
        {
            return ConvertTo<DateTime?>(obj);
        }

        /// <summary>Converts the given object to an <see cref="bool" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Bool representation of the object.</returns>
        public static bool ToBool(this object obj)
        {
            return ConvertTo<bool>(obj);
        }

        /// <summary>Converts the given object to an <see cref="bool" />.</summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Bool representation of the object.</returns>
        public static bool? ToNullableBool(this object obj)
        {
            return ConvertTo<bool?>(obj);
        }

        /// <summary>Indicates if the object is an anonymous type.</summary>
        /// <param name="obj">Object instance.</param>
        /// <returns>Returns true if the object is an anonymous type.</returns>
        public static bool IsAnonymousType(this object obj)
        {
            if (obj == null)
                return false;

            return obj.GetType().Namespace == null;
        }
    }

    /// <summary>Thrown when an exception occurs while converting a value from one type to another.</summary>
    [Serializable]
    public class TypeConversionException : Exception
    {
        /// <summary>Instantiates a new <see cref="TypeConversionException" /> with a specified error message.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner
        /// exception is specified.
        /// </param>
        public TypeConversionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}