using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgileConfig.Server.Common;

/// <summary>
///     Serialize configuration flat dictionaries into JSONC (JSON with Comments) format.
///     Each leaf value is followed by an inline comment containing its description.
/// </summary>
public static class DictionaryConvertToJsonC
{
    public static string ToJsonC(
        IDictionary<string, string> values,
        IDictionary<string, string> descriptions)
    {
        if (values.Count == 0) return "{}";

        var root = new SortedDictionary<string, object>();
        foreach (var kv in values)
            Generate(kv.Key, kv.Value, root);

        var sb = new StringBuilder();
        WriteJsonC(root, descriptions, sb, 0, "");
        // Remove trailing newline for cleaner output
        var result = sb.ToString();
        if (result.EndsWith(Environment.NewLine))
            result = result.Substring(0, result.Length - Environment.NewLine.Length);
        return result;
    }

    /// <summary>
    ///     Expand flattened key-value pairs into nested dictionaries.
    ///     Same logic as DictionaryConvertToJson.Generate.
    /// </summary>
    private static void Generate(string key, string value, IDictionary<string, object> parent)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        var groupArr = key.Split(':');
        if (groupArr.Length > 1)
        {
            var sonKey = groupArr[0];
            var newArr = new string[groupArr.Length - 1];
            for (var i = 1; i < groupArr.Length; i++) newArr[i - 1] = groupArr[i];
            var otherKeys = string.Join(':', newArr);
            if (parent.ContainsKey(sonKey))
            {
                var son = parent[sonKey] as IDictionary<string, object>;
                if (son != null) Generate(otherKeys, value, son);
            }
            else
            {
                var son = new SortedDictionary<string, object>();
                Generate(otherKeys, value, son);
                parent.Add(sonKey, son);
            }
        }
        else
        {
            parent.Add(key, value);
        }
    }

    /// <summary>
    ///     Recursively build the JSONC string with inline comments for leaf values.
    /// </summary>
    private static void WriteJsonC(
        object node,
        IDictionary<string, string> descriptions,
        StringBuilder sb,
        int indent,
        string currentPath)
    {
        var indentStr = new string(' ', indent * 2);

        if (node is IDictionary<string, object> dict)
        {
            // Check if this dictionary can be represented as an array
            if (JudgeDictIsJsonArray(dict))
            {
                var array = ConvertDictToJsonArray(dict);
                sb.AppendLine("[");
                for (var i = 0; i < array.Length; i++)
                {
                    var itemPath = string.IsNullOrEmpty(currentPath)
                        ? i.ToString()
                        : currentPath + ":" + i;
                    sb.Append(new string(' ', (indent + 1) * 2));
                    WriteJsonCValue(array[i], descriptions, sb, indent + 1, itemPath);
                    if (i < array.Length - 1) sb.AppendLine(",");
                    else sb.AppendLine();
                }
                sb.Append(indentStr + "]");
            }
            else
            {
                sb.AppendLine("{");
                var keys = dict.Keys.ToList();
                for (var i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var val = dict[key];
                    var childPath = string.IsNullOrEmpty(currentPath)
                        ? key
                        : currentPath + ":" + key;

                    sb.Append(new string(' ', (indent + 1) * 2));
                    sb.Append($"\"{EscapeJsonString(key)}\": ");
                    WriteJsonCValue(val, descriptions, sb, indent + 1, childPath);
                    if (i < keys.Count - 1) sb.AppendLine(",");
                    else sb.AppendLine();
                }
                sb.Append(indentStr + "}");
            }
        }
    }

    private static void WriteJsonCValue(
        object val,
        IDictionary<string, string> descriptions,
        StringBuilder sb,
        int indent,
        string currentPath)
    {
        if (val is IDictionary<string, object> childDict)
        {
            WriteJsonC(childDict, descriptions, sb, indent, currentPath);
        }
        else if (val is object[] childArray)
        {
            sb.AppendLine("[");
            for (var i = 0; i < childArray.Length; i++)
            {
                var itemPath = currentPath + ":" + i;
                sb.Append(new string(' ', (indent + 1) * 2));
                WriteJsonCValue(childArray[i], descriptions, sb, indent + 1, itemPath);
                if (i < childArray.Length - 1) sb.AppendLine(",");
                else sb.AppendLine();
            }
            sb.Append(new string(' ', indent * 2) + "]");
        }
        else
        {
            // Leaf value — serialize and append comment if description exists
            var jsonValue = SerializeJsonValue(val);
            sb.Append(jsonValue);

            if (descriptions.TryGetValue(currentPath, out var desc) && !string.IsNullOrWhiteSpace(desc))
            {
                // Sanitize description: remove newlines and escape */ to avoid breaking JSONC structure
                var safeDesc = SanitizeComment(desc);
                sb.Append(" // " + safeDesc);
            }
        }
    }

    private static string SerializeJsonValue(object val)
    {
        if (val == null) return "null";
        if (val is string s) return $"\"{EscapeJsonString(s)}\"";
        if (val is bool b) return b ? "true" : "false";
        return val.ToString();
    }

    private static string EscapeJsonString(string str)
    {
        return str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    ///     Sanitize a comment string to prevent it from breaking the JSONC structure.
    /// </summary>
    private static string SanitizeComment(string comment)
    {
        return comment
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\r", " ")
            .Replace("*/", "* /");
    }

    /// <summary>
    ///     Determine whether the dictionary represents a JSON array.
    /// </summary>
    private static bool JudgeDictIsJsonArray(IDictionary<string, object> dict)
    {
        var keys = dict.Keys;
        for (var i = 0; i < keys.Count; i++)
        {
            var key = i.ToString();
            if (!dict.ContainsKey(key)) return false;
        }

        return true;
    }

    /// <summary>
    ///     Convert the dictionary to an array.
    /// </summary>
    private static object[] ConvertDictToJsonArray(IDictionary<string, object> dict)
    {
        var keys = dict.Keys;
        var array = new object[keys.Count()];
        for (var i = 0; i < keys.Count(); i++)
        {
            var key = i.ToString();
            array[i] = dict[key];
        }

        return array;
    }
}
