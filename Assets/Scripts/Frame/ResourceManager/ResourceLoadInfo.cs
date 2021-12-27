using System.Collections.Generic;
using UnityEngine;

// 资源加载的信息,表示一个非AssetBundle的资源
public class ResourceLoadInfo : FrameBase
{	
	public List<AssetLoadDoneCallback> mCallback;	// 回调列表
	public List<object> mUserData;					// 回调的自定义参数列表
	public List<string> mLoadPath;					// 用于回调传参的加载路径列表,实际上里面都是mResourceName
	public Object[] mSubObjects;					// 子物体列表,比如图集中的所有Sprite
	public Object mObject;							// 资源物体
	public LOAD_STATE mState;						// 加载状态
	public string mPath;							// 加载路径,也就是mResouceName中的路径
	public string mResouceName;						// GameResources下的相对路径,不带后缀
	public ResourceLoadInfo()
	{
		mCallback = new List<AssetLoadDoneCallback>();
		mUserData = new List<object>();
		mLoadPath = new List<string>();
		mState = LOAD_STATE.UNLOAD;
	}
	public void addCallback(AssetLoadDoneCallback callback, object userData, string loadPath)
	{
		if(callback == null)
		{
			return;
		}
		mCallback.Add(callback);
		mUserData.Add(userData);
		mLoadPath.Add(loadPath);
	}
	public void callbackAll(Object asset, Object[] subAssets, byte[] bytes)
	{
		int count = mCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mCallback[i](asset, subAssets, bytes, mUserData[i], mLoadPath[i]);
		}
		mCallback.Clear();
		mUserData.Clear();
		mLoadPath.Clear();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mPath = null;
		mResouceName = null;
		mObject = null;
		mSubObjects = null;
		mState = LOAD_STATE.UNLOAD;
		mCallback.Clear();
		mUserData.Clear();
		mLoadPath.Clear();
	}
}