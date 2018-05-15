using System.Collections.Generic;
using DataPowerTools.DataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class DynamicDictionaryTests
    {
        [TestMethod]
        public void Accessing_A_Not_Existent_Property_Should_Return_Null()
        {
            dynamic obj = new DynamicDictionary();

            var firstName = obj.FirstName;

            Assert.IsNull(firstName);
        }

        [TestMethod]
        public void Accessing_A_Defined_Property_Should_Return_The_Defined_Value()
        {
            dynamic obj = new DynamicDictionary();

            obj.FirstName = "Clark";

            Assert.IsTrue(obj.FirstName == "Clark");
        }

        [TestMethod]
        public void The_DynamicDictionary_Should_Be_Castable_To_An_IDictionary_Of_String_Object()
        {
            dynamic obj = new DynamicDictionary();

            var dictionary = (IDictionary<string, object>)obj;

            Assert.IsNotNull(dictionary);
        }

        [TestMethod]
        public void Properties_Defined_On_The_Dynamic_Should_Be_Accessible_When_Cast_To_A_Dictionary()
        {
            dynamic obj = new DynamicDictionary();

            obj.FirstName = "Clark";

            var dictionary = (IDictionary<string, object>)obj;

            Assert.IsTrue(dictionary.ContainsKey("FirstName"));

            Assert.IsTrue(dictionary["FirstName"].ToString() == "Clark");
        }

        [TestMethod]
        public void Properties_Defined_When_Cast_To_A_Dictionary_Should_Be_Accessible_When_Accessed_Dynamically()
        {
            dynamic obj = new DynamicDictionary();

            var dictionary = (IDictionary<string, object>)obj;

            dictionary.Add("FirstName", "Clark");

            Assert.IsTrue(obj.FirstName == "Clark");
        }

        [TestMethod]
        public void Accessing_A_Defined_Property_Using_Different_Cases_Should_Return_The_Defined_Value()
        {
            dynamic obj = new DynamicDictionary();

            obj.FirstName = "Clark";

            Assert.IsTrue(obj.FIRSTNAME == "Clark");

            Assert.IsTrue(obj.firstname == "Clark");

            Assert.IsTrue(obj.fIrStNaMe == "Clark");
        }

        [TestMethod]
        public void A_Defined_Anonymous_Object_Should_Be_Accessible_Dynamically()
        {
            dynamic obj = new DynamicDictionary();

            obj.Customer = new { FirstName = "Clark", LastName = "Kent" };

            Assert.IsTrue(obj.Customer.FirstName == "Clark");
        }
    }
}