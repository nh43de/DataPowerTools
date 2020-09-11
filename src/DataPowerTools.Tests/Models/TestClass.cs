using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExcelDataReader.Tests
{
    [Serializable]
    public class TestClass
    {
        public string Name { get; set; }
        public TestClassReference TestRef { get; set; } = new TestClassReference();
        public TestClassReference TestRefNull { get; set; } = null;

        [JsonIgnore]
        public TestClassReference TestNonSerializedRefNull { get; set; } = null;

        public int TestIntP { get; set; }
        public Test123 Test123 = new Test123();
        public IEnumerable<Test123> Test123s = new Test123[]{};
    }
}