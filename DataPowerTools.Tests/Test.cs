using DataPowerTools.Serialization.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExcelDataReader.Tests
{
    [TestClass]
    public class Test
    {

        [TestMethod]
        public void TestSerial()
        {
            var dd = @" 
 [{
 ""order"": 1,
 ""cdpPropertyPath"": ""$.cdpId"",
 ""matchOnPropertyPath"": ""$.cdpId"",
 ""dataType"": ""string""
 }, 
 
 
 
 
 {""order"": 2,""cdpPropertyPath"": ""$.matchKeys.gigyaUId"",""matchOnPropertyPath"": ""$.matchKeys.gigyaUId"",""dataType"": ""string""},
 
 
 {""order"": 3,""cdpPropertyPath"": ""$.matchKeys.anieProfileId"",""matchOnPropertyPath"": ""$.matchKeys.anieProfileId"",""dataType"": ""string""}, {""order"": 4,""cdpPropertyPath"": ""$.matchKeys.sprinklrUniversalProfileId"",""matchOnPropertyPath"": ""$.matchKeys.sprinklrUniversalProfileId"",""dataType"": ""string""}, {""order"": 5,""cdpPropertyPath"": ""$.matchKeys.gigyaFacebookId"",""matchOnPropertyPath"": ""$.matchKeys.gigyaFacebookId"",""dataType"": ""string""}, {""order"": 6,""cdpPropertyPath"": ""$.matchKeys.gigyaFacebookId"",""matchOnPropertyPath"": ""$.matchKeys.sprinklrFacebookId"",""dataType"": ""string""}, {""order"": 7,""cdpPropertyPath"": ""$.matchKeys.sprinklrFacebookId"",""matchOnPropertyPath"": ""$.matchKeys.sprinklrFacebookId"",""dataType"": ""string""}, {""order"": 8,""cdpPropertyPath"": ""$.matchKeys.sprinklrFacebookId"",""matchOnPropertyPath"": ""$.matchKeys.gigyaFacebookId"",""dataType"": ""string""}, {""order"": 9,""cdpPropertyPath"": ""$.matchKeys.gigyatwitterId"",""matchOnPropertyPath"": ""$.matchKeys.gigyaFacebookId"",""dataType"": ""string""}, {""order"": 10,""cdpPropertyPath"": ""$.matchKeys.gigyatwitterId"",""matchOnPropertyPath"": ""$.matchKeys.sprinklrTwitterId"",""dataType"": ""string""}, {""order"": 11,""cdpPropertyPath"": ""$.matchKeys.sprinklrTwitterId"",""matchOnPropertyPath"": ""$.matchKeys.gigyatwitterId"",""dataType"": ""string""}, {""order"": 12,""cdpPropertyPath"": ""$.matchKeys.sprinklrTwitterId"",""matchOnPropertyPath"": ""$.matchKeys.sprinklrTwitterId"",""dataType"": ""string""}, {""order"": 13,""cdpPropertyPath"": ""$.matchKeys.similacEmail"",""matchOnPropertyPath"": ""$.matchKeys.similacEmail"",""dataType"": ""string""}, {""order"": 14,""cdpPropertyPath"": ""$.matchKeys.similacEmail"",""matchOnPropertyPath"": ""$.matchKeys.gigyaEmail"",""dataType"": ""string""}, {""order"": 15,""cdpPropertyPath"": ""$.matchKeys.gigyaEmail"",""matchOnPropertyPath"": ""$.matchKeys.similacEmail"",""dataType"": ""string""}, {""order"": 16,""cdpPropertyPath"": ""$.matchKeys.gigyaEmail"",""matchOnPropertyPath"": ""$.matchKeys.gigyaEmail"",""dataType"": ""string""}, {""order"": 17,""cdpPropertyPath"": ""$.matchKeys.gigyaPhone"",""matchOnPropertyPath"": ""$.matchKeys.gigyaPhone"",""dataType"": ""string""}, {""order"": 18,""cdpPropertyPath"": ""$.matchKeys.gigyaPhone"",""matchOnPropertyPath"": ""$.matchKeys.simliacPhone"",""dataType"": ""string""}, {""order"": 19,""cdpPropertyPath"": ""$.matchKeys.simliacPhone"",""matchOnPropertyPath"": ""$.matchKeys.gigyaPhone"",""dataType"": ""string""}, {""order"": 20,""cdpPropertyPath"": ""$.matchKeys.simliacPhone"",""matchOnPropertyPath"": ""$.matchKeys.simliacPhone"",""dataType"": ""string""},{""order"": 21,""cdpPropertyPath"": ""$.matchKeys.identityId"",""matchOnPropertyPath"": ""$.matchKeys.identityId"",""dataType"": ""string""},{""order"": 22,""cdpPropertyPath"": ""$.matchKeys.CON_ID"",""matchOnPropertyPath"": ""$.matchKeys.CON_ID"",""dataType"": ""string""},{""order"": 23,""cdpPropertyPath"": ""$.matchKeys.OPTY_ID"",""matchOnPropertyPath"": ""$.matchKeys.OPTY_ID"",""dataType"": ""string""},{""order"": 24,""cdpPropertyPath"": ""$.matchKeys.cdpId"",""matchOnPropertyPath"": ""$.matchKeys.cdpId"",""dataType"": ""string""},{""order"": 25,""cdpPropertyPath"": ""$.matchKeys.MS_IDENT"",""matchOnPropertyPath"": ""$.matchKeys.MS_IDENT"",""dataType"": ""string""},{""order"": 26,""cdpPropertyPath"": ""$.emails"",""matchOnPropertyPath"": ""$.matchKeys.emails"",""dataType"": ""array""}]
   ";

            var r = dd.ToObject<object>().ToJson(true);
        }
        //public string Field1 { get; set; }
        //public string Field2 { get; set; }  
        //public string Field3 { get; set; }  
    }
}