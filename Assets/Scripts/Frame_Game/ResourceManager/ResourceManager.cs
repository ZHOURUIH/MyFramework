using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;
using static FrameBaseUtility;
using static StringUtility;

// 资源管理器,管理所有资源的加载
public class ResourceManager : FrameSystem
{
	protected string mDownloadURL;
	protected ResourcesLoader mResourcesLoader = new();		// 通过Resources加载资源的加载器,Resources在编辑器或者打包后都会使用,用于加载Resources中的非热更资源
	protected static int mDownloadTimeout = 10;				// 下载超时时间,秒
	public override void destroy()
	{
		mResourcesLoader?.destroy();
		base.destroy();
	}
	public void setDownloadURL(string url) { mDownloadURL = url; }
	public string getDownloadURL() { return mDownloadURL; }
	// 卸载从Resources中加载的资源
	public bool unloadInResources<T>(ref T obj, bool showError = true) where T : UObject
	{
		return mResourcesLoader.unloadResource(ref obj, showError);
	}
	// 强制从Resources中同步加载指定资源,name是Resources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public T loadInResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logErrorBase("资源文件名需要带后缀:" + name);
			return null;
		}
		T res = mResourcesLoader.loadResource<T>(removeSuffix(name));
		if (res == null && errorIfNull)
		{
			logErrorBase("can not find resource : " + name);
		}
		return res;
	}
	// 强制在Resource中异步加载资源,name是Resources下的相对路径,带后缀,errorIfNull表示当找不到资源时是否报错提示
	public void loadInResourceAsync<T>(string name, Action<T> doneCallback) where T : UObject
	{
		loadInResourceAsync<T>(name, (UObject asset, UObject[] _, byte[] _, string _) =>  { doneCallback?.Invoke(asset as T); });
	}
	// 强制在Resource中异步加载资源,name是Resources下的相对路径,带后缀,errorIfNull表示当找不到资源时是否报错提示
	public void loadInResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logErrorBase("资源文件名需要带后缀:" + name);
			doneCallback?.Invoke(null, null, null, name);
			return;
		}
		mResourcesLoader.loadResourcesAsync<T>(removeSuffix(name), doneCallback);
	}
	public static void loadAssetsFromUrl(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
	{
		GameEntry.getInstance().StartCoroutine(loadAssetsUrl(url, (UObject _, UObject[] _, byte[] bytes, string _) =>
		{
			callback?.Invoke(bytes);
		}, downloadingCallback));
	}
	public static IEnumerator loadAssetsUrl(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback)
	{
		logBase("开始下载: " + url);
		float timer = 0.0f;
		ulong lastDownloaded = 0;
		using var www = UnityWebRequest.Get(url);
		www.timeout = 0;
		www.SendWebRequest();
		DateTime startTime = DateTime.Now;
		while (!www.isDone)
		{
			// 累计每秒下载的字节数,计算下载速度
			int downloadDelta = 0;
			if (www.downloadedBytes > lastDownloaded)
			{
				lastDownloaded = www.downloadedBytes;
				downloadDelta = (int)(www.downloadedBytes - lastDownloaded);
				timer = 0.0f;
			}
			else
			{
				timer += Time.unscaledDeltaTime;
				if (timer >= mDownloadTimeout)
				{
					logBase("下载超时");
					break;
				}
			}
			double deltaTimeMillis = (DateTime.Now - startTime).TotalMilliseconds;
			downloadingCallback?.Invoke(www.downloadedBytes, downloadDelta, deltaTimeMillis, www.downloadProgress);
			yield return null;
		}
		try
		{
			if (www.error != null || www.downloadHandler?.data == null)
			{
				logBase("下载失败 : " + url + ", info : " + www.error);
				callback?.Invoke(null, null, null, url);
			}
			else
			{
				logBase("下载成功:" + url + ", size:" + www.downloadHandler.data.Length);
				callback?.Invoke(null, null, www.downloadHandler.data, url);
			}
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
}