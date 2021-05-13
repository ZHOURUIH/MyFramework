using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

public class ResourceManager : FrameSystem
{
	protected Dictionary<Type, List<string>> mTypeSuffixList;    // 资源类型对应的后缀名
	protected AssetBundleLoader mAssetBundleLoader;
	protected ResourceLoader mResourceLoader;
	protected string mResourceRootPath;							// 当mLoadSource为1时,AssetBundle资源所在根目录,以/结尾,默认为StreamingAssets,可以为远端目录
	protected bool mLocalRootPath;								// mResourceRootPath是否为本地的路径
	protected LOAD_SOURCE mLoadSource;                          // 0为从Resources加载,1为从AssetBundle加载
	public ResourceManager()
	{
		mCreateObject = true;
		mResourceRootPath = FrameDefine.FULL_RESOURCE_PATH;
		mLocalRootPath = true;
		mAssetBundleLoader = new AssetBundleLoader();
		mResourceLoader = new ResourceLoader();
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
		mLoadSource = (LOAD_SOURCE)(int)mFrameConfig.getFloat(FRAME_FLOAT.LOAD_RESOURCES);
#else
		mLoadSource = LOAD_SOURCE.ASSET_BUNDLE;
#endif
#if UNITY_EDITOR
		mObject.AddComponent<ResourcesManagerDebug>();
#endif
		// 如果从Resources加载,则固定为默认值
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			mResourceRootPath = FrameDefine.F_STREAMING_ASSETS_PATH;
			mLocalRootPath = true;
		}
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			mResourceLoader.init();
		}
		else if(mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.init();
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAssetBundleLoader.update(elapsedTime);
		mResourceLoader.update(elapsedTime);
	}
	public override void destroy()
	{
		mAssetBundleLoader?.destroy();
		mResourceLoader?.destroy();
		base.destroy();
	}
	public string getResourceRootPath() { return mResourceRootPath; }
	public bool isLocalRootPath() { return mLocalRootPath; }
	public AssetBundleLoader getAssetBundleLoader() { return mAssetBundleLoader; }
	// 卸载加载的资源,不是实例化出的物体
	public bool unload<T>(ref T obj) where T : UnityEngine.Object
	{
		if(mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			return mResourceLoader.unloadResource(ref obj);
		}
		if(mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			return mAssetBundleLoader.unloadAsset(ref obj);
		}
		return false;
	}
	// 根据文件夹前缀卸载文件夹,实际上与unloadPath逻辑完全一致
	public void unloadPathPreName(string pathPreName)
	{
		unloadPath(pathPreName);
	}
	public void unloadPath(string path)
	{
		removeEndSlash(ref path);
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			mResourceLoader.unloadPath(path);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.unloadPath(path.ToLower());
		}
	}
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
	// 指定资源是否已经加载
	public bool isResourceLoaded<T>(string name) where T : UnityEngine.Object
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			ret = mResourceLoader.isResourceLoaded(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			ret = mAssetBundleLoader.isAssetLoaded<T>(name);
		}
		return ret;
	}
	// 获得资源
	public T getResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		T res = null;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			res = mResourceLoader.getResource(name) as T;
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
	// path为resources下相对路径,返回的列表中文件名带相对路径不带后缀
	// 如果从Resource中加载,则区分大小写,如果从AssetBundle中加载,传入的路径不区分大小写,返回的文件列表全部为小写
	// lower表示是否将返回列表中的字符串全部转为小写
	public void getFileList(string path, List<string> fileList, bool lower = false)
	{
		fileList.Clear();
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			mResourceLoader.getFileList(path, fileList);
			if(lower)
			{
				int count = fileList.Count;
				for (int i = 0; i < count; ++i)
				{
					fileList[i] = fileList[i].ToLower();
				}
			}
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.getFileList(path.ToLower(), fileList);
		}
	}
	public bool isSyncLoadAvalaible()
	{
		// 如果从AssetBundle加载,并且资源目录为远端目录,则不能同步加载资源
		return mLoadSource != LOAD_SOURCE.ASSET_BUNDLE || mLocalRootPath;
	}
	public void checkAssetBundleDependenceLoaded(string bundleName)
	{
		if(mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.checkAssetBundleDependenceLoaded(bundleName.ToLower());
		}
	}
	public void loadAssetBundle(string bundleName)
	{
		// 只有从AssetBundle加载时才能加载AssetBundle
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundle(bundleName.ToLower(), null);
		}
	}
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
	// name是GameResources下的相对路径,errorIfNull表示当找不到资源时是否报错提示
	public T loadResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		T res = null;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			res = mResourceLoader.loadResource<T>(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			if(!mLocalRootPath)
			{
				logError("资源根目录不是本地目录,不能同步加载:" + name);
			}
			res = mAssetBundleLoader.loadAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	public UnityEngine.Object[] loadSubResource<T>(string name, bool errorIfNull = true) where T : UnityEngine.Object
	{
		UnityEngine.Object[] res = null;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			res = mResourceLoader.loadSubResource<T>(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			if (!mLocalRootPath)
			{
				logError("资源根目录不是本地目录,不能同步加载:" + name);
			}
			res = mAssetBundleLoader.loadSubAsset<T>(name);
		}
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name);
		}
		return res;
	}
	public bool loadSubResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null, bool errorIfNull = true) where T : UnityEngine.Object
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			ret = mResourceLoader.loadResourcesAsync<T>(name, doneCallback, userData);
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
	// name是Resources下的相对路径,errorIfNull表示当找不到资源时是否报错提示
	public bool loadResourceAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null, bool errorIfNull = true) where T : UnityEngine.Object
	{
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.RESOURCES)
		{
			ret = mResourceLoader.loadResourcesAsync<T>(name, doneCallback, userData);
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
	public void loadAssetsFromUrl<T>(string url, AssetLoadDoneCallback callback, object userData = null) where T : UnityEngine.Object
	{
		mGameFramework.StartCoroutine(loadAssetsUrl(url, Typeof<T>(), callback, userData));
	}
	public void loadAssetsFromUrl(string url, AssetLoadDoneCallback callback, object userData = null)
	{
		mGameFramework.StartCoroutine(loadAssetsUrl(url, null, callback, userData));
	}
	// 加载StreamingAssets中不打包AB的资源,路径为StreamingAssets下的相对路径,带后缀名
	// 需要调用FileUtility.releaseFileBuffer回收fileBytes
	public void loadStreamingAssetsFile(string filePath, out byte[] fileBytes)
	{
		// 优先从PersistentDataPath中加载
		openFile(FrameDefine.FULL_RESOURCE_PATH + filePath, out fileBytes, false);
	}
	public List<string> adjustResourceName<T>(string fileName, List<string> fileList, bool lower = true) where T : UnityEngine.Object
	{
		// 将\\转为/,加上后缀名,转为小写
		rightToLeft(ref fileName);
		addSuffix(fileName, Typeof<T>(), fileList);
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
	//--------------------------------------------------------------------------------------------------------------------------------------------
	protected IEnumerator loadAssetsUrl(string url, Type assetsType, AssetLoadDoneCallback callback, object userData = null)
	{
		log("开始下载: " + url, LOG_LEVEL.HIGH);
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
			log("下载失败 : " + url + ", info : " + www.error, LOG_LEVEL.FORCE);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			log("下载成功:" + url, LOG_LEVEL.FORCE);
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
			log("下载失败 : " + url + ", info : " + www.error, LOG_LEVEL.FORCE);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			log("下载成功:" + url, LOG_LEVEL.FORCE);
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
			log("下载失败 : " + url + ", info : " + www.error, LOG_LEVEL.FORCE);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			log("下载成功:" + url, LOG_LEVEL.FORCE);
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
			log("下载失败 : " + url + ", info : " + www.error, LOG_LEVEL.FORCE);
			callback?.Invoke(null, null, null, userData, url);
		}
		else
		{
			log("下载成功:" + url + ", size:" + www.bytes.Length, LOG_LEVEL.FORCE);
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