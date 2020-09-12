using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    //[TestClass] //uncomment to enable
    public static class TestDb
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static DatabaseTestHarness Instance { get; private set; }

        [AssemblyInitialize]
        public static void InitializeHarness(TestContext ctx)
        {
            Instance = new DatabaseTestHarness();
            Instance.Initialize();
        }

        [AssemblyCleanup]
        public static void CleanupHarness()
        {
            Instance.Dispose();
        }
    }
}