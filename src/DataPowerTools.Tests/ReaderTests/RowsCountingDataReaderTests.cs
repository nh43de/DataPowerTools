using System.Linq;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.ReaderTests
{
    [TestClass]
    public class RowsCountingDataReaderTests
    {
        [TestMethod]
        public void TestRowsCountMaterialize()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();

            var drr = r3.CountRows();

            var dt = drr.ToDataTable();

            Assert.AreEqual(100, drr.Depth);
            Assert.AreEqual(100, dt.Rows.Count);
        }
        
        [TestMethod]
        public void TestRowsCount()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();

            var drr = r3.CountRows();

            drr.ReadToEnd();

            Assert.AreEqual(100, drr.Depth);
        }

        [TestMethod]
        public void TestRowsCountNoRead()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();

            var drr = r3.CountRows();

            Assert.AreEqual(0, drr.Depth);
        }


        [TestMethod]
        public void TestRowsCountOneRead()
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();

            var drr = r3.CountRows();

            drr.Read();

            Assert.AreEqual(1, drr.Depth);
        }
    }
}