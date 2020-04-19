﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class EnumerableExtensionsTests
    {

        [TestMethod]
        public void TestZip()
        {
            var a = new[] {1, 2, 3};
            var b = new[] {10, 20, 30};
            var c = new[] {100, 200, 300};

            var dd = a.Zip(b, c, (x, y, z) => x + y + z);

            Assert.AreEqual(dd.Count(), 3);
            Assert.AreEqual(dd.Sum(), 111+222+333);
        }

        [TestMethod]
        public void TestRepeat()
        {
            var a = new[] { 1, 2, 3 };

            var b = a.Repeat(3).ToArray();

            Assert.AreEqual(b.Length, a.Length * 3);
        }


        [TestMethod]
        public void TestCountInt()
        {
            var a = 4.Count();

            Assert.AreEqual(4, a.Count());

            var b = 0.Count();

            Assert.AreEqual(0, b.Count());
        }

        [TestMethod]
        public void TestCountShort()
        {
            var a = ((short) 4).Count();

            Assert.AreEqual(4, a.Count());

            var b = ((short)0).Count();

            Assert.AreEqual(0, b.Count());
        }

        [TestMethod]
        public void TestConnString()
        {
            var dd = Database.GetConnectionString("FossData", "localhost", null, null, true);

        }
    }
}