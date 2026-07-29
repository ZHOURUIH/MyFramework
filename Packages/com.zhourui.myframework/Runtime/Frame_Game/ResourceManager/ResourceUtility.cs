using System;
using System.Collections;
using UnityEngine;
using UObject = UnityEngine.Object;
using static FrameBaseUtility;
using static FrameBaseDefine;

// 资源管理器,管理所有资源的加载
public class ResourceUtility
{
	public static void loadAssetsFromUrl(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, (UObject _, UObject[] _, byte[] bytes, string _) =>
		{
			callback?.Invoke(bytes);
		}, downloadingCallback));
	}
	public static IEnumerator loadAssetsUrl(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback)
	{
		// 这里由于需要计算下载进度,就不再支持小游戏上读取本地文件了
		if ((isByteDance() || isWeiXin()) &&
			url.startWith(F_ASSET_BUNDLE_PATH) || url.startWith(F_PERSISTENT_ASSETS_PATH))
		{
			logErrorBase("小游戏上不支持使用loadFileWithURL读取本地文件");
			callback?.Invoke(null, null, null, url);
			yield break;
		}
		logBase("开始下载: " + url);
		float timer = 0.0f;
		ulong lastDownloaded = 0;
		using var www = unityWebRequest(url);
		www.timeout = 0;
		www.SendWebRequest();
		DateTime startTime = DateTime.Now;
		while (!www.isDone)
		{
			// 累计每秒下载的字节数,计算下载速度
			int downloadDelta = 0;
			if (www.downloadedBytes > lastDownloaded)
			{
				downloadDelta = (int)(www.downloadedBytes - lastDownloaded);
				lastDownloaded = www.downloadedBytes;
				timer = 0.0f;
			}
			else
			{
				timer += Time.unscaledDeltaTime;
				// 默认30秒超时
				if (timer >= 30)
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