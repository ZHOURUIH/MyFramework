using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static StringUtility;
using static FrameBaseHotFix;
using static FrameEditorUtility;

// 资源管理器,管理所有资源的加载
public class ResourceManager : FrameSystem
{
	protected AssetDataBaseLoader mAssetDataBaseLoader = new();         // 通过AssetDataBase加载资源的加载器,只会在编辑器下使用
	protected AssetBundleLoader mAssetBundleLoader = new();             // 通过AssetBundle加载资源的加载器,打包后强制使用AssetBundle加载
	protected ResourcesLoader mResourcesLoader = new();                 // 通过Resources加载资源的加载器,Resources在编辑器或者打包后都会使用,用于加载Resources中的非热更资源
	protected List<UObjectCallback> mUnloadObjectCallback = new();      // 卸载某个单独资源的回调
	protected List<StringCallback> mUnloadPathCallback = new();         // 卸载目录中所有资源的回调,不会再次通知其中的单个资源
	protected LOAD_SOURCE mLoadSource;                                  // 加载源,从AssetBundle加载还是从AssetDataBase加载
	protected static int mDownloadTimeout = 10;							// 下载超时时间,秒
	public ResourceManager()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		mLoadSource = isEditor() ? mGameFrameworkHotFix.mParam.mLoadSource : LOAD_SOURCE.ASSET_BUNDLE;
		if (isEditor())
		{
			mObject.AddComponent<ResourcesManagerDebug>();
		}
	}
	public override void preInitAsync(Action callback)
	{
		if (mLoadSource != LOAD_SOURCE.ASSET_BUNDLE)
		{
			callback?.Invoke();
			return;
		}
		mAssetBundleLoader.initAssets(callback);
	}
	public bool isResourceInited()
	{
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			return mAssetBundleLoader.isInited();
		}
		return true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.update(elapsedTime);
		}
	}
	public override void destroy()
	{
		mAssetBundleLoader?.destroy();
		mAssetDataBaseLoader?.destroy();
		mResourcesLoader?.destroy();
		base.destroy();
	}
	public void addUnloadObjectCallback(UObjectCallback callback) { mUnloadObjectCallback.Add(callback); }
	public void addUnloadPathCallback(StringCallback callback) { mUnloadPathCallback.Add(callback); }
	public void removeUnloadObjectCallback(UObjectCallback callback) { mUnloadObjectCallback.Remove(callback); }
	public void removeUnloadPathCallback(StringCallback callback) { mUnloadPathCallback.Remove(callback); }
	public AssetBundleLoader getAssetBundleLoader() { return mAssetBundleLoader; }
	public AssetDataBaseLoader getAssetDataBaseLoader() { return mAssetDataBaseLoader; }
	public ResourcesLoader getResourcesLoader() { return mResourcesLoader; }
	public void setDownloadURL(string url) { mAssetBundleLoader.setDownloadURL(url); }
	public string getDownloadURL() { return mAssetBundleLoader.getDownloadURL(); }
	public int getDownloadTimeout() { return mDownloadTimeout; }
	public void setDownloadTimeout(int timeout) { mDownloadTimeout = timeout; }
	// 卸载加载的资源,不是实例化出的物体
	public bool unload<T>(ref T obj, bool showError = true) where T : UObject
	{
		if (obj == null)
		{
			return false;
		}
		foreach (UObjectCallback callback in mUnloadObjectCallback)
		{
			callback.Invoke(obj);
		}
		bool success = false;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			success = mAssetDataBaseLoader.unloadResource(ref obj, showError);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			success = mAssetBundleLoader.unloadAsset(ref obj, showError);
		}
		return success;
	}
	// 卸载从Resources中加载的资源
	public bool unloadInResources<T>(ref T obj, bool showError = true) where T : UObject
	{
		return mResourcesLoader.unloadResource(ref obj, showError);
	}
	// 卸载指定目录中的所有资源,path为GameResources下的相对路径
	public void unloadPath(string path)
	{
		removeEndSlash(ref path);
		foreach (StringCallback callback in mUnloadPathCallback)
		{
			callback.Invoke(path);
		}
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			mAssetDataBaseLoader.unloadPath(path);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.unloadPath(path);
		}
	}
	// 卸载Resources指定目录中的所有资源
	public void unloadPathInResources(string path)
	{
		removeEndSlash(ref path);
		mResourcesLoader.unloadPath(path);
	}
	// 指定卸载资源包
	public void unloadAssetBundle(string bundleName)
	{
		// 只有从AssetBundle加载才能卸载AssetBundle
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.unloadAssetBundle(bundleName);
		}
	}
	// 指定资源是否已经加载,name是GameResources下的相对路径,带后缀
	public bool isGameResourceLoaded<T>(string name) where T : UObject
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			ret = mAssetDataBaseLoader.isResourceLoaded(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			ret = mAssetBundleLoader.isAssetLoaded<T>(name);
		}
		return ret;
	}
	// 在Resources中的指定资源是否已经加载,带后缀
	public bool isInResourceLoaded<T>(string name) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return false;
		}
		return mResourcesLoader.isResourceLoaded(removeSuffix(name));
	}
	// 获得资源,如果没有加载,则获取不到,使用频率可能比较低,name是GameResources下的相对路径,带后缀
	public T getGameResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return null;
		}
		T res = null;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			res = mAssetDataBaseLoader.getResource(name) as T;
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.getAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		return res;
	}
	// 强制在Resources中获得资源,如果未加载,则无法获取,name是Resources下的相对路径,带后缀
	public T getInResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return null;
		}
		T res = mResourcesLoader.getResource(removeSuffix(name)) as T;
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		return res;
	}
	// 检查指定资源包的依赖项是否已经加载
	public void checkAssetBundleDependenceLoaded(string bundleName)
	{
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.checkAssetBundleDependenceLoaded(bundleName);
		}
	}
	// 同步加载资源包
	public void loadAssetBundle(string bundleName)
	{
		// 只有从AssetBundle加载时才能加载AssetBundle
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundle(bundleName, null);
		}
	}
	// 异步加载资源包
	public void loadAssetBundleAsync(string bundleName, AssetBundleCallback callback)
	{
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			// 从Resource加载不能加载AssetBundle
			callback?.Invoke(null);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundleAsync(bundleName, callback);
		}
	}
	// 同步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public T loadGameResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		using var a = new ProfilerScope(0);
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return null;
		}
		T res = null;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			res = mAssetDataBaseLoader.loadResource<T>(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.loadAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		return res;
	}
	// 强制从Resources中同步加载指定资源,name是Resources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public T loadInResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return null;
		}
		T res = mResourcesLoader.loadResource<T>(removeSuffix(name));
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// 同步加载资源的子资源,一般是图集才会有子资源
	public UObject[] loadGameSubResource<T>(string name, out UObject mainAsset, bool errorIfNull = true) where T : UObject
	{
		using var a = new ProfilerScope(0);
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			mainAsset = null;
			return null;
		}
		mainAsset = null;
		UObject[] res = null;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			res = mAssetDataBaseLoader.loadSubResource<T>(name, out mainAsset);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.loadSubAsset<T>(name, out mainAsset);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		return res;
	}
	// 强制从Resources中同步加载资源的子资源,一般是图集才会有子资源
	public UObject[] loadInSubResource<T>(string name, out UObject mainAsset, bool errorIfNull = true) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			mainAsset = null;
			return null;
		}
		UObject[] res = mResourcesLoader.loadSubResource<T>(removeSuffix(name), out mainAsset);
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		return res;
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, bool errorIfNull = true) where T : UObject
	{
		using var a = new ProfilerScope(0);
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			doneCallback?.Invoke(null, null, null, name);
			return null;
		}
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			return mAssetDataBaseLoader.loadResourcesAsync<T>(name, doneCallback);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			return mAssetBundleLoader.loadAssetAsync<T>(name, errorIfNull, doneCallback);
		}
		return null;
	}
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<T, string> doneCallback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsync<T>(name, (UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
		{
			doneCallback?.Invoke(asset as T, loadPath);
		}, errorIfNull);
	}
	public CustomAsyncOperation loadGameResourceAsyncSafe<T>(ClassObject relatedObj, string name, Action<T, string> doneCallback, bool errorIfNull = true) where T : UObject
	{
		long assignID = relatedObj.getAssignID();
		return loadGameResourceAsync<T>(name, (UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
		{
			if (assignID != relatedObj.getAssignID())
			{
				return;
			}
			doneCallback?.Invoke(asset as T, loadPath);
		}, errorIfNull);
	}
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<T> doneCallback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsync<T>(name, (UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
		{
			doneCallback?.Invoke(asset as T);
		}, errorIfNull);
	}
	public CustomAsyncOperation loadGameResourceAsyncSafe<T>(ClassObject relatedObj, string name, Action<T> doneCallback, bool errorIfNull = true) where T : UObject
	{
		long assignID = relatedObj.getAssignID();
		return loadGameResourceAsync<T>(name, (UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
		{
			if (assignID != relatedObj.getAssignID())
			{
				return;
			}
			doneCallback?.Invoke(asset as T);
		}, errorIfNull);
	}
	// 强制在Resource中异步加载资源,name是Resources下的相对路径,带后缀,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadInResourceAsync<T>(string name, Action<T> doneCallback) where T : UObject
	{
		return loadInResourceAsync<T>(name, (UObject asset, UObject[] assets, byte[] bytes, string loadPath) => 
		{
			doneCallback?.Invoke(asset as T);
		});
	}
	// 强制在Resource中异步加载资源,name是Resources下的相对路径,带后缀,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadInResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback) where T : UObject
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return null;
		}
		return mResourcesLoader.loadResourcesAsync<T>(removeSuffix(name), doneCallback);
	}
	// 仅下载一个资源,下载后会写入本地文件,并且更新本地文件信息列表,fileName为带后缀,GameResources下的相对路径
	public void downloadGameResource(string name, BytesCallback callback)
	{
		if (!name.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + name);
			return;
		}
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.downloadAsset(name, callback);
		}
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl<T>(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, typeof(T), callback, downloadingCallback));
	}
	public static void loadAssetsFromUrl<T>(string url, BytesCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, typeof(T), (UObject _, UObject[] _, byte[] bytes, string _) =>
		{
			callback?.Invoke(bytes);
		}, downloadingCallback));
	}
	public static void loadAssetsFromUrl<T>(string url, Action<T> callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, typeof(T), (UObject obj, UObject[] _, byte[] _, string _) =>
		{
			callback?.Invoke(obj as T);
		}, downloadingCallback));
	}
	public static void loadAssetsFromUrl<T>(string url, Action<T, string> callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, typeof(T), (UObject obj, UObject[] _, byte[] _, string loadPath) =>
		{
			callback?.Invoke(obj as T, loadPath);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static void loadAssetsFromUrl(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null)
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, null, callback, downloadingCallback));
	}
	public static void loadAssetsFromUrl(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, null, (UObject _, UObject[] _, byte[] bytes, string _) =>
		{
			callback?.Invoke(bytes);
		}, downloadingCallback));
	}
	public static void loadAssetsFromUrl(string url, BytesStringCallback callback, DownloadCallback downloadingCallback = null)
	{
		mGameFrameworkHotFix.StartCoroutine(loadAssetsUrl(url, null, (UObject _, UObject[] _, byte[] bytes, string loadPath) =>
		{
			callback?.Invoke(bytes, loadPath);
		}, downloadingCallback));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static IEnumerator loadAssetsFromUrlWaiting<T>(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null) where T : UObject
	{
		return loadAssetsUrl(url, typeof(T), callback, downloadingCallback);
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public static IEnumerator loadAssetsFromUrlWaiting(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback = null)
	{
		return loadAssetsUrl(url, null, callback, downloadingCallback);
	}
	public static IEnumerator loadAssetsFromUrlWaiting(string url, BytesCallback callback, DownloadCallback downloadingCallback = null)
	{
		return loadAssetsUrl(url, null, (UObject _, UObject[] _, byte[] bytes, string _) => { callback?.Invoke(bytes); }, downloadingCallback);
	}
	public static IEnumerator loadAssetsUrl(string url, Type assetsType, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback)
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
	public static IEnumerator loadAssetBundleWithURL(string url, AssetLoadDoneCallback callback)
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
				timer += Time.unscaledDeltaTime;
				if (timer >= mDownloadTimeout)
				{
					log("下载超时");
					break;
				}
			}
			log("当前计时:" + timer);
			log("下载中,www.downloadedBytes:" + www.downloadedBytes + ", www.downloadProgress:" + www.downloadProgress);
			yield return null;
		}
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
				AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(www);
				assetBundle.name = url;
				callback?.Invoke(assetBundle, null, www.downloadHandler.data, url);
			}
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	public static IEnumerator loadAudioClipWithURL(string url, AssetLoadDoneCallback callback)
	{
		using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
		www.timeout = mDownloadTimeout;
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
	public static IEnumerator loadTextureWithURL(string url, AssetLoadDoneCallback callback)
	{
		using var www = UnityWebRequestTexture.GetTexture(url);
		www.timeout = mDownloadTimeout;
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
	public static IEnumerator loadFileWithURL(string url, AssetLoadDoneCallback callback, DownloadCallback downloadingCallback)
	{
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
					log("下载超时");
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