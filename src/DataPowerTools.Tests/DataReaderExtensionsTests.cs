using System.Collections.Generic;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class DataReaderExtensionsTests
    {
        //parttial con 

        //bvatch empty


        [TestMethod]
        public void TestReadBatchEmptySet()
        {
            var r3 =
                Enumerable.Range(0, 0).Select(i => new
                    {
                        Col1 = i
                    })
                    .ToDataReader();

            var readers = r3.Batch(10);

            var i = 0;
            foreach (var dataReader in readers)
            {
                var r = dataReader.SelectRows(p => p.GetInt32(0)).ToArray();

                Assert.AreEqual(i + 1, r[0]);

                Assert.AreEqual(-1, r.Length);

                i += r.Length;
            }

            Assert.AreEqual(i, 0);
        }

        [TestMethod]
        public void TestReadBatchPartialConsumption()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i
                    })
                    .ToDataReader();

            var readers = r3.Batch(10);

            var i = 0;
            foreach (var dataReader in readers)
            {
                var r = dataReader.SelectRows(p => p.GetInt32(0)).Take(8).ToArray();

                Assert.AreEqual(i*10 + 1, r[0]);

                Assert.AreEqual(8, r.Length);

                i += 1;
            }

            Assert.AreEqual(i, 10);
        }
        
        [TestMethod]
        public void TestReadBatch()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i
                    })
                    .ToDataReader();

            var readers = r3.Batch(10);

            var i = 0;
            foreach (var dataReader in readers)
            {
                var r = dataReader.SelectRows(p => p.GetInt32(0)).ToArray();

                Assert.AreEqual(i + 1, r[0]);

                Assert.AreEqual(10, r.Length);

                i += r.Length;
            }

            Assert.AreEqual(i, 100);
        }
        
        [TestMethod]
        public void TestReadMultiple()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i
                    })
                    .ToDataReader();

            r3.Read();

            Assert.AreEqual(1, r3["Col1"]);
            
            r3.Read(3);

            Assert.AreEqual(4, r3["Col1"]);
        }
        
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

            Assert.AreEqual(
                "[{\"NewCol1\": \"Test_New\"}, {\"NewCol2\": \"135\"},\r\n{\"NewCol1\": \"Test1_New\"}, {\"NewCol2\": \"110\"},\r\n{\"NewCol1\": \"Test2_New\"}, {\"NewCol2\": \"220\"},\r\n{\"NewCol1\": \"Test3_New\"}, {\"NewCol2\": \"330\"}]",
                r.PrintData());
        }


        [TestMethod]
        public void TestTransformation()
        {
            
        }
        public class ItemResult
        {
            public string? SearchTerm { get; set; }
            public string? VendorItemId { get; set; }
            public decimal? Price { get; set; }
        }

        [TestMethod]
        public void TestSelectRowsFromCsvWithSkipUnmatchedProperties()
        {
            // Tab-delimited CSV string with missing SearchTerm column
            string csvData = "VendorItemId\tPrice\n123\t10.5\n456\t20.0";

            // Convert DataTable to IDataReader
            var reader = csvData.ReadCsvString('\t');

            // Map to ItemResult class
            var results = reader.SelectRows<ItemResult>(null, true).ToArray();

            // Assertions
            Assert.AreEqual(2, results.Length);
            Assert.IsNull(results[0].SearchTerm);
            Assert.AreEqual("123", results[0].VendorItemId);
            Assert.AreEqual(10.5m, results[0].Price);
            Assert.IsNull(results[1].SearchTerm);
            Assert.AreEqual("456", results[1].VendorItemId);
            Assert.AreEqual(20.0m, results[1].Price);
        }

    }
}