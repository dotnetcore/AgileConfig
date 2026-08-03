using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AgileConfig.Server.Common;

public static class DictionaryConvertToJson
{
    private const string IndentUnit = "  ";

    public static string ToJson(IDictionary<string, string> dict)
    {
        if (dict.Count == 0) return "{}";

        var root = new SortedDictionary<string, object>();
        foreach (var kv in dict) Generate(kv.Key, kv.Value, root);

        var newDict = RebuildDict(root);

        return JsonConvert.SerializeObject(newDict, Formatting.Indented);
    }

    /// <summary>
    ///     Convert flattened key-value pairs to jsonc, writing each comment above the item it describes.
    /// </summary>
    /// <param name="dict">Flattened configuration items.</param>
    /// <param name="comments">Comments keyed by the same flattened key. Multi line comments are separated by \n.</param>
    /// <returns>A jsonc document.</returns>
    public static string ToJsonc(IDictionary<string, string> dict, IDictionary<string, string> comments)
    {
        if (dict.Count == 0) return "{}";

        if (comments == null || comments.Count == 0) return ToJson(dict);

        var root = new SortedDictionary<string, object>();
        foreach (var kv in dict) Generate(kv.Key, kv.Value, root);

        var newDict = RebuildDict(root);

        var sb = new StringBuilder();
        WriteJsonc(sb, newDict, "", comments, 0);

        return sb.ToString();
    }

    private static void WriteJsonc(StringBuilder sb, object value, string path, IDictionary<string, string> comments,
        int level)
    {
        if (value is IDictionary<string, object> dict)
        {
            if (dict.Count == 0)
            {
                sb.Append("{}");
                return;
            }

            sb.Append('{').Append(Environment.NewLine);
            var index = 0;
            foreach (var kv in dict)
            {
                var childPath = string.IsNullOrEmpty(path) ? kv.Key : path + ":" + kv.Key;
                WriteComment(sb, childPath, comments, level + 1);
                Indent(sb, level + 1);
                sb.Append(JsonConvert.ToString(kv.Key)).Append(": ");
                WriteJsonc(sb, kv.Value, childPath, comments, level + 1);
                if (++index < dict.Count) sb.Append(',');

                sb.Append(Environment.NewLine);
            }

            Indent(sb, level);
            sb.Append('}');
            return;
        }

        if (value is object[] array)
        {
            if (array.Length == 0)
            {
                sb.Append("[]");
                return;
            }

            sb.Append('[').Append(Environment.NewLine);
            for (var i = 0; i < array.Length; i++)
            {
                var childPath = string.IsNullOrEmpty(path) ? i.ToString() : path + ":" + i;
                WriteComment(sb, childPath, comments, level + 1);
                Indent(sb, level + 1);
                WriteJsonc(sb, array[i], childPath, comments, level + 1);
                if (i < array.Length - 1) sb.Append(',');

                sb.Append(Environment.NewLine);
            }

            Indent(sb, level);
            sb.Append(']');
            return;
        }

        sb.Append(JsonConvert.ToString(value as string));
    }

    private static void WriteComment(StringBuilder sb, string path, IDictionary<string, string> comments, int level)
    {
        if (!comments.TryGetValue(path, out var comment) || string.IsNullOrWhiteSpace(comment)) return;

        foreach (var line in comment.Replace("\r\n", "\n").Split('\n'))
        {
            Indent(sb, level);
            sb.Append("// ").Append(line.Trim()).Append(Environment.NewLine);
        }
    }

    private static void Indent(StringBuilder sb, int level)
    {
        for (var i = 0; i < level; i++) sb.Append(IndentUnit);
    }

    /// <summary>
    ///     Determine whether the dictionary represents a JSON array.
    /// </summary>
    /// <param name="dict">Dictionary to inspect for sequential numeric keys.</param>
    /// <returns>True when the dictionary can be treated as an array.</returns>
    private static bool JudgeDictIsJsonArray(IDictionary<string, object> dict)
    {
        // Check keys starting from index 0.
        // If all keys exist consecutively, the dictionary represents an array.
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
    /// <param name="dict">Dictionary whose values should be projected into an array.</param>
    /// <returns>Array built from the dictionary values.</returns>
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

    /// <summary>
    ///     Rebuild the structure, turning dictionaries that represent arrays into actual arrays.
    /// </summary>
    /// <param name="dictOrArray">Dictionary or array to normalize.</param>
    /// <returns>Normalized object graph with arrays materialized.</returns>
    private static object RebuildDict(object dictOrArray)
    {
        var dict = dictOrArray as IDictionary<string, object>;
        if (dict != null)
        {
            if (JudgeDictIsJsonArray(dict))
            {
                object array = ConvertDictToJsonArray(dict);

                array = RebuildDict(array);

                return array;
            }

            var keys = dict.Keys.Select(x => x).ToList();
            foreach (var key in keys)
            {
                var val = dict[key];
                dict[key] = RebuildDict(val);
            }

            return dict;
        }

        var jsonArray = dictOrArray as object[];
        if (jsonArray != null)
        {
            for (var i = 0; i < jsonArray.Length; i++) jsonArray[i] = RebuildDict(jsonArray[i]);

            return jsonArray;
        }

        return dictOrArray;
    }

    /// <summary>
    ///     Expand flattened key-value pairs into nested dictionaries.
    /// </summary>
    /// <param name="key">Flattened key representing the nested structure.</param>
    /// <param name="value">Value to assign at the end of the key path.</param>
    /// <param name="parent">Dictionary to populate with the nested structure.</param>
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
                // If a child dictionary already exists.
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
}