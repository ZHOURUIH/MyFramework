using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static FrameDefine;
using static CSharpUtility;
using static FrameEditorUtility;

// AssetBundle的信息,存储了AssetBundle中相关的所有数据
public class AssetBundleInfo : ClassObject
{
	protected Dictionary<string, AssetBundleInfo> mChildren = new();	// 依赖自己的AssetBundle列表,即引用了自己的AssetBundle
	protected Dictionary<string, AssetBundleInfo> mParents = new();		// 依赖的AssetBundle列表,即自己引用的AssetBundle,包含所有的直接和间接的依赖项
	protected Dictionary<UObject, AssetInfo> mObjectToAsset = new();	// 通过Object查找AssetInfo的列表
	protected Dictionary<string, AssetInfo> mAssetList = new();			// 资源包中的所有资源,初始化时就会填充此列表
	protected List<AssetBundleCallback> mLoadCallback = new();			// 资源包加载完毕后的回调列表
	protected List<AssetBundleBytesCallback> mDownloadCallback = new(); // 资源包下载完毕后的回调列表
	protected HashSet<AssetInfo> mLoadAsyncList = new();				// AssetBundle还未加载完时请求的异步加载的资源列表
	protected AssetBundle mAssetBundle;									// 资源包内存镜像
	protected LOAD_STATE mLoadState = LOAD_STATE.NONE;				// 资源包加载状态
	protected string mBundleFileName;									// 资源所在的AssetBundle名,相对于StreamingAsset,含后缀
	protected string mBundleName;										// 资源所在的AssetBundle名,相对于StreamingAsset,不含后缀
	protected float mWillUnloadTime = -1.0f;							// 引用计数变为0时的计时,小于0表示还有引用,不会被卸载,大于等于0表示计数为0,即将在一定时间后卸载
	protected const float UNLOAD_DELAY_TIME = 5.0f;						// 没有引用时延迟5秒卸载
	public AssetBundleInfo(string bundleName)
	{
		mBundleName = bundleName;
		mBundleFileName = mBundleName + ASSET_BUNDLE_SUFFIX;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mChildren.Clear();
		mParents.Clear();
		mObjectToAsset.Clear();
		mAssetList.Clear();
		mLoadCallback.Clear();
		mDownloadCallback.Clear();
		mLoadAsyncList.Clear();
		mAssetBundle = null;
		mLoadState = LOAD_STATE.NONE;
		// mBundleName,mBundleFileName不重置
		// mBundleFileName = null;
		// mBundleName = null;
		mWillUnloadTime = -1.0f;
	}
	public void update(float elapsedTime)
	{
		// 需要再次确认是否有引用
		if (tickTimerOnce(ref mWillUnloadTime, elapsedTime) && canUnload())
		{
			unload();
		}
	}
	// 卸载整个资源包
	public void unload()
	{
		if (mResourceManager.getAssetBundleLoader().isDontUnloadAssetBundle(mBundleFileName))
		{
			return;
		}
		if (mAssetBundle != null)
		{
			// 为true表示会卸载掉LoadAsset加载的资源,并不影响该资源实例化的物体
			// 只支持参数为true,如果是false,则是只卸载AssetBundle镜像,但是加载资源包中时会需要使用内存镜像
			// 其他资源包中的资源引用到此资源时,也会自动从此AssetBundle内存镜像中加载需要的资源
			// 所以卸载镜像,将会造成这些自动加载失败,仅在当前资源包内已经没有任何资源在使用了,并且
			// 其他资源包中的资源实例没有对当前资源包进行引用时才会卸载
			mAssetBundle.Unload(true);
			mAssetBundle = null;
		}
		mObjectToAsset.Clear();
		foreach (AssetInfo item in mAssetList.Values)
		{
			item.clear();
		}
		mLoadState = LOAD_STATE.NONE;
		// 通知依赖项,自己被卸载了
		foreach (AssetBundleInfo item in mParents.Values)
		{
			item.notifyChildUnload();
		}
	}
	// 卸载包中单个资源
	public bool unloadAsset(UObject obj)
	{
		if (!mObjectToAsset.Remove(obj, out AssetInfo info))
		{
			logError("object doesn't exist! name:" + obj.name + ", can not unload!");
			return false;
		}
		// 预设类型不真正进行卸载,否则在AssetBundle内存镜像重新加载之前,无法再次从AssetBundle加载此资源
		if (obj is GameObject || obj is Component)
		{
			// UObject.DestroyImmediate(obj, true);
		}
		// 其他独立资源可以使用此方式卸载,使用Resources.UnloadAsset及时卸载资源
		// 可以减少Resourecs.UnloadUnusedAssets的耗时
		else
		{
			Resources.UnloadAsset(obj);
		}
		info.clear();
		if (canUnload())
		{
			mWillUnloadTime = UNLOAD_DELAY_TIME;
		}
		return true;
	}
	public Dictionary<string, AssetBundleInfo> getParents()		{ return mParents; }
	public Dictionary<string, AssetBundleInfo> getChildren()	{ return mChildren; }
	public Dictionary<string, AssetInfo> getAssetList()			{ return mAssetList; }
	public string getBundleName()								{ return mBundleName; }
	public string getBundleFileName()							{ return mBundleFileName; }
	public LOAD_STATE getLoadState()							{ return mLoadState; }
	public AssetBundle getAssetBundle()							{ return mAssetBundle; }
	public void setLoadState(LOAD_STATE state)					{ mLoadState = state; }
	public void addAssetName(string fileNameWithSuffix)
	{
		if (mAssetList.ContainsKey(fileNameWithSuffix))
		{
			logError("there is asset in asset bundle, asset : " + fileNameWithSuffix + ", asset bundle : " + mBundleFileName);
			return;
		}
		AssetInfo info = mAssetList.add(fileNameWithSuffix, new());
		info.setAssetBundleInfo(this);
		info.setAssetName(fileNameWithSuffix);
	}
	public AssetInfo getAssetInfo(string fileNameWithSuffix) { return mAssetList.get(fileNameWithSuffix); }
	// 添加依赖项
	public void addParent(string dep)
	{
		mParents.TryAdd(dep, null);
	}
	// 通知有其他的AssetBundle依赖了自己
	public void addChild(AssetBundleInfo other)
	{
		mChildren.TryAdd(other.mBundleName, other);
	}
	// 有一个引用了自己的AssetBundle被卸载了,尝试检查当前AssetBundle是否可以被卸载
	public void notifyChildUnload()
	{
		if (canUnload())
		{
			mWillUnloadTime = UNLOAD_DELAY_TIME;
		}
	}
	// 查找所有依赖项
	public void findAllDependence()
	{
		using var a = new ListScope<string>(out var tempList);
		tempList.AddRange(mParents.Keys);
		foreach (string depName in tempList)
		{
			AssetBundleInfo info = mResourceManager.getAssetBundleLoader().getAssetBundleInfo(depName);
			// 找到自己的父节点
			mParents.set(depName, info);
			// 并且通知父节点添加自己为子节点
			info.addChild(this);
		}
	}
	// 所有依赖项是否都已经加载完成
	public bool isAllParentLoaded()
	{
		foreach (AssetBundleInfo item in mParents.Values)
		{
			if (item.mLoadState != LOAD_STATE.LOADED)
			{
				return false;
			}
		}
		return true;
	}
	// 同步加载资源包
	public void loadAssetBundle()
	{
		if (isWebGL())
		{
			logError("webgl无法使用loadAssetBundle");
			return;
		}
		if (mAssetBundle != null)
		{
			return;
		}
		if (mLoadState != LOAD_STATE.NONE)
		{
			logError("资源包正在异步加载,无法开始同步加载." + mBundleFileName);
			return;
		}
		// 先确保所有依赖项已经加载
		foreach (AssetBundleInfo item in mParents.Values)
		{
			item.loadAssetBundle();
		}
		mAssetBundle = AssetBundle.LoadFromFile(availableReadPath(mBundleFileName));
		if (mAssetBundle == null)
		{
			logError("can not load asset bundle : " + mBundleFileName);
		}
		mLoadState = LOAD_STATE.LOADED;
		mWillUnloadTime = -1.0f;
	}
	// 异步加载所有依赖项,确认依赖项即将加载或者已加载
	public void loadParentAsync()
	{
		foreach (AssetBundleInfo item in mParents.Values)
		{
			item.loadAssetBundleAsync(null);
		}
	}
	public void checkAssetBundleDependenceLoaded()
	{
		// 先确保所有依赖项已经加载
		foreach (AssetBundleInfo item in mParents.Values)
		{
			item.checkAssetBundleDependenceLoaded();
		}
		if (mLoadState == LOAD_STATE.NONE)
		{
			loadAssetBundle();
		}
	}
	// 异步加载资源包
	public void loadAssetBundleAsync(AssetBundleCallback callback)
	{
		mWillUnloadTime = -1.0f;
		// 加载完毕,直接调用回调
		if (mLoadState == LOAD_STATE.LOADED)
		{
			callback?.Invoke(this);
			return;
		}
		// 还未加载完成时,则加入等待列表
		mLoadCallback.addNotNull(callback);
		// 如果还未开始加载,则加载资源包
		if (mLoadState == LOAD_STATE.NONE)
		{
			// 先确保所有依赖项已经加载
			loadParentAsync();
			mLoadState = LOAD_STATE.WAIT_FOR_LOAD;
			// 通知AssetBundleLoader请求异步加载AssetBundle,只在真正开始异步加载时才标记为正在加载状态,此处只是加入等待列表
			mResourceManager.getAssetBundleLoader().requestLoadAssetBundle(this);
		}
	}
	// 同步加载资源
	public T loadAsset<T>(string fileNameWithSuffix) where T : UObject
	{
		mWillUnloadTime = -1.0f;
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LOADED)
		{
			loadAssetBundle();
		}
		AssetInfo info = mAssetList.get(fileNameWithSuffix);
		T asset = info.loadAsset<T>();
		if (asset != null)
		{
			mObjectToAsset.TryAdd(asset, info);
			mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
		}
		return asset;
	}
	// 同步加载资源的子集
	public UObject[] loadSubAssets(string fileNameWithSuffix, out UObject mainAsset)
	{
		mWillUnloadTime = -1.0f;
		// 如果AssetBundle还没有加载,则先加载AssetBundle
		if (mLoadState != LOAD_STATE.LOADED)
		{
			loadAssetBundle();
		}
		AssetInfo info = mAssetList.get(fileNameWithSuffix);
		UObject[] objs = info.loadAsset();
		UObject asset = objs.getSafe(0);
		mainAsset = asset;
		if (asset != null)
		{
			mObjectToAsset.TryAdd(asset, info);
			mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
		}
		return objs;
	}
	// 同步加载所有子集
	public void loadAllSubAssets()
	{
		foreach (AssetInfo assetInfo in mAssetList.Values)
		{
			// 确认是否正常加载完成,如果当前资源包已经卸载,则无法完成加载资源
			if (mLoadState != LOAD_STATE.NONE)
			{
				assetInfo.loadAsset();
				UObject asset = assetInfo.getAsset();
				if (asset != null)
				{
					mObjectToAsset.TryAdd(asset, assetInfo);
					mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
				}
			}
			assetInfo.callbackAll();
		}
	}
	// 异步加载资源
	public CustomAsyncOperation loadAssetAsync(string fileNameWithSuffix, AssetLoadDoneCallback callback, string loadPath)
	{
		mWillUnloadTime = -1.0f;
		CustomAsyncOperation op = new();
		AssetInfo info = mAssetList.get(fileNameWithSuffix);
		info.addCallback((UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
		{
			callback?.Invoke(asset, assets, bytes, loadPath);
			op.mFinish = true;
		}, loadPath);

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
			if (mLoadState == LOAD_STATE.NONE)
			{
				loadAssetBundleAsync(null);
			}
		}
		return op;
	}
	// 资源异步加载完成
	public void notifyAssetLoaded(string fileNameWithSuffix, UObject[] assets)
	{
		AssetInfo assetInfo = mAssetList.get(fileNameWithSuffix);
		// 确认是否正常加载完成,如果当前资源包已经卸载,则无法完成加载资源
		if (mLoadState != LOAD_STATE.NONE)
		{
			assetInfo.setSubAssets(assets);
			UObject asset = assetInfo.getAsset();
			if (asset != null)
			{
				mObjectToAsset.TryAdd(asset, assetInfo);
				mResourceManager.getAssetBundleLoader().notifyAssetLoaded(asset, this);
			}
		}
		assetInfo.callbackAll();
	}
	// 资源包异步加载完成
	public void notifyAssetBundleAsyncLoaded(AssetBundle assetBundle)
	{
		mAssetBundle = assetBundle;
		if (mLoadState != LOAD_STATE.NONE)
		{
			mLoadState = LOAD_STATE.LOADED;
			// 异步加载请求的资源
			foreach (AssetInfo item in mLoadAsyncList)
			{
				mAssetList.get(item.getAssetName()).loadAssetAsync();
			}
		}
		// 加载状态为已卸载,表示在异步加载过程中,资源包被卸载掉了
		else
		{
			logWarning("资源包异步加载完成,但是异步加载过程中被卸载");
			unload();
		}
		mLoadAsyncList.Clear();

		using var a = new ListScope<AssetBundleCallback>(out var callbacks);
		foreach (AssetBundleCallback callback in callbacks.move(mLoadCallback))
		{
			callback(this);
		}
	}
	public void addDownloadCallback(AssetBundleBytesCallback callback)
	{
		mDownloadCallback.addNotNull(callback);
	}
	public void notifyAssetBundleDownloaded(byte[] bytes)
	{
		using var a = new ListScope<AssetBundleBytesCallback>(out var callbacks);
		foreach (AssetBundleBytesCallback call in callbacks.move(mDownloadCallback))
		{
			call(this, bytes);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 尝试卸载AssetBundle,卸载需要满足两个条件
	// 当前AssetBundle内的所有资源已经没有正在使用
	// 已经没有其他的正在使用的AssetBundle引用了自己
	protected bool canUnload()
	{
		if (mLoadState != LOAD_STATE.LOADED)
		{
			return false;
		}
		// 如果资源包的资源已经没有在使用中,则卸载当前资源包
		foreach (AssetInfo item in mAssetList.Values)
		{
			if (item.getLoadState() != LOAD_STATE.NONE)
			{
				return false;
			}
		}
		// 如果已经没有资源被引用了,则卸载AssetBundle
		// 当前已经没有正在使用的AssetBundle引用了自己时才可以卸载
		foreach (AssetBundleInfo item in mChildren.Values)
		{
			if (item.getLoadState() != LOAD_STATE.NONE)
			{
				return false;
			}
		}
		return true;
	}
}