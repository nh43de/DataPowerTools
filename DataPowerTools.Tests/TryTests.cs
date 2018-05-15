using System;
using System.Threading;
using System.Threading.Tasks;
using DataPowerTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class TryTests
    {
        [TestInitialize]
        public void TestInitialize()
        {

        }

        private int _actionVoid1 = 0;
        public void ActionVoid()
        {
            Thread.Sleep(500);
            _actionVoid1++;
        }

        private int _actionTask1 = 0;
        public async Task ActionTask()
        {
            await Task.Delay(500);
            _actionTask1++;
        }

        public int ActionInt()
        {
            Thread.Sleep(500);
            return 1;
        }

        public async Task<int> ActionTaskInt()
        {
            await Task.Delay(500);
            return 1;
        }

        public int Fail()
        {
            throw new Exception("FALSE");
        }

        public async Task<int> FailAsync()
        {
            await Task.Delay(500);
            throw new Exception("FALSE");
        }

        [TestMethod]
        public async Task Test1()
        {
            Try.Do(ActionVoid);
            Assert.AreEqual(1, _actionVoid1);

            await Try.DoAsync(async (token) => await ActionTask());
            Assert.AreEqual(1, _actionTask1);

            var a = Try.Get(ActionInt);
            Assert.AreEqual(1, a);

            var b = await Try.GetAsync(async (token) => await ActionTaskInt());
            Assert.AreEqual(1, b);

            //
            var c = await Try.GetAsync(async (token) => await FailAsync(), 1);
            Assert.AreEqual(1, c);
            
            var d = await Try.GetAsync(async (token) => await FailAsync(), ex => 1);
            Assert.AreEqual(1, c);
            
            var e = Try.Get(Fail, 1);
            Assert.AreEqual(1, c);
            
            var f = Try.Get(Fail, ex => 1);
            Assert.AreEqual(1, c);
        }


    }
}