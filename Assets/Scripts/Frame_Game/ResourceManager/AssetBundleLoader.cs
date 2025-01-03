﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static FrameUtility;
using static FileUtility;
using static FrameBase;
using static StringUtility;
using static BinaryUtility;
using static FrameDefine;
using static FrameDefineBase;
using static FrameEditorUtility;

// 从AssetBundle中加载资源
public class AssetBundleLoader
{
	protected Dictionary<UObject, AssetBundleInfo> mAssetToAssetBundleInfo = new();	// 根据加载的Asset查找所属AssetBundle的列表
	protected Dictionary<string, AssetBundleInfo> mAssetBundleInfoList = new();		// 根据名字查找AssetBundle的列表,此名字不含后缀
	protected Dictionary<string, AssetBundleInfo> mRequestAssetList = new();		// 请求异步加载的Asset列表
	protected Dictionary<string, AssetInfo> mAssetToBundleInfo = new();				// 根据资源文件名查找Asset信息的列表,初始化时就会填充此列表
	protected List<AssetBundleInfo> mRequestBundleList = new();						// 请求异步加载的AssetBundle列表
	protected HashSet<Coroutine> mCoroutineList = new();							// 当前的协程列表
	protected HashSet<string> mDontUnloadAssetBundle = new();						// 即使没有引用也不会调用卸载的AssetBundle
	protected WaitForEndOfFrame mWaitForEndOfFrame = new();                         // 用于避免GC
	protected string mDownloadURL;													// 资源包下载的地址
	protected int mAssetBundleCoroutineCount;										// 当前加载AssetBundle的协程数量
	protected int mAssetCoroutineCount;												// 当前加载Asset的协程数量
	protected bool mAutoLoad = true;												// 当资源可用时是否自动初始化AssetBundle
	protected bool mInited;															// AssetBundleLoader是否已经初始化
	protected const int ASSET_BUNDLE_COROUTINE = 8;									// 加载AssetBundle的协程最大数量
	protected const int ASSET_COROUTINE = 4;									    // 加载Asset的协程最大数量
	public void initAssets(Action callback)
	{
		if (!mAutoLoad)
		{
			return;
		}

		// 卸载所有已加载的AssetBundle
		unloadAll();
		// 加载AssetBundle的配置文件
		string filePath = availableReadPath(STREAMING_ASSET_FILE);
		openFileAsync(filePath, false, (byte[] fileBuffer)=>
		{
			if (fileBuffer == null)
			{
				logError(STREAMING_ASSET_FILE + "描述文件加载失败,路径:" + filePath);
				return;
			}
			initAssetConfig(fileBuffer);
			callback?.Invoke();
		});
	}
	public void update(float elapsedTime)
	{
		if (!mInited)
		{
			return;
		}

		// 处理资源包异步加载请求
		if (mRequestBundleList.Count > 0 && mAssetBundleCoroutineCount < ASSET_BUNDLE_COROUTINE)
		{
			// 找到第一个依赖项已经加载完毕的资源
			for (int i = 0; i < mRequestBundleList.Count; ++i)
			{
				// 因为新的加载请求是加入到列表的末尾,所以不会影响当前的遍历顺序
				AssetBundleInfo info = mRequestBundleList[i];
				info.loadParentAsync();
				if (info.isAllParentLoaded())
				{
					mCoroutineList.Add(mGameFramework.StartCoroutine(loadAssetBundleCoroutine(info)));
					mRequestBundleList.RemoveAt(i);
					break;
				}
			}
		}
		// 处理资源异步加载请求
		if (mRequestAssetList.Count > 0 && mAssetCoroutineCount < ASSET_COROUTINE)
		{
			foreach (var item in mRequestAssetList)
			{
				mCoroutineList.Add(mGameFramework.StartCoroutine(loadAssetCoroutine(item.Value, item.Key)));
				mRequestAssetList.Remove(item.Key);
				break;
			}
		}

		// 更新检查所有资源包是否需要卸载
		foreach (AssetBundleInfo bundle in mAssetBundleInfoList.Values)
		{
			bundle.update(elapsedTime);
		}
	}
	public void destroy()
	{
		unloadAll();
	}
	public bool isDontUnloadAssetBundle(string bundleFileName) { return mDontUnloadAssetBundle.Contains(bundleFileName); }
	public void unloadAll()
	{
		foreach (Coroutine item in mCoroutineList)
		{
			mGameFramework.StopCoroutine(item);
		}
		mCoroutineList.Clear();
		mAssetBundleCoroutineCount = 0;
		mAssetCoroutineCount = 0;
		// 还未开始加载的异步加载资源需要从等待列表中移除
		mRequestAssetList.Clear();
		mRequestBundleList.Clear();
		mAssetToAssetBundleInfo.Clear();
		foreach (AssetBundleInfo item in mAssetBundleInfoList.Values)
		{
			item.unload();
		}
	}
	public bool unloadAsset<T>(ref T asset, bool showError) where T : UObject
	{
		if (asset == null)
		{
			return false;
		}
		// 查找对应的AssetBundle
		if (!mAssetToAssetBundleInfo.Remove(asset, out AssetBundleInfo info))
		{
			if (showError)
			{
				logWarning("卸载失败,资源可能已经卸载,asset:" + asset.name);
			}
			return false;
		}
		if (!info.unloadAsset(asset))
		{
			return false;
		}
		asset = null;
		return true;
	}
	public bool isInited() { return mInited; }
	public void setDownloadURL(string url) { mDownloadURL = url; }
	public string getDownloadURL() { return mDownloadURL; }
	// 因为在初始化过程中需要调用该函数,所以此处不检测是否初始化完成
	public AssetBundleInfo getAssetBundleInfo(string name) { return mAssetBundleInfoList.get(name); }
	public void unloadAssetBundle(string bundleName)
	{
		if (!mAssetBundleInfoList.TryGetValue(bundleName.ToLower(), out AssetBundleInfo info))
		{
			return;
		}
		foreach (AssetInfo item in info.getAssetList().Values)
		{
			mAssetToAssetBundleInfo.Remove(item.getAsset());
		}
		info.unload();
	}
	// 卸载指定路径中的所有资源包
	public void unloadPath(string path)
	{
		path = path.ToLower();
		using var a = new ListScope<string>(out var tempList);
		foreach (var item in mAssetBundleInfoList)
		{
			if (!startWith(item.Key, path))
			{
				continue;
			}
			// 还未开始加载的异步加载资源需要从等待列表中移除
			tempList.setRange(mRequestAssetList.Keys);
			foreach (string assetName in tempList)
			{
				if (mRequestAssetList.get(assetName) == item.Value)
				{
					mRequestAssetList.Remove(assetName);
				}
			}
			mRequestBundleList.Remove(item.Value);
			item.Value.unload();
		}
	}
	// 资源是否已经加载,文件名称带后缀,GameResources下的相对路径
	public bool isAssetLoaded<T>(string fileName) where T : UObject
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return false;
		}
		// 找不到资源则直接返回
		string fileNameLower = fileName.ToLower();
		if (!mAssetToBundleInfo.TryGetValue(fileNameLower, out AssetInfo asset))
		{
			logError("can not find resource : " + fileName + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (fileName.Contains(' ') || fileName.Contains('　') ? "注意此文件名中带有空格" : ""));
			return false;
		}
		AssetBundleInfo bundleInfo = asset.getAssetBundle();
		return bundleInfo.getLoadState() == LOAD_STATE.LOADED && bundleInfo.getAssetInfo(fileNameLower).isLoaded();
	}
	// 获得资源,如果资源包未加载,则返回空,文件名称带后缀,GameResources下的相对路径
	public T getAsset<T>(string fileName) where T : UObject
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		// 只返回第一个找到的资源
		string fileNameLower = fileName.ToLower();
		return mAssetToBundleInfo.get(fileNameLower)?.getAssetBundle().getAssetInfo(fileNameLower).getAsset() as T;
	}
	// 检查指定的已加载的AssetBundle的依赖项是否有未加载的情况,如果有未加载的则同步加载
	public void checkAssetBundleDependenceLoaded(string bundleName)
	{
		if (!mInited)
		{
			return;
		}
		mAssetBundleInfoList.get(bundleName.ToLower())?.checkAssetBundleDependenceLoaded();
	}
	// 异步加载资源包,如果资源包未下载,则会先开始下载
	public void loadAssetBundleAsync(string bundleName, AssetBundleCallback callback)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			callback?.Invoke(null);
			return;
		}
		if (!mAssetBundleInfoList.TryGetValue(bundleName.ToLower(), out AssetBundleInfo info))
		{
			logError("can not find AssetBundle : " + bundleName);
			return;
		}
		info.loadAssetBundleAsync(callback);
	}
	// 同步加载资源包
	public void loadAssetBundle(string bundleName, List<UObject> assetList)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return;
		}
		if (mAssetBundleInfoList.TryGetValue(bundleName.ToLower(), out AssetBundleInfo bundleInfo))
		{
			if (bundleInfo.getLoadState() == LOAD_STATE.DOWNLOADING ||
				bundleInfo.getLoadState() == LOAD_STATE.LOADING ||
				bundleInfo.getLoadState() == LOAD_STATE.WAIT_FOR_LOAD)
			{
				logError("asset bundle is loading or waiting for load, can not load again! name : " + bundleName);
				return;
			}
			// 如果还未加载,则加载资源包
			if (bundleInfo.getLoadState() == LOAD_STATE.UNLOAD)
			{
				bundleInfo.loadAssetBundle();
			}
			// 加载完毕,返回资源列表
			if (bundleInfo.getLoadState() == LOAD_STATE.LOADED)
			{
				if (assetList == null)
				{
					return;
				}
				foreach (AssetInfo item in bundleInfo.getAssetList().Values)
				{
					if (item.isLoaded())
					{
						assetList.Add(item.getAsset());
					}
				}
				return;
			}
		}
		return;
	}
	// 同步加载资源,文件名称带后缀,GameResources下的相对路径
	public UObject[] loadSubAsset<T>(string fileName, out UObject mainAsset) where T : UObject
	{
		mainAsset = null;
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		// 只加载第一个找到的资源,所以不允许有重名的同类资源
		string fileNameLower = fileName.ToLower();
		return mAssetToBundleInfo.get(fileNameLower)?.getAssetBundle().loadSubAssets(fileNameLower, out mainAsset);
	}
	// 同步加载资源,文件名称带后缀,GameResources下的相对路径
	public T loadAsset<T>(string fileName) where T : UObject
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		string fileNameLower = fileName.ToLower();
		return mAssetToBundleInfo.get(fileNameLower)?.getAssetBundle().loadAsset<T>(fileNameLower);
	}
	// 异步加载资源,文件名称带后缀,GameResources下的相对路径
	public CustomAsyncOperation loadAssetAsync<T>(string fileName, bool errorIfNull, AssetLoadDoneCallback doneCallback) where T : UObject
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return null;
		}
		string fileNameLower = fileName.ToLower();
		if (!mAssetToBundleInfo.TryGetValue(fileNameLower, out AssetInfo asset))
		{
			if (errorIfNull)
			{
				logError("can not find resource : " + fileName + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (fileName.Contains(' ') || fileName.Contains('　') ? "注意此文件名中带有空格" : ""));
			}
			doneCallback?.Invoke(null, null, null, fileName);
			return null;
		}
		return asset.getAssetBundle().loadAssetAsync(fileNameLower, doneCallback, fileName);
	}
	// 请求异步加载资源包
	public void requestLoadAssetBundle(AssetBundleInfo bundleInfo)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return;
		}
		mRequestBundleList.addUnique(bundleInfo);
	}
	public void requestLoadAsset(AssetBundleInfo bundleInfo, string fileNameWithSuffix)
	{
		if (!mInited)
		{
			logError("AssetBundleLoader is not inited!");
			return;
		}
		mRequestAssetList.TryAdd(fileNameWithSuffix, bundleInfo);
	}
	public void notifyAssetLoaded(UObject asset, AssetBundleInfo bundle)
	{
		// 保存加载出的资源与资源包的信息
		if (asset != null)
		{
			mAssetToAssetBundleInfo.TryAdd(asset, bundle);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 下载资源包的协程
	protected IEnumerator downloadAssetBundleCoroutine(AssetBundleInfo bundleInfo, BytesCallback callback)
	{
		if (mDownloadURL == null)
		{
			logError("资源下载地址未设置,无法动态下载资源文件");
			callback?.Invoke(null);
			yield break;
		}

		// 已经正在下载,则只是加入到回调列表
		if (bundleInfo.getLoadState() == LOAD_STATE.DOWNLOADING)
		{
			CustomAsyncOperation op = new();
			bundleInfo.addDownloadCallback((AssetBundleInfo info, byte[] bytes)=>
			{
				op.mFinish = true;
				callback?.Invoke(bytes);
			});
			yield return op;
		}
		// 没有正在下载,开始下载
		else
		{
			bundleInfo.setLoadState(LOAD_STATE.DOWNLOADING);
			string bundleFileName = bundleInfo.getBundleFileName();
			yield return ResourceManager.loadAssetsFromUrlWaiting(mDownloadURL + bundleFileName, (byte[] bytes) =>
			{
				if (bytes != null)
				{
					// 写入到本地,并且更新资源列表
					writeFile(F_PERSISTENT_ASSETS_PATH + bundleFileName, bytes, bytes.Length);
					GameFileInfo fileInfo = new()
					{
						mFileName = bundleFileName,
						mFileSize = bytes.Length,
						mMD5 = generateFileMD5(bytes)
					};
					mAssetVersionSystem.addPersistentFile(fileInfo);
					// 更新本地的文件列表
					writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList());
				}
				// 回调
				bundleInfo.notifyAssetBundleDownloaded(bytes);
				callback?.Invoke(bytes);
			}, null);
		}
	}
	// 加载资源包的协程
	protected IEnumerator loadAssetBundleCoroutine(AssetBundleInfo bundleInfo)
	{
		++mAssetBundleCoroutineCount;
		if (isEditor() || isDevelopment())
		{
			log(bundleInfo.getBundleFileName() + " start load bundle", LOG_LEVEL.NORMAL);
		}
		AssetBundle assetBundle = null;
		string bundleFileName = bundleInfo.getBundleFileName();
		string fullPath = availableReadPath(bundleFileName);
		// 返回空表示本地没有此文件,需要先下载
		if (fullPath == null)
		{
			byte[] assetBundleBytes = null;
			yield return downloadAssetBundleCoroutine(bundleInfo, (byte[] bytes)=>
			{
				assetBundleBytes = bytes;
			});

			// 先确保依赖项全部已经加载完成,才能开始加载当前请求的资源包
			while (!bundleInfo.isAllParentLoaded())
			{
				yield return null;
			}
			bundleInfo.setLoadState(LOAD_STATE.LOADING);
			AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(assetBundleBytes);
			if (request != null)
			{
				yield return request;
				assetBundle = request.assetBundle;
			}
		}
		else
		{
			// 先确保依赖项全部已经加载完成,才能开始加载当前请求的资源包
			while (!bundleInfo.isAllParentLoaded())
			{
				yield return null;
			}
			bundleInfo.setLoadState(LOAD_STATE.LOADING);
			AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(fullPath);
			if (request != null)
			{
				yield return request;
				assetBundle = request.assetBundle;
			}
		}
		if (isEditor() || isDevelopment())
		{
			if (assetBundle != null)
			{
				log(bundleFileName + " load bundle done", LOG_LEVEL.NORMAL);
			}
		}
		if (assetBundle == null)
		{
			logError("can not load asset bundle async : " + fullPath);
		}
		yield return mWaitForEndOfFrame;
		// 通知AssetBundleInfo
		try
		{
			bundleInfo.notifyAssetBundleAsyncLoaded(assetBundle);
		}
		catch (Exception e)
		{
			logException(e);
		}
		--mAssetBundleCoroutineCount;
	}
	// 加载资源包内单个资源的协程
	protected IEnumerator loadAssetCoroutine(AssetBundleInfo bundle, string fileNameWithSuffix)
	{
		++mAssetCoroutineCount;
		// 只有等资源所属的AssetBundle加载完毕以后才能开始加载其中的单个资源
		if (bundle.getLoadState() != LOAD_STATE.LOADED)
		{
			logError("asset bundle is not loaded, can not load asset async!");
			--mAssetCoroutineCount;
			yield break;
		}

		// 异步从资源包中加载资源
		bundle.getAssetInfo(fileNameWithSuffix).setLoadState(LOAD_STATE.LOADING);
		string assetPath = P_GAME_RESOURCES_PATH + fileNameWithSuffix;
		AssetBundleRequest assetRequest = bundle.loadAssetWithSubAssetsAsync(assetPath);
		if (assetRequest == null)
		{
			bundle.notifyAssetLoaded(fileNameWithSuffix, null);
			logError("can not load asset async : " + fileNameWithSuffix);
			--mAssetCoroutineCount;
			yield break;
		}
		yield return assetRequest;
		try
		{
			bundle.notifyAssetLoaded(fileNameWithSuffix, assetRequest.allAssets);
		}
		catch (Exception e)
		{
			logException(e);
		}
		--mAssetCoroutineCount;
	}
	protected void initAssetConfig(byte[] fileBuffer)
	{
		mInited = false;
		mAssetBundleInfoList.Clear();
		mAssetToBundleInfo.Clear();
		Span<byte> tempStringBuffer = stackalloc byte[256];
		using var a = new ClassScope<SerializerRead>(out var serializer);
		serializer.init(fileBuffer, fileBuffer.Length);
		serializer.read(out int assetBundleCount);
		for (int i = 0; i < assetBundleCount; ++i)
		{
			// AssetBundle名字
			serializer.readString(tempStringBuffer, tempStringBuffer.Length);
			string bundleName = removeSuffix(bytesToString(tempStringBuffer));
			if (!mAssetBundleInfoList.TryGetValue(bundleName, out AssetBundleInfo bundleInfo))
			{
				bundleInfo = mAssetBundleInfoList.add(bundleName, new(bundleName));
			}
			// AssetBundle包含的所有Asset的名字
			serializer.read(out int assetCount);
			for (int k = 0; k < assetCount; ++k)
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
				bundleInfo.addParent(removeSuffix(bytesToString(tempStringBuffer)));
			}
		}
		// 配置清单解析完毕后,为每个AssetBundleInfo查找对应的依赖项
		foreach (AssetBundleInfo info in mAssetBundleInfoList.Values)
		{
			info.findAllDependence();
		}
		mInited = true;
		log("AssetBundle初始化完成, AssetBundle count : " + mAssetBundleInfoList.Count);
	}
}