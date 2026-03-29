using System;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static StringUtility;
using static FrameBaseUtility;
using static FrameUtility;

// 资源管理器,管理所有资源的加载
public class ResourceManager : FrameSystem
{
	protected Dictionary<UObject, HashSet<long>> mReferenceTokenList = new();	// 记录了每个资源的引用凭证ID
	protected AssetDataBaseLoader mAssetDataBaseLoader = new();					// 通过AssetDataBase加载资源的加载器,只会在编辑器下使用
	protected AssetBundleLoader mAssetBundleLoader = new();						// 通过AssetBundle加载资源的加载器,打包后强制使用AssetBundle加载
	protected List<UObjectCallback> mUnloadObjectCallback = new();				// 卸载某个单独资源的回调
	protected List<StringCallback> mUnloadPathCallback = new();					// 卸载目录中所有资源的回调,不会再次通知其中的单个资源
	protected LOAD_SOURCE mLoadSource;											// 加载源,从AssetBundle加载还是从AssetDataBase加载
	protected float mCheckRefTimer;												// 检查资源引用的计时器
	protected const float CHECK_REF_INTERVAL = 3.0f;							// 检查资源引用的间隔时间
	protected static int mDownloadTimeout = 10;									// 下载超时时间,秒
	public ResourceManager()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		mLoadSource = isEditor() ? GameEntry.getInstance().mFramworkParam.mLoadSource : LOAD_SOURCE.ASSET_BUNDLE;
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
		if (tickTimerLoop(ref mCheckRefTimer, elapsedTime, CHECK_REF_INTERVAL))
		{
			List<UObject> willRemoveList = null;
			foreach (var item in mReferenceTokenList)
			{
				if (item.Value.isEmpty())
				{
					if (willRemoveList == null)
					{
						LIST(out willRemoveList);
					}
					willRemoveList.add(item.Key);
				}
			}
			if (willRemoveList != null)
			{
				foreach (UObject item in willRemoveList)
				{
					unloadInternal(item);
					mReferenceTokenList.Remove(item);
				}
				UN_LIST(ref willRemoveList);
			}
		}
	}
	public override void destroy()
	{
		mAssetBundleLoader?.destroy();
		mAssetDataBaseLoader?.destroy();
		base.destroy();
	}
	public void addUnloadObjectCallback(UObjectCallback callback)			{ mUnloadObjectCallback.Add(callback); }
	public void addUnloadPathCallback(StringCallback callback)				{ mUnloadPathCallback.Add(callback); }
	public void removeUnloadObjectCallback(UObjectCallback callback)		{ mUnloadObjectCallback.Remove(callback); }
	public void removeUnloadPathCallback(StringCallback callback)			{ mUnloadPathCallback.Remove(callback); }
	public void requestLoadAssetBundle(AssetBundleInfo bundleInfo)			{ mAssetBundleLoader.requestLoadAssetBundle(bundleInfo); }
	public void requestLoadAsset(AssetBundleInfo bundleInfo, string fileNameWithSuffix) { mAssetBundleLoader.requestLoadAsset(bundleInfo, fileNameWithSuffix); }
	public void setDownloadURL(string url)									{ mAssetBundleLoader.setDownloadURL(url); }
	public string getDownloadURL()											{ return mAssetBundleLoader.getDownloadURL(); }
	public Dictionary<string, AssetBundleInfo> getAssetBundleInfoList()		{ return mAssetBundleLoader.getAssetBundleInfoList(); }
	public bool isDontUnloadAssetBundle(string bundleFileName)				{ return mAssetBundleLoader.isDontUnloadAssetBundle(bundleFileName); }
	public AssetBundleInfo getAssetBundleInfo(string name)					{ return mAssetBundleLoader.getAssetBundleInfo(name); }
	public int getDownloadTimeout()											{ return mDownloadTimeout; }
	public void setDownloadTimeout(int timeout)								{ mDownloadTimeout = timeout; }
	public void addDontUnloadAssetBundle(string bundleFileName)				{ mAssetBundleLoader.addDontUnloadAssetBundle(bundleFileName); }
	public void notifyAssetLoaded(UObject asset, AssetBundleInfo bundle)	{ mAssetBundleLoader.notifyAssetLoaded(asset, bundle); }
	public void unload<T>(ref ResourceRef<T> res) where T : UObject
	{
		if (res == null)
		{
			return;
		}
		res.unuse();
		UN_CLASS(ref res);
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
	// 指定卸载资源包,StreamingAssets/平台名下的路径,不带后缀
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
		checkRelativePath(name);
		bool ret = false;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			ret = mAssetDataBaseLoader.isAssetLoaded(name);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			ret = mAssetBundleLoader.isAssetLoaded<T>(name);
		}
		return ret;
	}
	// 获得资源,如果没有加载,则获取不到,使用频率可能比较低,name是GameResources下的相对路径,带后缀
	public T getGameResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		checkRelativePath(name);
		T res = null;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			res = mAssetDataBaseLoader.getAsset(name) as T;
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
	// 检查指定资源包的依赖项是否已经加载,如果没有会强制加载,一般来说用不上
	// 不会出现还在被其他资源包依赖就已经被卸载的情况,因为卸载的时候会检查是否有被其他资源包依赖,除非是手动强制卸载
	public void checkAssetBundleDependenceLoaded(string bundleName)
	{
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.checkAssetBundleDependenceLoaded(bundleName);
		}
	}
	// 同步预加载资源包,一般不需要调用,只有需要预加载时才会用到
	public void preloadAssetBundle(string bundleName)
	{
		// 只有从AssetBundle加载时才能加载AssetBundle
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundle(bundleName, null);
		}
	}
	// 异步预加载资源包,一般不需要调用,只有需要预加载时才会用到
	public void preloadAssetBundleAsync(string bundleName, AssetBundleCallback callback)
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
	public ResourceRef<T> loadGameResource<T>(string name, bool errorIfNull = true) where T : UObject
	{
		using var a = new ProfilerScope(0);
		checkRelativePath(name);
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
		CLASS(out ResourceRef<T> resRef).setResource(res);
		return resRef;
	}
	// 同步加载资源的子资源,一般是图集才会有子资源,或者是fbx
	public UObject[] loadSubGameResource<T>(string name, out ResourceRef<UObject> mainAsset, bool errorIfNull = true) where T : UObject
	{
		using var a = new ProfilerScope(0);
		checkRelativePath(name);
		UObject[] res = null;
		UObject main = null;
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			res = mAssetDataBaseLoader.loadSubResource<T>(name, out main);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			res = mAssetBundleLoader.loadSubAsset<T>(name, out main);
		}
		CLASS(out mainAsset).setResource(main);
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		return res;
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, AssetRefLoadCallback<T> callback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsyncInternal<T>(name, (UObject res, UObject[] subRes, byte[] bytes, string loadPath) => 
		{
			if (callback == null)
			{
				unloadInternal(res);
				return;
			}
			// 只需要对主资源添加引用封装,子资源都是跟随主资源的生命周期,不需要单独添加引用封装
			CLASS(out ResourceRef<T> resRef).setResource(res as T);
			callback(resRef, subRes, bytes, loadPath);
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<ResourceRef<T>, string> callback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string loadPath) =>
		{
			if (callback == null)
			{
				unloadInternal(asset);
				return;
			}
			CLASS(out ResourceRef<T> resRef).setResource(asset as T);
			callback(resRef, loadPath);
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	// 在relatedObj生命周期内加载资源,如果完成加载后relatedObj已经被销毁,则会自动卸载资源并且不会调用回调
	public CustomAsyncOperation loadGameResourceAsyncSafe<T>(IRecyclable relatedObj, string name, Action<ResourceRef<T>, string> callback, bool errorIfNull = true) where T : UObject
	{
		long assignID = relatedObj?.getAssignID() ?? 0;
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string loadPath) =>
		{
			if (callback == null || assignID != (relatedObj?.getAssignID() ?? 0))
			{
				unloadInternal(asset);
				return;
			}
			CLASS(out ResourceRef<T> resRef).setResource(asset as T);
			callback(resRef, loadPath);
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<ResourceRef<T>> callback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string _) =>
		{
			if (callback == null)
			{
				unloadInternal(asset);
				return;
			}
			CLASS(out ResourceRef<T> resRef).setResource(asset as T);
			callback(resRef);
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	// 在relatedObj生命周期内加载资源,如果完成加载后relatedObj已经被销毁,则会自动卸载资源并且不会调用回调
	public CustomAsyncOperation loadGameResourceAsyncSafe<T>(IRecyclable relatedObj, string name, Action<ResourceRef<T>> callback, bool errorIfNull = true) where T : UObject
	{
		long assignID = relatedObj?.getAssignID() ?? 0;
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string _) =>
		{
			if (callback == null || assignID != (relatedObj?.getAssignID() ?? 0))
			{
				unloadInternal(asset);
				return;
			}
			CLASS(out ResourceRef<T> resRef).setResource(asset as T);
			callback(resRef);
		}, errorIfNull);
	}
	// 仅下载一个资源,下载后会写入本地文件,并且更新本地文件信息列表,fileName为带后缀,GameResources下的相对路径
	public void downloadGameResource(string name, BytesCallback callback)
	{
		checkRelativePath(name);
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.downloadAsset(name, callback);
		}
	}
	public void addReference(UObject res, long token)
	{
		if (!mReferenceTokenList.getOrAddNew(res).Add(token))
		{
			logError("添加资源引用凭证失败");
		}
	}
	public void removeReference(UObject res, long token)
	{
		if (!mReferenceTokenList.TryGetValue(res, out var list) || !list.Remove(token))
		{
			logError("移除资源引用凭证失败,可能是重复移除一个资源");
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected bool unloadInternal(UObject obj, bool showError = true)
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
			success = mAssetDataBaseLoader.unloadAsset(obj, showError);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			success = mAssetBundleLoader.unloadAsset(obj, showError);
		}
		return success;
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	protected CustomAsyncOperation loadGameResourceAsyncInternal<T>(string name, AssetLoadCallback callback, bool errorIfNull = true) where T : UObject
	{
		using var a = new ProfilerScope(0);
		checkRelativePath(name);
		if (mLoadSource == LOAD_SOURCE.ASSET_DATABASE)
		{
			return mAssetDataBaseLoader.loadResourcesAsync<T>(name, callback);
		}
		else if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			return mAssetBundleLoader.loadAssetAsync<T>(name, errorIfNull, callback);
		}
		return null;
	}
	// 检查路径的合法性,需要带后缀,且需要是相对于GameResources的路径
	protected static void checkRelativePath(string path)
	{
		// 需要带后缀
		if (!path.Contains('.'))
		{
			logError("资源文件名需要带后缀:" + path);
			return;
		}
		// 不能是绝对路径
		if (path.startWith(FrameBaseDefine.F_ASSETS_PATH))
		{
			logError("不能传入绝对路径:" + path);
			return;
		}
		// 不能是以Assets或者Assets/GameResources开头的相对路径
		if (path.startWith(FrameDefine.P_GAME_RESOURCES_PATH) || path.startWith(FrameBaseDefine.ASSETS))
		{
			logError("不能是以Assets或者Assets/GameResources开头的相对路径:" + path);
			return;
		}
	}
}