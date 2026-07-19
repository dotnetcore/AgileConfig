using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Common.Tests;

[TestClass]
public class DictionaryConvertToJsonCTests
{
    [TestMethod]
    public void ToJsonC_BasicTest()
    {
        var values = new Dictionary<string, string>
        {
            { "a", "1" }
        };
        var descriptions = new Dictionary<string, string>
        {
            { "a", "simple value" }
        };
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("\"a\": \"1\" // simple value"));
        Console.WriteLine(json);
    }

    [TestMethod]
    public void ToJsonC_NoDescriptionTest()
    {
        var values = new Dictionary<string, string>
        {
            { "a", "1" },
            { "b", "2" }
        };
        var descriptions = new Dictionary<string, string>();
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("\"a\": \"1\""));
        Assert.IsTrue(json.Contains("\"b\": \"2\""));
        // No comments should be present
        Assert.IsFalse(json.Contains("//"));
        Console.WriteLine(json);
    }

    [TestMethod]
    public void ToJsonC_NestedTest()
    {
        var values = new Dictionary<string, string>
        {
            { "oss:custom_domain", "oss-ruzhou-web.rzshow.com" }
        };
        var descriptions = new Dictionary<string, string>
        {
            { "oss:custom_domain", "自定义域名" }
        };
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("// 自定义域名"));
        Console.WriteLine(json);
    }

    [TestMethod]
    public void ToJsonC_MultipleNestedTest()
    {
        var values = new Dictionary<string, string>
        {
            { "oss:custom_domain", "oss-ruzhou-web.rzshow.com" },
            { "oss:bucket", "my-bucket" },
            { "db:host", "localhost" }
        };
        var descriptions = new Dictionary<string, string>
        {
            { "oss:custom_domain", "自定义域名" },
            { "oss:bucket", "存储桶名称" },
            { "db:host", "数据库地址" }
        };
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("// 自定义域名"));
        Assert.IsTrue(json.Contains("// 存储桶名称"));
        Assert.IsTrue(json.Contains("// 数据库地址"));
        Console.WriteLine(json);
    }

    [TestMethod]
    public void ToJsonC_SensitiveDescriptionsTest()
    {
        var values = new Dictionary<string, string>
        {
            { "secret:key", "abc123" }
        };
        var descriptions = new Dictionary<string, string>
        {
            { "secret:key", "very secret */ don't break" }
        };
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.IsNotNull(json);
        // The */ should be sanitized to prevent breaking the JSONC structure
        Assert.IsTrue(json.Contains("// very secret * / don't break"));
        Assert.IsFalse(json.Contains("*/"));
        Console.WriteLine(json);
    }

    [TestMethod]
    public void ToJsonC_EmptyDictTest()
    {
        var values = new Dictionary<string, string>();
        var descriptions = new Dictionary<string, string>();
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.AreEqual("{}", json);
    }

    [TestMethod]
    public void ToJsonC_ArrayTest()
    {
        var values = new Dictionary<string, string>
        {
            { "arr:0", "1" },
            { "arr:1", "2" },
            { "arr:2", "3" }
        };
        var descriptions = new Dictionary<string, string>
        {
            { "arr:0", "first" },
            { "arr:1", "second" }
        };
        var json = DictionaryConvertToJsonC.ToJsonC(values, descriptions);
        Assert.IsNotNull(json);
        Assert.IsTrue(json.Contains("// first"));
        Assert.IsTrue(json.Contains("// second"));
        Console.WriteLine(json);
    }
}
