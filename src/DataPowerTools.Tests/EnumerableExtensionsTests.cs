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

        [TestMethod]
        public void TestChoose()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var pickedItems = a.ChooseRandom(3).ToArray();
            
            Assert.AreEqual(3, pickedItems.Length);

            var leftOvers = a.Except(pickedItems).ToArray();

            var union = pickedItems.Union(leftOvers).ToArray();

            Assert.AreEqual(5, union.Length);
        }


    }
}