using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using UnityEngine.Networking;
using Newtonsoft.Json;
using static UnityUtility;
using static FrameUtility;
using static BinaryUtility;
using static StringUtility;
using static FrameBase;

// 封装的Http的相关操作,因为其中全是静态工具函数,所以名字为Utility,但是由于需要管理一些线程,所以与普通的工具函数类不同
public class HttpUtility
{
	// 同步下载文件
	public static byte[] downloadFile(string url, int offset = 0, byte[] helperBytes = null, string fileName = EMPTY,
										StartDownloadCallback startCallback = null, DownloadingCallback downloading = null)
	{
		log("开始http下载:" + url);
		try
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.AddRange(offset);
			using WebResponse response = request.GetResponse();
			long fileSize = response.ContentLength + offset;
			// response.ContentLength只是剩余需要下载的长度,需要加上下载起始偏移才是文件的真实大小
			startCallback?.Invoke(fileName, fileSize);
			using Stream inStream = response.GetResponseStream();
			byte[] downloadStream = new byte[fileSize];
			int writeCount = 0;
			bool isTempHelperBytes = helperBytes == null;
			if (helperBytes == null)
			{
				ARRAY_BYTE_THREAD(out helperBytes, 1024);
			}
			int readCount;
			do
			{
				// 从输入流中读取数据放入内存中
				readCount = inStream.Read(helperBytes, 0, helperBytes.Length);
				writeBytes(downloadStream, ref writeCount, helperBytes, -1, -1, readCount);
				downloading?.Invoke(fileName, fileSize, writeCount);
			} while (readCount > 0);
			if (isTempHelperBytes)
			{
				UN_ARRAY_BYTE_THREAD(ref helperBytes);
			}
			log("http下载完成:" + url);
			return downloadStream;
		}
		catch (Exception e)
		{
			log("http下载失败:" + e.Message + ", url:" + url);
		}
		return null;
	}
	// 同步删除文件
	public static string httpDelete(string url, out WebExceptionStatus status, out HttpStatusCode code, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType)
	{
		return httpRequest(prepareDelete(url, paramList, header, contentType), url, out status, out code);
	}
	// 异步删除文件
	public static void httpDeleteAsync(string url, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType, HttpCallback callback)
	{
		httpRequestAsync(prepareDelete(url, paramList, header, contentType), url, callback);
	}
	// 同步上传文件
	public static string httpPostFile(string url, out WebExceptionStatus status, out HttpStatusCode code, List<FormItem> formList)
	{
		if (formList == null)
		{
			status = WebExceptionStatus.UnknownError;
			code = HttpStatusCode.OK;
			return null;
		}
		using MemoryStream postStream = new();
		preparePostFile(postStream, formList, out string contentType);
		// 发送请求
		return httpPost(url, out status, out code, postStream.GetBuffer(), (int)postStream.Length, contentType, null);
	}
	// 异步上传文件
	public static void httpPostFileAsync(string url, List<FormItem> formList, HttpCallback callback)
	{
		if (formList == null)
		{
			callback?.Invoke(null, WebExceptionStatus.UnknownError, HttpStatusCode.OK);
			return;
		}
		using MemoryStream postStream = new();
		preparePostFile(postStream, formList, out string contentType);
		// 发送请求
		httpPostAsync(url, postStream.GetBuffer(), (int)postStream.Length, contentType, null, callback);
	}
	// 同步post请求
	public static string httpPost(string url, out WebExceptionStatus status, out HttpStatusCode code, byte[] data, int dataLength, string contentType, Dictionary<string, string> header)
	{
		return httpRequest(preparePost(url, data, dataLength, contentType, header), url, out status, out code);
	}
	// 异步post请求
	public static void httpPostAsync(string url, byte[] data, int dataLength, string contentType, Dictionary<string, string> header, HttpCallback callback)
	{
		httpRequestAsync(preparePost(url, data, dataLength, contentType, header), url, callback);
	}
	// 异步post请求
	public static void httpPostAsyncWebGL(string url, byte[] data, string contentType, Dictionary<string, string> header, UnityHttpCallback callback)
	{
		mGameFramework.StartCoroutine(sendPostRequest(url, data, contentType, header, callback));
	}
	// 同步post请求
	public static string httpPost(string url, out WebExceptionStatus status, out HttpStatusCode code, byte[] data, int dataLength = -1)
	{
		return httpPost(url, out status, out code, data, dataLength, "application/x-www-form-urlencoded", null);
	}
	// 同步post请求
	public static string httpPost(string url, out WebExceptionStatus status, out HttpStatusCode code, string param)
	{
		return httpPost(url, out status, out code, stringToBytes(param), -1, "application/x-www-form-urlencoded", null);
	}
	// 同步post请求
	public static string httpPost(string url, out WebExceptionStatus status, out HttpStatusCode code, string param, string contentType)
	{
		return httpPost(url, out status, out code, stringToBytes(param), -1, contentType, null);
	}
	// 同步post请求
	public static string httpPost(string url, out WebExceptionStatus status, out HttpStatusCode code, string param, string contentType, Dictionary<string, string> header)
	{
		return httpPost(url, out status, out code, stringToBytes(param), -1, contentType, header);
	}
	// 同步post请求
	public static string httpPost(string url, out WebExceptionStatus status, out HttpStatusCode code, Dictionary<string, string> header)
	{
		return httpPost(url, out status, out code, null, -1, "application/x-www-form-urlencoded", header);
	}
	// 异步post请求
	public static void httpPostAsync(string url, byte[] data, int dataLength = -1, HttpCallback callback = null)
	{
		httpPostAsync(url, data, dataLength, "application/x-www-form-urlencoded", null, callback);
	}
	// 异步post请求
	public static void httpPostAsync(string url, string param, HttpCallback callback = null)
	{
		httpPostAsync(url, stringToBytes(param), -1, "application/x-www-form-urlencoded", null, callback);
	}
	// 异步post请求
	public static void httpPostAsync(string url, string param, string contentType, HttpCallback callback = null)
	{
		httpPostAsync(url, stringToBytes(param), -1, contentType, null, callback);
	}
	// 异步post请求
	public static void httpPostAsync(string url, string param, string contentType, Dictionary<string, string> header, HttpCallback callback = null)
	{
		httpPostAsync(url, stringToBytes(param), -1, contentType, header, callback);
	}
	// 异步post请求
	public static void httPostAsync(string url, Dictionary<string, string> header, HttpCallback callback)
	{
		httpPostAsync(url, null, -1, "application/x-www-form-urlencoded", header, callback);
	}
	// 同步get请求
	public static T httpGet<T>(string url, out WebExceptionStatus status, out HttpStatusCode code) where T : class
	{
		string str = httpGet(url, out status, out code, null, null, "application/x-www-form-urlencoded");
		if (str.isEmpty())
		{
			return null;
		}
		return JsonConvert.DeserializeObject<T>(str);
	}
	// 同步get请求
	public static string httpGet(string url, out WebExceptionStatus status, out HttpStatusCode code)
	{
		return httpGet(url, out status, out code, null, null, "application/x-www-form-urlencoded");
	}
	// 同步get请求
	public static string httpGet(string url, out WebExceptionStatus status, out HttpStatusCode code, string contentType)
	{
		return httpGet(url, out status, out code, null, null, contentType);
	}
	// 同步get请求
	public static string httpGet(string url, out WebExceptionStatus status, out HttpStatusCode code, Dictionary<string, string> paramList)
	{
		return httpGet(url, out status, out code, paramList, null, "application/x-www-form-urlencoded");
	}
	// 同步get请求
	public static string httpGet(string url, out WebExceptionStatus status, out HttpStatusCode code, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType)
	{
		return httpRequest(prepareGet(url, paramList, header, contentType), url, out status, out code);
	}
	// 异步get请求
	public static void httpGetAsyncWithParam(string url, Dictionary<string, string> paramList, HttpCallback callback)
	{
		httpGetAsync(url, paramList, null, "application/x-www-form-urlencoded", callback, 10000);
	}
	public static void httpGetAsyncWithHeader(string url, Dictionary<string, string> header, HttpCallback callback)
	{
		httpGetAsync(url, null, header, "application/x-www-form-urlencoded", callback, 10000);
	}
	// 异步get请求
	public static void httpGetAsync(string url, HttpCallback callback)
	{
		httpGetAsync(url, null, null, "application/x-www-form-urlencoded", callback, 10000);
	}
	// 异步get请求
	public static void httpGetAsync(string url, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType, HttpCallback callback, int timeout)
	{
		httpRequestAsync(prepareGet(url, paramList, header, contentType, timeout), url, callback);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static HttpWebRequest prepareDelete(string url, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType)
	{
		// 根据url地址创建HttpWebRequest对象
		url += generateGetParams(paramList);
		var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		webRequest.Method = "DELETE";
		webRequest.KeepAlive = true;
		webRequest.ProtocolVersion = HttpVersion.Version11;
		webRequest.ContentType = contentType;
		webRequest.AllowAutoRedirect = true;
		webRequest.MaximumAutomaticRedirections = 2;
		webRequest.Timeout = 10000;
		if (header != null)
		{
			foreach (var item in header)
			{
				webRequest.Headers.Add(item.Key, item.Value);
			}
		}
		return webRequest;
	}
	protected static void preparePostFile(MemoryStream stream, List<FormItem> formList, out string contentType)
	{
		// 以模拟表单的形式上传数据
		string boundary = DateTime.Now.Ticks.ToString("x");
		foreach (FormItem item in formList.safe())
		{
			item.write(stream, boundary);
		}
		// 结尾
		byte[] footer = stringToBytes("\r\n--" + boundary + "--\r\n");
		stream.Write(footer, 0, footer.Length);
		contentType = "multipart/form-data; boundary=" + boundary;
	}
	protected static HttpWebRequest preparePostWebGL(string url, byte[] data, int dataLength, string contentType, Dictionary<string, string> header, int timeout = 10000)
	{
		// 确认数据长度
		if (dataLength < 0)
		{
			dataLength = data.count();
		}

		// 初始化新的webRequst
		// 创建httpWebRequest对象
		ServicePointManager.ServerCertificateValidationCallback = myRemoteCertificateValidationCallback;
		var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		// 初始化HttpWebRequest对象
		webRequest.Method = "POST";
		foreach (var item in header.safe())
		{
			webRequest.Headers.Add(item.Key, item.Value);
		}
		webRequest.ContentType = contentType;
		webRequest.ContentLength = dataLength;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.Timeout = timeout;
		webRequest.AllowAutoRedirect = true;
		// 附加要POST给服务器的数据到HttpWebRequest对象,附加POST数据的过程比较特殊
		// 它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面
		// 创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
		if (data != null)
		{
			using Stream newStream = webRequest.GetRequestStream();
			newStream.Write(data, 0, dataLength);
		}
		return webRequest;
	}
	protected static HttpWebRequest preparePost(string url, byte[] data, int dataLength, string contentType, Dictionary<string, string> header, int timeout = 10000)
	{
		// 确认数据长度
		if (dataLength < 0)
		{
			dataLength = data.count();
		}

		// 初始化新的webRequst
		// 创建httpWebRequest对象
		ServicePointManager.ServerCertificateValidationCallback = myRemoteCertificateValidationCallback;
		var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		// 初始化HttpWebRequest对象
		webRequest.Method = "POST";
		foreach (var item in header.safe())
		{
			webRequest.Headers.Add(item.Key, item.Value);
		}
		webRequest.ContentType = contentType;
		webRequest.ContentLength = dataLength;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.Timeout = timeout;
		webRequest.AllowAutoRedirect = true;
		// 附加要POST给服务器的数据到HttpWebRequest对象,附加POST数据的过程比较特殊
		// 它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面
		// 创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
		if (data != null)
		{
			using Stream newStream = webRequest.GetRequestStream();
			newStream.Write(data, 0, dataLength);
		}
		return webRequest;
	}
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
		foreach (var item in header.safe())
		{
			webRequest.Headers.Add(item.Key, item.Value);
		}
		return webRequest;
	}
	// 同步Http请求
	protected static string httpRequest(HttpWebRequest webRequest, string url, out WebExceptionStatus status, out HttpStatusCode code)
	{
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
				logWarning("http post result error, code:" + code + ", url:" + url);
			}
			webRequest.Abort();
			return str;
		}
		catch (WebException e)
		{
			status = e.Status;
			logWarning("http request web exception:" + e.Message + ", status:" + status + ", code:" + code + ", url:" + url);
		}
		catch (Exception e)
		{
			status = WebExceptionStatus.UnknownError;
			logWarning("http request exception:" + e.Message + ", code:" + code + ", url:" + url);
		}
		return null;
	}
	protected async static void httpRequestAsync(HttpWebRequest webRequest, string url, HttpCallback callback)
	{
		WebExceptionStatus status;
		HttpStatusCode statusCode = HttpStatusCode.OK;
		string resStr = null;
		try
		{
			var httpResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
			statusCode = httpResponse.StatusCode;
			// 读取服务器的返回信息
			if (statusCode == HttpStatusCode.OK)
			{
				using StreamReader reader = new(httpResponse.GetResponseStream(), Encoding.UTF8);
				resStr = reader.ReadToEnd();
				status = WebExceptionStatus.Success;
			}
			else
			{
				resStr = null;
				status = WebExceptionStatus.UnknownError;
				logWarning("http post result error, code:" + statusCode + ", url:" + url);
			}
		}
		catch (WebException e)
		{
			status = e.Status;
			logWarning("http post result web exception:" + e.Message + ", status:" + e.Status + ", code:" + statusCode + ", url:" + url);
		}
		catch (Exception e)
		{
			status = WebExceptionStatus.UnknownError;
			logWarning("http post result exception:" + e.Message + ", statusCode:" + statusCode + ", url:" + url);
		}
		if (mGameFramework != null && !mGameFramework.isDestroy())
		{
			callback?.Invoke(resStr, status, statusCode);
		}
		webRequest.Abort();
	}
	protected async static void httpRequestAsyncWebGL(HttpWebRequest webRequest, string url, HttpCallback callback)
	{
		WebExceptionStatus status;
		HttpStatusCode statusCode = HttpStatusCode.OK;
		string resStr = null;
		try
		{
			var httpResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
			statusCode = httpResponse.StatusCode;
			// 读取服务器的返回信息
			if (statusCode == HttpStatusCode.OK)
			{
				using StreamReader reader = new(httpResponse.GetResponseStream(), Encoding.UTF8);
				resStr = reader.ReadToEnd();
				status = WebExceptionStatus.Success;
			}
			else
			{
				resStr = null;
				status = WebExceptionStatus.UnknownError;
				logWarning("http post result error, code:" + statusCode + ", url:" + url);
			}
		}
		catch (WebException e)
		{
			status = e.Status;
			logWarning("http post result web exception:" + e.Message + ", status:" + e.Status + ", code:" + statusCode + ", url:" + url);
		}
		catch (Exception e)
		{
			status = WebExceptionStatus.UnknownError;
			logWarning("http post result exception:" + e.Message + ", statusCode:" + statusCode + ", url:" + url);
		}
		if (mGameFramework != null && !mGameFramework.isDestroy())
		{
			callback?.Invoke(resStr, status, statusCode);
		}
		webRequest.Abort();
	}
	protected static string generateGetParams(Dictionary<string, string> paramList)
	{
		if (paramList.isEmpty())
		{
			return EMPTY;
		}
		int count = paramList.Count;
		using var a = new MyStringBuilderScope(out var parameters);
		parameters.append("?");
		// 从集合中取出所有参数，设置表单参数（AddField())
		int index = 0;
		foreach (var item in paramList)
		{
			parameters.append(item.Key, "=", item.Value);
			if (index++ != count - 1)
			{
				parameters.append('&');
			}
		}
		return parameters.ToString();
	}
	protected static IEnumerator sendPostRequest(string url, byte[] data, string contentType, Dictionary<string, string> header, UnityHttpCallback callback)
	{
		// 创建一个UnityWebRequest对象，指定为POST请求
		UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbPOST);
		request.uploadHandler = new UploadHandlerRaw(data);
		// 设置Content-Type头为application/json
		request.SetRequestHeader("Content-Type", contentType);
		request.timeout = 10;
		foreach (var item in header)
		{
			request.SetRequestHeader(item.Key, item.Value);
		}
		// 设置下载处理器
		request.downloadHandler = new DownloadHandlerBuffer();
		// 发送请求并等待完成
		yield return request.SendWebRequest();
		callback?.Invoke(request.downloadHandler.text, request.result, request.responseCode);
	}
	protected static bool myRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		// If there are errors in the certificate chain,
		// look at each error to determine the cause.
		if (sslPolicyErrors == SslPolicyErrors.None)
		{
			return true;
		}
		for (int i = 0; i < chain.ChainStatus.Length; i++)
		{
			if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
			{
				continue;
			}
			chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
			chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
			chain.ChainPolicy.UrlRetrievalTimeout = new(0, 1, 0);
			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
			if (!chain.Build((X509Certificate2)certificate))
			{
				return false;
			}
		}
		return true;
	}
}