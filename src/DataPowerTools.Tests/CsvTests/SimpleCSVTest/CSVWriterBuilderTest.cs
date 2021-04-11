//
// CSVWriterBuilderTest.cs
//
// Author:
//       tsntsumi <tsntsumi@tsntsumi.com>
//
// Copyright (c) 2016 tsntsumi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCSV;

namespace SimpleCSVTest
{
    [TestClass]
    public class CSVWriterBuilderTest
    {
        private CSVWriterBuilder builder;
        private TextWriter writer;

        [TestInitialize]
        public void SetUp()
        {
            writer = new StringWriter();
            builder = new CSVWriterBuilder(writer);
        }

        [TestMethod]
        public void TestDefaultBuilder()
        {
            Assert.AreSame(writer, builder.Writer);
            Assert.AreEqual(CSVWriter.DefaultSeparator, builder.Separator);
            Assert.AreEqual(CSVWriter.DefaultQuoteCharacter, builder.QuoteChar);
            Assert.AreEqual(CSVWriter.DefaultEscapeCharacter, builder.EscapeChar);
            Assert.AreEqual(CSVWriter.DefaultLineEnd, builder.LineEnd);

            var cw = builder.Build();
            Assert.AreSame(writer, cw.Writer);
            Assert.AreEqual(CSVWriter.DefaultSeparator, cw.Separator);
            Assert.AreEqual(CSVWriter.DefaultQuoteCharacter, cw.QuoteChar);
            Assert.AreEqual(CSVWriter.DefaultEscapeCharacter, cw.EscapeChar);
            Assert.AreEqual(CSVWriter.DefaultLineEnd, cw.LineEnd);
        }

        [TestMethod]
        public void TestNullWriter()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new CSVWriterBuilder(null));
        }

        [TestMethod]
        public void TestSeparator()
        {
            builder.WithSeparator('1');
            Assert.AreEqual('1', builder.Separator);
            Assert.AreEqual('1', builder.Build().Separator);
        }

        [TestMethod]
        public void TestQuoteChar()
        {
            builder.WithQuoteChar('2');
            Assert.AreEqual('2', builder.QuoteChar);
            Assert.AreEqual('2', builder.Build().QuoteChar);
        }

        [TestMethod]
        public void TestEscapeChar()
        {
            builder.WithEscapeChar('3');
            Assert.AreEqual('3', builder.EscapeChar);
            Assert.AreEqual('3', builder.Build().EscapeChar);
        }

        [TestMethod]
        public void TestLineEnd()
        {
            builder.WithLineEnd("4");
            Assert.AreEqual("4", builder.LineEnd);
            Assert.AreEqual("4", builder.Build().LineEnd);
        }
    }
}
