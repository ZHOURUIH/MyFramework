using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ResoucesLoadInfo : IClassObject
{
	public string mPath;
	public string mResouceName;
	public Object mObject;
	public Object[] mSubObjects;
	public LOAD_STATE mState;
	public List<AssetLoadDoneCallback> mCallback;
	public List<object[]> mUserData;
	public List<string> mLoadPath;
	public ResoucesLoadInfo()
	{
		mCallback = new List<AssetLoadDoneCallback>();
		mUserData = new List<object[]>();
		mLoadPath = new List<string>();
		mState = LOAD_STATE.LS_UNLOAD;
	}
	public void addCallback(AssetLoadDoneCallback callback, object[] userData, string loadPath)
	{
		if(callback != null)
		{
			mCallback.Add(callback);
			mUserData.Add(userData);
			mLoadPath.Add(loadPath);
		}
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
		mPath = "";
		mResouceName = "";
		mObject = null;
		mSubObjects = null;
		mState = LOAD_STATE.LS_NONE;
		mCallback.Clear();
		mUserData.Clear();
		mLoadPath.Clear();
	}
}