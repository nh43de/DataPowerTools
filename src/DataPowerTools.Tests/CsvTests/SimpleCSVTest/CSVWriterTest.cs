//
// CSVWriterTest.cs
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
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCSV;

namespace SimpleCSVTest
{
    [TestClass]
    public class CSVWriterTest
    {
        private string InvokeWriter(string[] fields)
        {
            var sw = new StringWriter();
            var cw = new CSVWriter(sw, ',', '\'');
            cw.WriteNext(fields);
            return sw.ToString();
        }

        private string InvokeNoEscapeWriter(string[] fields)
        {
            var sw = new StringWriter();
            var cw = new CSVWriter(sw, ',', '\'', CSVWriter.NoEscapeCharacter);
            cw.WriteNext(fields);
            return sw.ToString();
        }

        [TestMethod]
        public void CorrectlyParseNullString()
        {
            var sw = new StringWriter();
            var cw = new CSVWriter(sw, ',', '\'');
            cw.WriteNext(null);
            Assert.AreEqual(0, sw.ToString().Length);
        }

        [TestMethod]
        public void CorrectlyParseNullObject()
        {
            var sw = new StringWriter();
            var cw = new CSVWriter(sw, ',', '\'');
            cw.WriteNext(null, false);
            Assert.AreEqual(0, sw.ToString().Length);
        }

        [TestMethod]
        public void TestParseNormalField()
        {
            string[] fields = { "abc", "def", "ghi" };
            string output = InvokeWriter(fields);
            Assert.AreEqual("'abc','def','ghi'\n", output);
        }

        [TestMethod]
        public void TestParseCommaField()
        {
            string[] fields = { "abc", "d,e,f", "ghi" };
            string output = InvokeWriter(fields);
            Assert.AreEqual("'abc','d,e,f','ghi'\n", output);
        }

        [TestMethod]
        public void TestParseEmptyField()
        {
            string[] fields = { null };
            string output = InvokeWriter(fields);
            Assert.AreEqual("\n", output);
        }

        [TestMethod]
        public void TestParseMultiLineField()
        {
            string[] fields = { "ab \n c", "de\nf", "ghi" };
            string output = InvokeWriter(fields);
            Assert.AreEqual("'ab \n c','de\nf','ghi'\n", output);
        }

        [TestMethod]
        public void TestParseEscapeField()
        {
            string[] fields = { "a \" bc", "def", "ghi" };
            string output = InvokeWriter(fields);
            Assert.AreEqual("'a \"\" bc','def','ghi'\n", output);
        }

        [TestMethod]
        public void TestSpecialCharacters()
        {
            string[] fields = { "a \r bc", "d \n ef", "ghi" };
            string output = InvokeWriter(fields);
            Assert.AreEqual("'a \r bc','d \n ef','ghi'\n", output);
        }

        [TestMethod]
        public void ParseLineWithBothEscapeAndQuoteChar()
        {
            string[] fields = { "a 'b' c", "def", "ghi" };
            string output = InvokeWriter(fields);
            Assert.AreEqual("'a \"'b\"' c','def','ghi'\n", output);
        }

        [TestMethod]
        public void TestParseNormalFieldWithNoEscapeChar()
        {
            string[] fields = { "abc", "def", "ghi" };
            string output = InvokeNoEscapeWriter(fields);
            Assert.AreEqual("'abc','def','ghi'\n", output);
        }

        [TestMethod]
        public void TestParseCommaFieldWithNoEscapeChar()
        {
            string[] fields = { "abc", "d,e,f", "ghi" };
            string output = InvokeNoEscapeWriter(fields);
            Assert.AreEqual("'abc','d,e,f','ghi'\n", output);
        }

        [TestMethod]
        public void TestParseEmptyFieldWithNoEscapeChar()
        {
            string[] fields = { null };
            string output = InvokeNoEscapeWriter(fields);
            Assert.AreEqual("\n", output);
        }

        [TestMethod]
        public void TestParseMultiLineFieldWithNoEscapeChar()
        {
            string[] fields = { "ab \n c", "de\nf", "ghi" };
            string output = InvokeNoEscapeWriter(fields);
            Assert.AreEqual("'ab \n c','de\nf','ghi'\n", output);
        }

        [TestMethod]
        public void ParseLineWithNoEscapeAndQuoteChar()
        {
            string[] fields = { "a \" 'b' c", "def", "ghi" };
            string output = InvokeNoEscapeWriter(fields);
            Assert.AreEqual("'a \" 'b' c','def','ghi'\n", output);
        }

        [TestMethod]
        public void TestWriteAll() 
        {
            IList<string[]> allElements = new List<string[]>();
            string[] line1 = "Name#Phone#Email".Split('#');
            string[] line2 = "Glen#1234#glen@abcd.com".Split('#');
            string[] line3 = "John#5678#john@efgh.com".Split('#');
            allElements.Add(line1);
            allElements.Add(line2);
            allElements.Add(line3);

            StringWriter sw = new StringWriter();
            CSVWriter cw = new CSVWriter(sw);
            cw.WriteAll(allElements);

            string result = sw.ToString();
            string[] lines = result.Split('\n');

            Assert.AreEqual(4, lines.Length);
        }

        [TestMethod]
        public void TestWriteAllObjects() 
        {
            IList<string[]> allElements = new List<string[]>();
            string[] line1 = "Name#Phone#Email".Split('#');
            string[] line2 = "Glen#1234#glen@abcd.com".Split('#');
            string[] line3 = "John#5678#john@efgh.com".Split('#');
            allElements.Add(line1);
            allElements.Add(line2);
            allElements.Add(line3);

            StringWriter sw = new StringWriter();
            CSVWriter cw = new CSVWriter(sw);
            cw.WriteAll(allElements, false);

            string result = sw.ToString();
            string[] lines = result.Split('\n');

            Assert.AreEqual(4, lines.Length);

            string[] values = lines[1].Split(',');

            Assert.AreEqual("1234", values[1]);
        }

        [TestMethod]
        public void TestNoQuoteChars()
        {
            string[] line = { "abc", "def", "ghi" };
            var sw = new StringWriter();
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.NoQuoteCharacter))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("abc,def,ghi\n", result);
            }
        }

        [TestMethod]
        public void TestNoQuoteCharsAndNoEscapeChars()
        {
            string[] line = { "abc", "def", "ghi" };
            var sw = new StringWriter();
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.NoQuoteCharacter, CSVWriter.NoEscapeCharacter))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("abc,def,ghi\n", result);
            }
        }

        [TestMethod]
        public void TestIntelligentQuote()
        {
            string[] line = { "abc", "d,e,f", "g\nh\ni", "j\"k\"l" };
            var sw = new StringWriter();
            using (var cw = new CSVWriter(sw))
            {
                cw.WriteNext(line, false);
                string result = sw.ToString();

                Assert.AreEqual("abc,\"d,e,f\",\"g\nh\ni\",\"j\"\"k\"\"l\"\n", result);
            }
        }

        [TestMethod]
        public void TestNullValues()
        {
            string[] line = { "abc", null, "def", "ghi" };
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("\"abc\",,\"def\",\"ghi\"\n", result);
            }
        }

        [TestMethod]
        public void TestAlternateEscapeChar()
        {
            string[] line = { "abc", "de'f" };
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.DefaultQuoteCharacter, '\''))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("\"abc\",\"de''f\"\n", result);
            }
        }

        [TestMethod]
        public void TestEmbeddedQuoteInString()
        {
            string[] line = { "abc", "def \\\"ghi\\\" jkl" };
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.DefaultQuoteCharacter, CSVWriter.NoEscapeCharacter))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("\"abc\",\"def \\\"ghi\\\" jkl\"\n", result);
            }
        }

        [TestMethod]
        public void TestNoQuotingNoEscaping()
        {
            string[] line = { "\"abc\",\"def\"" };
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.NoQuoteCharacter, CSVWriter.NoEscapeCharacter))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("\"abc\",\"def\"\n", result);
            }
        }

        [TestMethod]
        public void TestNestedQuotes()
        {
            string[] line = { "\"\"", "abc" };
            string expected = "\"\"\"\"\"\",\"abc\"\n";
            string temporaryFilename = "csvWriterNestedQuoteTestTemp.csv";

            try
            {
                using (var temporaryWriter = new StreamWriter(temporaryFilename))
                using (var cw = new CSVWriter(temporaryWriter))
                {
                    cw.WriteNext(line);
                }
                string result = File.ReadAllText(temporaryFilename);
                Assert.AreEqual(expected, result);
            }
            finally
            {
                File.Delete(temporaryFilename);
            }
        }
        
        [TestMethod]
        public void TestAlternateLineEnd()
        {
            string[] line = { "abc", "def", "ghi" };
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.DefaultQuoteCharacter, "\r"))
            {
                cw.WriteNext(line);
                string result = sw.ToString();

                Assert.AreEqual("\"abc\",\"def\",\"ghi\"\r", result);
            }
        }

        [TestMethod]
        public void TestAlternateLineEndWithEmbeddedLineFeed()
        {
            string[] line = { "abc", "d\nef", "g\r\nhi" };
            using (var sw = new StringWriter())
            using (var cw = new CSVWriterBuilder(sw).WithLineEnd("\r\n").Build())
            {
                cw.WriteNext(line, false);
                Assert.AreEqual("abc,\"d\r\nef\",\"g\r\nhi\"\r\n", sw.ToString());
            }
        }
        
        [TestMethod]
        public void TestSeparatorEscapedWhenQuoteIsNotQuoteChar()
        {
            var lines = new List<string[]>();
            lines.Add(new string[] { "abc", "def", "ghi" });
            lines.Add(new string[] { "jkl", "m\"no", "pqr" });
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.NoQuoteCharacter, CSVWriter.DefaultEscapeCharacter))
            {
                cw.WriteAll(lines);
                string result = sw.ToString();

                Assert.AreEqual("abc,def,ghi\njkl,m\"\"no,pqr\n", result);
            }
        }

        [TestMethod]
        public void TestSeparatorEscapedWhenQuoteIsNoQuoteCharSpecifingNoneDefaultEscapeChar()
        {
            var lines = new List<string[]>();
            lines.Add(new string[] { "abc", "def", "ghi" });
            lines.Add(new string[] { "jkl", "m\\n,o", "pqr" });
            using (var sw = new StringWriter())
            using (var cw = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.NoQuoteCharacter, '\\'))
            {
                cw.WriteAll(lines);
                string result = sw.ToString();

                Assert.AreEqual("abc,def,ghi\njkl,m\\\\n\\,o,pqr\n", result);
            }
        }
    }
}

