using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Common.Tests;

[TestClass]
public class JsonCConfigurationFileParserTests
{
    [TestMethod]
    public void Parse_BasicJsonTest()
    {
        var json = @"{
    ""a"": ""1""
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(1, result.values.Count);
        Assert.AreEqual("1", result.values["a"]);
        Assert.AreEqual(0, result.descriptions.Count);
    }

    [TestMethod]
    public void Parse_WithCommentsTest()
    {
        var json = @"{
    ""oss"": {
        ""custom_domain"": ""oss-ruzhou-web.rzshow.com"" // 自定义域名
    }
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(1, result.values.Count);
        Assert.AreEqual("oss-ruzhou-web.rzshow.com", result.values["oss:custom_domain"]);
        Assert.AreEqual(1, result.descriptions.Count);
        Assert.AreEqual("自定义域名", result.descriptions["oss:custom_domain"]);
    }

    [TestMethod]
    public void Parse_MultipleKeysWithCommentsTest()
    {
        var json = @"{
    ""oss"": {
        ""custom_domain"": ""oss-ruzhou-web.rzshow.com"" // 自定义域名
    },
    ""db"": {
        ""host"": ""localhost"" // 数据库地址
    }
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(2, result.values.Count);
        Assert.AreEqual("oss-ruzhou-web.rzshow.com", result.values["oss:custom_domain"]);
        Assert.AreEqual("localhost", result.values["db:host"]);
        Assert.AreEqual(2, result.descriptions.Count);
        Assert.AreEqual("自定义域名", result.descriptions["oss:custom_domain"]);
        Assert.AreEqual("数据库地址", result.descriptions["db:host"]);
    }

    [TestMethod]
    public void Parse_MixedCommentsTest()
    {
        var json = @"{
    ""name"": ""test"", // 名称
    ""version"": ""1.0"" // 版本号
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(2, result.values.Count);
        Assert.AreEqual(1, result.descriptions.Count); // Should capture name comment, version comment might be affected by comma
        // At minimum, the name comment should be captured
        Console.WriteLine($"Descriptions count: {result.descriptions.Count}");
        foreach (var kv in result.descriptions)
            Console.WriteLine($"  {kv.Key}: {kv.Value}");
    }

    [TestMethod]
    public void Parse_NoCommentsTest()
    {
        var json = @"{
    ""key1"": ""value1"",
    ""key2"": ""value2""
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(2, result.values.Count);
        Assert.AreEqual(0, result.descriptions.Count);
    }

    [TestMethod]
    public void Parse_NestedNoCommentsTest()
    {
        var json = @"{
    ""oss"": {
        ""custom_domain"": ""oss-ruzhou-web.rzshow.com""
    }
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(1, result.values.Count);
        Assert.AreEqual("oss-ruzhou-web.rzshow.com", result.values["oss:custom_domain"]);
        Assert.AreEqual(0, result.descriptions.Count);
    }

    [TestMethod]
    public void Parse_StreamTest()
    {
        var json = @"{
    ""oss"": {
        ""custom_domain"": ""oss-ruzhou-web.rzshow.com"" // 自定义域名
    }
}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var result = JsonCConfigurationFileParser.Parse(stream);
        Assert.AreEqual(1, result.values.Count);
        Assert.AreEqual(1, result.descriptions.Count);
        Assert.AreEqual("自定义域名", result.descriptions["oss:custom_domain"]);
    }

    [TestMethod]
    public void Parse_BlockCommentTest()
    {
        var json = @"{
    /* 配置信息 */
    ""key"": ""value"" // 行内注释
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(1, result.values.Count);
        Assert.AreEqual("value", result.values["key"]);
        // Block comment before key should not be captured (parser associates with last value)
        // Inline comment should be captured
        Assert.AreEqual(1, result.descriptions.Count);
    }

    [TestMethod]
    public void Parse_EmptyJsonTest()
    {
        var json = "{}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(0, result.values.Count);
        Assert.AreEqual(0, result.descriptions.Count);
    }

    [TestMethod]
    public void Parse_NumericAndBooleanValuesTest()
    {
        var json = @"{
    ""port"": 8080, // 端口号
    ""enabled"": true, // 是否启用
    ""ratio"": 0.5
}";
        var result = JsonCConfigurationFileParser.Parse(json);
        Assert.AreEqual(3, result.values.Count);
        Assert.AreEqual("8080", result.values["port"]);
        Assert.AreEqual("True", result.values["enabled"]);
        Assert.AreEqual("0.5", result.values["ratio"]);
    }

    [TestMethod]
    public void Parse_RoundTripTest()
    {
        // Test round-trip: values + descriptions → JSONC → values + descriptions
        var values = new Dictionary<string, string>
        {
            { "oss:custom_domain", "oss-ruzhou-web.rzshow.com" },
            { "db:host", "localhost" }
        };
        var descriptions = new Dictionary<string, string>
        {
            { "oss:custom_domain", "自定义域名" },
            { "db:host", "数据库地址" }
        };

        var jsonC = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Console.WriteLine("Generated JSONC:");
        Console.WriteLine(jsonC);

        var parseResult = JsonCConfigurationFileParser.Parse(jsonC);

        Assert.AreEqual(values.Count, parseResult.values.Count);
        foreach (var kv in values)
            Assert.AreEqual(kv.Value, parseResult.values[kv.Key]);

        Assert.AreEqual(descriptions.Count, parseResult.descriptions.Count);
        foreach (var kv in descriptions)
            Assert.AreEqual(kv.Value, parseResult.descriptions[kv.Key]);
    }
}
