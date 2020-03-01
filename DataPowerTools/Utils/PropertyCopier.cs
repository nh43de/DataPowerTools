using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataPowerTools
{
    /// <summary>
    /// Static class to efficiently store the compiled delegate which can
    /// do the copying. We need a bit of work to ensure that exceptions are
    /// appropriately propagated, as the exception is generated at type initialization
    /// time, but we wish it to be thrown as an ArgumentException.
    /// Note that this type we do not have a constructor constraint on TTarget, because
    /// we only use the constructor when we use the form which creates a new instance.
    /// From: https://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
    /// </summary>
    internal class PropertyCopier<TSource, TTarget>
    {
        private readonly bool _strictMode;
        private readonly string[] _fieldNames;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strictMode">Whether properties are required to be one-to-one</param>
        /// <param name="fieldNames"></param>
        public PropertyCopier(bool strictMode, params string[] fieldNames)
        {
            _strictMode = strictMode;
            _fieldNames = fieldNames;
            
            try
            {
                _creator = BuildCreator();
                InitializationException = null;
            }
            catch (Exception e)
            {
                InitializationException = e;
            }
        }

        /// <summary>
        /// Delegate to create a new instance of the target type given an instance of the
        /// source type. This is a single delegate from an expression tree.
        /// </summary>
        private readonly Func<TSource, TTarget> _creator = null;

        /// <summary>
        /// List of properties to grab values from. The corresponding targetProperties 
        /// list contains the same properties in the target type. Unfortunately we can't
        /// use expression trees to do this, because we basically need a sequence of statements.
        /// We could build a DynamicMethod, but that's significantly more work :) Please mail
        /// me if you really need this...
        /// </summary>
        private readonly List<PropertyInfo> SourceProperties = new List<PropertyInfo>();
        private readonly List<PropertyInfo> TargetProperties = new List<PropertyInfo>();
        private readonly Exception InitializationException;
        
        internal TTarget Copy(TSource source)
        {
            if (InitializationException != null)
            {
                throw InitializationException;
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return _creator(source);
        }

        internal void Copy(TSource source, TTarget target)
        {
            if (InitializationException != null)
            {
                throw InitializationException;
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            for (var i = 0; i < SourceProperties.Count; i++)
            {
                TargetProperties[i].SetValue(target, SourceProperties[i].GetValue(source, null), null);
            }
        }
        
        private Func<TSource, TTarget> BuildCreator()
        {
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();
            foreach (var sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!sourceProperty.CanRead)
                {
                    continue;
                }
                var targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);
                if (targetProperty == null)
                {
                    if (_strictMode)
                        throw new ArgumentException("Property " + sourceProperty.Name +
                                                    " is not present and accessible in " + typeof(TTarget).FullName);
                    
                    continue;
                }
                if (!targetProperty.CanWrite)
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not writable in " + typeof(TTarget).FullName);
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " is static in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " has an incompatible type in " + typeof(TTarget).FullName);
                }
                bindings.Add(Expression.Bind(targetProperty, Expression.Property(sourceParameter, sourceProperty)));

                if (_fieldNames == null  || _fieldNames.Length == 0 || _fieldNames.Contains(sourceProperty.Name))
                {
                    SourceProperties.Add(sourceProperty);
                    TargetProperties.Add(targetProperty);
                }
            }
            Expression initializer = Expression.MemberInit(Expression.New(typeof(TTarget)), bindings);
            return Expression.Lambda<Func<TSource, TTarget>>(initializer, sourceParameter).Compile();
        }
    }
}