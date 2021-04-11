using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Markup;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests.ReaderTests
{

    [TestClass]
    public class NotifyingDataReaderTests
    {
        [DataTestMethod]
        [DynamicData(nameof(TestDataHelpers.GetDataReaderSourceTypes), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
        public void TestNotifyEveryRow(DataReaderSource source)
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();
            
            var i = 0;

            var drr = r3.NotifyOn(p =>
            {
                i++;
            }, 1);

            drr.Read();
            drr.Read();
            
            Assert.AreEqual(2, i); //two notifications
        }

        [DataTestMethod]
        [DynamicData(nameof(TestDataHelpers.GetDataReaderSourceTypes), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
        public void TestNotifyEveryRowNoParam(DataReaderSource source)
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();

            var i = 0;

            var drr = r3.NotifyOn(p =>
            {
                i++;
            });

            drr.Read();
            drr.Read();

            Assert.AreEqual(2, i); //two notifications
        }


        [DataTestMethod]
        [DynamicData(nameof(TestDataHelpers.GetDataReaderSourceTypes), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
        public void TestNotifyEveryRowAll(DataReaderSource source)
        {
            var r3 = TestDataHelpers.GetSampleDataReader(source, 100);

            var i = 0;

            var drr = r3.NotifyOn(p => i++, 1);

            drr.ReadToEnd();

            Assert.AreEqual(100, i); //two notifications
        }


        [DataTestMethod]
        [DynamicData(nameof(TestDataHelpers.GetDataReaderSourceTypes), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
        public void TestNotifyLastRow(DataReaderSource source)
        {
            var r3 = TestDataHelpers.GetSampleDataReader(DataReaderSource.DataTable, 100);
            //var r3 = GetADataReader(source, 3);
            
            var i = 0;
            var c = 0;

            var drr = r3.NotifyOn(p =>
            {
                i = p;
                c++;
            }, 1024);

            drr.ReadToEnd();

            Assert.AreEqual(100, i);
            Assert.AreEqual(1, c);
        }

        [DataTestMethod]
        [DynamicData(nameof(TestDataHelpers.GetDataReaderSourceTypes), typeof(TestDataHelpers), DynamicDataSourceType.Method)]
        public void TestModulus(DataReaderSource source)
        {
            var r3 =
                Enumerable.Range(1, 100).Select(i => new
                    {
                        Col1 = i,
                        Col2 = 20,
                        Col3 = "abc",
                    })
                    .ToDataReader();
            
            var i = 0;

            var drr = r3.NotifyOn(p => i = p, 10);

            drr.Read(3);

            Assert.AreEqual(0, i); //two notifications
        }

    }
}