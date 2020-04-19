using System.Collections.Generic;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class DataReaderExtensionsTests
    {
        [TestMethod]
        public void TestProjection()
        {
            var a = new[]
            {
                new
                {
                    StrVal = "Test",
                    NumVal = 123,
                    NumVal2 = 12
                },
                new
                {
                    StrVal = "Test1",
                    NumVal = 100,
                    NumVal2 = 10
                },
                new
                {
                    StrVal = "Test2",
                    NumVal = 200,
                    NumVal2 = 20
                },
                new
                {
                    StrVal = "Test3",
                    NumVal = 300,
                    NumVal2 = 30
                }
            };

            var dr = a.ToDataReader();

            var drp = dr.ApplyProjection(new Dictionary<string, RowProjection<object>>
            {
                { "NewCol1", row => row["StrVal"] + "_New" },
                { "NewCol2", row => (row["NumVal"] as int?) + (row["NumVal2"] as int?) }
            });

            var r = drp.ToDataTable();

            Assert.AreEqual(r.PrintData(),
                "'NewCol1': 'Test_New', 'NewCol2': '135'\r\n'NewCol1': 'Test1_New', 'NewCol2': '110'\r\n'NewCol1': 'Test2_New', 'NewCol2': '220'\r\n'NewCol1': 'Test3_New', 'NewCol2': '330'\r\n");
        }


        [TestMethod]
        public void TestTransformation()
        {
            
        }
    }
}