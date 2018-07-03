/*
    Sequelocity.NET v0.6.0

    Sequelocity.NET is a simple data access library for the Microsoft .NET
    Framework providing lightweight ADO.NET wrapper, object mapper, and helper
    functions. To find out more, visit the project home page at: 
    https://github.com/AmbitEnergyLabs/Sequelocity.NET

    The MIT License (MIT)

    Copyright (c) 2015 Ambit Energy. All rights reserved.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using DataPowerTools.Extensions;

namespace DataPowerTools
{
    /// <summary>Provides methods for accessing and caching <see cref="Type" /> metadata.</summary>
    public static class TypeCacher
    {
        /// <summary>
        /// Cache that stores types as the key and the type's PropertyInfo and FieldInfo in a <see cref="OrderedDictionary"/> as the value.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> PropertiesAndFieldsCache 
            = new ConcurrentDictionary<Type, Dictionary<string, object>>();

        /// <summary>Gets the types properties and fields and caches the results.</summary>
        /// <param name="type">Type.</param>
        /// <returns><see cref="OrderedDictionary"/> of lowercase member names and PropertyInfo or FieldInfo as the values.</returns>
        public static Dictionary<string, object> GetPropertiesAndFields( Type type )
        {
            if ( PropertiesAndFieldsCache.TryGetValue( type, out var orderedDictionary ) )
            {
                return orderedDictionary;
            }

            orderedDictionary = type.GetPropertiesAndFields();

            return PropertiesAndFieldsCache.TryAdd( type, orderedDictionary ) 
                ? orderedDictionary 
                : PropertiesAndFieldsCache[ type ];
        }
    }
}
