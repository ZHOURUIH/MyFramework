using UnityEngine;
using System;
using System.Collections.Generic;

// 加载图集的参数,用于回调的传参
public class AtlasLoadParam : FrameBase
{
	public string mName;
	public AtlasLoadDone mCallback;
	public object mUserData;
	public bool mInResources;
	public bool mErrorIfNull;
	public override void resetProperty()
	{
		base.resetProperty();
		mName = null;
		mCallback = null;
		mUserData = null;
		mInResources = false;
		mErrorIfNull = false;
	}
}