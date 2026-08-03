using System.Collections.Generic;
using System.IO;
using System.Text;
using AgileConfig.Server.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.CommonTests;

[TestClass]
public class JsonConfigurationFileParserTests
{
    private static JsonParseResult Parse(string jsonc)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonc));
        return JsonConfigurationFileParser.ParseWithComments(stream);
    }

    [TestMethod]
    public void ParseWithComments_LeadingComment_AttachedToFollowingKey()
    {
        var result = Parse(@"{
  // db host
  ""host"": ""localhost""
}");

        Assert.AreEqual("localhost", result.Data["host"]);
        Assert.AreEqual("db host", result.Comments["host"]);
    }

    [TestMethod]
    public void ParseWithComments_TrailingCommentOnSameLine_AttachedToPreviousKey()
    {
        var result = Parse(@"{
  ""host"": ""localhost"", // db host
  ""port"": 3306
}");

        Assert.AreEqual("db host", result.Comments["host"]);
        Assert.IsFalse(result.Comments.ContainsKey("port"));
    }

    [TestMethod]
    public void ParseWithComments_NestedObjectsAndArrays_FlattensKeys()
    {
        var result = Parse(@"{
  ""db"": {
    // primary connection
    ""conn"": ""x"",
    ""ports"": [
      // first
      1,
      2
    ]
  },
  ""enabled"": true
}");

        Assert.AreEqual("x", result.Data["db:conn"]);
        Assert.AreEqual("1", result.Data["db:ports:0"]);
        Assert.AreEqual("2", result.Data["db:ports:1"]);
        Assert.AreEqual("True", result.Data["enabled"]);
        Assert.AreEqual("primary connection", result.Comments["db:conn"]);
        Assert.AreEqual("first", result.Comments["db:ports:0"]);
    }

    [TestMethod]
    public void ParseWithComments_BlockComment_KeptAsMultipleLines()
    {
        var result = Parse(@"{
  /*
   line1
   line2
  */
  ""k"": ""v""
}");

        Assert.AreEqual("line1\nline2", result.Comments["k"]);
    }

    [TestMethod]
    public void ToJsonc_RoundTrip_KeepsValuesAndComments()
    {
        var dict = new Dictionary<string, string>
        {
            { "db:conn", "x" },
            { "db:ports:0", "1" },
            { "name", "agile" }
        };
        var comments = new Dictionary<string, string>
        {
            { "db:conn", "primary connection" },
            { "name", "app\nname" }
        };

        var jsonc = DictionaryConvertToJson.ToJsonc(dict, comments);
        var result = Parse(jsonc);

        CollectionAssert.AreEquivalent(new List<string> { "db:conn", "db:ports:0", "name" },
            new List<string>(result.Data.Keys));
        Assert.AreEqual("x", result.Data["db:conn"]);
        Assert.AreEqual("1", result.Data["db:ports:0"]);
        Assert.AreEqual("agile", result.Data["name"]);
        Assert.AreEqual("primary connection", result.Comments["db:conn"]);
        Assert.AreEqual("app\nname", result.Comments["name"]);
    }
}
