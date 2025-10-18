using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgileConfig.Server.Common
{
    public static class DictionaryConvertToJson
    {
        public static string ToJson(IDictionary<string, string> dict)
        {
            if (dict.Count == 0)
            {
                return "{}";
            }
            
            var root = new SortedDictionary<string, object>();
            foreach (var kv in dict)
            {
                Generate(kv.Key, kv.Value, root);
            }

            var newDict = RebuildDict(root);
            
            return JsonConvert.SerializeObject(newDict, Formatting.Indented);
        }

        /// <summary>
        /// Determine whether the dictionary represents a JSON array.
        /// </summary>
        /// <param name="dict">Dictionary to inspect for sequential numeric keys.</param>
        /// <returns>True when the dictionary can be treated as an array.</returns>
        private static bool JudgeDictIsJsonArray(IDictionary<string, object> dict)
        {
            // Check keys starting from index 0.
            // If all keys exist consecutively, the dictionary represents an array.
            var keys = dict.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                var key = i.ToString();
                if (!dict.ContainsKey(key))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Convert the dictionary to an array.
        /// </summary>
        /// <param name="dict">Dictionary whose values should be projected into an array.</param>
        /// <returns>Array built from the dictionary values.</returns>
        private static object[] ConvertDictToJsonArray(IDictionary<string, object> dict)
        {
            var keys = dict.Keys;
            var array = new object[keys.Count()];
            for (int i = 0; i < keys.Count(); i++)
            {
                var key = i.ToString();
                array[i] = dict[key];
            }

            return array;
        }

        /// <summary>
        /// Rebuild the structure, turning dictionaries that represent arrays into actual arrays.
        /// </summary>
        /// <param name="dictOrArray">Dictionary or array to normalize.</param>
        /// <returns>Normalized object graph with arrays materialized.</returns>
        private static object RebuildDict(object dictOrArray)
        {
            var dict = dictOrArray as IDictionary<string,object>;
            if (dict != null)
            {
                if (JudgeDictIsJsonArray(dict))
                {
                    object array = ConvertDictToJsonArray(dict);

                    array = RebuildDict(array);
                    
                    return array;
                }
                else
                {
                    var keys = dict.Keys.Select(x => x).ToList();
                    foreach (var key in keys)
                    {
                        var val = dict[key];
                        dict[key] = RebuildDict(val);
                    }
                    return dict;
                }
            }
            
            var jsonArray = dictOrArray as object[];
            if (jsonArray != null)
            {
                for (int i = 0; i < jsonArray.Length; i++)
                {
                    jsonArray[i] = RebuildDict(jsonArray[i]);
                }

                return jsonArray;
            }

            return dictOrArray;
        }

        /// <summary>
        /// Expand flattened key-value pairs into nested dictionaries.
        /// </summary>
        /// <param name="key">Flattened key representing the nested structure.</param>
        /// <param name="value">Value to assign at the end of the key path.</param>
        /// <param name="parent">Dictionary to populate with the nested structure.</param>
        private static void Generate(string key, string value, IDictionary<string, object> parent)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var groupArr = key.Split(':');
            if (groupArr.Length > 1)
            {
                var sonKey = groupArr[0];
                var newArr = new string[groupArr.Length - 1];
                for (int i = 1; i < groupArr.Length; i++)
                {
                    newArr[i - 1] = groupArr[i];
                }
                var otherKeys = string.Join(':', newArr);
                if (parent.ContainsKey(sonKey))
                {
                    // If a child dictionary already exists.
                    var son = parent[sonKey] as IDictionary<string, object>;
                    if (son != null)
                    {
                        Generate(otherKeys, value, son);
                    }
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
}
