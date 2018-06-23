using System;
using System.Collections.Generic;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void TestBoolean2()
        {
            // 0) null or empty string -> false
            // 1) "True" (String) = true
            // 2) "False" (String) = false
            // 3) "0" (String) = false
            // 4) Any other string = true

            //0
            Assert.AreEqual(false, "  ".ToBoolean2());
            Assert.AreEqual(false, ((string)null).ToBoolean2());
            Assert.AreEqual(false, "".ToBoolean2());

            //1+2
            Assert.AreEqual(true, " TRUE ".ToBoolean2());
            Assert.AreEqual(false, "False ".ToBoolean2());
            Assert.AreEqual(true, " true ".ToBoolean2());
            Assert.AreEqual(false, " FALSE ".ToBoolean2());

            //3
            Assert.AreEqual(false, " 0 ".ToBoolean2());
            Assert.AreEqual(false, " 000 ".ToBoolean2());

            //4
            Assert.AreEqual(true, " tesatesasdaf".ToBoolean2());
            Assert.AreEqual(true, " asd;fkljasd;    flkj ".ToBoolean2());
            Assert.AreEqual(true, " 004588700 ".ToBoolean2());
            Assert.AreEqual(true, "8".ToBoolean2());
        }


        [TestMethod]
        public void TestBoolean3()
        {
            // 0) null or empty string -> false
            // 1) "True" (string) = true
            // 2) "False" (string) = false
            // 3) "0" (string) = false
            // 4) non-zero numeric (or string) = true
            // 5) Any other string throws exception

            //0
            Assert.AreEqual(false, "  ".ToBoolean3());
            Assert.AreEqual(false, ((string)null).ToBoolean3());
            Assert.AreEqual(false, "".ToBoolean3());

            //1+2
            Assert.AreEqual(true, " TRUE ".ToBoolean3());
            Assert.AreEqual(false, "False ".ToBoolean3());
            Assert.AreEqual(true, " true ".ToBoolean3());
            Assert.AreEqual(false, " FALSE ".ToBoolean3());

            //3
            Assert.AreEqual(false, " 0 ".ToBoolean3());
            Assert.AreEqual(false, " 000 ".ToBoolean3());

            //4
            Assert.AreEqual(true, " 156 ".ToBoolean3());
            Assert.AreEqual(true, " 8798798 ".ToBoolean3());

            //5
            Assert.ThrowsException<Exception>(() =>
            {
                var fs = " tesatesasdaf".ToBoolean3();
                var dd = " 89asdfasdf    ".ToBoolean3();
            });
        }


    }
}