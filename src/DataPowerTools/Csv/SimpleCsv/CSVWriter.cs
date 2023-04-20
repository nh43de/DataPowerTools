//https://github.com/tsntsumi/SimpleCSV
//
// CSVWriter.cs
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
using System.Text;

namespace SimpleCSV
{
    //TODO: modified so that default line endings are \r\n not just \n


    /// <summary>
    /// A simple CSV writer.
    /// </summary>
    public class CSVWriter : IDisposable
    {
        /// <summary>
        /// The character used for escaping quotes.
        /// </summary>
        public static readonly char DefaultEscapeCharacter = '"';
        /// <summary>
        /// The default separator to use if none is supplied to the constructor.
        /// </summary>
        public static readonly char DefaultSeparator = ',';
        /// <summary>
        /// The default quote character to use if none is supplied to the constructor.
        /// </summary>
        public static readonly char DefaultQuoteCharacter = '"';
        /// <summary>
        /// The quote constant to use when you wish to suppress all quoting.
        /// </summary>
        public static readonly char NoQuoteCharacter = '\u0000';
        /// <summary>
        /// The escape constant to use when you wish to suppress all escaping.
        /// </summary>
        public static readonly char NoEscapeCharacter = '\u0000';
        /// <summary>
        /// Default line terminator.
        /// </summary>
        public static readonly String DefaultLineEnd = "\n";
        /// <summary>
        /// RFC 4180 compliant line terminator.
        /// </summary>
        public static readonly String Rfc4180LineEnd = "\r\n";

        private bool disposed = false;

        /// <summary>
        /// Gets the writer.
        /// </summary>
        /// <value>The writer.</value>
        public TextWriter Writer { get; private set; }

        /// <summary>
        /// Gets the separator.
        /// </summary>
        /// <value>The separator.</value>
        public char Separator { get; private set; }

        /// <summary>
        /// Gets the quote char.
        /// </summary>
        /// <value>The quote char.</value>
        public char QuoteChar { get; private set; }

        /// <summary>
        /// Gets the escape char.
        /// </summary>
        /// <value>The escape char.</value>
        public char EscapeChar { get; private set; }

        /// <summary>
        /// Gets the line end.
        /// </summary>
        /// <value>The line end.</value>
        public string LineEnd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriter"/> class
        /// using a comma for the separator.
        /// </summary>
        /// <param name="writer">The writer to an underlying CSV source.</param>
        public CSVWriter(TextWriter writer)
            : this(writer, DefaultSeparator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriter"/> class
        /// with supplied separator.
        /// </summary>
        /// <param name="writer">The writer to an underlying CSV source.</param>
        /// <param name="separator">The delimiter to use for separating entries.</param>
        public CSVWriter(TextWriter writer, char separator)
            : this(writer, separator, DefaultQuoteCharacter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriter"/> class
        /// with supplied separator and quote char.
        /// </summary>
        /// <param name="writer">The writer to an underlying CSV source.</param>
        /// <param name="separator">The delimiter to use for separating entries.</param>
        /// <param name="quoteChar">The charactor to use for quoted elements.</param>
        public CSVWriter(TextWriter writer, char separator, char quoteChar)
            : this(writer, separator, quoteChar, DefaultEscapeCharacter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriter"/> class
        /// with supplied separator, quote char and escape char.
        /// </summary>
        /// <param name="writer">The writer to an underlying CSV source.</param>
        /// <param name="separator">The delimiter to use for separating entries.</param>
        /// <param name="quoteChar">The charactor to use for quoted elements.</param>
        /// <param name="escapeChar">The character to use for escaping quoteChars and escapeChars.</param>
        public CSVWriter(TextWriter writer, char separator, char quoteChar, char escapeChar)
            : this(writer, separator, quoteChar, escapeChar, DefaultLineEnd)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriter"/> class
        /// with supplied separator, quote char and line ending.
        /// </summary>
        /// <param name="writer">The writer to an underlying CSV source.</param>
        /// <param name="separator">The delimiter to use for separating entries.</param>
        /// <param name="quoteChar">The charactor to use for quoted elements.</param>
        /// <param name="lineEnd">The line feed terminator to use.</param>
        public CSVWriter(TextWriter writer, char separator, char quoteChar, string lineEnd)
            : this(writer, separator, quoteChar, DefaultEscapeCharacter, lineEnd)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCSV.CSVWriter"/> class
        /// with supplied separator, quote char, escape char and line ending.
        /// </summary>
        /// <param name="writer">The writer to an underlying CSV source.</param>
        /// <param name="separator">The delimiter to use for separating entries.</param>
        /// <param name="quoteChar">The charactor to use for quoted elements.</param>
        /// <param name="escapeChar">The character to use for escaping quoteChars and escapeChars.</param>
        /// <param name="lineEnd">The line feed terminator to use.</param>
        public CSVWriter(TextWriter writer, char separator, char quoteChar, char escapeChar, string lineEnd)
        {
            Writer = writer;
            Separator = separator;
            QuoteChar = quoteChar;
            EscapeChar = escapeChar;
            LineEnd = lineEnd;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="SimpleCSV.CSVWriter"/> is reclaimed by garbage collection.
        /// </summary>
        ~CSVWriter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Writes the entire list to a CSV file. The list is assumed to be a string[].
        /// </summary>
        /// <param name="allLines">
        /// a List of string[], with each string[] representing a line of the file.</param>
        /// <param name="applyQuotesToAll">
        /// true if all values are to be quoted.  false if quotes only
        /// to be applied to values which contain the separator, escape,
        /// quote or new line characters.</param>
        public void WriteAll(IList<string[]> allLines, bool applyQuotesToAll = true)
        {
            foreach (var line in allLines)
            {
                WriteNext(line, applyQuotesToAll);
            }
        }

        /// <summary>
        /// Writes the next line to the file.
        /// </summary>
        /// <param name="nextFields">A string array with each comma-separated element as a separate entry.</param>
        /// <param name="applyQuotesToAll">true if all values are to be quoted.  false applies quotes only
        ///                                to values which contain the separator, escape, quote or new line characters.
        /// </param>
        public void WriteNext(string[] nextFields, bool applyQuotesToAll = true)
        {
            if (nextFields == null)
            {
                return;
            }

            var nextLine = new StringBuilder();
            for (int i = 0; i < nextFields.Length; i++)
            {
                if (i > 0)
                {
                    nextLine.Append(Separator);
                }

                if (nextFields[i] == null)
                {
                    continue;
                }

                string field = nextFields[i];
                bool fieldContainsSpecialCharacters = ContainsSpecialCharacters(field);
                if ((applyQuotesToAll || fieldContainsSpecialCharacters) && QuoteChar != NoQuoteCharacter)
                {
                    nextLine.Append(QuoteChar);
                }
                if (fieldContainsSpecialCharacters)
                {
                    nextLine.Append(Escape(field));
                }
                else
                {
                    nextLine.Append(field);
                }
                if ((applyQuotesToAll || fieldContainsSpecialCharacters) && QuoteChar != NoQuoteCharacter)
                {
                    nextLine.Append(QuoteChar);
                }
            }
            nextLine.Append(LineEnd);
            Writer.Write(nextLine.ToString());
        }

        /// <summary>
        /// Escape the specified plain string.
        /// </summary>
        /// <param name="plain">Plain string.</param>
        private string Escape(string plain)
        {
            var sb = new StringBuilder(plain.Length * 2);
            char prevChar = '\0';
            for (int i = 0; i < plain.Length; i++)
            {
                char nextChar = plain[i];
                if (nextChar == '\n' && prevChar != '\r')
                {
                    sb.Append(LineEnd);
                }
                else
                {
                    if (EscapeChar != NoEscapeCharacter && CheckCharacterToEscape(nextChar))
                    {
                        sb.Append(EscapeChar);
                    }
                    sb.Append(nextChar);
                }
                prevChar = nextChar;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Check containses the special characters.
        /// </summary>
        /// <returns><c>true</c>, if special characters was containsed, <c>false</c> otherwise.</returns>
        /// <param name="str">String.</param>
        private bool ContainsSpecialCharacters(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (IsSpecialCharacter(str[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified c is special character.
        /// </summary>
        /// <returns><c>true</c> if the specified c is special character; otherwise, <c>false</c>.</returns>
        /// <param name="c">A character.</param>
        private bool IsSpecialCharacter(char c)
        {
            return c == QuoteChar || c == EscapeChar || c == Separator || c == '\n' || c == '\r';
        }

        /// <summary>
        /// Checks the character to escape.
        /// </summary>
        /// <returns><c>true</c>, if character to escape was checked, <c>false</c> otherwise.</returns>
        /// <param name="c">A character.</param>
        private bool CheckCharacterToEscape(char c)
        {
            return (QuoteChar == NoQuoteCharacter ?
                (c == QuoteChar || c == EscapeChar || c == Separator) :
                (c == QuoteChar || c == EscapeChar));
        }

        /// <summary>
        /// Releases all resource used by the <see cref="SimpleCSV.CSVWriter"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="SimpleCSV.CSVWriter"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="SimpleCSV.CSVWriter"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="SimpleCSV.CSVWriter"/> so
        /// the garbage collector can reclaim the memory that the <see cref="SimpleCSV.CSVWriter"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                    Writer = null;
                }
            }

            disposed = true;
        }
    }
}
