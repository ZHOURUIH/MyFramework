using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Linq;
using System.Net;
using Qiniu.Util;

namespace Qiniu.Http
{
    /// <summary>
    /// HttpManager for .NET 2.0/3.0/3.5/4.0
    /// </summary>
    public class HttpManager
    {
        private bool allowAutoRedirect;
        private string userAgent;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="allowAutoRedirect">是否允许HttpWebRequest的“重定向”，默认禁止</param>
        public HttpManager(bool allowAutoRedirect = false)
        {
            this.allowAutoRedirect = allowAutoRedirect;
            userAgent = GetUserAgent();
        }

        /// <summary>
        /// 客户端标识(UserAgent)，示例："SepcifiedClient/1.1 (Universal)"
        /// </summary>
        /// <returns>客户端标识UA</returns>
        public static string GetUserAgent()
        {
            string osDesc = Environment.OSVersion.Platform + "; " + Environment.OSVersion.Version;
            return string.Format("{0}/{1} ({2}; {3})", QiniuCSharpSDK.ALIAS, QiniuCSharpSDK.VERSION, QiniuCSharpSDK.RTFX, osDesc);
        }

        /// <summary>
        /// 设置自定义的客户端标识(UserAgent)，示例："SepcifiedClient/1.1 (Universal)"
        /// 如果设置为空白或者不设置，SDK会自动使用默认的UserAgent
        /// </summary>
        /// <param name="userAgent">用户自定义的UserAgent</param>
        /// <returns>客户端标识UA</returns>
        public void SetUserAgent(string userAgent)
        {
            if (!string.IsNullOrEmpty(userAgent))
            {
                this.userAgent = userAgent;
            }
        }

        /// <summary>
        /// 多部分表单数据(multi-part form-data)的分界(boundary)标识
        /// </summary>
        /// <returns>分界(boundary)标识字符串</returns>
        public static string CreateFormDataBoundary()
        {
            string now = DateTime.UtcNow.Ticks.ToString();
            return string.Format("-------{0}Boundary{1}", QiniuCSharpSDK.ALIAS, Hashing.CalcMD5X(now));
        }

        public HttpRequestOptions CreateHttpRequestOptions(
            string method,
            string url,
            StringDictionary headers,
            string token = null
        )
        {
            HttpRequestOptions reqOpts = new HttpRequestOptions();

            reqOpts.Method = method;
            reqOpts.Url = url;

            if (headers != null)
            {
                reqOpts.Headers = headers;
            }

            if (!string.IsNullOrEmpty(token))
            {
                reqOpts.Headers.Add("Authorization", token);
            }

            reqOpts.Headers.Add("User-Agent", userAgent);
            reqOpts.AllowAutoRedirect = allowAutoRedirect;

            return reqOpts;
        }

        public HttpResult CreateHttpResult(HttpWebResponse wResp, bool binaryMode = false)
        {
            HttpResult result = new HttpResult();

            if (wResp == null)
            {
                return result;
            }

            result.Code = (int)wResp.StatusCode;
            result.RefCode = (int)wResp.StatusCode;

            getHeaders(ref result, wResp);

            Stream respStream = wResp.GetResponseStream();
            if (respStream == null)
            {
                wResp.Close();
                return result;
            }

            if (binaryMode)
            {
                int len = (int)wResp.ContentLength;
                result.Data = new byte[len];
                int bytesLeft = len;
                int bytesRead = 0;

                using (BinaryReader br = new BinaryReader(respStream))
                {
                    while (bytesLeft > 0)
                    {
                        bytesRead = br.Read(result.Data, len - bytesLeft, bytesLeft);
                        bytesLeft -= bytesRead;
                    }
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(respStream))
                {
                    result.Text = sr.ReadToEnd();
                }
            }

            wResp.Close();
            return result;
        }

        public HttpResult SendRequest(HttpRequestOptions reqOpts, Boolean binaryMode = false)
        {
            HttpResult result;
            HttpWebRequest wReq = null;

            try
            {
                wReq = reqOpts.CreateHttpWebRequest();
                HttpWebResponse wResp = wReq.GetResponse() as HttpWebResponse;

                result = CreateHttpResult(wResp, binaryMode);
            }
            catch (WebException wex)
            {
                HttpWebResponse xResp = wex.Response as HttpWebResponse;
                result = CreateHttpResult(xResp);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(
                    "[{0}] [{1}] [HTTP-{2}] Error:  ",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                    userAgent,
                    reqOpts.Method
                );
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                result = CreateHttpResult(null);
                result.RefCode = (int)HttpCode.USER_UNDEF;
                result.RefText += sb.ToString();
            }
            finally
            {
                if (wReq != null)
                {
                    wReq.Abort();
                }
            }

            return result;
        }

        public HttpResult SendRequest(HttpRequestOptions reqOpts, List<IMiddleware> middlewares, Boolean binaryMode = false)
        {
            if (middlewares == null || middlewares.Count == 0)
            {
                return SendRequest(reqOpts, binaryMode);
            }

            List<IMiddleware> reversedMiddlewares = new List<IMiddleware>(middlewares);
            reversedMiddlewares.Reverse();
            DNextSend composedHandle = reversedMiddlewares.Aggregate<IMiddleware, DNextSend>(
                req => SendRequest(req, binaryMode),
                (handle, middleware) => request => middleware.Send(request, handle)
            );

            return composedHandle(reqOpts);
        }

        /// <summary>
        /// HTTP-GET 方法（不包含 headers）
        /// </summary>
        /// <param name="url">请求目标 URL</param>
        /// <param name="token">令牌(凭证)[可选 -> 设置为 null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-GET的响应结果</returns>
        public HttpResult Get(string url, string token, bool binaryMode = false)
        {
            return Get(url, null, token, binaryMode);
        }

        public HttpResult Get(string url, StringDictionary headers, Auth auth, bool binaryMode = false)
        {
            if (headers == null)
            {
                headers = new StringDictionary{
                    {"Content-Type", ContentType.WWW_FORM_URLENC}
                };
            }

            if (!headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = ContentType.WWW_FORM_URLENC;
            }
            
            addAuthHeaders(ref headers, auth);

            string token = auth.CreateManageTokenV2("GET", url, headers);
            return Get(url, headers, token, binaryMode);
        }

        /// <summary>
        /// HTTP-GET 方法
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="headers">请求 Headers[可选 -> 设置为 null]</param>
        /// <param name="token">令牌(凭证)[可选 -> 设置为 null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-GET的响应结果</returns>
        public HttpResult Get(string url, StringDictionary headers, string token, bool binaryMode = false)
        {
            return Get(url, headers, token, null, binaryMode);
        }

        public HttpResult Get(string url, StringDictionary headers, string token, List<IMiddleware> middlewares, bool binaryMode = false)
        {
            if (headers == null)
            {
                headers = new StringDictionary{
                    {"Content-Type", ContentType.WWW_FORM_URLENC}
                };
            }

            if (!headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = ContentType.WWW_FORM_URLENC;
            }

            HttpRequestOptions requestOptions = CreateHttpRequestOptions("GET", url, headers, token);
            return SendRequest(requestOptions, middlewares, binaryMode);
        }

        /// <summary>
        /// HTTP-POST 方法（不包含 headers，不包含 body 数据）
        /// </summary>
        /// <param name="url">请求目标 URL</param>
        /// <param name="token">令牌(凭证)[可选 -> 设置为 null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST 的响应结果</returns>
        public HttpResult Post(string url, string token, bool binaryMode = false)
        {
            return Post(url, null, token, binaryMode);
        }

        public HttpResult Post(string url, StringDictionary headers, Auth auth, bool binaryMode = false)
        {
            if (headers == null)
            {
                headers = new StringDictionary{
                    {"Content-Type", ContentType.WWW_FORM_URLENC}
                };
            }

            if (!headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = ContentType.WWW_FORM_URLENC;
            }

            addAuthHeaders(ref headers, auth);

            string token = auth.CreateManageTokenV2("POST", url, headers);
            return Post(url, headers, token, binaryMode);
        }

        /// <summary>
        /// HTTP-POST 方法（不包含 body 数据）
        /// </summary>
        /// <param name="url">请求目标 URL</param>
        /// <param name="token">令牌(凭证)[可选 -> 设置为 null]</param>
        /// <param name="headers">请求 Headers[可选 -> 设置为 null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST 的响应结果</returns>
        public HttpResult Post(string url, StringDictionary headers, string token, bool binaryMode = false)
        {
            HttpRequestOptions reqOpts = CreateHttpRequestOptions("POST", url, headers, token);
            return SendRequest(reqOpts, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含body数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(字节数据)</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostData(string url, byte[] data, string token, bool binaryMode = false)
        {
            return PostData(
                url,
                data,
                ContentType.APPLICATION_OCTET_STREAM,
                token,
                binaryMode
            );
        }

        /// <summary>
        /// HTTP-POST方法(包含body数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(字节数据)</param>
        /// <param name="mimeType">主体数据内容类型</param>
        /// <param name="token">令牌(凭证)[可选]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostData(string url, byte[] data, string mimeType, string token, bool binaryMode = false)
        {
            HttpRequestOptions reqOpts = CreateHttpRequestOptions("POST", url, null, token);

            reqOpts.Headers.Add("Content-Type", mimeType);
            if (data != null)
            {
                reqOpts.AllowWriteStreamBuffering = true;
                reqOpts.RequestData = data;
            }

            return SendRequest(reqOpts, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含JSON文本的body数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(JSON文本)</param>
        /// <param name="token">令牌(凭证)[可选]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostJson(string url, string data, string token, bool binaryMode = false)
        {
            byte[] utf8Data = null;
            if (!string.IsNullOrEmpty(data))
            {
                utf8Data = Encoding.UTF8.GetBytes(data);
            }
            return PostData(url, utf8Data, ContentType.APPLICATION_JSON, token, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含普通文本的body数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(普通文本)</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostText(string url, string data, string token, bool binaryMode = false)
        {
            byte[] utf8Data = null;
            if (!string.IsNullOrEmpty(data))
            {
                utf8Data = Encoding.UTF8.GetBytes(data);
            }
            return PostData(url, utf8Data, ContentType.TEXT_PLAIN, token, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含表单数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="kvData">键值对数据</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostForm(string url, Dictionary<string, string> kvData, string token, bool binaryMode = false)
        {
            string data = null;
            if (kvData != null)
            {
                data = string.Join("&",
                    kvData.Select(kvp => Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value)));
            }
            return PostForm(url, data, token, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含表单数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">表单数据</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostForm(string url, string data, string token, bool binaryMode = false)
        {
            byte[] utf8Data = null;
            if (!string.IsNullOrEmpty(data))
            {
                utf8Data = Encoding.UTF8.GetBytes(data);
            }
            return PostData(url, utf8Data, ContentType.WWW_FORM_URLENC, token, binaryMode);
        }

        public HttpResult PostForm(string url, StringDictionary headers, string data, Auth auth, bool binaryMode = false)
        {
            if (headers == null)
            {
                headers = new StringDictionary{
                    {"Content-Type", ContentType.WWW_FORM_URLENC}
                };
            }

            if (!headers.ContainsKey("Content-Type"))
            {
                headers["Content-Type"] = ContentType.WWW_FORM_URLENC;
            }
            
            addAuthHeaders(ref headers, auth);

            string token = auth.CreateManageTokenV2("POST", url, headers, data);
            return PostForm(url, headers, Encoding.UTF8.GetBytes(data), token, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含表单数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">表单数据</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostForm(string url, byte[] data, string token, bool binaryMode = false)
        {
            return PostForm(url, null, data, token, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含表单数据)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="headers">请求 Headers[可选 -> 设置为 null]</param>
        /// <param name="data">表单数据</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostForm(string url, StringDictionary headers, byte[] data, string token, bool binaryMode = false)
        {
            HttpRequestOptions reqOpts = CreateHttpRequestOptions("POST", url, headers, token);
            if (!reqOpts.Headers.ContainsKey("Content-Type"))
            {
                reqOpts.Headers.Add("Content-Type", ContentType.WWW_FORM_URLENC);
            }
            if (data != null)
            {
                reqOpts.AllowWriteStreamBuffering = true;
                reqOpts.RequestData = data;
            }

            return SendRequest(reqOpts, binaryMode);
        }

        /// <summary>
        /// HTTP-POST方法(包含多分部数据,multipart/form-data)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据</param>
        /// <param name="boundary">分界标志</param>
        /// <param name="token">令牌(凭证)[可选->设置为null]</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-POST的响应结果</returns>
        public HttpResult PostMultipart(string url, byte[] data, string boundary, string token, bool binaryMode = false)
        {
            HttpRequestOptions reqOpts = CreateHttpRequestOptions("POST", url, null, token);

            reqOpts.Headers.Add(
                "Content-Type",
                string.Format("{0}; boundary={1}", ContentType.MULTIPART_FORM_DATA, boundary)
            );

            if (data != null)
            {
                reqOpts.AllowWriteStreamBuffering = true;
                reqOpts.RequestData = data;
            }

            return SendRequest(reqOpts, binaryMode);
        }

        /// <summary>
        /// HTTP-PUT方法(包含body数据, headers)
        /// </summary>
        /// <param name="url">请求目标URL</param>
        /// <param name="data">主体数据(字节数据)</param>
        /// <param name="headers">上传设置的headers</param>
        /// <param name="binaryMode">是否以二进制模式读取响应内容(默认:否，即表示以文本方式读取)</param>
        /// <returns>HTTP-PUT的响应结果</returns>
        public HttpResult PutDataWithHeaders(string url, byte[] data, Dictionary<string, string> headers, bool binaryMode = false)
        {
            // converse headers type for compaction
            StringDictionary headersDict = new StringDictionary();
            foreach (KeyValuePair<string, string> kvp in headers)
            {
                headersDict.Add(kvp.Key, kvp.Value);
            }

            HttpRequestOptions wReq = CreateHttpRequestOptions("PUT", url, headersDict);

            wReq.Headers.Add("Content-Type", ContentType.APPLICATION_OCTET_STREAM);

            if (data != null)
            {
                wReq.AllowWriteStreamBuffering = true;
                wReq.RequestData = data;
            }

            return SendRequest(wReq, binaryMode);
        }

        /// <summary>
        /// 获取返回信息头
        /// </summary>
        /// <param name="hr">即将被HTTP请求封装函数返回的HttpResult变量</param>
        /// <param name="resp">正在被读取的HTTP响应</param>
        private void getHeaders(ref HttpResult hr, HttpWebResponse resp)
        {
            if (resp != null)
            {
                if (hr.RefInfo == null)
                {
                    hr.RefInfo = new Dictionary<string, string>();
                }

                hr.RefInfo.Add("ProtocolVersion", resp.ProtocolVersion.ToString());

                if (!string.IsNullOrEmpty(resp.CharacterSet))
                {
                    hr.RefInfo.Add("Characterset", resp.CharacterSet);
                }

                if (!string.IsNullOrEmpty(resp.ContentEncoding))
                {
                    hr.RefInfo.Add("ContentEncoding", resp.ContentEncoding);
                }

                if (!string.IsNullOrEmpty(resp.ContentType))
                {
                    hr.RefInfo.Add("ContentType", resp.ContentType);
                }

                hr.RefInfo.Add("ContentLength", resp.ContentLength.ToString());

                var headers = resp.Headers;
                if (headers != null && headers.Count > 0)
                {
                    if (hr.RefInfo == null)
                    {
                        hr.RefInfo = new Dictionary<string, string>();
                    }
                    foreach (var key in headers.AllKeys)
                    {
                        hr.RefInfo.Add(key, headers[key]);
                    }
                }
            }
        }


        private void addAuthHeaders(ref StringDictionary headers, Auth auth)
        {
            string xQiniuDate = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
            string xQiniuDateDisableEnv = Environment.GetEnvironmentVariable("DISABLE_QINIU_TIMESTAMP_SIGNATURE");
            if (auth.AuthOptions.DisableQiniuTimestampSignature is bool disableQiniuTimestampSignature)
            {
                if (!disableQiniuTimestampSignature)
                {
                    headers["X-Qiniu-Date"] = xQiniuDate;
                }
            }
            else if (!String.IsNullOrEmpty(xQiniuDateDisableEnv))
            {
                if (xQiniuDateDisableEnv.ToLower() != "true")
                {
                    headers["X-Qiniu-Date"] = xQiniuDate;
                }
            }
            else
            {
                headers["X-Qiniu-Date"] = xQiniuDate;
            }
        }
    }
}