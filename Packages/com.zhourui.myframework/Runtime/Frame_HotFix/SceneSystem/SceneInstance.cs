using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityUtility;

// 3D场景的实例
public class SceneInstance : DelayCmdWatcher
{
	protected FloatCallback mLoadingCallback;   // 加载中回调
	protected Action mLoadedCallback;           // 加载完成回调
	protected GameObject mRoot;                 // 场景根节点,每个场景都应该添加一个名称格式固定的根节点,场景名_Root
	protected Scene mScene;                     // Unity场景实例
	protected Type mType;                       // 类型
	protected string mName;                     // 场景名
	protected bool mActiveLoaded;               // 加载完毕后是否立即显示
	protected bool mMainScene;                  // 是否为主场景
	protected bool mInited;                     // 是否已经执行了初始化
	protected LOAD_STATE mState;                // 加载状态
	public SceneInstance()
	{
		mState = LOAD_STATE.NONE;
	}
	public virtual void init()
	{
		if (mInited)
		{
			return;
		}
		findShaders(mRoot);
		mInited = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLoadingCallback = null;
		mLoadedCallback = null;
		mRoot = null;
		mScene = default;
		mType = null;
		mName = null;
		mActiveLoaded = false;
		mMainScene = false;
		mInited = false;
		mState = LOAD_STATE.NONE;
	}
	public override void destroy()
	{
		base.destroy();
		mInited = false;
	}
	public virtual void onShow() { }
	public virtual void onHide() { }
	public virtual void update(float elapsedTime) { }
	public virtual void lateUpdate(float elapsedTime) { }
	// 不要直接调用SceneInstance的setActive,应该使用SceneSystem的showScene或者hideScene
	public void setActive(bool active)
	{
		if (mRoot != null && mRoot.activeSelf != active)
		{
			mRoot.SetActive(active);
		}
	}
	public void setType(Type type)							{ mType = type; }
	public void setName(string name)						{ mName = name; }
	public void setState(LOAD_STATE state)					{ mState = state; }
	public void setActiveLoaded(bool active)				{ mActiveLoaded = active; }
	public void setLoadedCallback(Action callback)			{ mLoadedCallback = callback; }
	public void setLoadingCallback(FloatCallback callback)	{ mLoadingCallback = callback; }
	public void setScene(Scene scene)						{ mScene = scene; }
	public void setRoot(GameObject root)					{ mRoot = root; }
	public void setMainScene(bool main)						{ mMainScene = main; }
	public Type getType()									{ return mType; }
	public bool getActive()									{ return mRoot != null && mRoot.activeSelf; }
	public GameObject getRoot()								{ return mRoot; }
	public string getName()									{ return mName; }
	public LOAD_STATE getState()							{ return mState; }
	public bool isActiveLoaded()							{ return mActiveLoaded; }
	public bool isInited()									{ return mInited; }
	public bool isMainScene()								{ return mMainScene; }
	public Scene getScene()									{ return mScene; }
	public void callLoading(float percent)					{ mLoadingCallback?.Invoke(percent); }
	public void callLoaded()								{ mLoadedCallback?.Invoke(); }
}