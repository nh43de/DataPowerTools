using System;
using System.Collections.Generic;
using System.Text;
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
            Assert.AreEqual("", finalData[1].StringValue);
            Assert.AreEqual("", finalData[2].StringValue);
        }
    }
}
