using System;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static StringUtility;
using static FrameBaseUtility;
using static FrameUtility;

// 资源管理器,管理所有资源的加载
// 支持AssetDataBase(编辑器)和AssetBundle(打包)两种加载源,提供引用计数管理、定时清理、异步安全加载等功能
public class ResourceManager : FrameSystem
{
	protected Dictionary<int, HashSet<long>> mReferenceTokenList = new();		// 记录了每个资源的引用凭证ID,由于UObject重载了==,所以一旦外部卸载了UObject,这里就会出现GetHashCode不变,但是引用资源为空的问题,所以使用GetInstanceID作为Key
	protected Dictionary<int, UObject> mInstanceIDToUObject = new();			// 根据GetInstanceID查找UObject
	protected AssetDataBaseLoader mAssetDataBaseLoader = new();					// 通过AssetDataBase加载资源的加载器,只会在编辑器下使用
	protected AssetBundleLoader mAssetBundleLoader = new();						// 通过AssetBundle加载资源的加载器,打包后强制使用AssetBundle加载
	protected List<UObjectCallback> mUnloadObjectCallback = new();				// 卸载某个单独资源的回调
	protected List<StringCallback> mUnloadPathCallback = new();					// 卸载目录中所有资源的回调,不会再次通知其中的单个资源
	protected LOAD_SOURCE mLoadSource;											// 加载源,从AssetBundle加载还是从AssetDataBase加载
	protected float mCheckRefTimer;												// 检查资源引用的计时器
	protected const float CHECK_REF_INTERVAL = 3.0f;							// 检查资源引用的间隔时间
	protected static int mDownloadTimeout = 10;                                 // 下载超时时间,秒
	protected static long mTokenSeed;                                           // 用于生成一个引用凭证,不能放在ResourceRef<T>中,因为每个模板类型都有一个静态变量,这样就不能保证同一个资源的引用凭证在不同模板类型中是唯一的了
	public ResourceManager()
	{
		mCreateObject = true;
	}
	// 初始化,根据运行环境决定加载源(编辑器用AssetDatabase,打包后用AssetBundle)
	public override void init()
	{
		base.init();
		mLoadSource = isEditor() ? GameEntryBase.getInstance().mFrameworkParam.mLoadSource : LOAD_SOURCE.ASSET_BUNDLE;
		if (isEditor())
		{
			mObject.AddComponent<ResourcesManagerDebug>();
		}
	}
	// 预异步初始化,在正式init前调用,如果是AssetBundle模式则需要先初始化AssetBundle的依赖关系
	public override void preInitAsync(Action callback)
	{
		if (mLoadSource != LOAD_SOURCE.ASSET_BUNDLE)
		{
			callback?.Invoke();
			return;
		}
		mAssetBundleLoader.initAssets(callback);
	}
	// 资源系统是否已经初始化完成,AssetDatabase模式永远返回true
	public bool isResourceInited()
	{
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			return mAssetBundleLoader.isInited();
		}
		return true;
	}
	// 每帧更新,更新AssetBundle加载器的同时,每3秒检查一次引用归零的资源并自动卸载
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.update(elapsedTime);
		}
		if (tickTimerLoop(ref mCheckRefTimer, elapsedTime, CHECK_REF_INTERVAL))
		{
			List<int> willRemoveList = null;
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
				foreach (int id in willRemoveList)
				{
					mInstanceIDToUObject.Remove(id, out UObject item);
					mReferenceTokenList.Remove(id);
					unloadInternal(item);
				}
				UN_LIST(ref willRemoveList);
			}
		}
	}
	// 销毁资源管理器,释放所有加载器
	public override void destroy()
	{
		mAssetBundleLoader?.destroy();
		mAssetDataBaseLoader?.destroy();
		base.destroy();
	}
	// 注册卸载单个资源的回调,卸载任何资源时都会触发
	public void addUnloadObjectCallback(UObjectCallback callback)			{ mUnloadObjectCallback.Add(callback); }
	// 注册卸载路径的回调,卸载指定目录下的所有资源时触发
	public void addUnloadPathCallback(StringCallback callback)				{ mUnloadPathCallback.Add(callback); }
	// 移除卸载单个资源的回调
	public void removeUnloadObjectCallback(UObjectCallback callback)		{ mUnloadObjectCallback.Remove(callback); }
	// 移除卸载路径的回调
	public void removeUnloadPathCallback(StringCallback callback)			{ mUnloadPathCallback.Remove(callback); }
	// 请求加载指定AssetBundle(包括其依赖),由AssetBundleLoader内部调度
	public void requestLoadAssetBundle(AssetBundleInfo bundleInfo)			{ mAssetBundleLoader.requestLoadAssetBundle(bundleInfo); }
	// 请求加载AssetBundle中的某个资源文件,由AssetBundleLoader内部调度
	public void requestLoadAsset(AssetBundleInfo bundleInfo, string fileNameWithSuffix) { mAssetBundleLoader.requestLoadAsset(bundleInfo, fileNameWithSuffix); }
	// 设置资源下载的URL地址
	public void setDownloadURL(string url)									{ mAssetBundleLoader.setDownloadURL(url); }
	// 获取当前资源下载的URL地址
	public string getDownloadURL()											{ return mAssetBundleLoader.getDownloadURL(); }
	// 获取所有AssetBundle信息列表
	public Dictionary<string, AssetBundleInfo> getAssetBundleInfoList()		{ return mAssetBundleLoader.getAssetBundleInfoList(); }
	// 检查指定AssetBundle是否被标记为禁止卸载
	public bool isDontUnloadAssetBundle(string bundleFileName)				{ return mAssetBundleLoader.isDontUnloadAssetBundle(bundleFileName); }
	// 根据名称获取AssetBundle信息
	public AssetBundleInfo getAssetBundleInfo(string name)					{ return mAssetBundleLoader.getAssetBundleInfo(name); }
	// 获取下载超时时间(秒)
	public int getDownloadTimeout()											{ return mDownloadTimeout; }
	// 设置下载超时时间(秒)
	public void setDownloadTimeout(int timeout)								{ mDownloadTimeout = timeout; }
	// 将指定AssetBundle标记为禁止卸载
	public void addDontUnloadAssetBundle(string bundleFileName)				{ mAssetBundleLoader.addDontUnloadAssetBundle(bundleFileName); }
	// 通知AssetBundle加载器某个资源已加载完成,由加载流程内部调用
	public void notifyAssetLoaded(UObject asset, AssetBundleInfo bundle)	{ mAssetBundleLoader.notifyAssetLoaded(asset, bundle); }
	// 卸载指定的资源引用,释放ResourceRef对象
	public void unload<T>(ref ResourceRef<T> res) where T : UObject
	{
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
	// 同步预加载资源包,一般不需要调用,只有需要预加载时才会用到,不含后缀
	public void preloadAssetBundle(string bundleName)
	{
		// 只有从AssetBundle加载时才能加载AssetBundle
		if (mLoadSource == LOAD_SOURCE.ASSET_BUNDLE)
		{
			mAssetBundleLoader.loadAssetBundle(bundleName, null);
		}
	}
	// 异步预加载资源包,一般不需要调用,只有需要预加载时才会用到,不含后缀
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
		if (res == null)
		{
			return null;
		}
		CLASS(out ResourceRef<T> resRef).set(res);
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
		if (res == null && errorIfNull)
		{
			logError("can not find resource : " + name + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (name.Contains(' ') || name.Contains('　') ? "注意此文件名中带有空格" : ""));
		}
		if (res == null)
		{
			mainAsset = null;
			return null;
		}
		CLASS(out mainAsset).set(main);
		return res;
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, AssetRefLoadCallback<T> callback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsyncInternal<T>(name, (UObject res, UObject[] subRes, byte[] bytes, string loadPath) => 
		{
			CLASS(out ResourceRef<T> resRef);
            if (res != null)
			{
                // 只需要对主资源添加引用封装,子资源都是跟随主资源的生命周期,不需要单独添加引用封装
                resRef.set(res as T);
			}
			if (callback == null)
			{
				UN_CLASS(ref resRef);
			}
			else 
			{
				callback(resRef, subRes, bytes, loadPath);
			}
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<ResourceRef<T>, string> callback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string loadPath) =>
		{
			CLASS(out ResourceRef<T> resRef);
			if (asset != null)
			{
				resRef.set(asset as T);
			}
            if (callback == null)
            {
				UN_CLASS(ref resRef);
            }
			else
			{
                callback(resRef, loadPath);
            }
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	// 在relatedObj生命周期内加载资源,如果完成加载后relatedObj已经被销毁,则会自动卸载资源并且不会调用回调
	public CustomAsyncOperation loadGameResourceAsyncSafe<T>(IRecyclable relatedObj, string name, Action<ResourceRef<T>, string> callback, bool errorIfNull = true) where T : UObject
	{
		long assignID = relatedObj?.getAssignID() ?? 0;
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string loadPath) =>
		{
			CLASS(out ResourceRef<T> resRef);
            if (asset != null)
			{
				resRef.set(asset as T);
			}
            if (callback == null || assignID != (relatedObj?.getAssignID() ?? 0))
            {
                UN_CLASS(ref resRef);
            }
			else
			{
                callback(resRef, loadPath);
            }
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	public CustomAsyncOperation loadGameResourceAsync<T>(string name, Action<ResourceRef<T>> callback, bool errorIfNull = true) where T : UObject
	{
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string _) =>
		{
			CLASS(out ResourceRef<T> resRef);
			if (asset != null)
			{
				resRef.set(asset as T);
			}
            if (callback == null)
            {
				UN_CLASS(ref resRef);
            }
			else
			{
                callback(resRef);
            }
		}, errorIfNull);
	}
	// 异步加载资源,name是GameResources下的相对路径,带后缀名,errorIfNull表示当找不到资源时是否报错提示
	// 在relatedObj生命周期内加载资源,如果完成加载后relatedObj已经被销毁,则会自动卸载资源并且不会调用回调
	public CustomAsyncOperation loadGameResourceAsyncSafe<T>(IRecyclable relatedObj, string name, Action<ResourceRef<T>> callback, bool errorIfNull = true) where T : UObject
	{
		long assignID = relatedObj?.getAssignID() ?? 0;
		return loadGameResourceAsyncInternal<T>(name, (UObject asset, UObject[] _, byte[] _, string _) =>
		{
			CLASS(out ResourceRef<T> resRef);
            if (asset != null)
			{
                resRef.set(asset as T);
			}
            if (callback == null || assignID != (relatedObj?.getAssignID() ?? 0))
            {
                UN_CLASS(ref resRef);
            }
			else
			{
                callback(resRef);
            }
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
	// 添加一个资源的引用凭证,返回唯一的token,只能由ResourceRef调用
	public long addReference(UObject res)
	{
		long token = ++mTokenSeed;
		int instanceID = res.GetInstanceID();
		mInstanceIDToUObject.TryAdd(instanceID, res);
		if (!mReferenceTokenList.getOrAddNew(instanceID).Add(token))
		{
			logError("添加资源引用凭证失败:" + token);
		}
		return token;
	}
	// 移除一个资源的引用凭证,token会被置0,只能由ResourceRef调用
	public void removeReference(UObject res, ref long token)
	{
		if (!mReferenceTokenList.TryGetValue(res.GetInstanceID(), out var list) || !list.Remove(token))
		{
			logError("移除资源引用凭证失败,可能是重复移除一个资源:" + token);
		}
		token = 0;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 内部卸载接口,触发卸载回调后根据加载源走对应的卸载流程
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