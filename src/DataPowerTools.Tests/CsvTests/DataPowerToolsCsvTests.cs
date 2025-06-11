using System;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.CsvTests
{
    [TestClass]
    public class DataPowerToolsCsvTests
    {
        [TestMethod]
        public void TestCsvWritingAndReading()
        {
            var escapedString = "Test this \"escaped string\"";

            var d =
                new[]
                {
                    new
                    {
                        Id = "100",
                        StringValue = escapedString
                    },
                    new
                    {
                        Id = "200",
                        StringValue = ""
                    },
                    new
                    {
                        Id = "300",
                        StringValue = (string)null
                    }
                };

            var csvStr = d.ToCsvString();

            var rr = Csv.ReadString(csvStr);

            var finalData = rr.ToArray<TestString>();

            Assert.AreEqual(escapedString, finalData[0].StringValue);
            Assert.AreEqual(null, finalData[1].StringValue);
            Assert.AreEqual(null, finalData[2].StringValue);
        }

        [TestMethod]
        public void TestCsvExportUsesWindowsLineEndings()
        {
            // Create some test data
            var testData = new[]
            {
                new { Name = "John Doe", Age = 25, City = "New York" },
                new { Name = "Jane Smith", Age = 30, City = "Chicago" },
                new { Name = "Bob Johnson", Age = 35, City = "Los Angeles" }
            };

            // Test string output
            var csvString = testData.ToCsvString();

            Console.WriteLine("CSV String Output:");
            Console.WriteLine(csvString);
            Console.WriteLine($"Contains CRLF: {csvString.Contains("\r\n")}");
            Console.WriteLine($"Contains LF only: {csvString.Contains("\n") && !csvString.Contains("\r\n")}");

            // Test file output
            string testFile = "test_line_endings.csv";
            testData.WriteCsv(testFile);

            // Read raw bytes to verify line endings
            byte[] fileBytes = System.IO.File.ReadAllBytes(testFile);

            int crlfCount = 0;
            int lfOnlyCount = 0;

            for (int i = 0; i < fileBytes.Length - 1; i++)
            {
                if (fileBytes[i] == 13 && fileBytes[i + 1] == 10) // \r\n
                {
                    crlfCount++;
                    i++; // Skip the \n
                }
                else if (fileBytes[i] == 10 && (i == 0 || fileBytes[i - 1] != 13)) // \n not preceded by \r
                {
                    lfOnlyCount++;
                }
            }

            Console.WriteLine($"File CRLF count: {crlfCount}");
            Console.WriteLine($"File LF-only count: {lfOnlyCount}");

            // Assertions
            Assert.IsTrue(csvString.Contains("\r\n"), "CSV string should contain CRLF line endings");
            Assert.IsFalse(csvString.Contains("\n") && !csvString.Contains("\r\n"), "CSV string should not contain LF-only endings");
            Assert.IsTrue(crlfCount > 0, "CSV file should contain CRLF line endings");
            Assert.AreEqual(0, lfOnlyCount, "CSV file should not contain LF-only line endings");

            // Clean up
            if (System.IO.File.Exists(testFile))
                System.IO.File.Delete(testFile);
        }
    }
}
