using System;

// 加载图集的参数,用于回调的传参
public class AtlasLoadParam : ClassObject
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