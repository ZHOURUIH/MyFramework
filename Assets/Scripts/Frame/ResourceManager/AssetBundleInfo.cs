using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleInfo : GameBase
{
	private List<string> mTempList0;                        // 为避免GC而创建的变量
	protected Dictionary<string, AssetBundleInfo> mParents; // 依赖的AssetBundle列表,包含所有的直接和间接的依赖项
	protected Dictionary<string, AssetBundleInfo> mChildren;// 依赖自己的AssetBundle列表
	protected Dictionary<string, AssetInfo> mAssetList;     // 资源包中已加载的所有资源
	protected Dictionary<Object, AssetInfo> mObjectToAsset; // 通过Object查找AssetInfo的列表
	protected List<AssetBundleLoadCallback> mLoadCallback;  // 资源包加载完毕后的回调列表
	protected List<object> mLoadUserData;                   // 资源包加载完毕后的回调列表参数
	protected List<AssetInfo> mLoadAsyncList;               // 需要异步加载的资源列表
	protected AssetBundle mAssetBundle;                     // 资源包
	protected LOAD_STATE mLoadState;                        // 资源包加载状态
	protected string mBundleName;                           // 资源所在的AssetBundle名,相对于StreamingAsset,不含后缀
	protected string mBundleFileName;						// 资源所在的AssetBundle名,相对于StreamingAsset,含后缀
	public AssetBundleInfo(string bundleName)
	{
		mBundleName = bundleName;
		mBundleFileName = mBundleName + CommonDefine.ASSET_BUNDLE_SUFFIX;
		mLoadState = LOAD_STATE.LS_UNLOAD;
		mParents = new Dictionary<string, AssetBundleInfo>();
		mChildren = new Dictionary<string, AssetBundleInfo>();
		mLoadAsyncList = new List<AssetInfo>();
		mAssetList = new Dictionary<string, AssetInfo>();
		mTempList0 = new List<string>();
		mObjectToAsset = new Dictionary<Object, AssetInfo>();
		mLoadCallback = new List<AssetBundleLoadCallback>();
		mLoadUserData = new List<object>();
	}
	public void destroy()
	{
		unload();
	}
	public void unload()
	{
		if (mAssetBundle != null)
		{
			logInfo("unload : " + mBundleName);
			// 为true表示会卸载掉LoadAsset加载的资源,并不影响该资源实例化的物体
			mAssetBundle.Unload(true);
			mAssetBundle = null;
		}
		mObjectToAsset.Clear();
		foreach (var item in mAssetList)
		{
			item.Value.unload();
		}
		mLoadState = LOAD_STATE.LS_UNLOAD;
	}
	public bool unloadAsset(Object obj)
	{
		if(!mObjectToAsset.ContainsKey(obj))
		{
			logError("object doesn't exist! name:" + obj.name + ", can not unload!");
			return false;
		}
		mObjectToAsset[obj].unload();
		mObjectToAsset.Remove(obj);
		bool hasAsset = false;
		// 如果资源包的资源已经全部加载过,并且已经全部卸载了,则卸载当前资源包
		foreach(var item in mAssetList)
		{
			if(!item.Value.isUnloaded())
			{
				hasAsset = true;
				break;
			}
		}
		// 如果已经没有资源被引用了,则卸载AssetBundle,但是只能卸载没有子依赖的包
		if(!hasAsset && mChildren.Count == 0)
		{
			unload();
		}
		return true;
	}
	public Dictionary<string, AssetBundleInfo> getParents() { return mParents; }
	public Dictionary<string, AssetBundleInfo> getChildren() { return mChildren; }
	public Dictionary<string, AssetInfo> getAssetList() { return mAssetList; }
	public AssetBundle getAssetBundle() { return mAssetBundle; }
	public string getBundleName() { return mBundleName; }
	public string getBundleFileName() { return mBundleFileName; }
	public LOAD_STATE getLoadState() { return mLoadState; }
	public void setLoadState(LOAD_STATE state) { mLoadState = state; }
	public void addAssetName(string fileNameWithSuffix)
	{
		if (!mAssetList.ContainsKey(fileNameWithSuffix))
		{
			mAssetList.Add(fileNameWithSuffix, new AssetInfo(this, fileNameWithSuffix));
		}
		else
		{
			logError("there is asset in asset bundle, asset : " + fileNameWithSuffix + ", asset bundle : " + mBundleFileName);
		}
	}
	public AssetInfo getAssetInfo(string fileNameWithSuffix)
	{
		if (mAssetList.ContainsKey(fileNameWithSuffix))
		{
			return mAssetList[fileNameWithSuffix];
		}
		return null;
	}
	// 添加依赖项
	public void addParent(string dep)
	{
		if (!mParents.ContainsKey(dep))
		{
			mParents.Add(dep, null);
		}
	}
	// 通知有其他的AssetBundle依赖了自己
	public void addChild(AssetBundleInfo other)
	{
		if (!mChildren.ContainsKey(other.mBundleName))
		{
			mChildren.Add(other.mBundleName, other);
		}
	}
	// 查找所有依赖项
	public void findAllDependence()
	{
		mTempList0.Clear();
		mTempList0.AddRange(mParents.Keys);
		foreach (var depName in mTempList0)
		{
			// 找到自己的父节点
			mParents[depName] = mResourceManager.mAssetBundleLoader.getAssetBundleInfo(depName);
			// 并且通知父节点添加自己为子节点
			mParents[depName].addChild(this);
		}
	}
	// 所有依赖项是否都已经加载完成
	public bool isAllParentLoaded()
	{
		foreach(var item in mParents)
		{
			if(item.Value.mLoadState != LOAD_STATE.LS_LOADED)
			{
				return false;
			}
		}
		return true;
	}
	// 同步加载资源包
	public void loadAssetBundle()
	{
		if (mLoadState != LOAD_STATE.LS_UNLOAD && mLoadState != LOAD_STATE.LS_NONE)
		{
			return;
		}
		// 先确保所有依赖项已经加载
		foreach (var info in mParents)
		{
			info.Value.loadAssetBundle();
		}
		logInfo("加载AssetBundle:" + mBundleFileName, LOG_LEVEL.LL_NORMAL);
		// 先去persistentDataPath中查找资源
		string path = CommonDefine.F_PERSISTENT_DATA_PATH + mBundleFileName;
		if (ResourceManager.mPersistentFirst && isFileExist(path))
		{
			mAssetBundle = AssetBundle.LoadFromFile(path);
		}
		// 找不到再去指定目录中查找资源
		if (mAssetBundle == null)
		{
			path = ResourceManager.mResourceRootPath + mBundleFileName;
			// 安卓平台下,如果是从StreamingAssets读取AssetBundle文件,则需要使用指定的路径
#if UNITY_ANDROID && !UNITY_EDITOR
			if (ResourceManager.mResourceRootPath == CommonDefine.F_STREAMING_ASSETS_PATH)
			{
				path = CommonDefine.F_ASSET_BUNDLE_PATH + mBundleFileName;
			}
#endif
			// 远端目录不判断文件是否存在,本地路径如果是StreamingAssets中
			// 则需要使用CommonDefine.F_STREAMING_ASSETS_PATH而不是CommonDefine.F_ASSET_BUNDLE_PATH
			if (!ResourceManager.mLocalRootPath || isFileExist(ResourceManager.mResourceRootPath + mBundleFileName))
			{
				mAssetBundle = AssetBundle.LoadFromFile(path);
			}
		}
		if (mAssetBundle == null)
		{
			logError("can not load asset bundle : " + mBundleFileName);
		}
		mLoadState = LOAD_STATE.LS_LOADED;
	}
	// 异步加载所有依赖项,确认依赖项即将加载或者已加载
	public void loadParentAsync()
	{
		foreach(var item in mParents)
		{
			item.Value.loadAssetBundleAsync(null, null);
		}
	}
	public void checkAssetBundleDependenceLoaded()
	{
		// 先确保所有依赖项已经加载
		foreach (var info in mParents)
		{
			info.Value.checkAssetBundleDependenceLoaded();
		}
		if(mLoadState == LOAD_STATE.LS_UNLOAD || mLoadState == LOAD_STATE.LS_NONE)
		{
			loadAssetBundle();
		}
	}
	// 异步加载资源包
	public void loadAssetBundleAsync(AssetBundleLoadCallback callback, object userData)
	{
		// 正在加载,则加入等待列表
		if (mLoadState == LOAD_STATE.LS_LOADING || mLoadState == LOAD_STATE.LS_WAIT_FOR_LOAD)
		{
			if(callback != null)
			{
				mLoadCallback.Add(callback);
			}
		}
		// 如果还未加载,则加载资源包,回调加入列表
		else if (mLoadState == LOAD_STATE.LS_UNLOAD)
		{
			if (callback != null)
			{
				mLoadCallback.Add(callback);
				mLoadUserData.Add(userData);
			}
			// 先确保所有依赖项已经加载
			loadParentAsync();
			mLoadState = LOAD_STATE.LS_WAIT_FOR_LOAD;
			// 通知AssetBundleLoader请求异步加载AssetBundle,只在真正开始异步加载时才标记为正在加载状态,此处只是加入等待列表
			mResourceManager.mAssetBundleLoader.requestLoadAssetBundle(this);
		}
		// 加载完毕,直接调用回调
		else if (mLoadState == LOAD_STATE.LS_LOADED)
		{
			callback?.Invoke(this, userData);
		}
	}
	// 同步加载资源
	public T loadAsset<T>(ref string fileNameWithSuffix) where T : Object
	{
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LS_LOADED)
		{
			loadAssetBundle();
		}
		T asset = mAssetList[fileNameWithSuffix].loadAsset<T>();
		if(asset != null && !mObjectToAsset.ContainsKey(asset))
		{
			mObjectToAsset.Add(asset, mAssetList[fileNameWithSuffix]);
		}
		mResourceManager.mAssetBundleLoader.notifyAssetLoaded(asset, this);
		return asset;
	}
	// 异步加载资源
	public bool loadAssetAsync(string fileNameWithSuffix, AssetLoadDoneCallback callback, string loadPath, object[] userData)
	{
		// 记录下需要异步加载的资源名
		if (!mLoadAsyncList.Contains(mAssetList[fileNameWithSuffix]))
		{
			mLoadAsyncList.Add(mAssetList[fileNameWithSuffix]);
		}
		mAssetList[fileNameWithSuffix].addCallback(callback, userData, loadPath);
		// 如果当前资源包还未加载完毕,则需要等待资源包加载完以后才能加载资源
		if (mLoadState == LOAD_STATE.LS_UNLOAD)
		{
			loadAssetBundleAsync(null, null);
		}
		// 如果资源包已经加载,则可以直接异步加载资源
		else if (mLoadState == LOAD_STATE.LS_LOADED)
		{
			mAssetList[fileNameWithSuffix].loadAssetAsync();
		}
		return true;
	}
	// 同步加载资源的子集
	public Object[] loadSubAsset(ref string fileNameWithSuffix)
	{
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LS_LOADED)
		{
			loadAssetBundle();
		}
		return mAssetList[fileNameWithSuffix].loadSubAssets();
	}
	//异步加载资源的子集
	public bool loadSubAssetAsync(ref string fileNameWithSuffix, AssetLoadDoneCallback callback, object[] userData, string loadPath)
	{
		// 记录下需要异步加载的资源名
		if (!mLoadAsyncList.Contains(mAssetList[fileNameWithSuffix]))
		{
			mLoadAsyncList.Add(mAssetList[fileNameWithSuffix]);
		}
		mAssetList[fileNameWithSuffix].addCallback(callback, userData, loadPath);
		// 如果当前资源包还未加载完毕,则需要等待资源包加载完以后才能加载资源
		if (mLoadState == LOAD_STATE.LS_UNLOAD)
		{
			loadAssetBundleAsync(null, null);
		}
		// 如果资源包已经加载,则可以直接异步加载资源
		else if (mLoadState == LOAD_STATE.LS_LOADED)
		{
			mAssetList[fileNameWithSuffix].loadSubAssetsAsync();
		}
		return true;
	}
	// 资源异步加载完成
	public void notifyAssetLoaded(string fileNameWithSuffix, Object[] assets)
	{
		AssetInfo assetInfo = mAssetList[fileNameWithSuffix];
		if (mLoadAsyncList.Contains(assetInfo))
		{
			assetInfo.notifyAssetLoaded(assets);
			mLoadAsyncList.Remove(assetInfo);
		}
		// 确认是否正常加载完成,如果当前资源包已经卸载,则无法完成加载资源
		if (mLoadState != LOAD_STATE.LS_UNLOAD)
		{
			Object asset = assetInfo.getAsset();
			if (!mObjectToAsset.ContainsKey(asset))
			{
				mObjectToAsset.Add(asset, assetInfo);
			}
			mResourceManager.mAssetBundleLoader.notifyAssetLoaded(asset, this);
		}
	}
	// 资源包异步加载完成
	public void notifyAssetBundleAsyncLoadedDone(AssetBundle assetBundle)
	{
		mAssetBundle = assetBundle;
		if (mLoadState != LOAD_STATE.LS_UNLOAD)
		{
			mLoadState = LOAD_STATE.LS_LOADED;
			// 异步加载请求的资源
			foreach (var assetInfo in mLoadAsyncList)
			{
				loadAssetAsync(assetInfo.getAssetName(), null, EMPTY_STRING, null);
			}
		}
		// 加载状态为已卸载,表示在异步加载过程中,资源包被卸载掉了
		else
		{
			logWarning("资源包异步加载完成,但是异步加载过程中被卸载");
			unload();
		}
		int count = mLoadCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mLoadCallback[i](this, mLoadUserData[i]);
		}
		mLoadCallback.Clear();
		mLoadUserData.Clear();
	}
	//-----------------------------------------------------------------------------------------------------------------------------
}