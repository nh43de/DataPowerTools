using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="ExpandoObject"/> class.
    /// </summary>
    public static class ExpandoObjectExtensions
    {
        /// <summary>
        /// Adds a sequence of values as properties on the <see cref="ExpandoObject"/>. Any existing properties with the same name are overwritten. Returns the same <see cref="ExpandoObject"/> for chaining.
        /// </summary>
        /// <typeparam name="T">The type of values to add.</typeparam>
        /// <param name="expandoObject">The object to which to add the properties.</param>
        /// <param name="values">The values to add as properties.</param>
        /// <param name="names">The names to use for the properties. This may be <c>null</c>. If this parameter is <c>null</c> or does not contain enough names for the values, the property name will be of the form "Property<i>n</i>", where <i>n</i> is the index in the value sequence.</param>
        /// <returns>The <see cref="ExpandoObject"/> <paramref name="expandoObject"/>.</returns>
        public static ExpandoObject AddProperties<T>(this ExpandoObject expandoObject, IEnumerable<T> values, IEnumerable<string> names)
        {
            IDictionary<string, object> obj = expandoObject;
            
            var results = values.Zip(names, (val, name) =>
            {
                // Save the value of the field
                if (obj.ContainsKey(name))
                {
                    obj[name] = val;
                }
                else
                {
                    obj.Add(name, val);
                }

                return true;
            }).ToArray();
            
            return expandoObject;
        }
    }
}
