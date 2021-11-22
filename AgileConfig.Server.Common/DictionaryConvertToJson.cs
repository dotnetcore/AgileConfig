using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgileConfig.Server.Common
{
    public class DictionaryConvertToJson
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
        /// 判断字典是否符合json数组的格式
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static bool JudgeDictIsJsonArray(IDictionary<string, object> dict)
        {
            //从0号index开始测试这个字典的key值是否存在
            //如果全部存在那么就是数组
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
        /// 把字典转成Array
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
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
        /// 重构字典，如果有字典的值对应的是json的数组，那么就把这个数组转成Array
        /// </summary>
        /// <param name="dictOrArray"></param>
        /// <returns></returns>
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
        /// 把扁平化的键值对还原成字典嵌套字典的模式
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="parent"></param>
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
                    //如果已经有子字典
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
