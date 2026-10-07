using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif
using System.Text;

namespace Qiniu.Util
{
    /// <summary>
    /// 签名/加密
    /// 特别注意，不同平台使用的Cryptography可能略有不同，使用中如有遇到问题，请反馈
    /// 提交您的issue到 https://github.com/qiniu/csharp-sdk
    /// </summary>
    public class Signature
    {
        private Mac mac;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mac">账号(密钥)</param>
        public Signature(Mac mac)
        {
            this.mac = mac;
        }

        private string encodedSign(byte[] data)
        {
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(mac.SecretKey));
            byte[] digest = hmac.ComputeHash(data);
            return Base64.UrlSafeBase64Encode(digest);
        }

        private string encodedSign(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return encodedSign(data);
        }

        /// <summary>
        /// 签名-字节数据
        /// </summary>
        /// <param name="data">待签名的数据</param>
        /// <returns></returns>
        public string Sign(byte[] data)
        {
            return string.Format("{0}:{1}", mac.AccessKey, encodedSign(data));
        }

        /// <summary>
        /// 签名-字符串数据
        /// </summary>
        /// <param name="str">待签名的数据</param>
        /// <returns></returns>
        public string Sign(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return Sign(data);
        }

        /// <summary>
        /// 附带数据的签名
        /// </summary>
        /// <param name="data">待签名的数据</param>
        /// <returns></returns>
        public string SignWithData(byte[] data)
        {
            string sstr = Base64.UrlSafeBase64Encode(data);
            return string.Format("{0}:{1}:{2}", mac.AccessKey, encodedSign(sstr), sstr);
        }

        /// <summary>
        /// 附带数据的签名
        /// </summary>
        /// <param name="str">待签名的数据</param>
        /// <returns>签名结果</returns>
        public string SignWithData(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            return SignWithData(data);
        }

        /// <summary>
        /// HTTP请求签名
        /// </summary>
        /// <param name="url">请求目标的URL</param>
        /// <param name="body">请求的主体数据</param>
        /// <returns></returns>
        public string SignRequest(string url, byte[] body)
        {
            Uri u = new Uri(url);
            string pathAndQuery = u.PathAndQuery;
            byte[] pathAndQueryBytes = Encoding.UTF8.GetBytes(pathAndQuery);

            using (MemoryStream buffer = new MemoryStream())
            {
                buffer.Write(pathAndQueryBytes, 0, pathAndQueryBytes.Length);
                buffer.WriteByte((byte)'\n');
                if (body != null && body.Length > 0)
                {
                    buffer.Write(body, 0, body.Length);
                }
                HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(mac.SecretKey));
                byte[] digest = hmac.ComputeHash(buffer.ToArray());
                string digestBase64 = Base64.UrlSafeBase64Encode(digest);
                return string.Format("{0}:{1}", mac.AccessKey, digestBase64);
            }
        }

        /// <summary>
        /// HTTP请求签名
        /// </summary>
        /// <param name="url">请求目标的URL</param>
        /// <param name="body">请求的主体数据</param>
        /// <returns></returns>
        public string SignRequest(string url, string body)
        {
            byte[] data = Encoding.UTF8.GetBytes(body);
            return SignRequest(url, data);
        }

        /// <summary>
        /// HTTP 请求签名 V2 版本（Qiniu 签名），详见：https://developer.qiniu.com/kodo/1201/access-token
        /// </summary>
        /// <param name="method">请求的方法，GET, POST 等。支持非全大写，内部将自动转换，例如：Get -> GET</param>
        /// <param name="url">请求目标的URL</param>
        /// <param name="headers">请求的 Header，支持非规范化的字段名，内部自动转换，例如：CONTENT-TYPE -> Content-Type</param>
        /// <param name="body">请求的主体数据，要求 UTF-8 编码</param>
        /// <returns>签名结果，但不包括 Qiniu 这一开头，例如："access_key:token"</returns>
        public string SignRequestV2(string method, string url, StringDictionary headers, string body)
        {
            Dictionary<string, string> canonicalHeaders = new Dictionary<string, string>();

            if (headers != null)
            {
                foreach (string fieldName in headers.Keys)
                {
                    canonicalHeaders.Add(StringHelper.CanonicalMimeHeaderKey(fieldName), headers[fieldName]);
                }
            }

            Uri parsedUrl = new Uri(url);
            StringBuilder strToSignBuilder = new StringBuilder();
            strToSignBuilder.AppendFormat("{0} {1}", method.ToUpper(), parsedUrl.PathAndQuery);

            // add Host
            strToSignBuilder.AppendFormat("\nHost: {0}", parsedUrl.Host);

            // add Content-Type
            if (canonicalHeaders.ContainsKey("Content-Type"))
            {
                strToSignBuilder.AppendFormat("\nContent-Type: {0}", canonicalHeaders["Content-Type"]);
            }

            // add Headers
            if (canonicalHeaders.Count > 0)
            {
                List<string> qiniuHeaderNames = canonicalHeaders.Keys.ToList()
                    .Where(k => k.StartsWith("X-Qiniu-") && k.Length > "X-Qiniu-".Length).ToList();
                qiniuHeaderNames.Sort();
                foreach (var headerName in qiniuHeaderNames)
                {
                    strToSignBuilder.AppendFormat("\n{0}: {1}", headerName, canonicalHeaders[headerName]);
                }
            }

            // add new lines
            strToSignBuilder.Append("\n\n");

            // add Body
            if (canonicalHeaders.ContainsKey("Content-Type") && canonicalHeaders["Content-Type"] != "application/octet-stream")
            {
                strToSignBuilder.Append(body);
            }

            // calculate sign
            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(mac.SecretKey));
            byte[] digest = hmac.ComputeHash(Encoding.UTF8.GetBytes(strToSignBuilder.ToString()));
            string digestBase64 = Base64.UrlSafeBase64Encode(digest);

            return string.Format("{0}:{1}", mac.AccessKey, digestBase64);
        }

        /// <summary>
        /// HTTP 请求签名 V2 版本（Qiniu 签名），详见：https://developer.qiniu.com/kodo/1201/access-token
        /// </summary>
        /// <param name="method">请求的方法，GET, POST 等。支持非全大写，内部将自动转换，例如：Get -> GET</param>
        /// <param name="url">请求目标的URL</param>
        /// <param name="headers">请求的 Header，支持非规范化的字段名，内部自动转换，例如：CONTENT-TYPE -> Content-Type</param>
        /// <param name="body">请求的主体数据，要求 UTF-8 编码</param>
        /// <returns>签名结果，但不包括 Qiniu 这一开头，例如："access_key:token"</returns>
        public string SignRequestV2(string method, string url, StringDictionary headers, byte[] body)
        {
            return SignRequestV2(method, url, headers, Encoding.UTF8.GetString(body));
        }

        public bool VerifyRequest(
            string method,
            string url,
            StringDictionary headers,
            string body = null
        )
        {
            byte[] bodyBytes = null;
            if (!string.IsNullOrEmpty(body)) {
                bodyBytes = Encoding.UTF8.GetBytes(body);
            }
            return VerifyRequest(
                method,
                url,
                headers,
                bodyBytes
            );
        }

        public bool VerifyRequest(
            string method,
            string url,
            StringDictionary headers,
            byte[] body = null
        )
        {
            if (!headers.ContainsKey("Authorization"))
            {
                return false;
            }

            string authString = headers["Authorization"];
            if (authString.StartsWith("QBox "))
            {
                return authString == "QBox " + SignRequest(url, body);
            }

            if (authString.StartsWith("Qiniu "))
            {
                return authString == "Qiniu " + SignRequestV2(method, url, headers, body);
            }

            return false;
        }
    }
}
