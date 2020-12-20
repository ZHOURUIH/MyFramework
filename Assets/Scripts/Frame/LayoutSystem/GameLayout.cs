using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class GameLayout : FrameBase
{
	protected Dictionary<GameObject, myUIObject> mGameObjectSearchList; // 用于根据GameObject查找UI
	// 布局中UI物体列表,用于保存所有已获取的UI
	// 更新过程中窗口创建或者销毁时并不会立即更新到此列表
	protected SafeDictionary<int, myUIObject> mObjectList;
#if USE_NGUI
	protected myNGUIPanel mNGUILayoutRoot;			// NGUI的Panel
#endif
	protected myUGUICanvas mUGUILayoutRoot;         // UGUI的Canvas
	protected LayoutScript mScript;			// 布局脚本
	protected GameObject mPrefab;           // 布局预设,布局从该预设实例化
	protected myUIObject mRoot;				// 布局根节点
	protected LAYOUT_ORDER mRenderOrderType;// 布局渲染顺序的计算方式
	protected GUI_TYPE mGUIType;            // UI插件类型
	protected string mName;					// 布局名称
	protected bool mScriptControlHide;      // 是否由脚本来控制隐藏
	protected bool mIgnoreTimeScale;        // 更新布局时是否忽略时间缩放
	protected bool mCheckBoxAnchor;         // 是否检查布局中所有带碰撞盒的窗口是否自适应分辨率
	protected bool mAnchorApplied;          // 是否已经完成了自适应的调整
	protected bool mScriptInited;           // 脚本是否已经初始化
	protected bool mBlurBack;               // 布局显示时是否需要使布局背后(比当前布局层级低)的所有布局模糊显示
	protected bool mIsScene;                // 是否为场景,如果是场景,就不将布局挂在NGUIRoot或者UGUIRoot下
	protected int mDefaultLayer;            // 布局加载时所处的层
	protected int mRenderOrder;             // 渲染顺序,越大则渲染优先级越高,不能小于0
	protected int mID;						// 布局ID
	protected static List<LayoutScriptCallback> mLayoutScriptCallback = new List<LayoutScriptCallback>();
	public GameLayout()
	{
		mGameObjectSearchList = new Dictionary<GameObject, myUIObject>();
		mObjectList = new SafeDictionary<int, myUIObject>();
		mCheckBoxAnchor = true;
		mRenderOrder = 0;
	}
	public static void addScriptCallback(LayoutScriptCallback callback)
	{
		if (!mLayoutScriptCallback.Contains(callback))
		{
			mLayoutScriptCallback.Add(callback);
		}
	}
	public static void removeScriptCallback(LayoutScriptCallback callback)
	{
		mLayoutScriptCallback.Remove(callback);
	}
	public void setPrefab(GameObject prefab) { mPrefab = prefab; }
	public void setOrderType(LAYOUT_ORDER orderType) { mRenderOrderType = orderType; }
	public void setRenderOrder(int renderOrder)
	{
		mRenderOrder = renderOrder;
		if (mRenderOrder < 0)
		{
			logError("布局深度不能小于0,否则无法正确计算窗口深度");
			return;
		}
		if(mGUIType == GUI_TYPE.NGUI)
		{
#if USE_NGUI
			mNGUILayoutRoot.setDepth(mRenderOrder);
#endif
		}
		else if (mGUIType == GUI_TYPE.UGUI)
		{
			mUGUILayoutRoot.setSortingOrder(mRenderOrder);
		}
		// 刷新所有窗口注册的深度
		setUIDepth(mRoot, mRenderOrder);
	}
	public LAYOUT_ORDER getRenderOrderType() { return mRenderOrderType; }
	public int getRenderOrder(){return mRenderOrder;}
	public void setBlurBack(bool blurBack) { mBlurBack = blurBack; }
	public bool isBlurBack() { return mBlurBack; }
	public int getDefaultLayer() { return mDefaultLayer; }
	public bool isAnchorApplied() { return mAnchorApplied; }
	public void setID(int id) { mID = id; }
	public void setName(string name) { mName = name; }
	public void setGUIType(GUI_TYPE guiType) { mGUIType = guiType; }
	public void setIsScene(bool isScene) { mIsScene = isScene; }
	public myUIObject getUIObject(GameObject go)
	{
		mGameObjectSearchList.TryGetValue(go, out myUIObject obj);
		return obj;
	}
	public void init(int renderOrder, LAYOUT_ORDER orderType)
	{
		mRenderOrderType = orderType;
		mScript = mLayoutManager.createScript(this);
		int count = mLayoutScriptCallback.Count;
		for(int i = 0; i < count; ++i)
		{
			mLayoutScriptCallback[i].Invoke(mScript, true);
		}
		if (mScript == null)
		{
			logError("can not create layout script! id : " + mID);
			return;
		}
		myUIObject parent = mIsScene ? null : mLayoutManager.getUIRoot(mGUIType);
		// 初始化布局脚本
		if(mGUIType == GUI_TYPE.NGUI)
		{
#if USE_NGUI
			mScript.newObject(out mNGUILayoutRoot, parent, mName);
			mRoot = mNGUILayoutRoot;
#endif
		}
		else if(mGUIType == GUI_TYPE.UGUI)
		{
			mScript.newObject(out mUGUILayoutRoot, parent, mName);
			mRoot = mUGUILayoutRoot;
		}
		mRoot.setDestroyImmediately(true);
		mDefaultLayer = mRoot.getObject().layer;
		setOrderType(orderType);
		mScript.setRoot(mRoot);
		mScript.assignWindow();
		// assignWindow后设置布局的渲染顺序,这样可以在此处刷新所有窗口的深度
		setRenderOrder(renderOrder);
		// 布局实例化完成,初始化之前,需要调用自适应组件的更新
		if (mLayoutManager.isUseAnchor())
		{
			applyAnchor(mRoot.getObject(), true, this);
		}
		mAnchorApplied = true;
		mScript.init();
		mScriptInited = true;
		// 加载完布局后强制隐藏
		setVisibleForce(false);
#if UNITY_EDITOR
		mRoot.getObject().AddComponent<LayoutDebug>().setLayout(this);
#endif
	}
	public void update(float elapsedTime)
	{
		if(mIgnoreTimeScale)
		{
			elapsedTime = Time.unscaledDeltaTime;
		}
		if (isVisible() && mScript != null && mScriptInited)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Profiler.BeginSample("UpdateLayout:" + getName());
#endif
			// 先更新所有的UI物体
			var updateList = mObjectList.GetUpdateList();
			foreach (var obj in updateList)
			{
				if (obj.Value.canUpdate())
				{
					obj.Value.update(elapsedTime);
				}
			}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Profiler.EndSample();
			Profiler.BeginSample("UpdateScript:" + getName());
#endif
			mScript.update(elapsedTime);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Profiler.EndSample();
#endif
		}
	}
	public void onDrawGizmos()
	{
		if (isVisible() && mScript != null && mScriptInited)
		{
			mScript.onDrawGizmos();
		}
	}
	public void lateUpdate(float elapsedTime)
	{
		if(isVisible() && mScript != null && mScriptInited)
		{
			mScript.lateUpdate(elapsedTime);
		}
	}
	public void destroy()
	{
		if (mScript != null)
		{
			int count = mLayoutScriptCallback.Count;
			for(int i = 0; i < count; ++i)
			{
				mLayoutScriptCallback[i].Invoke(mScript, false);
			}
			mScript.destroy();
			mScript = null;
		}
		myUIObject.destroyWindow(mRoot, true);
		mRoot = null;
		if (mPrefab != null)
		{
			mResourceManager.unload(ref mPrefab);
		}
	}
	public void getAllCollider(List<Collider> colliders, bool append = false)
	{
		if (!append)
		{
			colliders.Clear();
		}
		var mainList = mObjectList.GetMainList();
		foreach (var obj in mainList)
		{
			Collider collider = obj.Value.getCollider();
			if(collider != null)
			{
				colliders.Add(collider);
			}
		}
	}
	public bool isScriptControlHide() { return mScriptControlHide; }
	// 设置是否会立即隐藏,应该由布局脚本调用
	public void setScriptControlHide(bool control) { mScriptControlHide = control; }
	public void setVisible(bool visible, bool immediately, string param)
	{
		if (mScript == null || !mScriptInited)
		{
			return;
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutVisible(visible, this);
		// 设置布局显示或者隐藏时需要先通知脚本开始显示或隐藏
		mScript.notifyStartShowOrHide();
		// 显示布局时立即显示
		if (visible)
		{
			mRoot.setActive(visible);
			mScript.onReset();
			mScript.onGameState();
			mScript.onShow(immediately, param);
		}
		// 隐藏布局时需要判断
		else
		{
			if (!mScriptControlHide)
			{
				mRoot.setActive(visible);
			}
			mScript.onHide(immediately, param);
		}
	}
	public void setVisibleForce(bool visible)
	{
		if (mScript == null || !mScriptInited)
		{
			return;
		}
		mLayoutManager.notifyLayoutVisible(visible, this);
		// 直接设置布局显示或隐藏
		mRoot.setActive(visible);
	}
	public bool isVisible() { return mRoot.isActive(); }
	public myUIObject getRoot() { return mRoot; }
	public LayoutScript getScript() { return mScript; }
	public int getID() { return mID; }
	public string getName() { return mName; }
	public GUI_TYPE getGUIType() { return mGUIType; }
	public bool isScene() { return mIsScene; }
	public void setCheckBoxAnchor(bool check) { mCheckBoxAnchor = check; }
	public bool isCheckBoxAnchor() { return mCheckBoxAnchor; }
	public void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public void registerUIObject(myUIObject uiObj)
	{
		mObjectList.Add(uiObj.getID(), uiObj);
		mGameObjectSearchList.Add(uiObj.getObject(), uiObj);
	}
	public void unregisterUIObject(myUIObject uiObj)
	{
		mObjectList.Remove(uiObj.getID());
		mGameObjectSearchList.Remove(uiObj.getObject());
	}
	public void setLayer(string layer)
	{
		setGameObjectLayer(mRoot.getObject(), layer);
	}
	// 有节点删除或者增加,或者节点在当前父节点中的位置有改变,parent表示有变动的节点的父节点
	public void notifyChildChanged(myUIObject parent, bool ignoreInactive = false)
	{
		setUIDepth(parent, 0, false, ignoreInactive);
	}
	//------------------------------------------------------------------------------------------------------------
	// ignoreInactive表示是否忽略未启用的节点,当includeSelf为true时orderInParent才会生效
	protected void setUIDepth(myUIObject window, int orderInParent, bool includeSelf = true, bool ignoreInactive = false)
	{
		// 只有UGUI需要设置所有窗口的深度
		if (mGUIType != GUI_TYPE.UGUI)
		{
			return;
		}
		// 先设置当前窗口的深度
		if (includeSelf)
		{
			UIDepth parentDepth = null;
			if (window != mRoot)
			{
				parentDepth = window.getParent().getDepth();
			}
			window.getDepth().setDepthValue(parentDepth, orderInParent, window.isDepthOverAllChild());
			mGlobalTouchSystem.notifyWindowDepthChanged(window);
		}
		if (ignoreInactive && !window.isActive())
		{
			return;
		}

		// 再设置子窗口的深度,子节点在父节点中的下标从1开始,如果从0开始,则会与默认值的0混淆
		var children = window.getChildList();
		int childCount = children.Count;
		for (int i = 0; i < childCount; ++i)
		{
			setUIDepth(children[i], i + 1, true, ignoreInactive);
		}
	}
}