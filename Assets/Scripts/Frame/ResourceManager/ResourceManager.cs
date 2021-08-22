using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

public class ResourceManager : FrameSystem
{
	protected Dictionary<Type, List<string>> mTypeSuffixList;   // 资源类型对应的后缀名
	protected AssetDataBaseLoader mAssetDataBaseLoader;			// 通过AssetDataBase加载资源的加载器,只会在编辑器下使用
	protected AssetBundleLoader mAssetBundleLoader;             // 通过AssetBundle加载资源的加载器,打包后强制使用AssetBundle加载
	protected ResourcesLoader mResourcesLoader;					// 通过Resources加载资源的加载器,Resources在编辑器或者打包后都会使用,用于加载Resources中的非热更资源
	protected LOAD_SOURCE mLoadSource;                          // 加载源
	public ResourceManager()
	{
		mCreateObject = true;
		mAssetDataBaseLoader = new AssetDataBaseLoader();
		mAssetBundleLoader = new AssetBundleLoader();
		mResourcesLoader = new ResourcesLoader();
		mTypeSuffixList = new Dictionary<Type, List<string>>();
		registeSuffix(typeof(Texture), ".png");
		registeSuffix(typeof(Texture2D), ".png");
		registeSuffix(typeof(GameObject), ".prefab");
		registeSuffix(typeof(GameObject), ".fbx");
		registeSuffix(typeof(Material), ".mat");
		registeSuffix(typeof(Shader), ".shader");
		registeSuffix(typeof(AudioClip), ".wav");
		registeSuffix(typeof(AudioClip), ".ogg");
		registeSuffix(typeof(AudioClip), ".mp3");
		registeSuffix(typeof(TextAsset), ".txt");
		registeSuffix(typeof(TextAsset), ".bytes");
		registeSuffix(typeof(RuntimeAnimatorController), ".controller");
		registeSuffix(typeof(RuntimeAnimatorController), ".overrideController");
		registeSuffix(typeof(SpriteAtlas), ".spriteatlas");
		registeSuffix(typeof(Sprite), ".png");
		registeSuffix(typeof(UnityEngine.Object), ".asset");
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mLoadSource = mGameFramework.mLoadSource;
#else
		mLoadSource = LOAD_SOURCE.ASSET_BUNDLE;
#endif
#if UNITY_EDITOR
		mObject.AddComponent<ResourcesManagerDebug>();
#endif
	}
	public override void resourceAvailable()
	{
		mAssetBundleLoader.resourceAvailable();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAssetBundleLoader.update();
	}
	public override void destroy()
	{
		mAssetBundleLoader?.destroy();
		mAssetDataBaseLoader?.destroy();
		mResourcesLoader?.destroy();
		base.destroy();
	}
	public AssetBundleLoader getAssetBundleLoader() { return mAssetBundleLoader; }
	public AssetDataBaseLoader getAssetDataBaseLoader() { return mAssetDataBaseLoader; }
	public ResourcesLoader getResourcesLoader() { return mResourcesLoader; }
	// 卸载加载的资源,不是实例化出的物体
	public bool unload<T>(ref T obj) where T : UnityEngine.Object
	{
		if(mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			return mAssetDataBaseLoader.unloadResource(ref obj);
		}
		if(mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			return mAssetBundleLoader.unloadAsset(ref obj);
		}
		return false;
	}
	// 卸载从Resources中加载的资源
	public bool unloadInResources<T>(ref T obj) where T : UnityEngine.Object
	{
		return mResourcesLoader.unloadResource(ref obj);
	}
	// 根据文件夹前缀卸载文件夹,实际上与unloadPath逻辑完全一致
	public void unloadPathPreName(string pathPreName)
	{
		unloadPath(pathPreName);
	}
	// 卸载指定目录中的所有资源
	public void unloadPath(string path)
	{
		removeEndSlash(ref path);
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			mAssetDataBaseLoader.unloadPath(path);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.unloadPath(path.ToLower());
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
	// AssetBundle是否可以使用
	public bool isAssetBundleAvalaible()
	{
		return mLoadSource == LOAD_SOURCE.RESOURCES || mAssetBundleLoader.isInited();
	}
	// 指定资源是否已经加载,name是GameResources下的相对路径,不带后缀
	public bool isResourceLoaded<T>(string name) where T : UnityEngine.Object
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			ret = mAssetDataBaseLoader.isResourceLoaded(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			ret = mAssetBundleLoader.isAssetLoaded<T>(name);
		}
		return ret;
	}
	// 在Resources中的指定资源是否已经加载
	public bool isInResourceLoaded<T>(string name) where T : UnityEngine.Object
	{
		return mResourcesLoader.isResourceLoaded(name);
	}
	// 获得资源,如果没有加载,则获取不到,使用频率可能比较低,name是GameResources下的相对路径,不带后缀
	public T getResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		T res = null;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			res = mAssetDataBaseLoader.getResource(name) as T;
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.getAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// 强制在Resources中获得资源,如果未加载,则无法获取,name是Resources下的相对路径,不带后缀
	public T getInResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		T res = mResourcesLoader.getResource(name) as T;
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// path为resources下相对路径,返回的列表中文件名带相对路径不带后缀
	// 如果从Resource中加载,则区分大小写,如果从AssetBundle中加载,传入的路径不区分大小写,返回的文件列表全部为小写
	// lower表示是否将返回列表中的字符串全部转为小写
	public void getFileList(string path, List<string> fileList, bool lower = false)
	{
		// 因为资源散布在不同的目录,PersistentDataPath和StreamingAssets,所以获得的文件列表可能不准确,所以不能调用
		logError("不允许获取文件列表");
	}
	// 检查指定资源包的依赖项是否已经加载
	public void checkAssetBundleDependenceLoaded(string bundleName)
	{
		if(mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.checkAssetBundleDependenceLoaded(bundleName.ToLower());
		}
	}
	// 同步加载资源包
	public void loadAssetBundle(string bundleName)
	{
		// 只有从AssetBundle加载时才能加载AssetBundle
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundle(bundleName.ToLower(), null);
		}
	}
	// 异步加载资源包
	public void loadAssetBundleAsync(string bundleName, AssetBundleLoadCallback callback, object userData = null)
	{
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			// 从Resource加载不能加载AssetBundle
			callback?.Invoke(null, userData);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundleAsync(bundleName.ToLower(), callback, userData);
		}
	}
	// 同步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public T loadResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		T res = null;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			res = mAssetDataBaseLoader.loadResource<T>(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.loadAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// 强制从Resources中同步加载指定资源,name是Resources下的相对路径,errorIfNull表示当找不到资源时是否报错提示
	public T loadInResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		T res = mResourcesLoader.loadResource<T>(name);
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// 同步加载资源的子资源,一般是图集才会有子资源
	public UnityEngine.Object[] loadSubResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		UnityEngine.Object[] res = null;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			res = mAssetDataBaseLoader.loadSubResource<T>(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.loadSubAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// 强制从Resources中同步加载资源的子资源,一般是图集才会有子资源
	public UnityEngine.Object[] loadInSubResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		UnityEngine.Object[] res = mResourcesLoader.loadSubResource<T>(name);
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	// 异步加载资源的子资源,一般是图集才会有子资源
	public bool loadSubResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null, bool errorIfNull = true) where T : UnityEngine.Object
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			ret = mAssetDataBaseLoader.loadResourcesAsync<T>(name, doneCallback, userData);
		}
		else if(mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			ret = mAssetBundleLoader.loadSubAssetAsync<T>(name, doneCallback, userData);
		}
		if (!ret && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return ret;
	}
	// 强制从Resources中异步加载资源的子资源,一般是图集才会有子资源
	public bool loadInSubResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null, bool errorIfNull = true) where T : UnityEngine.Object
	{
		bool ret = mAssetDataBaseLoader.loadResourcesAsync<T>(name, doneCallback, userData);
		if (!ret && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return ret;
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public bool loadResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null, bool errorIfNull = true) where T : UnityEngine.Object
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			ret = mAssetDataBaseLoader.loadResourcesAsync<T>(name, doneCallback, userData);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			ret = mAssetBundleLoader.loadAssetAsync<T>(name, doneCallback, userData);
		}
		if (!ret && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return ret;
	}
	// 强制在Resource中异步加载资源,name是Resources下的相对路径,errorIfNull表示当找不到资源时是否报错提示
	public bool loadInResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null, bool errorIfNull = true) where T : UnityEngine.Object
	{
		bool ret = mResourcesLoader.loadResourcesAsync<T>(name, doneCallback, userData);
		if (!ret && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return ret;
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public void loadAssetsFromUrl<T>(string url, AssetLoadDoneCallback callback, object userData = null) where T : UnityEngine.Object
	{
		mGameFramework.StartCoroutine(loadAssetsUrl(url, Typeof<T>(), callback, userData));
	}
	// 根据一个URL加载资源,一般都是一个网络资源
	public void loadAssetsFromUrl(string url, AssetLoadDoneCallback callback, object userData = null)
	{
		mGameFramework.StartCoroutine(loadAssetsUrl(url, null, callback, userData));
	}
	// 指定资源的类型,然后给fileName添加该类型可能拥有的后缀名,放到fileList中
	public List<string> adjustResourceName<T>(string fileName, List<string> fileList, bool lower = true) where T : UnityEngine.Object
	{
		// 将\\转为/
		rightToLeft(ref fileName);
		// 加上后缀名
		addSuffix(fileName, Typeof<T>(), fileList);
		// 转为小写
		if(lower)
		{
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				fileList[i] = fileList[i].ToLower();
			}
		}
		return fileList;
	}
	// 开放为公有的函数,让外部也能自己注册文件对应类型
	public void registeSuffix(Type t, string suffix)
	{
		if (!mTypeSuffixList.TryGetValue(t, out List<string> list))
		{
			list = new List<string>();
			mTypeSuffixList.Add(t, list);
		}
		list.Add(suffix);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected IEnumerator loadAssetsUrl(string url, Type assetsType, AssetLoadDoneCallback callback, object userData = null)
	{
		logForce("开始下载: " + url);
		if (assetsType == typeof(AudioClip))
		{
			yield return loadAudioClipWithURL(url, callback, userData);
		}
		else if (assetsType == typeof(Texture2D) || assetsType == typeof(Texture))
		{
			yield return loadTextureWithURL(url, callback, userData);
		}
		else if (assetsType == typeof(AssetBundle))
		{
			yield return loadAssetBundleWithURL(url, callback, userData);
		}
		else
		{
			yield return loadFileWithURL(url, callback, userData);
		}
	}
	protected IEnumerator loadAssetBundleWithURL(string url, AssetLoadDoneCallback callback, object userData = null)
	{
		UnityWebRequest www = new UnityWebRequest(url);
		DownloadHandlerAssetBundle downloadHandler = new DownloadHandlerAssetBundle(url, 0);
		www.downloadHandler = downloadHandler;
		yield return www;
		if (www.error != null)
		{
			logForce("下载失败 : " + url + ", info : " + www.error);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			logForce("下载成功:" + url);
			downloadHandler.assetBundle.name = url;
			callback?.Invoke(downloadHandler.assetBundle, null, www.downloadHandler.data, userData, url);
		}
		www.Dispose();
	}
	protected IEnumerator loadAudioClipWithURL(string url, AssetLoadDoneCallback callback, object userData = null)
	{
		UnityWebRequest www = new UnityWebRequest(url);
		DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(url, 0);
		www.downloadHandler = downloadHandler;
		yield return www;
		if (www.error != null)
		{
			logForce("下载失败 : " + url + ", info : " + www.error);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			logForce("下载成功:" + url);
			downloadHandler.audioClip.name = url;
			callback?.Invoke(downloadHandler.audioClip, null, www.downloadHandler.data, userData, url);
		}
		www.Dispose();
	}
	protected IEnumerator loadTextureWithURL(string url, AssetLoadDoneCallback callback, object userData = null)
	{
		UnityWebRequest www = new UnityWebRequest(url);
		www.downloadHandler = new DownloadHandlerTexture();
		yield return www;
		if (www.error != null)
		{
			logForce("下载失败 : " + url + ", info : " + www.error);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			logForce("下载成功:" + url);
			Texture2D tex = DownloadHandlerTexture.GetContent(www);
			tex.name = url;
			callback?.Invoke(tex, null, www.downloadHandler.data, userData, url);
		}
		www.Dispose();
	}
	protected IEnumerator loadFileWithURL(string url, AssetLoadDoneCallback callback, object userData = null)
	{
		// 由于UnityWebRequest无法下载成功,原因暂时未知,所以还是使用WWW进行下载
		WWW www = new WWW(url);
		yield return www;
		if (www.error != null)
		{
			logForce("下载失败 : " + url + ", info : " + www.error);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			logForce("下载成功:" + url + ", size:" + www.bytes.Length);
			callback?.Invoke(null, null, www.bytes, userData, url);
		}
		www.Dispose();
	}
	// 为资源名加上对应的后缀名
	protected List<string> addSuffix(string fileName, Type type, List<string> fileList)
	{
		fileList.Clear();
		if (!mTypeSuffixList.TryGetValue(type, out List<string> list))
		{
			logError("resource type : " + type.ToString() + " is not registered!");
		}
		int suffixCount = list.Count;
		for (int i = 0; i < suffixCount; ++i)
		{
			fileList.Add(fileName + list[i]);
		}
		return fileList;
	}
}