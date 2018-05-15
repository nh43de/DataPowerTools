using System;
using System.Dynamic;

#if !NO_DYNAMIC

#endif

namespace DataPowerTools.FastMember
{
    /// <summary>
    ///     Represents an individual object, allowing access to members by-name
    /// </summary>
    public abstract class ObjectAccessor
    {
        /// <summary>
        ///     Get or Set the value of a named member for the underlying object
        /// </summary>
        public abstract object this[string name] { get; set; }

        /// <summary>
        ///     The object represented by this instance
        /// </summary>
        public abstract object Target { get; }

        /// <summary>
        ///     Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target)
        {
            return Create(target, false);
        }

        /// <summary>
        ///     Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target, bool allowNonPublicAccessors)
        {
            if (target == null) throw new ArgumentNullException("target");
#if !NO_DYNAMIC
            var dlr = target as IDynamicMetaObjectProvider;
            if (dlr != null) return new DynamicWrapper(dlr); // use the DLR
#endif
            return new TypeAccessorWrapper(target, TypeAccessor.Create(target.GetType(), allowNonPublicAccessors));
        }

        /// <summary>
        ///     Use the target types definition of equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return Target.Equals(obj);
        }

        /// <summary>
        ///     Obtain the hash of the target object
        /// </summary>
        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }

        /// <summary>
        ///     Use the target's definition of a string representation
        /// </summary>
        public override string ToString()
        {
            return Target.ToString();
        }

        private sealed class TypeAccessorWrapper : ObjectAccessor
        {
            private readonly TypeAccessor accessor;
            private readonly object target;

            public TypeAccessorWrapper(object target, TypeAccessor accessor)
            {
                this.target = target;
                this.accessor = accessor;
            }

            public override object this[string name]
            {
                get { return accessor[target, name]; }
                set { accessor[target, name] = value; }
            }

            public override object Target
            {
                get { return target; }
            }
        }

#if !NO_DYNAMIC
        private sealed class DynamicWrapper : ObjectAccessor
        {
            private readonly IDynamicMetaObjectProvider target;

            public DynamicWrapper(IDynamicMetaObjectProvider target)
            {
                this.target = target;
            }

            public override object Target
            {
                get { return target; }
            }

            public override object this[string name]
            {
                get { return CallSiteCache.GetValue(name, target); }
                set { CallSiteCache.SetValue(name, target, value); }
            }
        }
#endif
    }
}