using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.FastMember;
using DataPowerTools.PowerTools;

namespace DataPowerTools.Extensions
{
    //TODO: needs some cleanup
    
    /// <summary>
    /// Extensions for dealing with types and columns.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if this type uses reference equality (i.e., does not override <see cref="object.Equals(object)"/>); returns <c>false</c> if this type or any of its base types override <see cref="object.Equals(object)"/>. This method returns <c>false</c> for any interface type, and returns <c>true</c> for any reference-equatable base class even if a derived class is not reference-equatable; the best way to determine if an object uses reference equality is to pass the exact type of that object.
        /// </summary>
        /// <param name="type">The type to test for reference equality.</param>
        /// <returns>Returns <c>true</c> if this type uses reference equality (i.e., does not override <see cref="object.Equals(object)"/>); returns <c>false</c> if this type or any of its base types override <see cref="object.Equals(object)"/>.</returns>
        public static bool IsReferenceEquatable(this Type type)
        {
            // Only reference types can use reference equality.
            if (!type.IsClass || type.IsPointer)
            {
                return false;
            }

            // Find all methods called "Equals" defined in the type's hierarchy (except object.Equals), and retrieve the base definitions.
            var equalsMethods = from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                where method.Name == "Equals" && method.DeclaringType != typeof(object)
                select method.GetBaseDefinition();

            // Take those base definitions and check if any of them are object.Equals. If there are any, then we know that the type overrides
            //  object.Equals or derives from a type that overrides object.Equals.
            var objectEqualsMethod = equalsMethods.Any(method => method.DeclaringType == typeof(object));

            return !objectEqualsMethod;
        }

        /// <summary>
        /// Returns column-type information about a reflected property.
        /// </summary>
        /// <param name="columnPropertyInfo"></param>
        /// <returns></returns>
        public static ColumnDisplayInformation GetColumnFieldInfo(this PropertyInfo columnPropertyInfo)
        {
            var colType = columnPropertyInfo.PropertyType;
         
            ////if enum get root type
            //if (colType.IsEnum)
            //    colType = colType.GetEnumUnderlyingType();

            //use column attribute as the column name if available. Otherwise, use property name.
            var columnAttribute = columnPropertyInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType == typeof(ColumnAttribute));

            var displayColName = (columnAttribute?.ConstructorArguments?.Any() ?? false)
                ? columnAttribute.ConstructorArguments[0].Value?.ToString()
                : null;
            
            return new ColumnDisplayInformation
            {
                FieldType = colType,
                ColumnName = columnPropertyInfo.Name,
                DisplayName = displayColName,
                PropertyInfo = columnPropertyInfo
            };
        }

        /// <summary>
        /// Determines whether a type is anonymous.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType2(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }

        /// <summary>
        /// Gets the non-nullable type (used when dealing with DataTables which do not support nullable types)
        /// </summary>
        /// <param name="colFieldType"></param>
        /// <returns></returns>
        public static Type GetNonNullableType(this Type colFieldType)
        {
            if (colFieldType == null)
                return null;

            var colType = colFieldType;

            //if nullable type get root type
            if (colType.IsGenericType && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                colType = colType.GetGenericArguments()[0];

            return colType;
        }
        
        /// <summary>
        /// Returns all column-type information about a type, enumerating all properties (and properties only).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propNames">Property names to only include.</param>
        /// <returns></returns>
        public static ColumnDisplayInformation[] GetColumnInfo(this Type type, string[] propNames)
        {
            //TODO: ??

            if (!((propNames?.Length ?? 0) > 0))
            {
                throw new Exception("Property names list cannot be null.");
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var propNamesSet = new HashSet<string>(propNames);

            var props = GetColumnInfo(type, false);
            
            return props
                .Where(p => propNamesSet.Contains(p.ColumnName))
                .ToArray(); 
        }

        /// <summary>
        /// Returns all column-type information about a type, enumerating all properties (and properties only).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignoreNonStringReferenceTypes">Ignores</param>
        /// <returns></returns>
        public static ColumnDisplayInformation[] GetColumnInfo(this Type type, bool ignoreNonStringReferenceTypes = true)
        {
            var props = type.GetProperties()
                .Select(GetColumnFieldInfo)
                .Select((p, i) =>
                {
                    p.Ordinal = i;
                    return p;
                });

            if (ignoreNonStringReferenceTypes)
            {
                props = props.Where(p => !p.FieldType.IsClass && !p.FieldType.IsInterface || p.FieldType == typeof(string));
            }

            return props.ToArray();
        }

        /// <summary>
        /// Gets a list of the property names on the type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] GetPropertyNames(this Type type)
        {
            return type.GetProperties().Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Returns all column-type information about a type, enumerating all properties (and properties only).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static string[] GetColumnMemberNames(this Type type, bool ignoreNonStringReferenceTypes = true)
        {
            var fi = type.GetColumnInfo(ignoreNonStringReferenceTypes);
            return fi.Select(f => f.ColumnName).ToArray();
        }


        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc) 
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(
            this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new[]
                {
                    typeof(string),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                (Convert.GetTypeCode(type) != TypeCode.Object);
        }

        /// <summary>
        /// Returns whether item implements enumerable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        /// Gets underlying type. From https://stackoverflow.com/questions/506096/comparing-object-properties-in-c-sharp
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
        
        /// <summary>Gets the default value for the given type.</summary>
        /// <param name="type">Type to get the default value for.</param>
        /// <returns>Default value of the given type.</returns>
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Gets the underlying nullable type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Type GetNullableType(this Type t)
        {
            var returnType = t;
            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                returnType = Nullable.GetUnderlyingType(t);
            return returnType;
        }

        /// <summary>
        /// Returns whether this type is nullable (can have a null value).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type type)
        {
            return (type == typeof(string)) ||
                   type.IsArray ||
                   (type.IsGenericType &&
                    (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>
        /// Generates a CREATE TABLE statement from a type definition, where the 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="outputTableName">Output table name, default will be class name.</param>
        /// <returns></returns>
        public static string GenerateCreateTableScript(this Type t, string outputTableName = null)
        {
            return CreateTableSql.GenerateCreateTableScriptFromType(t, outputTableName);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>s
        public static IEnumerable<BasicDataFieldInfo> GetPropertyAndFieldInfo(this Type type)
        {
            var p = TypeAccessor.Create(type).GetMembers();

            foreach (var entry in p)
            {
                yield return new BasicDataFieldInfo
                {
                    ColumnName = entry.Name,
                    FieldType = entry.Type
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>s
        public static IEnumerable<string> GetFieldNames(this Type type)
        {
            var accessor = TypeAccessor.Create(type);
            return accessor.GetMembers().Select(p => p.Name);
        }

        public static TypeAccessor GetTypeAccessor(this Type type, bool allowNonPublicFieldAccess = false)
        {
            return TypeAccessor.Create(type, allowNonPublicFieldAccess);
        }

        ///// <summary>
        ///// Gets the types properties and field as a dictionary of lowercase member names and PropertyInfo or FieldInfo as the values.
        ///// </summary>
        //[Obsolete("This is garbage")]
        //private static Dictionary<string, object> GetPropertiesAndFields(this Type type)
        //{
        //    var dictionary = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        //    var properties = type.GetProperties();

        //    foreach (var propertyInfo in properties)
        //    {
        //        dictionary[propertyInfo.Name] = propertyInfo;
        //    }

        //    var fields = type.GetFields();

        //    foreach (var fieldInfo in fields)
        //    {
        //        dictionary[fieldInfo.Name] = fieldInfo;
        //    }

        //    return dictionary;
        //}
    }
}