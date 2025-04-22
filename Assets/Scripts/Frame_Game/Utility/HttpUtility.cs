using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using static FrameBaseUtility;

// 封装的Http的相关操作,因为其中全是静态工具函数,所以名字为Utility,但是由于需要管理一些线程,所以与普通的工具函数类不同
public class HttpUtility
{
	// 同步get请求
	public static string httpGet(string url, out WebExceptionStatus status, out HttpStatusCode code, Dictionary<string, string> paramList, Dictionary<string, string> header)
	{
		return httpRequest(prepareGet(url, paramList, header, "application/x-www-form-urlencoded"), url, out status, out code);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static HttpWebRequest prepareGet(string url, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType, int timeout = 10000)
	{
		// 根据url地址创建HttpWebRequest对象
		url += generateGetParams(paramList);
		var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		webRequest.Method = "GET";
		webRequest.KeepAlive = true;
		webRequest.ProtocolVersion = HttpVersion.Version11;
		webRequest.ContentType = contentType;
		webRequest.AllowAutoRedirect = true;
		webRequest.MaximumAutomaticRedirections = 2;
		webRequest.Timeout = timeout;
		if (header != null)
		{
			foreach (var item in header)
			{
				webRequest.Headers.Add(item.Key, item.Value);
			}
		}
		return webRequest;
	}
	// 同步Http请求
	protected static string httpRequest(HttpWebRequest webRequest, string url, out WebExceptionStatus status, out HttpStatusCode code)
	{
		if (isWebGL())
		{
			logErrorBase("无法在WebGL平台使用C#的Http请求函数");
		}
		code = HttpStatusCode.OK;
		try
		{
			string str = null;
			using var response = (HttpWebResponse)webRequest.GetResponse();
			code = response.StatusCode;
			if (code == HttpStatusCode.OK || code == HttpStatusCode.NoContent)
			{
				status = WebExceptionStatus.Success;
				using StreamReader reader = new(response.GetResponseStream(), Encoding.UTF8);
				str = reader.ReadToEnd();
			}
			else
			{
				status = WebExceptionStatus.UnknownError;
				logWarningBase("http post result error, code:" + code + ", url:" + url);
			}
			webRequest.Abort();
			return str;
		}
		catch (WebException e)
		{
			status = e.Status;
			logWarningBase("http request web exception:" + e.Message + ", status:" + status + ", code:" + code + ", url:" + url);
		}
		catch (Exception e)
		{
			status = WebExceptionStatus.UnknownError;
			logWarningBase("http request exception:" + e.Message + ", code:" + code + ", url:" + url);
		}
		return null;
	}
	protected static string generateGetParams(Dictionary<string, string> paramList)
	{
		if (paramList.isEmpty())
		{
			return "";
		}
		int count = paramList.Count;
		StringBuilder parameters = new();
		parameters.Append("?");
		// 从集合中取出所有参数，设置表单参数（AddField())
		int index = 0;
		foreach (var item in paramList)
		{
			parameters.Append(item.Key);
			parameters.Append("=");
			parameters.Append(item.Value);
			if (index++ != count - 1)
			{
				parameters.Append('&');
			}
		}
		return parameters.ToString();
	}
}