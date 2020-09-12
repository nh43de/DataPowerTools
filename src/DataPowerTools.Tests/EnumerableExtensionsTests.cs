using System;
using System.Data;
using Microsoft.Data.SqlClient;
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
        public void TestRepeat()
        {
            var a = new[] { 1, 2, 3 };

            var b = a.Repeat(3).ToArray();

            Assert.AreEqual(b.Length, a.Length * 3);
        }

    }
}