using System;
using System.Collections.Generic;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class DataTransformsTest
    {
        [TestMethod]
        public void TestDecimalTransform()
        {
            var d = DataTransforms.TransformInt("(89)");

            Assert.AreEqual(-89, d);
        }

        [TestMethod]
        public void TestScientificDoubleToDecimal()
        {
            Assert.AreEqual(0.00123545m, DataTransforms.TransformDecimal("12.3545E-4"));
            Assert.AreEqual(123545m, DataTransforms.TransformDecimal("12.3545E4"));
            Assert.AreEqual(0m, DataTransforms.TransformDecimal("12.3545E-40"));
        }

        [TestMethod]
        public void TestDates()
        {
            var d = new Dictionary<string, DateTime>
            {
                // 7 and 8 lens
                { "10272017", new DateTime(2017, 10, 27)},
                { "8012035", new DateTime(2035, 8, 1)},
                { "6302045", new DateTime(2045, 6, 30)},
                { "8022022", new DateTime(2022, 8, 02)},
                { "12012038", new DateTime(2038, 12, 01)},
                { "11302046", new DateTime(2046, 11, 30)},
                { "11012029", new DateTime(2029, 11, 01)},
                { "2102019", new DateTime(2019, 2, 10)},
                { "4272023", new DateTime(2023, 4, 27)},
                { "8222022", new DateTime(2022, 8, 22)},
                // 8 lens
                { "08012035", new DateTime(2035, 8, 1)},
                { "06302045", new DateTime(2045, 6, 30)},
                { "08022022", new DateTime(2022, 8, 02)},
                { "02102019", new DateTime(2019, 2, 10)},
                { "04272023", new DateTime(2023, 4, 27)},
                { "08222022", new DateTime(2022, 8, 22)}
            };
            
            foreach (var s in d)
            {
                var date = (DateTime)DataTransforms.MMDDYYYY_Date(s.Key);

                Assert.AreEqual(date, s.Value);
            }
        }
    }
}