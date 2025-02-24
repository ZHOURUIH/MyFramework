using System;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static FrameBaseHotFix;
using static FrameDefine;

// AssetBundle中的Asset的信息
[Serializable]
public class AssetInfo : ClassObject
{
	protected List<AssetLoadDoneCallback> mCallback = new();    // 异步加载回调列表
	protected List<string> mLoadPath = new();                   // 加载资源时使用的路径
	protected UObject[] mSubAssets;								// 资源数组,数组第一个元素为主资源,后面的是子资源
	protected AssetBundleInfo mParentAssetBundle;				// 资源所属的AssetBundle
	protected string mAssetName;								// 资源文件名,带相对于StreamingAssets的相对路径,带后缀
	protected LOAD_STATE mLoadState = LOAD_STATE.NONE;        // 加载状态
	public void setAssetBundleInfo(AssetBundleInfo parent) { mParentAssetBundle = parent; }
	public void setAssetName(string name) { mAssetName = name; }
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback.Clear();
		mLoadPath.Clear();
		mSubAssets = null;
		mParentAssetBundle = null;
		mAssetName = null;
		mLoadState = LOAD_STATE.NONE;
	}
	public LOAD_STATE getLoadState() { return mLoadState; }
	public void clear()
	{
		mSubAssets = null;
		mLoadState = LOAD_STATE.NONE;
	}
	// 同步加载资源
	public T loadAsset<T>() where T : UObject
	{
		doLoadAssets();
		return mSubAssets.getSafe(0) as T;
	}
	// 同步加载所有子资源
	public UObject[] loadAsset()
	{
		doLoadAssets();
		return mSubAssets;
	}
	public UObject getAsset() { return mSubAssets.getSafe(0); }
	public string getAssetName() { return mAssetName; }
	public void setLoadState(LOAD_STATE state) { mLoadState = state; }
	public bool isLoaded() { return mSubAssets != null; }
	public AssetBundleInfo getAssetBundle() { return mParentAssetBundle; }
	// 异步加载资源
	public void loadAssetAsync()
	{
		// 如果资源已经存在,则直接返回
		if (mSubAssets != null)
		{
			callbackAll();
		}
		else
		{
			mLoadState = LOAD_STATE.WAIT_FOR_LOAD;
			mResourceManager.getAssetBundleLoader().requestLoadAsset(mParentAssetBundle, mAssetName);
		}
	}
	// 资源已经加载完毕
	public void setSubAssets(UObject[] assets)
	{
		// 检查资源是否正常加载完成,如果在资源异步加载过程中资源包被卸载了,则资源无法正常加载完成
		bool hasAssets = true;
		foreach (UObject obj in assets.safe())
		{
			if (obj == null)
			{
				hasAssets = false;
				break;
			}
		}
		if (hasAssets)
		{
			mSubAssets = assets;
			mLoadState = LOAD_STATE.LOADED;
		}
		else
		{
			mSubAssets = null;
			mLoadState = LOAD_STATE.NONE;
		}
	}
	public void addCallback(AssetLoadDoneCallback callback, string loadPath)
	{
		if (callback == null)
		{
			return;
		}
		mCallback.Add(callback);
		mLoadPath.Add(loadPath);
	}
	public void callbackAll()
	{
		// 复制一份列表,避免回调中再次修改回调列表而报错
		using var a = new ListScope2T<AssetLoadDoneCallback, string>(out var callbacks, out var paths);
		callbacks.move(mCallback);
		paths.move(mLoadPath);
		int callbackCount = callbacks.Count;
		for (int i = 0; i < callbackCount; ++i)
		{
			callbacks[i](mSubAssets.getSafe(0), mSubAssets, null, paths[i]);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void doLoadAssets()
	{
		if (mSubAssets != null)
		{
			return;
		}
		mSubAssets = mParentAssetBundle.getAssetBundle().LoadAssetWithSubAssets(P_GAME_RESOURCES_PATH + mAssetName);
		mLoadState = LOAD_STATE.LOADED;
	}
}