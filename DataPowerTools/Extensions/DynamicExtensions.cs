using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.DataStructures;

namespace DataPowerTools.Extensions
{
    public static class DyanamicExtensions
    {
        /// <summary>
        /// Converts type to expando object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ExpandoObject ToExpandoObject(this object obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var currentValue = propertyInfo.GetValue(obj);
                expando.Add(propertyInfo.Name, currentValue);
            }

            return expando as ExpandoObject;
        }

        /// <summary>
        /// Converts object to dynamic dictionary so that properties can be accessed like this: a["prop"] = val
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDynamicDictionary(this object obj)
        {
            var expando = new DynamicDictionary();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var currentValue = propertyInfo.GetValue(obj);
                expando[propertyInfo.Name] = currentValue;
            }

            return expando;
        }
    }
}
