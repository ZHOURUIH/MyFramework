using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using LitJson;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public delegate void OnHttpWebRequestCallback(JsonData data, object userData);
public struct RequestThreadParam
{
	public HttpWebRequest mRequest;
	public byte[] mByteArray;
	public OnHttpWebRequestCallback mCallback;
	public object mUserData;
	public Thread mThread;
	public string mFullURL;
	public bool mLogError;
}

// 作为字段参数时,只填写mKey和mValue
// 作为文件内容时,只填写mFileContont和mFileName
public struct FormItem
{
	public byte[] mFileContent;
	public string mFileName;
	public string mKey;
	public string mValue;
	public FormItem(byte[] file, string fileName)
	{
		mFileContent = file;
		mFileName = fileName;
		mKey = "";
		mValue = "";
	}
	public FormItem(string key, string value)
	{
		mFileContent = null;
		mFileName = "";
		mKey = key;
		mValue = value;
	}
}

public class HttpUtility : FrameComponent
{
	protected static List<Thread> mHttpThreadList;
	protected static ThreadLock ThreadListLock;
	public HttpUtility(string name)
		:base(name)
	{
		mHttpThreadList = new List<Thread>();
		ThreadListLock = new ThreadLock();
	}
	public override void destroy()
	{
		ThreadListLock.waitForUnlock();
		int count = mHttpThreadList.Count;
		for(int i = 0; i < count; ++i)
		{
			mHttpThreadList[i].Abort();
			mHttpThreadList[i] = null;
		}
		mHttpThreadList.Clear();
		mHttpThreadList = null;
		ThreadListLock.unlock();
		base.destroy();
	}
	// 同步下载文件
	public static byte[] downloadFile(string url, int offset = 0, byte[] helperBytes = null, string fileName = "", 
										StartDownloadCallback startCallback = null, DownloadingCallback downloading = null)
	{
		logInfo("开始http下载:" + url, LOG_LEVEL.LL_FORCE);
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		request.AddRange(offset);
		WebResponse response = request.GetResponse();
		long fileSize = response.ContentLength + offset;
		// response.ContentLength只是剩余需要下载的长度,需要加上下载起始偏移才是文件的真实大小
		startCallback?.Invoke(fileName, fileSize);
		Stream inStream = response.GetResponseStream();//获取http
		MemoryStream downloadStream = new MemoryStream();
		if(helperBytes == null)
		{
			helperBytes = new byte[1024];
		}
		int readCount = 0;
		do
		{
			// 从输入流中读取数据放入内存中
			readCount = inStream.Read(helperBytes, 0, helperBytes.Length);//读流
			downloadStream.Write(helperBytes, 0, readCount);//写流
			downloading?.Invoke(fileName, fileSize, downloadStream.Length);
		} while (readCount > 0);
		byte[] dataBytes = downloadStream.ToArray();
		downloadStream.Close();
		inStream.Close();
		response.Close();
		logInfo("http下载完成:" + url, LOG_LEVEL.LL_FORCE);
		return dataBytes;
	}
	public static JsonData httpWebRequestPostFile(string url, List<FormItem> itemList, OnHttpWebRequestCallback callback, object callbakcUserData, bool logError)
	{
		// 以模拟表单的形式上传数据
		string boundary = "----" + DateTime.Now.Ticks.ToString("x");
		string fileFormdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" + "\r\nContent-Type: application/octet-stream" + "\r\n\r\n";
		string dataFormdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"" + "\r\n\r\n{1}";
		MemoryStream postStream = new MemoryStream();
		foreach (var item in itemList)
		{
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
		var footer = stringToBytes("\r\n--" + boundary + "--\r\n", Encoding.UTF8);
		postStream.Write(footer, 0, footer.Length);

		byte[] postBytes = postStream.ToArray();
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
		HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		webRequest.Method = "POST";
		webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
		webRequest.Timeout = 10000;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.ContentLength = postBytes.Length;
		// 异步
		if (callback != null)
		{
			RequestThreadParam threadParam = new RequestThreadParam();
			threadParam.mRequest = webRequest;
			threadParam.mByteArray = postBytes;
			threadParam.mCallback = callback;
			threadParam.mUserData = callbakcUserData;
			threadParam.mFullURL = url;
			threadParam.mLogError = logError;
			Thread httpThread = new Thread(waitPostHttpWebRequest);
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
				// 3． 附加要POST给服务器的数据到HttpWebRequest对象(附加POST数据的过程比较特殊，它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面。)
				Stream newStream = webRequest.GetRequestStream();//创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
				newStream.Write(postBytes, 0, postBytes.Length);
				newStream.Close();
				// 4． 读取服务器的返回信息
				HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
				StreamReader php = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				string phpend = php.ReadToEnd();
				php.Close();
				response.Close();
				return JsonMapper.ToObject(phpend);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
	public static JsonData httpWebRequestPost(string url, byte[] data, string contentType, OnHttpWebRequestCallback callback, object callbakcUserData, bool logError)
	{
		// 初始化新的webRequst
		// 1． 创建httpWebRequest对象
		ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
		HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
		// 2． 初始化HttpWebRequest对象
		webRequest.Method = "POST";
		webRequest.ContentType = contentType;
		webRequest.ContentLength = data.Length;
		webRequest.Credentials = CredentialCache.DefaultCredentials;
		webRequest.Timeout = 10000;
		// 异步
		if (callback != null)
		{
			RequestThreadParam threadParam = new RequestThreadParam();
			threadParam.mRequest = webRequest;
			threadParam.mByteArray = data;
			threadParam.mCallback = callback;
			threadParam.mUserData = callbakcUserData;
			threadParam.mFullURL = url;
			threadParam.mLogError = logError;
			Thread httpThread = new Thread(waitPostHttpWebRequest);
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
				// 3． 附加要POST给服务器的数据到HttpWebRequest对象(附加POST数据的过程比较特殊，它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面。)
				Stream newStream = webRequest.GetRequestStream();//创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
				newStream.Write(data, 0, data.Length);
				newStream.Close();
				// 4． 读取服务器的返回信息
				HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
				StreamReader php = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
				string phpend = php.ReadToEnd();
				php.Close();
				response.Close();
				return JsonMapper.ToObject(phpend);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
	public static JsonData httpWebRequestPost(string url, byte[] data, OnHttpWebRequestCallback callback = null, object callbakcUserData = null, bool logError = true)
	{
		return httpWebRequestPost(url, data, "application/x-www-form-urlencoded", callback, callbakcUserData, logError);
	}
	public static JsonData httpWebRequestPost(string url, string param, OnHttpWebRequestCallback callback = null, object callbakcUserData = null, bool logError = true)
	{
		return httpWebRequestPost(url, stringToBytes(param, Encoding.UTF8), "application/x-www-form-urlencoded", callback, callbakcUserData, logError);
	}
	public static JsonData httpWebRequestPost(string url, string param, string contentType, OnHttpWebRequestCallback callback = null, object callbakcUserData = null, bool logError = true)
	{
		return httpWebRequestPost(url, stringToBytes(param, Encoding.UTF8), contentType, callback, callbakcUserData, logError);
	}
	static public string generateHttpGet(string url, Dictionary<string, string> get)
	{
		string Parameters = "";
		if (get.Count > 0)
		{
			Parameters = "?";
			//从集合中取出所有参数，设置表单参数（AddField()).  
			foreach (KeyValuePair<string, string> post_arg in get)
			{
				Parameters += post_arg.Key + "=" + post_arg.Value + "&";
			}
			removeLast(ref Parameters, '&');
		}
		return url + Parameters;
	}
	static public JsonData httpWebRequestGet(string urlString, OnHttpWebRequestCallback callback = null, object userData = null, bool logError = true)
	{
		HttpWebRequest httprequest = (HttpWebRequest)WebRequest.Create(new Uri(urlString));//根据url地址创建HTTpWebRequest对象
		httprequest.Method = "GET";
		httprequest.KeepAlive = false;//持久连接设置为false
		httprequest.ProtocolVersion = HttpVersion.Version11;// 网络协议的版本
		httprequest.ContentType = "application/x-www-form-urlencoded";//http 头
		httprequest.AllowAutoRedirect = true;
		httprequest.MaximumAutomaticRedirections = 2;
		httprequest.Timeout = 10000;//设定超时10秒（毫秒）
		// 异步
		if (callback != null)
		{
			RequestThreadParam threadParam = new RequestThreadParam();
			threadParam.mRequest = httprequest;
			threadParam.mByteArray = null;
			threadParam.mCallback = callback;
			threadParam.mUserData = userData;
			threadParam.mFullURL = urlString;
			threadParam.mLogError = logError;
			Thread httpThread = new Thread(waitGetHttpWebRequest);
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
				HttpWebResponse response = (HttpWebResponse)httprequest.GetResponse();
				Stream steam = response.GetResponseStream();
				StreamReader reader = new StreamReader(steam, Encoding.UTF8);
				string pageStr = reader.ReadToEnd();
				reader.Close();
				response.Close();
				httprequest.Abort();
				reader = null;
				response = null;
				httprequest = null;
				return JsonMapper.ToObject(pageStr);
			}
			catch (Exception e)
			{
				logInfo(e.Message);
				return null;
			}
		}
	}
	//--------------------------------------------------------------------------------------------------------------------------------------------------------------
	static protected void waitPostHttpWebRequest(object param)
	{
		RequestThreadParam threadParam = (RequestThreadParam)param;
		try
		{
			//3． 附加要POST给服务器的数据到HttpWebRequest对象(附加POST数据的过程比较特殊，它并没有提供一个属性给用户存取，需要写入HttpWebRequest对象提供的一个stream里面。)
			Stream newStream = threadParam.mRequest.GetRequestStream();//创建一个Stream,赋值是写入HttpWebRequest对象提供的一个stream里面
			newStream.Write(threadParam.mByteArray, 0, threadParam.mByteArray.Length);
			newStream.Close();
			//4． 读取服务器的返回信息
			HttpWebResponse response = (HttpWebResponse)threadParam.mRequest.GetResponse();
			StreamReader php = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
			string phpend = php.ReadToEnd();
			php.Close();
			response.Close();
			threadParam.mCallback(JsonMapper.ToObject(phpend), threadParam.mUserData);
		}
		catch (Exception e)
		{
			threadParam.mCallback(null, threadParam.mUserData);
			string info = "http post result exception:" + e.Message + ", url:" + threadParam.mFullURL;
			logInfo(info);
			if (threadParam.mLogError)
			{
				mFrameLogSystem?.logHttpOverTime(info);
			}
		}
		finally
		{
			ThreadListLock.waitForUnlock();
			mHttpThreadList?.Remove(threadParam.mThread);
			ThreadListLock.unlock();
		}
	}
	static protected void waitGetHttpWebRequest(object param)
	{
		RequestThreadParam threadParam = (RequestThreadParam)param;
		try
		{
			HttpWebResponse response = (HttpWebResponse)threadParam.mRequest.GetResponse();
			Stream steam = response.GetResponseStream();
			StreamReader reader = new StreamReader(steam, Encoding.UTF8);
			string pageStr = reader.ReadToEnd();
			reader.Close();
			response.Close();
			reader = null;
			response = null;
			threadParam.mRequest.Abort();
			threadParam.mRequest = null;
			threadParam.mCallback(JsonMapper.ToObject(pageStr), threadParam.mUserData);
		}
		catch (Exception e)
		{
			threadParam.mCallback(null, threadParam.mUserData);
			string info = "http get result exception : " + e.Message + ", url : " + threadParam.mFullURL;
			logInfo(info);
			if (threadParam.mLogError)
			{
				mFrameLogSystem?.logHttpOverTime(info);
			}
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
		bool isOk = true;
		// If there are errors in the certificate chain,
		// look at each error to determine the cause.
		if (sslPolicyErrors != SslPolicyErrors.None)
		{
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
				bool chainIsValid = chain.Build((X509Certificate2)certificate);
				if (!chainIsValid)
				{
					isOk = false;
					break;
				}
			}
		}
		return isOk;
	}
}