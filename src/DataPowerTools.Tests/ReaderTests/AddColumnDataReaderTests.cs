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
    public class AddColumnDataReaderTests
    {
        [TestMethod]
        public void TestAddOneCol()
        {
            var d = Test123.GetTest123s2();

            var clientId = Guid.NewGuid();

            var dr = d.ToDataReader()
                .AddColumn("ClientId", row => clientId);
            
            dr.Read();
            
            Assert.AreEqual(clientId.ToString(), dr["ClientId"].ToString());
        }
        
        [TestMethod]
        public void TestAddTwoCol()
        {
            var d = Test123.GetTest123s2();
            var dt = DateTime.Now;

            var clientId = Guid.NewGuid();
            
            var dr = d.ToDataReader()
                .AddColumn("ClientId", row => clientId)
                .AddColumn("Rd", row => dt);

            dr.Read();

            Assert.AreEqual(clientId.ToString(), dr["ClientId"].ToString());
            Assert.AreEqual(dt.ToString(), dr["Rd"].ToString());
        }
        
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
    }
}