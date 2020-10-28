using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AssetInfo : FrameBase
{
	protected List<AssetLoadDoneCallback> mCallback;// 异步加载回调列表
	protected List<object[]> mUserData;				// 异步加载回调参数列表
	protected List<string> mLoadPath;               // 加载资源时使用的路径
	protected AssetBundleInfo mParentAssetBundle;	// 资源所属的AssetBundle
	protected UnityEngine.Object[] mSubAssets;		// 资源数组,数组第一个元素为主资源,后面的是子资源
	protected LOAD_STATE mLoadState;                // LS_NONE表示未加载,LS_UNLOAD表示已卸载
	protected string mAssetName;                    // 资源文件名,带相对于StreamingAssets的相对路径,带后缀
	public AssetInfo(AssetBundleInfo parent, string name)
	{
		mParentAssetBundle = parent;
		mAssetName = name;
		mSubAssets = null;
		mCallback = new List<AssetLoadDoneCallback>();
		mUserData = new List<object[]>();
		mLoadPath = new List<string>();
		mLoadState = LOAD_STATE.NONE;
	}
	public bool isUnloaded() { return mLoadState == LOAD_STATE.UNLOAD; }
	public void unload()
	{
		mSubAssets = null;
		mLoadState = LOAD_STATE.UNLOAD;
	}
	// 同步加载资源
	public T loadAsset<T>() where T : UnityEngine.Object
	{
		doLoadAssets();
		return mSubAssets != null ? mSubAssets[0] as T : null;
	}
	public UnityEngine.Object getAsset()
	{
		return mSubAssets != null ? mSubAssets[0] : null;
	}
	public string getAssetName() { return mAssetName; }
	public void setLoadState(LOAD_STATE state) { mLoadState = state; }
	public bool isLoaded() { return mSubAssets != null; }
	public AssetBundleInfo getAssetBundle() { return mParentAssetBundle; }
	// 异步加载资源
	public void loadAssetAsync()
	{
		// 如果资源已经存在,则直接返回
		if(mSubAssets != null)
		{
			callbackAll(mSubAssets);
		}
		else
		{
			mLoadState = LOAD_STATE.WAIT_FOR_LOAD;
			mResourceManager.mAssetBundleLoader.requestLoadAsset(mParentAssetBundle, mAssetName);
		}
	}
	// 同步加载所有子资源
	public UnityEngine.Object[] loadSubAssets()
	{
		doLoadAssets();
		return mSubAssets;
	}
	// 异步加载所有子资源
	public void loadSubAssetsAsync()
	{
		// 如果资源已经存在,则直接返回
		if (mSubAssets != null)
		{
			callbackAll(mSubAssets);
		}
		else
		{
			mResourceManager.mAssetBundleLoader.requestLoadAsset(mParentAssetBundle, mAssetName);
		}
	}
	// 资源已经加载完毕
	public void notifyAssetLoaded(UnityEngine.Object[] assets)
	{
		// 检查资源是否正常加载完成,如果在资源异步加载过程中资源包被卸载了,则资源无法正常加载完成
		bool hasAssets = true;
		if (assets != null)
		{
			foreach (var item in assets)
			{
				if (item == null)
				{
					hasAssets = false;
					break;
				}
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
		callbackAll(mSubAssets);
	}
	public void addCallback(AssetLoadDoneCallback callback, object[] userData, string loadPath)
	{
		if (callback != null)
		{
			mCallback.Add(callback);
			mUserData.Add(userData);
			mLoadPath.Add(loadPath);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------------
	protected void callbackAll(UnityEngine.Object[] assets)
	{
		int callbackCount = mCallback.Count;
		for (int i = 0; i < callbackCount; ++i)
		{
			mCallback[i](assets[0], assets, null, mUserData[i], mLoadPath[i]);
		}
		mCallback.Clear();
		mUserData.Clear();
		mLoadPath.Clear();
	}
	protected void doLoadAssets()
	{
		if(mSubAssets == null)
		{
			if(mParentAssetBundle.getAssetBundle() != null)
			{
				mSubAssets = mParentAssetBundle.getAssetBundle().LoadAssetWithSubAssets(FrameDefine.P_GAME_RESOURCES_PATH + mAssetName);
			}
			mLoadState = LOAD_STATE.LOADED;
		}
	}
}