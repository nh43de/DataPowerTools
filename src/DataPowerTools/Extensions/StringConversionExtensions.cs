/* 

MIT License

Copyright (c) [2016] [Codenesium LLC]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

https://github.com/codenesium/DataConversionExtensions/blob/master/License.txt
 *
 */
using System;
using Newtonsoft.Json.Linq;

namespace DataPowerTools.Extensions.DataConversionExtensions
{
    public static class StringConversionExtensions
    {
        public static long? ToNullableShort(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (short.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static TimeSpan? ToNullableTimespan(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (TimeSpan.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static int? ToNullableInt(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (int.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static decimal? ToNullableDecimal(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (decimal.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static double? ToNullableDouble(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (double.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static long? ToNullableLong(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (long.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static Guid? ToNullableGuid(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (Guid.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

        public static float? ToNullableFloat(this string obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (float.TryParse(obj, out var result))
            {
                return result;
            }

            return null;
        }

    }
}