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
using DataPowerTools.Extensions.DataConversionExtensions;
using NUnit.Framework;

namespace DataConversionExtensionsTests
{
    [TestFixture]
    public class ToIntTests
    {
        [Test]
        public void ToNullableInt_Null_null()
        {
            string testString = null;
            Assert.AreEqual(null, testString.ToNullableInt());
        }

        [Test]
        public void ToNullableInt_ABC123_null()
        {
            string testString = "ABC123";
            Assert.AreEqual(null, testString.ToNullableInt());
        }

        [Test]
        public void ToNullableInt_12345_12345()
        {
            string testString = "12345";
            Assert.AreEqual(12345, testString.ToNullableInt());
        }
    }
}