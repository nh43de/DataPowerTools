using System;
using System.Collections.Generic;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;
using DataPowerTools.PowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class BiDirectionalMapTests
    {
        [TestMethod]
        public void Test1()
        {
            var d = new BidirectionalMap<string, int>();

            d.Add(new Tuple<string, int>("Col1", 1));
            d.Add(new Tuple<string, int>("Col2", 2));
            d.Add(new Tuple<string, int>("Col3", 3));

            Assert.ThrowsException<ItemsNotOneToOneException>(() =>
            {
                d.Add(new Tuple<string, int>("Col1", 3));
            });
        }

        [TestMethod]
        public void Test2()
        {
            var d = new BidirectionalMap<string, int>();

            d.Add(new Tuple<string, int>("Col1", 1));
            d.Add(new Tuple<string, int>("Col2", 2));
            d.Add(new Tuple<string, int>("Col3", 3));

            Assert.ThrowsException<ItemsNotOneToOneException>(() =>
            {
                d.Add(new Tuple<string, int>("Col1", 1));
            });
        }


        [TestMethod]
        public void Test3()
        {
            var d = new BidirectionalMap<string, int>();

            d.Add(new Tuple<string, int>("Col1", 1));
            d.Add(new Tuple<string, int>("Col2", 2));
            d.Add(new Tuple<string, int>("Col3", 3));

            Assert.ThrowsException<ItemsNotOneToOneException>(() =>
            {
                d.Add(new Tuple<string, int>("Col3", 1));
            });
        }

        [TestMethod]
        public void Test4()
        {
            var d = new BidirectionalMap<int, int>();

            d.Add(new Tuple<int, int>(1, 5));
            d.Add(new Tuple<int, int>(2, 6));
            d.Add(new Tuple<int, int>(3, 7));

            Assert.ThrowsException<ItemsNotOneToOneException>(() =>
            {
                d.Add(new Tuple<int, int>(4, 7));
            });
        }

        [TestMethod]
        public void Test5()
        {
            var d = new BidirectionalMap<int, int>();

            d.Add(new Tuple<int, int>(1, 5));
            d.Add(new Tuple<int, int>(2, 6));
            d.Add(new Tuple<int, int>(3, 7));

            Assert.AreEqual(d.Count, 3);

            Assert.AreEqual(d.GetLeft(7), 3);
            Assert.AreEqual(d.GetRight(3), 7);

            Assert.ThrowsException<Exception>(() =>
            {
                var e = d.GetLeft(11);
            });

            Assert.ThrowsException<Exception>(() =>
            {
                var e = d.GetRight(11);
            });
        }

        [TestMethod]
        public void Test6()
        {
            var d = new BidirectionalMap<string, int>(new Dictionary<string, int>()
            {

                {"Col1", 1},
                {"Col2", 2},
                {"Col3", 3}
            });

            Assert.AreEqual(d.Count, 3);

            Assert.AreEqual(d.GetLeft(1), "Col1");
            Assert.AreEqual(d.GetRight("Col3"), 3);

            Assert.ThrowsException<ItemsNotOneToOneException>(() =>
            {
                d.Add(new Tuple<string, int>("Col1", 3));
            });
        }
    }
}