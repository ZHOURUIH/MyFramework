using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using static UnityUtility;

// 3D场景的实例
public class SceneInstance : DelayCmdWatcher
{
	public Action<float, bool> mLoadCallback;		// 加载回调
	public AsyncOperation mOperation;				// Unity场景异步加载的操作句柄
	public GameObject mRoot;						// 场景根节点,每个场景都应该添加一个名称格式固定的根节点,场景名_Root
	public Scene mScene;							// Unity场景实例
	protected Type mType;							// 类型
	public string mName;							// 场景名
	public bool mActiveLoaded;						// 加载完毕后是否立即显示
	public bool mInited;							// 是否已经执行了初始化
	public LOAD_STATE mState = LOAD_STATE.UNLOAD;	// 加载状态
	public virtual void init()
	{
		if(mInited)
		{
			return;
		}
		findShaders(mRoot);
		findGameObject();
		initGameObject();
		mInited = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLoadCallback = null;
		mOperation = null;
		mRoot = null;
		mScene = default;
		mType = null;
		mName = null;
		mActiveLoaded = false;
		mInited = false;
		mState = LOAD_STATE.UNLOAD;
	}
	public virtual void destroy()
	{
		mInited = false;
	}
	public virtual void update(float elapsedTime){}
	public void setActive(bool active)
	{
		if (mRoot != null && mRoot.activeSelf != active)
		{
			mRoot.SetActive(active);
		}
	}
	public void setType(Type type) { mType = type; }
	public Type getType() { return mType; }
	public bool getActive() { return mRoot != null && mRoot.activeSelf; }
	public void setName(string name) { mName = name; }
	public GameObject getRoot() { return mRoot; }
	public virtual void onShow() { }
	public virtual void onHide() { }
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void findGameObject() { }
	protected virtual void initGameObject() { }
}