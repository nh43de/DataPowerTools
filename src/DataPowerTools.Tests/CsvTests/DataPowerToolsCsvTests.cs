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

        [TestMethod]
        public void TestCsvFormatOptions()
        {
            var testData = new[]
            {
                new { Name = "Test×Data", Description = "Special characters: ñ©€Á" },
                new { Name = "Normal", Description = "Regular text" }
            };

            // Test UTF8 format (now default)
            var csvContent = testData.ToCsvString(format: CSVFormat.UTF8);
            Assert.IsTrue(csvContent.Contains("×"));
            Assert.IsTrue(csvContent.Contains("ñ"));
            Assert.IsTrue(csvContent.Contains("©"));
            Assert.IsTrue(csvContent.Contains("€"));
            Assert.IsTrue(csvContent.Contains("Á"));

            // Test ANSI format (should work with fallback to UTF-8)
            var csvContentAnsi = testData.ToCsvString(format: CSVFormat.ANSI);
            Assert.IsTrue(csvContentAnsi.Contains("×"));

            // Test UTF16 format
            var csvContentUtf16 = testData.ToCsvString(format: CSVFormat.UTF16);
            Assert.IsTrue(csvContentUtf16.Contains("×"));
            Assert.IsTrue(csvContentUtf16.Contains("€"));
        }

        [TestMethod]
        public void TestCsvEmojiSupportAndExcelCompatibility()
        {
            // Test data with emojis and special characters that require full Unicode support
            var testData = new[]
            {
                new { Name = "John 😀 Doe", Status = "Happy 🎉", Country = "🇺🇸 USA" },
                new { Name = "María José 🌟", Status = "Café ☕", Country = "🇪🇸 España" },
                new { Name = "陈小明 🐉", Status = "茶 🍵", Country = "🇨🇳 中国" },
                new { Name = "Владимир 🚀", Status = "работа 💼", Country = "🇷🇺 Россия" }
            };

            // Test UTF8 format (default) - this should preserve all emojis and Unicode characters
            var csvContent = testData.ToCsvString(format: CSVFormat.UTF8);
            
            // Verify all emojis are preserved
            Assert.IsTrue(csvContent.Contains("😀"), "Should contain smile emoji");
            Assert.IsTrue(csvContent.Contains("🎉"), "Should contain party emoji");
            Assert.IsTrue(csvContent.Contains("🇺🇸"), "Should contain US flag emoji");
            Assert.IsTrue(csvContent.Contains("🌟"), "Should contain star emoji");
            Assert.IsTrue(csvContent.Contains("☕"), "Should contain coffee emoji");
            Assert.IsTrue(csvContent.Contains("🇪🇸"), "Should contain Spain flag emoji");
            Assert.IsTrue(csvContent.Contains("🐉"), "Should contain dragon emoji");
            Assert.IsTrue(csvContent.Contains("🍵"), "Should contain tea emoji");
            Assert.IsTrue(csvContent.Contains("🇨🇳"), "Should contain China flag emoji");
            Assert.IsTrue(csvContent.Contains("🚀"), "Should contain rocket emoji");
            Assert.IsTrue(csvContent.Contains("💼"), "Should contain briefcase emoji");
            Assert.IsTrue(csvContent.Contains("🇷🇺"), "Should contain Russia flag emoji");

            // Verify special characters from different languages
            Assert.IsTrue(csvContent.Contains("María"), "Should contain accented characters");
            Assert.IsTrue(csvContent.Contains("陈小明"), "Should contain Chinese characters");
            Assert.IsTrue(csvContent.Contains("Владимир"), "Should contain Cyrillic characters");
            Assert.IsTrue(csvContent.Contains("茶"), "Should contain Chinese tea character");
            Assert.IsTrue(csvContent.Contains("работа"), "Should contain Russian text");

            // Test file output with BOM for Excel compatibility
            string emojiTestFile = "emoji_test.csv";
            testData.WriteCsv(emojiTestFile, format: CSVFormat.UTF8);

            // Verify file was created
            Assert.IsTrue(System.IO.File.Exists(emojiTestFile), "Emoji CSV file should be created");

            // Read raw bytes to verify BOM is present for Excel compatibility
            byte[] fileBytes = System.IO.File.ReadAllBytes(emojiTestFile);
            
            // Check for UTF-8 BOM (0xEF, 0xBB, 0xBF)
            Assert.IsTrue(fileBytes.Length >= 3, "File should have at least 3 bytes");
            Assert.AreEqual(0xEF, fileBytes[0], "First byte should be 0xEF (UTF-8 BOM)");
            Assert.AreEqual(0xBB, fileBytes[1], "Second byte should be 0xBB (UTF-8 BOM)");
            Assert.AreEqual(0xBF, fileBytes[2], "Third byte should be 0xBF (UTF-8 BOM)");

            Console.WriteLine("✅ Emoji CSV Test Results:");
            Console.WriteLine($"📁 File size: {fileBytes.Length} bytes");
            Console.WriteLine($"📋 UTF-8 BOM detected: {fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF}");
            Console.WriteLine($"🎯 Contains emojis: {csvContent.Contains("😀")}");
            Console.WriteLine($"🌍 Contains international text: {csvContent.Contains("陈小明")}");
            
            Console.WriteLine("\n📝 For Excel users:");
            Console.WriteLine("1. The generated CSV uses UTF-8 with BOM for maximum compatibility");
            Console.WriteLine("2. If Excel shows one column, use Data > From Text/CSV and select UTF-8 encoding");
            Console.WriteLine("3. All emojis and international characters should display correctly");

            // Clean up
            //if (System.IO.File.Exists(emojiTestFile))
               // System.IO.File.Delete(emojiTestFile);
        }
    }
}
