using System.Collections.Generic;
using UObject = UnityEngine.Object;

// 资源加载的信息,表示一个非AssetBundle的资源
public class ResourceLoadInfo : ClassObject
{
	public List<AssetLoadDoneCallback> mCallback = new();   // 回调列表
	public List<string> mLoadPath = new();                  // 用于回调传参的加载路径列表,实际上里面都是mResourceName
	protected UObject[] mSubObjects;                        // 子物体列表,比如图集中的所有Sprite
	protected UObject mObject;								// 资源物体
	protected string mPath;									// 加载路径,也就是mResourceName中的路径,不带文件名
	protected string mResourceName;							// GameResources下的相对路径,带后缀
	protected LOAD_STATE mState = LOAD_STATE.NONE;        // 加载状态
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
		// 需要复制一份列表,避免回调期间又开始加载资源而造成逻辑错误
		using var a = new ListScope2T<AssetLoadDoneCallback, string>(out var tempCallbackList, out var tempLoadPath);
		mCallback.moveTo(tempCallbackList);
		mLoadPath.moveTo(tempLoadPath);
		UObject tempObj = mObject;
		UObject[] tempSubObjs = mSubObjects;
		int count = tempCallbackList.Count;
		for (int i = 0; i < count; ++i)
		{
			tempCallbackList[i](tempObj, tempSubObjs, null, tempLoadPath[i]);
		}
	}
	public UObject[] getSubObjects() { return mSubObjects; }
	public UObject getObject() { return mObject; }
	public LOAD_STATE getState() { return mState; }
	public string getPath() { return mPath; }
	public string getResourceName() { return mResourceName; }
	public void setPath(string path) { mPath = path; }
	public void setResourceName(string name) { mResourceName = name; }
	public void setObject(UObject obj) { mObject = obj; }
	public void setSubObjects(UObject[] objs) { mSubObjects = objs; }
	public void setState(LOAD_STATE state) { mState = state; }
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback.Clear();
		mLoadPath.Clear();
		mSubObjects = null;
		mObject = null;
		mPath = null;
		mResourceName = null;
		mState = LOAD_STATE.NONE;
	}
}