using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using UnityEngine;

// 封装的Http的相关操作,因为其中全是静态工具函数,所以名字为Utility,但是由于需要管理一些线程,所以与普通的工具函数类不同
public class HttpUtility : FrameSystem
{
	protected static List<Thread> mHttpThreadList;  // 线程列表
	protected static ThreadLock ThreadListLock;     // 线程列表的锁
	public HttpUtility()
	{
		mHttpThreadList = new List<Thread>();
		ThreadListLock = new ThreadLock();
	}
	public override void destroy()
	{
		ThreadListLock?.waitForUnlock();
		int count = mHttpThreadList.Count;
		for (int i = 0; i < count; ++i)
		{
			mHttpThreadList[i].Abort();
		}
		mHttpThreadList.Clear();
		mHttpThreadList = null;
		ThreadListLock?.unlock();
		base.destroy();
	}
	// 同步下载文件
	public static byte[] downloadFile(string url, int offset = 0, byte[] helperBytes = null, string fileName = EMPTY,
										StartDownloadCallback startCallback = null, DownloadingCallback downloading = null)
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		logForce("开始http下载:" + url);
#else
		logForce("开始http下载:" + getFileName(url));
#endif
		try
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.AddRange(offset);
			WebResponse response = request.GetResponse();
			long fileSize = response.ContentLength + offset;
			// response.ContentLength只是剩余需要下载的长度,需要加上下载起始偏移才是文件的真实大小
			startCallback?.Invoke(fileName, fileSize);
			Stream inStream = response.GetResponseStream();
			var downloadStream = new MemoryStream();
			bool isTempHelperBytes = helperBytes == null;
			if (helperBytes == null)
			{
				ARRAY_THREAD(out helperBytes, 1024);
			}
			int readCount;
			do
			{
				// 从输入流中读取数据放入内存中
				readCount = inStream.Read(helperBytes, 0, helperBytes.Length);
				downloadStream.Write(helperBytes, 0, readCount);
				downloading?.Invoke(fileName, fileSize, downloadStream.Length);
			} while (readCount > 0);
			if (isTempHelperBytes)
			{
				UN_ARRAY_THREAD(helperBytes);
			}
			byte[] dataBytes = downloadStream.ToArray();
			downloadStream.Close();
			inStream.Close();
			response.Close();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logForce("http下载完成:" + url);
#else
			logForce("http下载完成:" + getFileName(url));
#endif
			return dataBytes;
		}
		catch (Exception e)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logForce("http下载失败:" + e.Message + ", url:" + url);
#else
			logForce("http下载失败:" + e.Message + ", url:" + getFileName(url));
#endif
		}
		return null;
	}
	public static string delete(string url, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType, Action<string> callback)
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
		return httpRequest(webRequest, url, callback);
	}
	public static string postFile(string url, List<FormItem> formList)
	{
		return postFile(url, formList, null);
	}
	public static string postFile(string url, List<FormItem> formList, Action<string> callback)
	{
		// 以模拟表单的形式上传数据
		string boundary = DateTime.Now.Ticks.ToString("x");
		var postStream = new MemoryStream();
		if (formList != null)
		{
			int count = formList.Count;
			for (int i = 0; i < count; ++i)
			{
				formList[i].write(postStream, boundary);
			}
		}
		// 结尾
		byte[] footer = stringToBytes("\r\n--" + boundary + "--\r\n");
		postStream.Write(footer, 0, footer.Length);
		// 发送请求
		string contentType = "multipart/form-data; boundary=" + boundary;
		string ret = post(url, postStream.GetBuffer(), (int)postStream.Length, contentType, null, callback);
		postStream.Close();
		return ret;
	}
	public static string post(string url, byte[] data, int dataLength, string contentType, Dictionary<string, string> header, Action<string> callback)
	{
		// 确认数据长度
		if (dataLength < 0)
		{
			if (data != null)
			{
				dataLength = data.Length;
			}
			else
			{
				dataLength = 0;
			}
		}

		// 初始化新的webRequst
		// 创建httpWebRequest对象
		ServicePointManager.ServerCertificateValidationCallback = myRemoteCertificateValidationCallback;
		var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		// 初始化HttpWebRequest对象
		webRequest.Method = "POST";
		if (header != null)
		{
			foreach (var item in header)
			{
				webRequest.Headers.Add(item.Key, item.Value);
			}
		}
		webRequest.ContentType = contentType;
		webRequest.ContentLength = dataLength;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.Timeout = 10000;
		webRequest.AllowAutoRedirect = true;
		// 附加要POST给服务器的数据到HttpWebRequest对象,附加POST数据的过程比较特殊
		// 它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面
		// 创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
		if (data != null)
		{
			Stream newStream = webRequest.GetRequestStream();
			newStream.Write(data, 0, dataLength);
			newStream.Close();
		}

		return httpRequest(webRequest, url, callback);
	}
	public static string post(string url, byte[] data, int dataLength = -1, Action<string> callback = null)
	{
		return post(url, data, dataLength, "application/x-www-form-urlencoded", null, callback);
	}
	public static string post(string url, string param, Action<string> callback = null)
	{
		return post(url, stringToBytes(param), -1, "application/x-www-form-urlencoded", null, callback);
	}
	public static string post(string url, string param, string contentType, Action<string> callback = null)
	{
		return post(url, stringToBytes(param), -1, contentType, null, callback);
	}
	public static string post(string url, string param, string contentType, Dictionary<string, string> header)
	{
		return post(url, stringToBytes(param), -1, contentType, header, null);
	}
	public static string post(string url, Dictionary<string, string> header, Action<string> callback)
	{
		return post(url, null, -1, "application/x-www-form-urlencoded", header, null);
	}
	public static string get(string url)
	{
		return get(url, null, null, "application/x-www-form-urlencoded", null);
	}
	public static string get(string url, string contentType)
	{
		return get(url, null, null, contentType, null);
	}
	public static string get(string url, Dictionary<string, string> paramList)
	{
		return get(url, paramList, null, "application/x-www-form-urlencoded", null);
	}
	public static string get(string url, Dictionary<string, string> paramList, Action<string> callback)
	{
		return get(url, paramList, null, "application/x-www-form-urlencoded", callback);
	}
	public static string get(string url, Action<string> callback)
	{
		return get(url, null, null, "application/x-www-form-urlencoded", callback);
	}
	public static string get(string url, Dictionary<string, string> paramList, Dictionary<string, string> header, string contentType, Action<string> callback)
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
		webRequest.Timeout = 10000;
		if (header != null)
		{
			foreach (var item in header)
			{
				webRequest.Headers.Add(item.Key, item.Value);
			}
		}
		return httpRequest(webRequest, url, callback);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static string httpRequest(HttpWebRequest webRequest, string url, Action<string> callback)
	{
		// 异步
		if (callback != null)
		{
			var threadParam = new RequestThreadParam();
			threadParam.mRequest = webRequest;
			threadParam.mCallback = callback;
			threadParam.mFullURL = url;
			Thread httpThread = new Thread(waitResponse);
			threadParam.mThread = httpThread;
			httpThread.Start(threadParam);
			httpThread.IsBackground = true;
			ThreadListLock?.waitForUnlock();
			mHttpThreadList?.Add(httpThread);
			ThreadListLock?.unlock();
		}
		// 同步
		else
		{
			try
			{
				var response = (HttpWebResponse)webRequest.GetResponse();
				var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				string str = reader.ReadToEnd();
				reader.Close();
				response.Close();
				webRequest.Abort();
				return str;
			}
			catch (Exception e)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logForce("http request exception:" + e.Message + ", url:" + url);
#else
				logForce("http request exception:" + e.Message + ", url:" + getFileName(url));
#endif
			}
		}
		return null;
	}
	protected static void waitResponse(object param)
	{
		var threadParam = (RequestThreadParam)param;
		try
		{
			// 读取服务器的返回信息
			var response = (HttpWebResponse)threadParam.mRequest.GetResponse();
			var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

			// 延迟到主线程执行
			delayCallThread(threadParam.mCallback, reader.ReadToEnd());

			reader.Close();
			response.Close();
			threadParam.mRequest.Abort();
			threadParam.mRequest = null;
		}
		catch (Exception e)
		{
			delayCallThread(threadParam.mCallback, null);
			log("http post result exception:" + e.Message + ", url:" + threadParam.mFullURL);
		}
		finally
		{
			ThreadListLock?.waitForUnlock();
			mHttpThreadList?.Remove(threadParam.mThread);
			ThreadListLock?.unlock();
		}
	}
	protected static string generateGetParams(Dictionary<string, string> paramList)
	{
		if (paramList == null || paramList.Count == 0)
		{
			return EMPTY;
		}
		int count = paramList.Count;
		MyStringBuilder parameters = STRING("?");
		// 从集合中取出所有参数，设置表单参数（AddField())
		int index = 0;
		foreach (var item in paramList)
		{
			parameters.append(item.Key, "=", item.Value);
			if (index != count - 1)
			{
				parameters.append('&');
			}
			++index;
		}
		return END_STRING(parameters);
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
			chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
			if (!chain.Build((X509Certificate2)certificate))
			{
				return false;
			}
		}
		return true;
	}
}