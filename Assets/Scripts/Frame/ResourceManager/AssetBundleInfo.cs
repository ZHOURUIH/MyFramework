using System.Collections.Generic;
using UnityEngine;

public class AssetBundleInfo : FrameBase
{
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
		mBundleFileName = mBundleName + FrameDefine.ASSET_BUNDLE_SUFFIX;
		mLoadState = LOAD_STATE.UNLOAD;
		mParents = new Dictionary<string, AssetBundleInfo>();
		mChildren = new Dictionary<string, AssetBundleInfo>();
		mLoadAsyncList = new List<AssetInfo>();
		mAssetList = new Dictionary<string, AssetInfo>();
		mObjectToAsset = new Dictionary<Object, AssetInfo>();
		mLoadCallback = new List<AssetBundleLoadCallback>();
		mLoadUserData = new List<object>();
	}
	public void unload()
	{
		if (mAssetBundle != null)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			log("unload : " + mBundleName);
#endif
			// 为true表示会卸载掉LoadAsset加载的资源,并不影响该资源实例化的物体
			mAssetBundle.Unload(true);
			mAssetBundle = null;
		}
		mObjectToAsset.Clear();
		foreach (var item in mAssetList)
		{
			item.Value.unload();
		}
		mLoadState = LOAD_STATE.UNLOAD;
	}
	public bool unloadAsset(Object obj)
	{
		if(!mObjectToAsset.TryGetValue(obj, out AssetInfo info))
		{
			logError("object doesn't exist! name:" + obj.name + ", can not unload!");
			return false;
		}
		info.unload();
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
		if (mAssetList.ContainsKey(fileNameWithSuffix))
		{
			logError("there is asset in asset bundle, asset : " + fileNameWithSuffix + ", asset bundle : " + mBundleFileName);
		}
		mAssetList.Add(fileNameWithSuffix, new AssetInfo(this, fileNameWithSuffix));
	}
	public AssetInfo getAssetInfo(string fileNameWithSuffix)
	{
		mAssetList.TryGetValue(fileNameWithSuffix, out AssetInfo info);
		return info;
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
		LIST(out List<string> tempList);
		tempList.AddRange(mParents.Keys);
		int count = tempList.Count;
		for(int i = 0; i < count; ++i)
		{
			string depName = tempList[i];
			AssetBundleInfo info = mResourceManager.getAssetBundleLoader().getAssetBundleInfo(depName);
			// 找到自己的父节点
			mParents[depName] = info;
			// 并且通知父节点添加自己为子节点
			info.addChild(this);
		}
		UN_LIST(tempList);
	}
	// 所有依赖项是否都已经加载完成
	public bool isAllParentLoaded()
	{
		foreach(var item in mParents)
		{
			if(item.Value.mLoadState != LOAD_STATE.LOADED)
			{
				return false;
			}
		}
		return true;
	}
	// 同步加载资源包
	public void loadAssetBundle()
	{
		if(!mResourceManager.isLocalRootPath())
		{
			logError("当前资源目录为远程目录,无法同步加载");
			return;
		}
		if (mLoadState != LOAD_STATE.UNLOAD && mLoadState != LOAD_STATE.NONE)
		{
			return;
		}
		// 先确保所有依赖项已经加载
		foreach (var info in mParents)
		{
			info.Value.loadAssetBundle();
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		log("加载AssetBundle:" + mBundleFileName, LOG_LEVEL.NORMAL);
#endif
		string path = mResourceManager.getResourceRootPath() + mBundleFileName;
		// 安卓平台下,如果是从StreamingAssets读取AssetBundle文件,则需要使用指定的路径
#if UNITY_ANDROID && !UNITY_EDITOR
		if (mResourceManager.getResourceRootPath() == FrameDefine.F_STREAMING_ASSETS_PATH)
		{
			path = FrameDefine.F_ASSET_BUNDLE_PATH + mBundleFileName;
		}
#endif
		mAssetBundle = AssetBundle.LoadFromFile(path);
		if (mAssetBundle == null)
		{
			logError("can not load asset bundle : " + mBundleFileName);
		}
		mLoadState = LOAD_STATE.LOADED;
	}
	// 异步加载所有依赖项,确认依赖项即将加载或者已加载
	public void loadParentAsync()
	{
		foreach(var item in mParents)
		{
			item.Value.loadAssetBundleAsync(null);
		}
	}
	public void checkAssetBundleDependenceLoaded()
	{
		// 先确保所有依赖项已经加载
		foreach (var info in mParents)
		{
			info.Value.checkAssetBundleDependenceLoaded();
		}
		if(mLoadState == LOAD_STATE.UNLOAD || mLoadState == LOAD_STATE.NONE)
		{
			loadAssetBundle();
		}
	}
	// 异步加载资源包
	public void loadAssetBundleAsync(AssetBundleLoadCallback callback, object userData = null)
	{
		// 正在加载,则加入等待列表
		if (mLoadState == LOAD_STATE.LOADING || mLoadState == LOAD_STATE.WAIT_FOR_LOAD)
		{
			if(callback != null)
			{
				mLoadCallback.Add(callback);
			}
		}
		// 如果还未加载,则加载资源包,回调加入列表
		else if (mLoadState == LOAD_STATE.UNLOAD)
		{
			if (callback != null)
			{
				mLoadCallback.Add(callback);
				mLoadUserData.Add(userData);
			}
			// 先确保所有依赖项已经加载
			loadParentAsync();
			mLoadState = LOAD_STATE.WAIT_FOR_LOAD;
			// 通知AssetBundleLoader请求异步加载AssetBundle,只在真正开始异步加载时才标记为正在加载状态,此处只是加入等待列表
			mResourceManager.getAssetBundleLoader().requestLoadAssetBundle(this);
		}
		// 加载完毕,直接调用回调
		else if (mLoadState == LOAD_STATE.LOADED)
		{
			callback?.Invoke(this, userData);
		}
	}
	// 同步加载资源
	public T loadAsset<T>(ref string fileNameWithSuffix) where T : Object
	{
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LOADED)
		{
			loadAssetBundle();
		}
		AssetInfo info = mAssetList[fileNameWithSuffix];
		T asset = info.loadAsset<T>();
		if(asset != null && !mObjectToAsset.ContainsKey(asset))
		{
			mObjectToAsset.Add(asset, info);
		}
		mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
		return asset;
	}
	// 异步加载资源
	public bool loadAssetAsync(string fileNameWithSuffix, AssetLoadDoneCallback callback, string loadPath, object userData = null)
	{
		// 记录下需要异步加载的资源名
		AssetInfo info = mAssetList[fileNameWithSuffix];
		if (!mLoadAsyncList.Contains(info))
		{
			mLoadAsyncList.Add(info);
		}
		info.addCallback(callback, userData, loadPath);
		// 如果当前资源包还未加载完毕,则需要等待资源包加载完以后才能加载资源
		if (mLoadState == LOAD_STATE.UNLOAD)
		{
			loadAssetBundleAsync(null);
		}
		// 如果资源包已经加载,则可以直接异步加载资源
		else if (mLoadState == LOAD_STATE.LOADED)
		{
			info.loadAssetAsync();
		}
		return true;
	}
	// 同步加载资源的子集
	public Object[] loadSubAsset(ref string fileNameWithSuffix)
	{
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LOADED)
		{
			loadAssetBundle();
		}
		return mAssetList[fileNameWithSuffix].loadSubAssets();
	}
	//异步加载资源的子集
	public bool loadSubAssetAsync(ref string fileNameWithSuffix, AssetLoadDoneCallback callback, string loadPath, object userData = null)
	{
		// 记录下需要异步加载的资源名
		AssetInfo info = mAssetList[fileNameWithSuffix];
		if (!mLoadAsyncList.Contains(info))
		{
			mLoadAsyncList.Add(info);
		}
		info.addCallback(callback, userData, loadPath);
		// 如果当前资源包还未加载完毕,则需要等待资源包加载完以后才能加载资源
		if (mLoadState == LOAD_STATE.UNLOAD)
		{
			loadAssetBundleAsync(null);
		}
		// 如果资源包已经加载,则可以直接异步加载资源
		else if (mLoadState == LOAD_STATE.LOADED)
		{
			info.loadSubAssetsAsync();
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
		if (mLoadState != LOAD_STATE.UNLOAD)
		{
			Object asset = assetInfo.getAsset();
			if (!mObjectToAsset.ContainsKey(asset))
			{
				mObjectToAsset.Add(asset, assetInfo);
			}
			mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
		}
	}
	// 资源包异步加载完成
	public void notifyAssetBundleAsyncLoaded(AssetBundle assetBundle)
	{
		mAssetBundle = assetBundle;
		if (mLoadState != LOAD_STATE.UNLOAD)
		{
			mLoadState = LOAD_STATE.LOADED;
			// 异步加载请求的资源
			int asyncCount = mLoadAsyncList.Count;
			for(int i = 0; i < asyncCount; ++i)
			{
				loadAssetAsync(mLoadAsyncList[i].getAssetName(), null, null);
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