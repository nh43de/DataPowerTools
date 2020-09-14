using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Dynamic;
using System.IO;
using System.Linq;
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



        [TestInitialize]
        public void TestInitialize()
        {

            
        }



        [TestMethod]
        public void TestMapping()
        {
            var rr = SampleData.HeadersOnSecondRow;




        }
    }
}