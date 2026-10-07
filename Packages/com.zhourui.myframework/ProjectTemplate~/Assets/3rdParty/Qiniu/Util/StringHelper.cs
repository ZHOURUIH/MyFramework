using System;
using System.Collections.Generic;
using System.Text;

namespace Qiniu.Util
{
    /// <summary>
    /// 字符串处理工具
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="text">源字符串</param>
        /// <returns>URL编码字符串</returns>
        public static string UrlEncode(string text)
        {
            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// URL键值对编码
        /// </summary>
        /// <param name="values">键值对</param>
        /// <returns>URL编码的键值对数据</returns>
        public static string UrlFormEncode(Dictionary<string, string> values)
        {
            StringBuilder urlValuesBuilder = new StringBuilder();

            foreach (KeyValuePair<string, string> kvp in values)
            {
                urlValuesBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(kvp.Key),
                    Uri.EscapeDataString(kvp.Value));
            }

            string encodedStr = urlValuesBuilder.ToString();
            return encodedStr.Substring(0, encodedStr.Length - 1);
        }

        /// <summary>
        /// 合法 Header 字段名的字符
        /// </summary>
        private static readonly Dictionary<char, bool> ValidMimeHeaderNameTokens = new Dictionary<char, bool>
        {
            {'!', true},
            {'#', true},
            {'$', true},
            {'%', true},
            {'&', true},
            {'\\', true},
            {'*', true},
            {'+', true},
            {'-', true},
            {'.', true},

            {'0', true},
            {'1', true},
            {'2', true},
            {'3', true},
            {'4', true},
            {'5', true},
            {'6', true},
            {'7', true},
            {'8', true},
            {'9', true},

            {'A', true},
            {'B', true},
            {'C', true},
            {'D', true},
            {'E', true},
            {'F', true},
            {'G', true},
            {'H', true},
            {'I', true},
            {'J', true},
            {'K', true},
            {'L', true},
            {'M', true},
            {'N', true},
            {'O', true},
            {'P', true},
            {'Q', true},
            {'R', true},
            {'S', true},
            {'T', true},
            {'U', true},
            {'W', true},
            {'V', true},
            {'X', true},
            {'Y', true},
            {'Z', true},

            {'^', true},
            {'_', true},
            {'`', true},

            {'a', true},
            {'b', true},
            {'c', true},
            {'d', true},
            {'e', true},
            {'f', true},
            {'g', true},
            {'h', true},
            {'i', true},
            {'j', true},
            {'k', true},
            {'l', true},
            {'m', true},
            {'n', true},
            {'o', true},
            {'p', true},
            {'q', true},
            {'r', true},
            {'s', true},
            {'t', true},
            {'u', true},
            {'v', true},
            {'w', true},
            {'x', true},
            {'y', true},
            {'z', true},
            {'|', true},
            {'~', true}
        };

        /// <summary>
        /// 是否合法的 Header 字段名
        /// </summary>
        /// <param name="fieldName">Header 字段名</param>
        /// <returns>是否合法</returns>
        private static bool IsValidMimeHeaderName(string fieldName)
        {
            foreach (var ch in fieldName)
            {
                if (!ValidMimeHeaderNameTokens.ContainsKey(ch))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// 规范化 Header 字段名
        /// 将合法的字段名规范化为 Abc-Xyz 这种首字母大写的形式
        /// </summary>
        /// <param name="fieldName">Header 字段名</param>
        /// <returns>规范化后的 Header 字段名</returns>
        public static string CanonicalMimeHeaderKey(string fieldName)
        {
            if (!IsValidMimeHeaderName(fieldName))
            {
                return fieldName;
            }

            string result = "";
            bool upper = true;
            foreach (var ch in fieldName)
            {
                switch (upper)
                {
                    case true when 'a' <= ch && ch <= 'z':
                        result += char.ToUpper(ch);
                        break;
                    case false when 'A' <= ch && ch <= 'Z':
                        result += char.ToLower(ch);
                        break;
                    default:
                        result += ch;
                        break;
                }

                upper = ch == '-';
            }
            return result;
        }
    }
}