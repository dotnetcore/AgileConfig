using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Common.Tests
{
    [TestClass()]
    public class DictionaryConvertToJsonTests
    {
        [TestMethod()]
        public void ToJsonTest()
        {
            var dict = new Dictionary<string, string>() {
                {"a","1" }
            };
            var json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" }
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" }
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" }
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" },
                {"f:g","5" }
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" },
                {"f:g","5" },
                {"h:i:j","6" }
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" },
                {"f:g","5" },
                {"h:i:j","6" },
                {"k","7" },
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" },
                {"f:g","5" },
                {"h:i:j","6" },
                {"k","7" },
                {"c:d1","8" },
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" },
                {"f:g","5" },
                {"h:i:j","6" },
                {"k","7" },
                {"c:d1","8" },
                {"c:d2","9" },
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);

            dict = new Dictionary<string, string>() {
                {"a","1" },
                {"b","2" },
                {"c:d","3" },
                {"e","4" },
                {"f:g","5" },
                {"h:i:j","6" },
                {"k","7" },
                {"c:d1","8" },
                {"c:d2","9" },
                {"c:d2:e","10" },
                {"l","11" },
                {"n","12" },
            };
            json = DictionaryConvertToJson.ToJson(dict);
            Assert.IsNotNull(json);
            Console.WriteLine(json);
        }
    }
}