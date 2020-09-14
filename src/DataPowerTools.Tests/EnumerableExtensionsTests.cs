using System.Linq;
using DataPowerTools.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataPowerTools.Tests
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