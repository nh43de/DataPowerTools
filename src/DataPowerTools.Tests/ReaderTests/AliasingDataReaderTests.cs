using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using DataPowerTools.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class AliasingDataReaderTests
    {
        [TestMethod]
        public void TestAliasRead()
        {
            var d = Test123.GetTest123s2();

            var dr = d.ToDataReader().ApplyColumnsAliases(new Dictionary<string, string>() { { "Col1", "NewCol1" } });

            dr.Read();

            Assert.AreEqual(dr.GetName(0), "NewCol1");
            Assert.AreEqual(dr.GetName(1), "Col2");
            Assert.AreEqual(dr.GetName(2), "Col3");
            
            Assert.AreEqual(dr[0], 1);
            Assert.AreEqual(dr[1], 1.1m);
            Assert.AreEqual(dr[2], "TestCol3");

            Assert.AreEqual(dr["NewCol1"], 1);
            Assert.AreEqual(dr["Col2"], 1.1m);
            Assert.AreEqual(dr["Col3"], "TestCol3");
        }

        [TestMethod]
        public void TestAliasDictionary()
        {
            var d = Test123.GetTest123s2();

            var dt = d.ToDataReader().ApplyColumnsAliases(new Dictionary<string, string>() { { "Col1", "NewCol1" } }).ToDataTable();
            
            var cols = dt.GetDataColumns().Select(c => c.ColumnName).ToArray();

            Assert.AreEqual(cols[0], "NewCol1");
            Assert.AreEqual(cols[1], "Col2");
            Assert.AreEqual(cols[2], "Col3");

            var drow = dt.Rows[0]; //new Test123 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3"},

            Assert.AreEqual(drow[0], 1);
            Assert.AreEqual(drow[1], 1.1m);
            Assert.AreEqual(drow[2], "TestCol3");

            Assert.AreEqual(drow["NewCol1"], 1);
            Assert.AreEqual(drow["Col2"], 1.1m);
            Assert.AreEqual(drow["Col3"], "TestCol3");
        }
        
        [TestMethod]
        public void TestAliasDataAnnotations()
        {
            var d = Test123.GetTest123s2();

            var dt = d.ToDataReader().ApplyColumnsAliases<Test123, IDataReader>().ToDataTable();

            var cols = dt.GetDataColumns().Select(c => c.ColumnName).ToArray();

            Assert.AreEqual(cols[0], "Col1");
            Assert.AreEqual(cols[1], "Col2");
            Assert.AreEqual(cols[2], "NewCol3");

            var drow = dt.Rows[0]; //new Test123 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3"},

            Assert.AreEqual(drow[0], 1);
            Assert.AreEqual(drow[1], 1.1m);
            Assert.AreEqual(drow[2], "TestCol3");
            
            Assert.AreEqual(drow["Col1"], 1);
            Assert.AreEqual(drow["Col2"], 1.1m);
            Assert.AreEqual(drow["NewCol3"], "TestCol3");
        }

        [TestMethod]
        public void TestAliasDataAnnotationReverse()
        {
            var d = Test123.GetTest123s2();

            var dt = d.ToDataReader()
                .ApplyColumnAlias("Col3", "NewCol3")
                .ApplyColumnsAliasesReverse<Test123, IDataReader>()
                .ToDataTable();

            var cols = dt.GetDataColumns().Select(c => c.ColumnName).ToArray();

            Assert.AreEqual(cols[0], "Col1");
            Assert.AreEqual(cols[1], "Col2");
            Assert.AreEqual(cols[2], "Col3");

            var drow = dt.Rows[0]; //new Test123 {Col1 = 1, Col2 = 1.1m, Col3 = "TestCol3"},

            Assert.AreEqual(drow[0], 1);
            Assert.AreEqual(drow[1], 1.1m);
            Assert.AreEqual(drow[2], "TestCol3");

            Assert.AreEqual(drow["Col1"], 1);
            Assert.AreEqual(drow["Col2"], 1.1m);
            Assert.AreEqual(drow["Col3"], "TestCol3");
        }


        private static string Data = @"ItemId	Investor%	ClientId%	Investor
1	A	123	G
2	B	321	F
3	C	147	Z
4	D	158	D";


        [TestMethod]
        public void TestSelectRows()
        {
            var dr = Data
                .ReadCsvString('\t', true);

            var aliases = dr
                .GetFieldNames()
                .ToDictionary(name => name, name => name.Replace("%", "Pct"));

            dr = dr.ApplyColumnsAliases(aliases);

            var rr = dr.AsCsv();

            //var fq = dr.FitToCreateTableSql("MyTable", null);

            var expected = @"""ItemId"",""InvestorPct"",""ClientIdPct"",""Investor""
""1"",""A"",""123"",""G""
""2"",""B"",""321"",""F""
""3"",""C"",""147"",""Z""
""4"",""D"",""158"",""D""
";

            Assert.AreEqual(expected, rr);
        }

    }
}