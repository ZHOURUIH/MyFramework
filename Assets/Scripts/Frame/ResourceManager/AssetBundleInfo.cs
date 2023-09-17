using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static FrameDefine;

// AssetBundle的信息,存储了AssetBundle中相关的所有数据
public class AssetBundleInfo : ClassObject
{
	protected Dictionary<string, AssetBundleInfo> mChildren;// 依赖自己的AssetBundle列表,即引用了自己的AssetBundle
	protected Dictionary<string, AssetBundleInfo> mParents; // 依赖的AssetBundle列表,即自己引用的AssetBundle,包含所有的直接和间接的依赖项
	protected Dictionary<Object, AssetInfo> mObjectToAsset; // 通过Object查找AssetInfo的列表
	protected Dictionary<string, AssetInfo> mAssetList;     // 资源包中的所有资源,初始化时就会填充此列表
	protected List<AssetBundleLoadCallback> mLoadCallback;  // 资源包加载完毕后的回调列表
	protected HashSet<AssetInfo> mLoadAsyncList;			// AssetBundle还未加载完时请求的异步加载的资源列表
	protected List<object> mLoadUserData;                   // 资源包加载完毕后的回调列表参数
	protected AssetBundle mAssetBundle;                     // 资源包内存镜像
	protected LOAD_STATE mLoadState;                        // 资源包加载状态
	protected string mBundleFileName;						// 资源所在的AssetBundle名,相对于StreamingAsset,含后缀
	protected string mBundleName;                           // 资源所在的AssetBundle名,相对于StreamingAsset,不含后缀
	public AssetBundleInfo(string bundleName)
	{
		mBundleName = bundleName;
		mBundleFileName = mBundleName + ASSET_BUNDLE_SUFFIX;
		mLoadState = LOAD_STATE.UNLOAD;
		mChildren = new Dictionary<string, AssetBundleInfo>();
		mParents = new Dictionary<string, AssetBundleInfo>();
		mObjectToAsset = new Dictionary<Object, AssetInfo>();
		mAssetList = new Dictionary<string, AssetInfo>();
		mLoadCallback = new List<AssetBundleLoadCallback>();
		mLoadAsyncList = new HashSet<AssetInfo>();
		mLoadUserData = new List<object>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mChildren.Clear();
		mParents.Clear();
		mObjectToAsset.Clear();
		mAssetList.Clear();
		mLoadCallback.Clear();
		mLoadAsyncList.Clear();
		mLoadUserData.Clear();
		mAssetBundle = null;
		mLoadState = LOAD_STATE.UNLOAD;
		// mBundleName,mBundleFileName不重置
		// mBundleFileName = null;
		// mBundleName = null;
	}
	public void unload()
	{
		if (mResourceManager.getAssetBundleLoader().isDontUnloadAssetBundle(mBundleFileName))
		{
			return;
		}
		if (mLoadState != LOAD_STATE.LOADED)
		{
			return;
		}
		if (mAssetBundle != null)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			//logForce("unload assetbundle : " + mBundleName);
#endif
			// 为true表示会卸载掉LoadAsset加载的资源,并不影响该资源实例化的物体
			// 只支持参数为true,如果是false,则是只卸载AssetBundle镜像,但是加载资源包中时会需要使用内存镜像
			// 其他资源包中的资源引用到此资源时,也会自动从此AssetBundle内存镜像中加载需要的资源
			// 所以卸载镜像,将会造成这些自动加载失败,仅在当前资源包内已经没有任何资源在使用了,并且
			// 其他资源包中的资源实例没有对当前资源包进行引用时才会卸载
			mAssetBundle.Unload(true);
			mAssetBundle = null;
		}
		mObjectToAsset.Clear();
		foreach (var item in mAssetList)
		{
			item.Value.clear();
		}
		mLoadState = LOAD_STATE.UNLOAD;
		// 通知依赖项,自己被卸载了
		foreach (var item in mParents)
		{
			item.Value.notifyChildUnload();
		}
	}
	public bool unloadAsset(Object obj)
	{
		if(!mObjectToAsset.TryGetValue(obj, out AssetInfo info))
		{
			logError("object doesn't exist! name:" + obj.name + ", can not unload!");
			return false;
		}
		// 预设类型不真正进行卸载,否则在AssetBundle内存镜像重新加载之前,无法再次从AssetBundle加载此资源
		if (obj is GameObject || obj is Component)
		{
			//Object.DestroyImmediate(obj, true);
		}
		// 其他独立资源可以使用此方式卸载,使用Resources.UnloadAsset及时卸载资源
		// 可以减少Resourecs.UnloadUnusedAssets的耗时
		else
		{
			Resources.UnloadAsset(obj);
		}
		info.clear();
		mObjectToAsset.Remove(obj);
		tryUnload();
		return true;
	}
	public Dictionary<string, AssetBundleInfo> getParents() { return mParents; }
	public Dictionary<string, AssetBundleInfo> getChildren() { return mChildren; }
	public Dictionary<string, AssetInfo> getAssetList() { return mAssetList; }
	public string getBundleName() { return mBundleName; }
	public string getBundleFileName() { return mBundleFileName; }
	public LOAD_STATE getLoadState() { return mLoadState; }
	public void setLoadState(LOAD_STATE state) { mLoadState = state; }
	public Object[] loadAssetWithSubAssets(string name) { return mAssetBundle?.LoadAssetWithSubAssets(name); }
	public Object[] loadAll() { return mAssetBundle?.LoadAllAssets(); }
	public AssetBundleRequest loadAssetWithSubAssetsAsync(string name) { return mAssetBundle?.LoadAssetWithSubAssetsAsync(name); }
	public void addAssetName(string fileNameWithSuffix)
	{
		if (mAssetList.ContainsKey(fileNameWithSuffix))
		{
			logError("there is asset in asset bundle, asset : " + fileNameWithSuffix + ", asset bundle : " + mBundleFileName);
		}
		var info = new AssetInfo();
		info.setAssetBundleInfo(this);
		info.setAssetName(fileNameWithSuffix);
		mAssetList.Add(fileNameWithSuffix, info);
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
	// 有一个引用了自己的AssetBundle被卸载了,尝试检查当前AssetBundle是否可以被卸载
	public void notifyChildUnload()
	{
		tryUnload();
	}
	// 查找所有依赖项
	public void findAllDependence()
	{
		using (new ListScope<string>(out var tempList))
		{
			tempList.AddRange(mParents.Keys);
			int count = tempList.Count;
			for (int i = 0; i < count; ++i)
			{
				string depName = tempList[i];
				AssetBundleInfo info = mResourceManager.getAssetBundleLoader().getAssetBundleInfo(depName);
				// 找到自己的父节点
				mParents[depName] = info;
				// 并且通知父节点添加自己为子节点
				info.addChild(this);
			}
		}
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
		if (mAssetBundle != null || mLoadState != LOAD_STATE.UNLOAD)
		{
			return;
		}
		// 先确保所有依赖项已经加载
		foreach (var info in mParents)
		{
			info.Value.loadAssetBundle();
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		//logForce("加载AssetBundle:" + mBundleFileName);
#endif
		mAssetBundle = AssetBundle.LoadFromFile(availableReadPath(mBundleFileName));
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
		if(mLoadState == LOAD_STATE.UNLOAD)
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
	public T loadAsset<T>(string fileNameWithSuffix) where T : Object
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
		if (asset != null)
		{
			mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
		}
		return asset;
	}
	// 同步加载资源的子集
	public Object[] loadSubAssets(string fileNameWithSuffix, out Object mainAsset)
	{
		mainAsset = null;
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LOADED)
		{
			loadAssetBundle();
		}
		AssetInfo info = mAssetList[fileNameWithSuffix];
		Object[] objs = info.loadAsset();
		if (objs != null && objs.Length > 0)
		{
			Object asset = objs[0];
			mainAsset = asset;
			if (asset != null && !mObjectToAsset.ContainsKey(asset))
			{
				mObjectToAsset.Add(asset, info);
			}
			mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
		}
		return objs;
	}
	// 异步加载资源
	public bool loadAssetAsync(string fileNameWithSuffix, AssetLoadDoneCallback callback, string loadPath, object userData = null)
	{
		AssetInfo info = mAssetList[fileNameWithSuffix];
		if (callback != null)
		{
			info.addCallback(callback, userData, loadPath);
		}
		// 如果资源包已经加载,则可以直接异步加载资源
		if (mLoadState == LOAD_STATE.LOADED)
		{
			info.loadAssetAsync();
		}
		// 如果当前资源包还未加载完毕,则需要等待资源包加载完以后才能加载资源
		else
		{
			// AssetBundle未加载完成时,记录下需要异步加载的资源名,等待AssetBundle加载完毕后再加载Asset
			mLoadAsyncList.Add(info);
			// 还没有开始加载则开始加载AssetBundle
			if (mLoadState == LOAD_STATE.UNLOAD)
			{
				loadAssetBundleAsync(null);
			}
		}
		return true;
	}
	// 资源异步加载完成
	public void notifyAssetLoaded(string fileNameWithSuffix, Object[] assets)
	{
		AssetInfo assetInfo = mAssetList[fileNameWithSuffix];
		assetInfo.notifyAssetLoaded(assets);
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
			foreach (var item in mLoadAsyncList)
			{
				loadAssetAsync(item.getAssetName(), null, null);
			}
		}
		// 加载状态为已卸载,表示在异步加载过程中,资源包被卸载掉了
		else
		{
			logWarning("资源包异步加载完成,但是异步加载过程中被卸载");
			unload();
		}
		mLoadAsyncList.Clear();

		int count = mLoadCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mLoadCallback[i](this, mLoadUserData[i]);
		}
		mLoadCallback.Clear();
		mLoadUserData.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 尝试卸载AssetBundle,卸载需要满足两个条件
	// 当前AssetBundle内的所有资源已经没有正在使用
	// 已经没有其他的正在使用的AssetBundle引用了自己
	protected void tryUnload()
	{
		if (mLoadState != LOAD_STATE.LOADED)
		{
			return;
		}
		// 如果资源包的资源已经没有在使用中,则卸载当前资源包
		foreach (var item in mAssetList)
		{
			if (item.Value.getLoadState() != LOAD_STATE.UNLOAD)
			{
				return;
			}
		}
		// 如果已经没有资源被引用了,则卸载AssetBundle
		// 当前已经没有正在使用的AssetBundle引用了自己时才可以卸载
		foreach (var item in mChildren)
		{
			if (item.Value.getLoadState() != LOAD_STATE.UNLOAD)
			{
				return;
			}
		}
		unload();
	}
}