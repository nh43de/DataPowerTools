﻿using System;
using DataPowerTools.Extensions;
using ExcelDataReader.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class TypeExtensionsTests
    {
        [TestMethod]
        public void TestGetColumnMemberNames()
        {
            var dd = typeof(Test123V1);

            var r = dd.GetColumnMemberNames(true);
        }

        [TestMethod]
        public void TestCopy()
        {
            var d = new Test123
            {
                Col1 = 1,
                Col2 = 2,
                Col3 = "3"
            };

            var tt = new Test123
            {
                Col1 = 100,
                Col2 = 200,
                Col3 = "300"
            };

            tt.CopyTo(d);

            Assert.AreEqual(d.Col1, tt.Col1);
            Assert.AreEqual(d.Col1, 100);
            Assert.AreEqual(d.Col2, tt.Col2);
            Assert.AreEqual(d.Col2, 200);
            Assert.AreEqual(d.Col3, tt.Col3);
            Assert.AreEqual(d.Col3, "300");
        }

        [TestMethod]
        public void TestCopyWithFieldsSpecified()
        {
            var d = new Test123
            {
                Col1 = 1,
                Col2 = 2,
                Col3 = "3"
            };

            var tt = new Test123
            {
                Col1 = 100,
                Col2 = 200,
                Col3 = "300"
            };

            tt.CopyTo(d, new [] { nameof(Test123.Col2) });

            Assert.AreEqual(tt.Col1, 100);
            Assert.AreEqual(d.Col1, 1);

            Assert.AreEqual(tt.Col2, 200);
            Assert.AreEqual(d.Col2, 200);

            Assert.AreEqual(tt.Col3, "300");
            Assert.AreEqual(d.Col3, "3");
        }

        [TestMethod]
        public void TestCopy4()
        {
            var d = new Test1234
            {
                Col1 = 1,
                Col2 = 2,
                Col3 = "3",
                Col4 = "4"
            };

            var tt = new Test123
            {
                Col1 = 100,
                Col2 = 200,
                Col3 = "300"
            };

            tt.CopyTo(d);

            Assert.AreEqual(d.Col1, tt.Col1);
            Assert.AreEqual(d.Col1, 100);
            Assert.AreEqual(d.Col2, tt.Col2);
            Assert.AreEqual(d.Col2, 200);
            Assert.AreEqual(d.Col3, tt.Col3);
            Assert.AreEqual(d.Col3, "300");
            Assert.AreEqual(d.Col4, "4");
        }

        [TestMethod]
        public void TestCopyWithFieldsSpecified4()
        {
            var d = new Test123
            {
                Col1 = 1,
                Col2 = 2,
                Col3 = "3"
            };

            var tt = new Test1234
            {
                Col1 = 100,
                Col2 = 200,
                Col3 = "300",
                Col4 = "400"
            };

            tt.CopyTo(d, new[] { nameof(Test1234.Col4) });

            Assert.AreEqual(tt.Col1, 100);
            Assert.AreEqual(d.Col1, 1);

            Assert.AreEqual(tt.Col2, 200);
            Assert.AreEqual(d.Col2, 2);

            Assert.AreEqual(tt.Col3, "300");
            Assert.AreEqual(d.Col3, "3");
            
            Assert.AreEqual(tt.Col4, "400");
        }
    }
}
