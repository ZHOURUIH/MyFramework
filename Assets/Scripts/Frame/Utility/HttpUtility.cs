using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using LitJson;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class HttpUtility : FrameSystem
{
	protected static List<Thread> mHttpThreadList;
	protected static ThreadLock ThreadListLock;
	public HttpUtility()
	{
		mHttpThreadList = new List<Thread>();
		ThreadListLock = new ThreadLock();
	}
	public override void destroy()
	{
		ThreadListLock.waitForUnlock();
		int count = mHttpThreadList.Count;
		for (int i = 0; i < count; ++i)
		{
			mHttpThreadList[i].Abort();
		}
		mHttpThreadList.Clear();
		mHttpThreadList = null;
		ThreadListLock.unlock();
		base.destroy();
	}
	// 同步下载文件
	public static byte[] downloadFile(string url, int offset = 0, byte[] helperBytes = null, string fileName = EMPTY,
										StartDownloadCallback startCallback = null, DownloadingCallback downloading = null)
	{
		logForce("开始http下载:" + url);
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
		logForce("http下载完成:" + url);
		return dataBytes;
	}
	public static string httpPostFile(string url, List<FormItem> itemList, Action<string, object> callback, object callbakcUserData, bool logError)
	{
		// 以模拟表单的形式上传数据
		string boundary = "----" + DateTime.Now.Ticks.ToString("x");
		string fileFormdataTemplate = "\r\n--" + boundary + "\r\n" +
									"Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + "\r\n" +
									"Content-Type: application/octet-stream" + "\r\n\r\n";
		string dataFormdataTemplate = "\r\n--" + boundary + "\r\n" +
									"Content-Disposition: form-data; name=\"{0}\"" + "\r\n\r\n{1}";
		MemoryStream postStream = new MemoryStream();
		int count = itemList.Count;
		for (int i = 0; i < count; ++i)
		{
			FormItem item = itemList[i];
			string formdata = null;
			if (item.mFileContent != null)
			{
				formdata = string.Format(fileFormdataTemplate, "fileContent", item.mFileName);
			}
			else
			{
				formdata = string.Format(dataFormdataTemplate, item.mKey, item.mValue);
			}
			// 统一处理
			byte[] formdataBytes = null;
			// 第一行不需要换行
			if (postStream.Length == 0)
			{
				formdataBytes = stringToBytes(formdata.Substring(2, formdata.Length - 2), Encoding.UTF8);
			}
			else
			{
				formdataBytes = stringToBytes(formdata, Encoding.UTF8);
			}
			postStream.Write(formdataBytes, 0, formdataBytes.Length);
			// 写入文件内容
			if (item.mFileContent != null && item.mFileContent.Length > 0)
			{
				postStream.Write(item.mFileContent, 0, item.mFileContent.Length);
			}
		}
		// 结尾
		byte[] footer = stringToBytes("\r\n--" + boundary + "--\r\n", Encoding.UTF8);
		postStream.Write(footer, 0, footer.Length);

		byte[] postBytes = postStream.ToArray();
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
		var webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		webRequest.Method = "POST";
		webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
		webRequest.Timeout = 10000;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.ContentLength = postBytes.Length;
		// 异步
		if (callback != null)
		{
			var threadParam = new RequestThreadParam();
			threadParam.mRequest = webRequest;
			threadParam.mByteArray = postBytes;
			threadParam.mCallback = callback;
			threadParam.mUserData = callbakcUserData;
			threadParam.mFullURL = url;
			threadParam.mLogError = logError;
			Thread httpThread = new Thread(waitHttpPost);
			threadParam.mThread = httpThread;
			httpThread.Start(threadParam);
			httpThread.IsBackground = true;
			ThreadListLock.waitForUnlock();
			mHttpThreadList.Add(httpThread);
			ThreadListLock.unlock();
			return null;
		}
		// 同步
		else
		{
			try
			{
				// 附加要POST给服务器的数据到HttpWebRequest对象(附加POST数据的过程比较特殊，它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面。)
				// 创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
				Stream newStream = webRequest.GetRequestStream();
				newStream.Write(postBytes, 0, postBytes.Length);
				newStream.Close();
				// 读取服务器的返回信息
				var response = (HttpWebResponse)webRequest.GetResponse();
				var php = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				string str = php.ReadToEnd();
				php.Close();
				response.Close();
				return str;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
	public static string httpPost(string url, byte[] data, string contentType, Dictionary<string, string> header, Action<string, object> callback, object callbakcUserData, bool logError)
	{
		// 初始化新的webRequst
		// 创建httpWebRequest对象
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
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
		webRequest.ContentLength = data.Length;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.Timeout = 10000;
		// 异步
		if (callback != null)
		{
			var threadParam = new RequestThreadParam();
			threadParam.mRequest = webRequest;
			threadParam.mByteArray = data;
			threadParam.mCallback = callback;
			threadParam.mUserData = callbakcUserData;
			threadParam.mFullURL = url;
			threadParam.mLogError = logError;
			Thread httpThread = new Thread(waitHttpPost);
			threadParam.mThread = httpThread;
			httpThread.Start(threadParam);
			httpThread.IsBackground = true;
			ThreadListLock.waitForUnlock();
			mHttpThreadList.Add(httpThread);
			ThreadListLock.unlock();
		}
		// 同步
		else
		{
			try
			{
				// 附加要POST给服务器的数据到HttpWebRequest对象(附加POST数据的过程比较特殊，它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面。)
				// 创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
				Stream newStream = webRequest.GetRequestStream();
				newStream.Write(data, 0, data.Length);
				newStream.Close();
				// 读取服务器的返回信息
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
				logForce("http exception:" + e.Message + ", url:" + url);
			}
		}
		return null;
	}
	public static string httpPost(string url, byte[] data, Action<string, object> callback = null, object callbakcUserData = null, bool logError = true)
	{
		return httpPost(url, data, "application/x-www-form-urlencoded", null, callback, callbakcUserData, logError);
	}
	public static string httpPost(string url, string param, Action<string, object> callback = null, object callbakcUserData = null, bool logError = true)
	{
		return httpPost(url, stringToBytes(param, Encoding.UTF8), "application/x-www-form-urlencoded", null, callback, callbakcUserData, logError);
	}
	public static string httpPost(string url, string param, string contentType, Action<string, object> callback = null, object callbakcUserData = null, bool logError = true)
	{
		return httpPost(url, stringToBytes(param, Encoding.UTF8), contentType, null, callback, callbakcUserData, logError);
	}
	public static string httpPost(string url, string param, string contentType, Dictionary<string, string> header, bool logError = true)
	{
		return httpPost(url, stringToBytes(param, Encoding.UTF8), contentType, header, null, null, logError);
	}
	public static string generateHttpGet(string url, Dictionary<string, string> get)
	{
		if (get == null || get.Count == 0)
		{
			return url;
		}
		int count = get.Count;
		MyStringBuilder parameters = STRING(url, "?");
		// 从集合中取出所有参数，设置表单参数（AddField())
		int index = 0;
		foreach(var item in get)
		{
			if (index != count - 1)
			{
				parameters.Append(item.Key, "=", item.Value, "&");
			}
			else
			{
				parameters.Append(item.Key, "=", item.Value);
			}
			++index;
		}
		return END_STRING(parameters);
	}
	public static string httpGet(string urlString, Action<string, object> callback = null, object userData = null, bool logError = true)
	{
		// 根据url地址创建HTTpWebRequest对象
		var httprequest = (HttpWebRequest)WebRequest.Create(new Uri(urlString));
		httprequest.Method = "GET";
		httprequest.KeepAlive = false;
		httprequest.ProtocolVersion = HttpVersion.Version11;
		httprequest.ContentType = "application/x-www-form-urlencoded";
		httprequest.AllowAutoRedirect = true;
		httprequest.MaximumAutomaticRedirections = 2;
		httprequest.Timeout = 10000;
		// 异步
		if (callback != null)
		{
			var threadParam = new RequestThreadParam();
			threadParam.mRequest = httprequest;
			threadParam.mByteArray = null;
			threadParam.mCallback = callback;
			threadParam.mUserData = userData;
			threadParam.mFullURL = urlString;
			threadParam.mLogError = logError;
			Thread httpThread = new Thread(waitHttpGet);
			threadParam.mThread = httpThread;
			httpThread.Start(threadParam);
			httpThread.IsBackground = true;
			ThreadListLock.waitForUnlock();
			mHttpThreadList.Add(httpThread);
			ThreadListLock.unlock();
		}
		// 同步
		else
		{
			try
			{
				var response = (HttpWebResponse)httprequest.GetResponse();
				var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				string str = reader.ReadToEnd();
				reader.Close();
				response.Close();
				httprequest.Abort();
				return str;
			}
			catch (Exception e)
			{
				logForce("http get exception:" + e.Message + ", url:" + urlString);
			}
		}
		return null;
	}
	//--------------------------------------------------------------------------------------------------------------------------------------------------------------
	protected static void waitHttpPost(object param)
	{
		var threadParam = (RequestThreadParam)param;
		try
		{
			// 附加要POST给服务器的数据到HttpWebRequest对象(附加POST数据的过程比较特殊，它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面。)
			// 创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
			Stream newStream = threadParam.mRequest.GetRequestStream();
			newStream.Write(threadParam.mByteArray, 0, threadParam.mByteArray.Length);
			newStream.Close();
			// 读取服务器的返回信息
			var response = (HttpWebResponse)threadParam.mRequest.GetResponse();
			var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

			// 延迟到主线程执行
			CMD_DELAY(out CmdGlobalDelayCallParam2<string, object> cmd);
			cmd.mFunction = threadParam.mCallback;
			cmd.mParam0 = reader.ReadToEnd();
			cmd.mParam1 = threadParam.mUserData;
			pushDelayCommand(cmd, mGlobalCmdReceiver);

			reader.Close();
			response.Close();
			threadParam.mRequest.Abort();
			threadParam.mRequest = null;
		}
		catch (Exception e)
		{
			threadParam.mCallback(null, threadParam.mUserData);
			log("http post result exception:" + e.Message + ", url:" + threadParam.mFullURL);
		}
		finally
		{
			ThreadListLock.waitForUnlock();
			mHttpThreadList?.Remove(threadParam.mThread);
			ThreadListLock.unlock();
		}
	}
	protected static void waitHttpGet(object param)
	{
		var threadParam = (RequestThreadParam)param;
		try
		{
			var response = (HttpWebResponse)threadParam.mRequest.GetResponse();
			StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

			// 延迟到主线程执行
			CMD_DELAY(out CmdGlobalDelayCallParam2<string, object> cmd);
			cmd.mFunction = threadParam.mCallback;
			cmd.mParam0 = reader.ReadToEnd();
			cmd.mParam1 = threadParam.mUserData;
			pushDelayCommand(cmd, mGlobalCmdReceiver);

			reader.Close();
			response.Close();
			threadParam.mRequest.Abort();
			threadParam.mRequest = null;
		}
		catch (Exception e)
		{
			threadParam.mCallback(null, threadParam.mUserData);
			log("http get result exception : " + e.Message + ", url : " + threadParam.mFullURL);
		}
		finally
		{
			ThreadListLock.waitForUnlock();
			mHttpThreadList?.Remove(threadParam.mThread);
			ThreadListLock.unlock();
		}
	}
	protected static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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