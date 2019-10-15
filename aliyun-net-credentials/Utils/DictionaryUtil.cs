using System;
using System.Collections.Generic;
using System.Linq;

namespace Aliyun.Credentials.Utils
{
    public class DictionaryUtil
    {
        public static void Add<T>(Dictionary<string, string> dic, string key, T value)
        {
            var stringValue = value as string;
            Add<string, string>(dic, key, stringValue);
        }

        public static void Add<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (null == value)
            {
                return;
            }

            if (dic == null)
            {
                dic = new Dictionary<TKey, TValue>();
            }
            else if (dic.ContainsKey(key))
            {
                dic.Remove(key);
            }

            dic.Add(key, value);
        }

        public static TValue Get<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }

            return default(TValue);
        }

        public static string Get(Dictionary<string, string> dic, string key)
        {
            return Get<string, string>(dic, key);
        }

        public static TValue Pop<TKey, TValue>(Dictionary<TKey, TValue> dic, TKey key)
        {
            var value = default(TValue);

            if (dic.ContainsKey(key))
            {
                value = dic[key];
                dic.Remove(key);
            }

            return value;
        }

        public static string Pop(Dictionary<string, string> dic, string key)
        {
            return Pop<string, string>(dic, key);
        }

        public static void Print<TKey, TValue>(Dictionary<TKey, TValue> dic, char str)
        {
            foreach (var item in dic)
            {
                Console.WriteLine("{0} {1}: {2}", str, item.Key, item.Value);
            }

            Console.WriteLine();
        }

        public static string TransformDicToString<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            return string.Join(";", dic.Select(x => x.Key + "=" + x.Value));
        }

    }
}
