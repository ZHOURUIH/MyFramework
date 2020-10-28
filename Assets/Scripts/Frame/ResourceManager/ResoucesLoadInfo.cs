using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResoucesLoadInfo : IClassObject
{	
	public List<AssetLoadDoneCallback> mCallback;
	public List<object[]> mUserData;
	public List<string> mLoadPath;
	public Object[] mSubObjects;
	public Object mObject;
	public LOAD_STATE mState;
	public string mPath;
	public string mResouceName;
	public ResoucesLoadInfo()
	{
		mCallback = new List<AssetLoadDoneCallback>();
		mUserData = new List<object[]>();
		mLoadPath = new List<string>();
		mState = LOAD_STATE.UNLOAD;
	}
	public void addCallback(AssetLoadDoneCallback callback, object[] userData, string loadPath)
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
	public void resetProperty()
	{
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