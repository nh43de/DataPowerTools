using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Dynamic;
using System.IO;
using System.Linq;
using DataPowerTools.Connectivity.Helpers;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;
using DataPowerTools.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ExcelDataReader.Tests
{
    [TestClass]
    public class HeaderDataReaderTests
    {
        public static DataTable HeadersOnSecondRow
        {
            get
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

                var r3 = 
                    Enumerable.Range(1, 100).Select(i => new
                        {
                            Col1 = i,
                            Col2 = 20,
                            Col3 = "abc",
                        })
                    .ToDataReader();
                //100 records total

                //102 records total
                var allData = r1.Union(r2).Union(r3);

                var dTable = allData.ToDataTable();

                return dTable;
            }
        }


        [TestInitialize]
        public void TestInitialize()
        {

            
        }



        [TestMethod]
        public void TestHeaderDataReader1()
        {
            var rr = HeadersOnSecondRow;

            var d = rr.ToDataReader().ApplyHeaders(2);

            var dt = d.ToDataTable();

            Assert.AreEqual( 100, dt.Rows.Count);

            Assert.AreEqual("Header1", dt.Columns[0].ColumnName);
            Assert.AreEqual("Header2", dt.Columns[1].ColumnName);
            Assert.AreEqual("Header3", dt.Columns[2].ColumnName);

            Assert.AreEqual("Header1", d.GetName(0));
            Assert.AreEqual("Header2", d.GetName(1));
            Assert.AreEqual("Header3", d.GetName(2));

            Assert.AreEqual("1", dt.Rows[0][0].ToString());
            Assert.AreEqual("100", dt.Rows[99][0].ToString());
        }
    }
}