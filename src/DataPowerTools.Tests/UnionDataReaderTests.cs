using System.Linq;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class UnionDataReaderTests
    {
        public class Col123
        {
            public string Col1 { get; set; }
            public string Col2 { get; set; }
            public string Col3 { get; set; }
        }

        [TestMethod]
        public void TestUnionDataReader()
        {
            var r1 = new
            {
                Col1 = (string) null,
                Col2 = (string) null,
                Col3 = "abc",
            }.AsDataReader();

            var r2 = new
            {
                Col1 = "Header1",
                Col2 = "Header2",
                Col3 = "Header3",
            }.AsDataReader();

            var r3 = new[]
            {
                new
                {
                    Col1 = 10,
                    Col2 = 20,
                    Col3 = "abc",
                }
            }.Repeat(48).ToDataReader();

            var unionDataReader = r1.Union(r2).Union(r3);

            var resultSet = unionDataReader.SelectNonStrict<Col123>().ToArray();

            var count = resultSet.Length;

            Assert.AreEqual(count, 50);

            Assert.AreEqual(resultSet[1].Col1, "Header1");
            Assert.AreEqual(resultSet[1].Col2, "Header2");
            Assert.AreEqual(resultSet[1].Col3, "Header3");
        }
    }
}