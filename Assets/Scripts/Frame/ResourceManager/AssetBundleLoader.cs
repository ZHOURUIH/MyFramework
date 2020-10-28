using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetBundleLoader : FrameBase
{
	protected Dictionary<string, AssetBundleInfo> mAssetBundleInfoList;
	protected Dictionary<string, AssetInfo> mAssetToBundleInfo;
	protected Dictionary<UnityEngine.Object, AssetBundleInfo> mAssetToAssetBundleInfo;
	protected List<AssetBundleInfo> mRequestBundleList;
	protected Dictionary<string, AssetBundleInfo> mRequestAssetList;
	protected const int ASSET_BUNDLE_COROUTINE = 8;
	protected const int ASSET_COROUTINE = 4;
	protected int mAssetBundleCoroutineCount;
	protected int mAssetCoroutineCount;
	protected bool mInited;
	public AssetBundleLoader()
	{
		mAssetBundleInfoList = new Dictionary<string, AssetBundleInfo>();
		mAssetToBundleInfo = new Dictionary<string, AssetInfo>();
		mRequestBundleList = new List<AssetBundleInfo>();
		mRequestAssetList = new Dictionary<string, AssetBundleInfo>();
		mAssetToAssetBundleInfo = new Dictionary<UnityEngine.Object, AssetBundleInfo>();
	}
	public bool init()
	{
		// 如果资源目录为本地目录,则
		string path = ResourceManager.mResourceRootPath + "StreamingAssets.bytes";
		if (ResourceManager.mLocalRootPath)
		{
			if (!isFileExist(path))
			{
				return false;
			}
			byte[] fileBuffer;
			int fileSize = openFile(path, out fileBuffer, false);
			initAssetConfig(fileBuffer, fileSize);
			releaseFileBuffer(fileBuffer);
		}
		else
		{
			mResourceManager.loadAssetsFromUrl(path, onAssetConfigDownloaded);
		}
		return true;
	}
	public virtual void update(float elapsedTime)
	{
		if(!mInited)
		{
			return;
		}
		if(mAssetToAssetBundleInfo.Count > 0)
		{
			List<UnityEngine.Object> tempList = mListPool.newList(out tempList);
			tempList.AddRange(mAssetToAssetBundleInfo.Keys);
			foreach (var item in tempList)
			{
				if (item == null)
				{
					mAssetToAssetBundleInfo.Remove(item);
				}
			}
			mListPool.destroyList(tempList);
		}
		// 处理资源包异步加载请求
		if (mRequestBundleList.Count > 0 && mAssetBundleCoroutineCount < ASSET_BUNDLE_COROUTINE)
		{
			// 找到第一个依赖项已经加载完毕的资源
			for (int i = 0; i < mRequestBundleList.Count; ++i)
			{
				// 因为新的加载请求是加入到列表的末尾,所以不会影响当前的遍历顺序
				mRequestBundleList[i].loadParentAsync();
				if (mRequestBundleList[i].isAllParentLoaded())
				{
					mGameFramework.StartCoroutine(loadAssetBundleCoroutine(mRequestBundleList[i]));
					mRequestBundleList.RemoveAt(i);
					break;
				}
			}
		}
		// 处理资源异步加载请求
		if (mRequestAssetList.Count > 0 && mAssetCoroutineCount < ASSET_COROUTINE)
		{
			foreach(var item in mRequestAssetList)
			{
				mGameFramework.StartCoroutine(loadAssetCoroutine(item.Value, item.Key));
				mRequestAssetList.Remove(item.Key);
				break;
			}
		}
	}
	public void destroy()
	{
		unloadAll();
	}
	public void unloadAll()
	{
		// 还未开始加载的异步加载资源需要从等待列表中移除
		mRequestAssetList.Clear();
		mRequestBundleList.Clear();
		foreach (var item in mAssetBundleInfoList)
		{
			item.Value.unload();
		}
	}
	public bool unloadAsset<T>(ref T asset) where T : UnityEngine.Object
	{
		// 查找对应的AssetBundle
		if (!mAssetToAssetBundleInfo.ContainsKey(asset))
		{
			return false;			
		}
		bool ret = mAssetToAssetBundleInfo[asset].unloadAsset(asset);
		if (ret)
		{
			mAssetToAssetBundleInfo.Remove(asset);
			asset = null;
		}
		return ret;
	}
	public bool isInited() { return mInited; }
	public Dictionary<string, AssetBundleInfo> getAssetBundleInfoList() { return mAssetBundleInfoList; }
	public AssetBundleInfo getAssetBundleInfo(string name)
	{
		// 因为在初始化过程中需要调用该函数,所以此处不检测是否初始化完成
		if (mAssetBundleInfoList.ContainsKey(name))
		{
			return mAssetBundleInfoList[name];
		}
		return null;
	}
	public void unloadAssetBundle(string bundleName)
	{
		if(mAssetBundleInfoList.ContainsKey(bundleName))
		{
			mAssetBundleInfoList[bundleName].unload();
		}
	}
	// 卸载指定路径中的所有资源包
	public void unloadPath(string path)
	{
		foreach (var item in mAssetBundleInfoList)
		{
			if (startWith(item.Key, path))
			{
				// 还未开始加载的异步加载资源需要从等待列表中移除
				List<string> tempList = mListPool.newList(out tempList);
				tempList.AddRange(mRequestAssetList.Keys);
				foreach(var assetName in tempList)
				{
					if(mRequestAssetList[assetName] == item.Value)
					{
						mRequestAssetList.Remove(assetName);
					}
				}
				mListPool.destroyList(tempList);
				mRequestBundleList.Remove(item.Value);
				item.Value.unload();
			}
		}
	}
	// 得到文件夹中的所有文件,文件夹被打包成一个AssetBundle,返回AssetBundle中的所有资源名
	public void getFileList(string path, List<string> list)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return;
		}
		removeEndSlash(ref path);
		list.Clear();
		// 该文件夹被打包成一个AssetBundle
		foreach (var item in mAssetBundleInfoList)
		{
			if (startWith(item.Key, path))
			{
				var assetList = item.Value.getAssetList();
				foreach (var asset in assetList)
				{
					list.Add(getFileNameNoSuffix(asset.Key));
				}
			}
		}
	}
	// 资源是否已经加载
	public bool isAssetLoaded<T>(string fileName) where T : UnityEngine.Object
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return false;
		}
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(fileName, fileNameList);
		// 只返回第一个找到的资源
		int count = fileNameList.Count;
		for (int i = 0; i < count; ++i)
		{
			string fileNameWithSuffix = fileNameList[i];
			// 找不到资源则直接返回
			if (mAssetToBundleInfo.ContainsKey(fileNameWithSuffix))
			{
				mListPool.destroyList(fileNameList);
				AssetBundleInfo bundleInfo = mAssetToBundleInfo[fileNameWithSuffix].getAssetBundle();
				if (bundleInfo.getLoadState() != LOAD_STATE.LOADED)
				{
					return false;
				}
				return bundleInfo.getAssetInfo(fileNameWithSuffix).isLoaded();
			}
		}
		mListPool.destroyList(fileNameList);
		return false;
	}
	// 获得资源,如果资源包未加载,则返回空
	public T getAsset<T>(string fileName) where T : UnityEngine.Object
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(fileName, fileNameList);
		// 只返回第一个找到的资源
		int count = fileNameList.Count;
		for (int i = 0; i < count; ++i)
		{
			string fileNameWithSuffix = fileNameList[i];
			if (mAssetToBundleInfo.ContainsKey(fileNameWithSuffix))
			{
				mListPool.destroyList(fileNameList);
				AssetBundleInfo bundleInfo = mAssetToBundleInfo[fileNameWithSuffix].getAssetBundle();
				return bundleInfo.getAssetInfo(fileNameWithSuffix).getAsset() as T;
			}
		}
		mListPool.destroyList(fileNameList);
		return null;
	}
	// 检查指定的已加载的AssetBundle的依赖项是否有未加载的情况,如果有未加载的则同步加载
	public void checkAssetBundleDependenceLoaded(string bundleName)
	{
		if (!mInited)
		{
			return;
		}
		if (mAssetBundleInfoList.ContainsKey(bundleName))
		{
			mAssetBundleInfoList[bundleName].checkAssetBundleDependenceLoaded();
		}
	}
	// 异步加载资源包
	public void loadAssetBundleAsync(string bundleName, AssetBundleLoadCallback callback, object userData)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			callback?.Invoke(null, userData);
			return;
		}
		if (mAssetBundleInfoList.ContainsKey(bundleName))
		{
			mAssetBundleInfoList[bundleName].loadAssetBundleAsync(callback, userData);
		}
		else
		{
			logError("can not find AssetBundle : " + bundleName);
		}
	}
	// 同步加载资源包
	public List<UnityEngine.Object> loadAssetBundle(string bundleName)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		if (mAssetBundleInfoList.ContainsKey(bundleName))
		{
			AssetBundleInfo bundleInfo = mAssetBundleInfoList[bundleName];
			if (bundleInfo.getLoadState() == LOAD_STATE.LOADING ||
				bundleInfo.getLoadState() == LOAD_STATE.WAIT_FOR_LOAD)
			{
				logError("asset bundle is loading or waiting for load, can not load again! name : " + bundleName);
				return null;
			}
			// 如果还未加载,则加载资源包
			if (bundleInfo.getLoadState() == LOAD_STATE.UNLOAD)
			{
				bundleInfo.loadAssetBundle();
			}
			// 加载完毕,返回资源列表
			if (bundleInfo.getLoadState() == LOAD_STATE.LOADED)
			{
				var assetList = new List<UnityEngine.Object>();
				var bundleAssetlist = bundleInfo.getAssetList();
				foreach (var item in bundleAssetlist)
				{
					if(item.Value.isLoaded())
					{
						assetList.Add(item.Value.getAsset());
					}
				}
				return assetList;
			}
		}
		return null;
	}
	// 异步加载资源,文件名称,不带后缀,Resources下的相对路径
	public bool loadSubAssetAsync<T>(string fileName, AssetLoadDoneCallback callback, object[] userData) where T : UnityEngine.Object
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return false;
		}
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(fileName, fileNameList);
		// 只加载第一个找到的资源,所以不允许有重名的同类资源
		int loadedCount = 0;
		int count = fileNameList.Count;
		for (int i = 0; i < count; ++i)
		{
			string fileNameWithSuffix = fileNameList[i];
			if (mAssetToBundleInfo.ContainsKey(fileNameWithSuffix))
			{
				AssetBundleInfo bundleInfo = mAssetToBundleInfo[fileNameWithSuffix].getAssetBundle();
				if (bundleInfo.loadSubAssetAsync(ref fileNameWithSuffix, callback, userData, fileName))
				{
					++loadedCount;
					break;
				}
			}
		}
		mListPool.destroyList(fileNameList);
		return loadedCount != 0;
	}
	// 同步加载资源,文件名称,不带后缀,Resources下的相对路径
	public UnityEngine.Object[] loadSubAsset<T>(string fileName) where T : UnityEngine.Object
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(fileName, fileNameList);
		// 只加载第一个找到的资源,所以不允许有重名的同类资源
		UnityEngine.Object[] res = null;
		int count = fileNameList.Count;
		for (int i = 0; i < count; ++i)
		{
			string fileNameWithSuffix = fileNameList[i];
			if (mAssetToBundleInfo.ContainsKey(fileNameWithSuffix))
			{
				AssetBundleInfo bundleInfo = mAssetToBundleInfo[fileNameWithSuffix].getAssetBundle();
				res = bundleInfo.loadSubAsset(ref fileNameWithSuffix);
				if (res != null)
				{
					break;
				}
			}
		}
		mListPool.destroyList(fileNameList);
		return res;
	}
	// 同步加载资源,文件名称,不带后缀,Resources下的相对路径
	public T loadAsset<T>(string fileName) where T : UnityEngine.Object
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(fileName, fileNameList);
		// 只加载第一个找到的资源,所以不允许有重名的同类资源
		T res = null;
		int count = fileNameList.Count;
		for (int i = 0; i < count; ++i)
		{
			string fileNameWithSuffix = fileNameList[i];
			if (mAssetToBundleInfo.ContainsKey(fileNameWithSuffix))
			{
				res = mAssetToBundleInfo[fileNameWithSuffix].getAssetBundle().loadAsset<T>(ref fileNameWithSuffix);
				if(res != null)
				{
					break;
				}
			}
		}
		mListPool.destroyList(fileNameList);
		return res;
	}
	// 异步加载资源
	public bool loadAssetAsync<T>(string fileName, AssetLoadDoneCallback doneCallback, object[] userData) where T : UnityEngine.Object
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return false;
		}
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(fileName, fileNameList);
		// 只加载第一个找到的资源,所以不允许有重名的同类资源
		int loadedCount = 0;
		int count = fileNameList.Count;
		for(int i = 0; i < count; ++i)
		{
			string fileNameWithSuffix = fileNameList[i];
			if (mAssetToBundleInfo.ContainsKey(fileNameWithSuffix))
			{
				if (mAssetToBundleInfo[fileNameWithSuffix].getAssetBundle().loadAssetAsync(fileNameWithSuffix, doneCallback, fileName, userData))
				{
					++loadedCount;
					break;
				}
			}
		}
		mListPool.destroyList(fileNameList);
		return loadedCount != 0;
	}
	// 请求异步加载资源包
	public void requestLoadAssetBundle(AssetBundleInfo bundleInfo)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return;
		}
		if(!mRequestBundleList.Contains(bundleInfo))
		{
			mRequestBundleList.Add(bundleInfo);
		}
	}
	public void requestLoadAsset(AssetBundleInfo bundleInfo, string fileNameWithSuffix)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return;
		}
		if (!mRequestAssetList.ContainsKey(fileNameWithSuffix))
		{
			mRequestAssetList.Add(fileNameWithSuffix, bundleInfo);
		}
	}
	public void updateAssetBundleConfig(byte[] fileBuffer)
	{
		initAssetConfig(fileBuffer, fileBuffer.Length);
	}
	public void notifyAssetLoaded(UnityEngine.Object asset, AssetBundleInfo bundle)
	{
		// 保存加载出的资源与资源包的信息
		if (asset != null && !mAssetToAssetBundleInfo.ContainsKey(asset))
		{
			mAssetToAssetBundleInfo.Add(asset, bundle);
		}
	}
	//-------------------------------------------------------------------------------------------------------------------------------------
	protected IEnumerator loadAssetBundleCoroutine(AssetBundleInfo bundleInfo)
	{
		++mAssetBundleCoroutineCount;
		// 先确保依赖项全部已经加载完成,才能开始加载当前请求的资源包
		while (!bundleInfo.isAllParentLoaded())
		{
			bundleInfo.loadParentAsync();
			yield return null;
		}
		logInfo(bundleInfo.getBundleFileName() + " start load bundle", LOG_LEVEL.NORMAL);
		bundleInfo.setLoadState(LOAD_STATE.LOADING);
		AssetBundle assetBundle = null;
		// 加载远端文件时使用www
		if (!ResourceManager.mLocalRootPath)
		{
			if (ResourceManager.mPersistentFirst)
			{
				string path = FrameDefine.F_PERSISTENT_DATA_PATH + bundleInfo.getBundleFileName();
				checkDownloadPath(ref path, ResourceManager.mLocalRootPath);
				UnityWebRequest www = new UnityWebRequest(path);
				DownloadHandlerAssetBundle downloadHandler = new DownloadHandlerAssetBundle(path, 0);
				www.downloadHandler = downloadHandler;
				yield return www;
				if (www.error == null)
				{
					assetBundle = downloadHandler.assetBundle;
				}
				www.Dispose();
			}
			if(assetBundle == null)
			{
				string path = ResourceManager.mResourceRootPath + bundleInfo.getBundleFileName();
				checkDownloadPath(ref path, ResourceManager.mLocalRootPath);
				UnityWebRequest www = new UnityWebRequest(path);
				DownloadHandlerAssetBundle downloadHandler = new DownloadHandlerAssetBundle(path, 0);
				www.downloadHandler = downloadHandler;
				yield return www;
				if (www.error == null)
				{
					assetBundle = downloadHandler.assetBundle;
				}
				www.Dispose();
			}
		}
		// 直接从文件加载,只能加载本地文件
		else
		{
			AssetBundleCreateRequest request = null;
			string path = FrameDefine.F_PERSISTENT_DATA_PATH + bundleInfo.getBundleFileName();
			if (ResourceManager.mPersistentFirst && isFileExist(path))
			{
				request = AssetBundle.LoadFromFileAsync(path);
			}
			if(request == null)
			{
				path = ResourceManager.mResourceRootPath + bundleInfo.getBundleFileName();
				if(isFileExist(path))
				{
					request = AssetBundle.LoadFromFileAsync(path);
				}
			}
			if (request != null)
			{
				yield return request;
				assetBundle = request.assetBundle;
			}
			else
			{
				logError("can not load asset bundle async : " + bundleInfo.getBundleFileName());
				--mAssetBundleCoroutineCount;
			}
		}
		logInfo(bundleInfo.getBundleFileName() + " load bundle done", LOG_LEVEL.NORMAL);
		yield return new WaitForEndOfFrame();
		// 通知AssetBundleInfo
		bundleInfo.notifyAssetBundleAsyncLoadedDone(assetBundle);
		--mAssetBundleCoroutineCount;
	}
	protected IEnumerator loadAssetCoroutine(AssetBundleInfo bundle, string fileNameWithSuffix)
	{
		++mAssetCoroutineCount;
		if (bundle.getLoadState() != LOAD_STATE.LOADED)
		{
			logError("asset bundle is not loaded, can not load asset async!");
			--mAssetCoroutineCount;
			yield break;
		}
		bundle.getAssetInfo(fileNameWithSuffix).setLoadState(LOAD_STATE.LOADING);
		if(bundle.getAssetBundle() != null)
		{
			AssetBundleRequest assetRequest = bundle.getAssetBundle().LoadAssetWithSubAssetsAsync(FrameDefine.P_GAME_RESOURCES_PATH + fileNameWithSuffix);
			if (assetRequest == null)
			{
				logError("can not load asset async : " + fileNameWithSuffix);
				--mAssetCoroutineCount;
				yield break;
			}
			yield return assetRequest;
			bundle.notifyAssetLoaded(fileNameWithSuffix, assetRequest.allAssets);
		}
		else
		{
			bundle.notifyAssetLoaded(fileNameWithSuffix, null);
		}
		--mAssetCoroutineCount;
	}
	protected void onAssetConfigDownloaded(UnityEngine.Object res, UnityEngine.Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		initAssetConfig(bytes, bytes.Length);
	}
	protected void initAssetConfig(byte[] fileBuffer, int fileSize)
	{
		byte[] tempStringBuffer = mBytesPool.newBytes(256);
		mAssetBundleInfoList.Clear();
		mAssetToBundleInfo.Clear();
		Serializer serializer = new Serializer(fileBuffer, fileSize);
		serializer.read(out int assetBundleCount);
		for(int i = 0; i < assetBundleCount; ++i)
		{
			// AssetBundle名字
			serializer.readString(tempStringBuffer, tempStringBuffer.Length);
			string bundleName = getFileNameNoSuffix(bytesToString(tempStringBuffer));
			if (!mAssetBundleInfoList.ContainsKey(bundleName))
			{
				mAssetBundleInfoList.Add(bundleName, new AssetBundleInfo(bundleName));
			}
			// AssetBundle包含的所有Asset的名字
			AssetBundleInfo bundleInfo = mAssetBundleInfoList[bundleName];
			serializer.read(out int assetCount);
			for(int k = 0; k < assetCount; ++k)
			{
				serializer.readString(tempStringBuffer, tempStringBuffer.Length);
				string assetName = bytesToString(tempStringBuffer);
				bundleInfo.addAssetName(assetName);
				mAssetToBundleInfo.Add(assetName, bundleInfo.getAssetInfo(assetName));
			}
			// AssetBundle的所有依赖项
			serializer.read(out int depCount);
			for (int j = 0; j < depCount; ++j)
			{
				serializer.readString(tempStringBuffer, tempStringBuffer.Length);
				bundleInfo.addParent(getFileNameNoSuffix(bytesToString(tempStringBuffer)));
			}
		}
		mBytesPool.destroyBytes(tempStringBuffer);
		// 配置清单解析完毕后,为每个AssetBundleInfo查找对应的依赖项
		foreach (var info in mAssetBundleInfoList)
		{
			info.Value.findAllDependence();
		}
		mInited = true;
		logInfo("AssetBundle初始化完成, AssetBundle count : " + mAssetBundleInfoList.Count, LOG_LEVEL.FORCE);
	}
}