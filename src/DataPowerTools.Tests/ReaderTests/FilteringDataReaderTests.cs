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
    public class FilteringDataReaderTests
    {
        [TestMethod]
        public void TestAddFilter()
        {
            var d = Test123.GetTest123s2();

            var clientId = Guid.NewGuid();
            var dt = DateTime.Now;

            var dr = d.ToDataReader()
                .AddColumn("ClientId", row => clientId)
                .AddColumn("Rd", row => dt)
                .Where(row => row["Col1"].ToString() == "111");

            dr.ReadToEnd();

            Assert.AreEqual(clientId.ToString(), dr["ClientId"].ToString());
            Assert.AreEqual(dt.ToString(), dr["Rd"].ToString());
        }


        [TestMethod]
        public void TestAddFilterToHeaderReader()
        {
            var rr = HeaderDataReaderTests.HeadersOnSecondRow.ToDataReader().ApplyHeaders(2);
            
            var clientId = Guid.NewGuid();

            var dr = rr
                .AddColumn("ClientId", row => clientId)
                .Where(row => string.IsNullOrWhiteSpace(row["Header2"].ToString()) == false);

            var dTable = dr.ToDataTable();
        }



        [TestMethod]
        public void TestAddFilterToHeaderReaderAndCountRows()
        {
            var rr = HeaderDataReaderTests.HeadersOnSecondRow.ToDataReader().ApplyHeaders(2);

            var clientId = Guid.NewGuid();

            var unfilteredDr = rr
                .AddColumn("ClientId", row => clientId)
                .CountRows();

            var filteredDr = unfilteredDr
                .Where(row => string.IsNullOrWhiteSpace(row["Header2"].ToString()) == false && row["Header1"].ToString() == "2")
                .CountRows();

            var dTable = filteredDr.ToDataTable();

            var unfiltered = unfilteredDr.Depth;
            var filtered = filteredDr.Depth;

            Assert.AreEqual(1, filtered);
            Assert.AreEqual(100, unfiltered);

        }

    }

}