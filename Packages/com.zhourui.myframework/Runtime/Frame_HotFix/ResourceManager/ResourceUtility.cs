using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static FrameBaseUtility;
using static FrameBaseHotFix;
using static FrameBaseDefine;

// 加载资源的一些静态函数,由于没有经过资源管理器,所以返回的资源没有封装对象,不过外部拿到资源以后可以自己放到资源引用对象中
// 提供URL加载、Resources加载等便捷静态方法,适用于临时或非托管资源加载场景
public class ResourceUtility
{
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl<T>(string url, AssetLoadCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, typeof(T), callback, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl<T>(string url, BytesCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, typeof(T), (UObject _, UObject[] _, byte[] bytes, string _) =>
		{
			callback?.Invoke(bytes);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl<T>(string url, Action<T> callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, typeof(T), (UObject obj, UObject[] _, byte[] _, string _) =>
		{
			callback?.Invoke(obj as T);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl<T>(string url, Action<T, string> callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, typeof(T), (UObject obj, UObject[] _, byte[] _, string loadPath) =>
		{
			callback?.Invoke(obj as T, loadPath);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl(string url, AssetLoadCallback callback, DownloadCallback downloadingCallback = null)
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, null, callback, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, null, (UObject _, UObject[] _, byte[] bytes, string _) =>
		{
			callback?.Invoke(bytes);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl(string url, BytesStringCallback callback, DownloadCallback downloadingCallback = null)
	{
		GameEntryBase.startCoroutine(loadAssetsUrl(url, null, (UObject _, UObject[] _, byte[] bytes, string loadPath) =>
		{
			callback?.Invoke(bytes, loadPath);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
	public static IEnumerator loadAssetsFromUrlWaiting<T>(string url, AssetLoadCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		return loadAssetsUrl(url, typeof(T), callback, downloadingCallback);
	}
	// 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
	public static IEnumerator loadAssetsFromUrlWaiting<T>(string url, Action<T> callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		return loadAssetsUrl(url, typeof(T), (UObject asset, UObject[] _, byte[] _, string _) => { callback?.Invoke(asset as T); }, downloadingCallback);
	}
	// 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
	public static IEnumerator loadAssetsFromUrlWaiting(string url, AssetLoadCallback callback, DownloadCallback downloadingCallback = null)
	{
		return loadAssetsUrl(url, null, callback, downloadingCallback);
	}
	// 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
	public static IEnumerator loadAssetsFromUrlWaiting(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
	{
		return loadAssetsUrl(url, null, (UObject _, UObject[] _, byte[] bytes, string _) => { callback?.Invoke(bytes); }, downloadingCallback);
	}
	// 根据一个URL加载资源,一般都是一个网络资源,可在协程中等待
	public static IEnumerator loadAssetsUrl(string url, Type assetsType, AssetLoadCallback callback, DownloadCallback downloadingCallback)
	{
		log("开始下载: " + url);
		if (assetsType == typeof(AudioClip))
		{
			yield return loadAudioClipWithURL(url, callback);
		}
		else if (assetsType == typeof(Texture2D) || assetsType == typeof(Texture))
		{
			yield return loadTextureWithURL(url, callback);
		}
		else if (assetsType == typeof(AssetBundle))
		{
			yield return loadAssetBundleWithURL(url, callback);
		}
		else
		{
			yield return loadFileWithURL(url, callback, downloadingCallback);
		}
	}
	// 根据一个URL加载AssetBundle,可在协程中等待
	public static IEnumerator loadAssetBundleWithURL(string url, AssetLoadCallback callback)
	{
		// 小游戏中读取PersistentDataPath中的文件需要使用小游戏提供的接口
		if ((isByteDance() || isWeiXin()) && url.startWith(F_PERSISTENT_ASSETS_PATH))
		{
			yield return openFileAsyncInternal(url, true, (byte[] bytes) =>
			{
				try
				{
					callback?.Invoke(AssetBundle.LoadFromMemory(bytes), null, null, url);
				}
				catch (Exception e)
				{
					logException(e);
				}
			});
		}
		else
		{
			float timer = 0.0f;
			ulong lastDownloaded = 0;
			using var www = UnityWebRequestAssetBundle.GetAssetBundle(url);
			www.SendWebRequest();
			while (!www.isDone)
			{
				if (www.downloadedBytes > lastDownloaded)
				{
					lastDownloaded = www.downloadedBytes;
					timer = 0.0f;
				}
				else
				{
					timer += mGameFrameworkHotFix.getUnscaledTime();
					if (timer >= mResourceManager.getDownloadTimeout())
					{
						log("下载超时");
						break;
					}
				}
				if (isDevOrEditor())
				{
					log("当前计时:" + timer);
					log("下载中,www.downloadedBytes:" + www.downloadedBytes + ", www.downloadProgress:" + www.downloadProgress);
				}
				yield return null;
			}
			try
			{
				if (www.error != null)
				{
					log("下载失败 : " + url + ", info : " + www.error);
					try
					{
						callback?.Invoke(null, null, null, url);
					}
					catch (Exception e)
					{
						logException(e);
					}
				}
				else
				{
					log("下载成功:" + url);
					AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
					assetBundle.name = url;
					try
					{
						callback?.Invoke(assetBundle, null, null, url);
					}
					catch (Exception e)
					{
						logException(e);
					}
				}
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
	}
	public static IEnumerator loadAudioClipWithURL(string url, AssetLoadCallback callback)
	{
		using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
		www.timeout = mResourceManager.getDownloadTimeout();
		yield return www.SendWebRequest();
		try
		{
			if (www.error != null)
			{
				log("下载失败 : " + url + ", info : " + www.error);
				callback?.Invoke(null, null, null, url);
			}
			else
			{
				log("下载成功:" + url);
				AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
				clip.name = url;
				callback?.Invoke(clip, null, www.downloadHandler.data, url);
			}
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public static IEnumerator loadTextureWithURL(string url, AssetLoadCallback callback)
	{
		using var www = UnityWebRequestTexture.GetTexture(url);
		www.timeout = mResourceManager.getDownloadTimeout();
		yield return www.SendWebRequest();
		try
		{
			if (www.error != null)
			{
				log("下载失败 : " + url + ", info : " + www.error);
				callback?.Invoke(null, null, null, url);
			}
			else
			{
				log("下载成功:" + url);
				Texture2D tex = DownloadHandlerTexture.GetContent(www);
				tex.name = url;
				callback?.Invoke(tex, null, www.downloadHandler.data, url);
			}
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public static IEnumerator loadFileWithURL(string url, AssetLoadCallback callback, DownloadCallback downloadingCallback)
	{
		// 这里由于需要计算下载进度,就不再支持小游戏上读取本地文件了
		if ((isByteDance() || isWeiXin()) &&
			url.startWith(F_ASSET_BUNDLE_PATH) || url.startWith(F_PERSISTENT_ASSETS_PATH))
		{
			logErrorBase("小游戏上不支持使用loadFileWithURL读取本地文件");
			try
			{
				callback?.Invoke(null, null, null, url);
			}
			catch (Exception e)
			{
				logException(e);
			}
			yield break;
		}
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
				timer += mGameFrameworkHotFix.getUnscaledTime();
				if (timer >= mResourceManager.getDownloadTimeout())
				{
					log("下载超时");
					break;
				}
			}
			double deltaTimeMillis = (DateTime.Now - startTime).TotalMilliseconds;
			try
			{
				downloadingCallback?.Invoke(www.downloadedBytes, downloadDelta, deltaTimeMillis, www.downloadProgress);
			}
			catch (Exception e)
			{
				logException(e);
			}
			yield return null;
		}
		try
		{
			if (www.error != null || www.downloadHandler?.data == null)
			{
				log("下载失败 : " + url + ", info : " + www.error);
				callback?.Invoke(null, null, null, url);
			}
			else
			{
				log("下载成功:" + url + ", size:" + www.downloadHandler.data.Length);
				callback?.Invoke(null, null, www.downloadHandler.data, url);
			}
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
}