//https://github.com/tsntsumi/SimpleCSV
//
// CSVWriterBuilder.cs
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

namespace SimpleCSV
{
    /// <summary>
    /// Builder for creating a CSVWriter
    /// </summary>
    /// <seealso cref="CSVWriter"/>
    public class CSVWriterBuilder
    {
        /// <summary>
        /// Used by unit tests.
        /// </summary>
        /// <value>The writer.</value>
        public TextWriter Writer { get; private set; }

        /// <summary>
        /// Used by unit tests.
        /// </summary>
        /// <value>The separator.</value>
        public char Separator { get; private set; } = CSVWriter.DefaultSeparator;

        /// <summary>
        /// Used by unit tests.
        /// </summary>
        /// <value>The quote char.</value>
        public char QuoteChar { get; private set; } = CSVWriter.DefaultQuoteCharacter;

        /// <summary>
        /// Used by unit tests.
        /// </summary>
        /// <value>The escape char.</value>
        public char EscapeChar { get; private set; } = CSVWriter.DefaultEscapeCharacter;

        /// <summary>
        /// Used by unit tests.
        /// </summary>
        /// <value>The line end.</value>
        public string LineEnd { get; private set; } = CSVWriter.DefaultLineEnd;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriterBuilder"/> class.
        /// </summary>
        /// <param name="writer">Writer.</param>
        public CSVWriterBuilder(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer", "writer may not be null");
            }
            this.Writer = writer;
        }

        /// <summary>
        /// Sets the separator.
        /// </summary>
        /// <returns>The CSVWriterBuilder with separator set.</returns>
        /// <param name="separator">Separator.</param>
        public CSVWriterBuilder WithSeparator(char separator)
        {
            this.Separator = separator;
            return this;
        }

        /// <summary>
        /// Sets the quote char.
        /// </summary>
        /// <returns>The CSVWriterBuilder with quoteChar set.</returns>
        /// <param name="quoteChar">Quote char.</param>
        public CSVWriterBuilder WithQuoteChar(char quoteChar)
        {
            this.QuoteChar = quoteChar;
            return this;
        }

        /// <summary>
        /// Sets the escape char.
        /// </summary>
        /// <returns>The CSVWriterBuilder with escapeChar set.</returns>
        /// <param name="escapeChar">Escape char.</param>
        public CSVWriterBuilder WithEscapeChar(char escapeChar)
        {
            this.EscapeChar = escapeChar;
            return this;
        }

        /// <summary>
        /// Sets the line end.
        /// </summary>
        /// <returns>The CSVWriterBuilder with lineEnd set.</returns>
        /// <param name="lineEnd">Line end.</param>
        public CSVWriterBuilder WithLineEnd(string lineEnd)
        {
            this.LineEnd = lineEnd;
            return this;
        }

        /// <summary>
        /// Create the CSVWriter.
        /// </summary>
        /// <returns>The CSVWriter based on the set criteria.</returns>
        public CSVWriter Build()
        {
            return new CSVWriter(Writer, Separator, QuoteChar, EscapeChar, LineEnd);
        }
    }
}
