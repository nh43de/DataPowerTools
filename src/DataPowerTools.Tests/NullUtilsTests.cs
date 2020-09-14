using DataPowerTools.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
{
    [TestClass]
    public class NullUtilsTests
    {
        [TestMethod]
        public void TestNullUtils()
        {
            var a = (decimal?)3.0; //1
            var b = "3";
            var c = (float?)null; //3 
            var d = (int?)null;
            var e = new Test123(); //5
            Test123 f = null;

            var a1 = NullUtils.IsNullThen(a, val => val.Value + 1, 30);
            var a2 = NullUtils.IsNullThen(b, val => val + "1", "30");
            var a3 = NullUtils.IsNullThen(c, val => val.Value + 1, 30);
            var a4 = NullUtils.IsNullThen(d, val => val.Value + 1, 30);
            var a5 = NullUtils.IsNullThen(e, val => new Test123 { Col1 = 11 }, new Test123 { Col1 = 30 });
            var a6 = NullUtils.IsNullThen(f, val => new Test123 { Col1 = 11 }, new Test123 { Col1 = 30 });

            Assert.AreEqual(a1.Value, 4);
            Assert.AreEqual(a2, "31");
            Assert.AreEqual(a3.Value, 30);
            Assert.AreEqual(a4.Value, 30);
            Assert.AreEqual(a5.Col1, 11);
            Assert.AreEqual(a6.Col1, 30);
        }

        [TestMethod]
        public void TestNullUtils2()
        {
            var a = (decimal?)3.0; //1
            var b = "3";
            var c = (float?)null; //3 
            var d = (int?)null;
            var e = new Test123(); //5
            Test123 f = null;

            var a1 = NullUtils.IsNullThen(a, 1, 30);
            var a2 = NullUtils.IsNullThen(b, "1", "30");
            var a3 = NullUtils.IsNullThen(c, 1, 30);
            var a4 = NullUtils.IsNullThen(d, 1, 30);
            var a5 = NullUtils.IsNullThen(e, new Test123 { Col1 = 11 }, new Test123 { Col1 = 30 });
            var a6 = NullUtils.IsNullThen(f, new Test123 { Col1 = 11 }, new Test123 { Col1 = 30 });

            Assert.AreEqual(a1.Value, 1);
            Assert.AreEqual(a2, "1");
            Assert.AreEqual(a3.Value, 30);
            Assert.AreEqual(a4.Value, 30);
            Assert.AreEqual(a5.Col1, 11);
            Assert.AreEqual(a6.Col1, 30);
        }

        [TestMethod]
        public void TestNullUtils3()
        {
            var a = (decimal?)3.0; //1
            var b = "3";
            var c = (float?)null; //3 
            var d = (int?)null;
            var e = new Test123 { Col1 = 11 }; //5
            Test123 f = null;

            var a1 = NullUtils.IsNullThen(a, 30);
            var a2 = NullUtils.IsNullThen(b, "30");
            var a3 = NullUtils.IsNullThen(c, 30);
            var a4 = NullUtils.IsNullThen(d, 30);
            var a5 = NullUtils.IsNullThen(e, new Test123 { Col1 = 30 });
            var a6 = NullUtils.IsNullThen(f, new Test123 { Col1 = 30 });

            Assert.AreEqual(a1.Value, 3);
            Assert.AreEqual(a2, "3");
            Assert.AreEqual(a3.Value, 30);
            Assert.AreEqual(a4.Value, 30);
            Assert.AreEqual(a5.Col1, 11);
            Assert.AreEqual(a6.Col1, 30);
        }

    }
}