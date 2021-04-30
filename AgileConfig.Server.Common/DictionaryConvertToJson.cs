using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Common
{
    public class DictionaryConvertToJson
    {
        public static string ToJson(IDictionary<string, string> dict)
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            foreach (var kv in dict)
            {
                Generate(kv.Key, kv.Value, root);
            }

            return DictToJsonString(root);
        }

        private static string DictToJsonString(Dictionary<string, object> dict)
        {
            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        /// <summary>
        /// 把扁平化的键值对还原成字典嵌套字典的模式
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="parent"></param>
        private static void Generate(string key, string value, Dictionary<string, object> parent)
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
                    var son = parent[sonKey] as Dictionary<string, object>;
                    if (son != null)
                    {
                        Generate(otherKeys, value, son);
                    }
                }
                else
                {
                    var son = new Dictionary<string, object>();
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
