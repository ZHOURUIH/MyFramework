using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class GameLayout : FrameBase
{
	protected static List<LayoutScriptCallback> mLayoutScriptCallback = new List<LayoutScriptCallback>();
	protected Dictionary<GameObject, myUIObject> mGameObjectSearchList; // 用于根据GameObject查找UI
	protected SafeDictionary<uint, myUIObject> mNeedUpdateList;			// mObjectList中需要更新的窗口列表
	protected SafeDictionary<uint, myUIObject> mObjectList;             // 布局中UI物体列表,用于保存所有已获取的UI
	protected myUGUICanvas mRoot;				// 布局根节点
	protected LayoutScript mScript;				// 布局脚本
	protected myUIObject mParent;				// 布局父节点,可能是UGUIRoot,也可能为空
	protected GameObject mPrefab;				// 布局预设,布局从该预设实例化
	protected string mName;						// 布局名称
	protected int mDefaultLayer;				// 布局加载时所处的层
	protected int mRenderOrder;					// 渲染顺序,越大则渲染优先级越高,不能小于0
	protected int mID;							// 布局ID
	protected bool mDefaultUpdateWindow;		// 是否默认就将所有注册的窗口添加到更新列表中,默认是添加的,在某些需要重点优化的布局中可以选择将哪些窗口放入更新列表
	protected bool mScriptControlHide;			// 是否由脚本来控制隐藏
	protected bool mIgnoreTimeScale;			// 更新布局时是否忽略时间缩放
	protected bool mCheckBoxAnchor;				// 是否检查布局中所有带碰撞盒的窗口是否自适应分辨率
	protected bool mAnchorApplied;				// 是否已经完成了自适应的调整
	protected bool mScriptInited;				// 脚本是否已经初始化
	protected bool mInResources;				// 是否是从Resources中加载的资源,大部分布局都不是从Resources中加载的
	protected bool mBlurBack;					// 布局显示时是否需要使布局背后(比当前布局层级低)的所有布局模糊显示
	protected LAYOUT_ORDER mRenderOrderType;	// 布局渲染顺序的计算方式
#if UNITY_EDITOR || DEVELOPMENT_BUILD
	protected string Profiler_UpdateLayout;
	protected string Profiler_UpdateScript;
#endif
	public GameLayout()
	{
		mGameObjectSearchList = new Dictionary<GameObject, myUIObject>();
		mNeedUpdateList = new SafeDictionary<uint, myUIObject>();
		mObjectList = new SafeDictionary<uint, myUIObject>();
		mCheckBoxAnchor = true;
		mDefaultUpdateWindow = true;
	}
	public void init()
	{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler_UpdateLayout = "UpdateLayout" + getName();
		Profiler_UpdateScript = "UpdateScript" + getName();
#endif
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

		// 初始化布局脚本
		mScript.newObject(out mRoot, mParent, mName);
		
		// 去除自带的锚点
		// 在unity2020中,不知道为什么实例化以后的RectTransform的大小会自动变为视图窗口大小,为了适配计算正确,这里需要重置一次
		RectTransform rectTransform = mRoot.getRectTransform();
		Vector3 size = getRectSize(rectTransform);
		rectTransform.anchorMin = Vector2.one * 0.5f;
		rectTransform.anchorMax = Vector2.one * 0.5f;
		setRectSize(rectTransform, new Vector2(FrameDefineExtension.STANDARD_WIDTH, FrameDefineExtension.STANDARD_HEIGHT));

		mRoot.setDestroyImmediately(true);
		mDefaultLayer = mRoot.getObject().layer;
		mScript.setRoot(mRoot);
		mScript.assignWindow();
		// assignWindow后再次设置布局的渲染顺序,这样可以在此处刷新所有窗口的深度
		// 所以在assignWindow中不需要在创建窗口对象时刷新深度,这样会造成很大的性能浪费
		setRenderOrder(mRenderOrder);
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
		mRoot.getUnityComponent<LayoutDebug>().setLayout(this);
#endif
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mGameObjectSearchList.Clear();
		mNeedUpdateList.clear();
		mObjectList.clear();
		mRoot = null;
		mScript = null;
		mParent = null;
		mPrefab = null;
		mName = null;
		mDefaultLayer = 0;
		mRenderOrder = 0;
		mID = 0;
		mDefaultUpdateWindow = true;
		mScriptControlHide = false;
		mIgnoreTimeScale = false;
		mCheckBoxAnchor = true;
		mAnchorApplied = false;
		mScriptInited = false;
		mInResources = false;
		mBlurBack = false;
		mRenderOrderType = LAYOUT_ORDER.ALWAYS_TOP;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler_UpdateLayout = null;
		Profiler_UpdateScript = null;
#endif
	}
	public void update(float elapsedTime)
	{
		if (!isVisible() || mScript == null || !mScriptInited)
		{
			return;
		}

		if (mIgnoreTimeScale)
		{
			elapsedTime = Time.unscaledDeltaTime;
		}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.BeginSample(Profiler_UpdateLayout);
#endif
		// 更新所有的UI物体
		foreach (var item in mObjectList.startForeach())
		{
			myUIObject uiObj = item.Value;
			if (uiObj.canUpdate())
			{
				uiObj.update(elapsedTime);
			}
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.EndSample();
#endif

		// 更新脚本逻辑
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.BeginSample(Profiler_UpdateScript);
#endif
		if (mScript.isNeedUpdate())
		{
			mScript.update(elapsedTime);
		}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Profiler.EndSample();
#endif
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
	public void setRenderOrder(int renderOrder)
	{
		mRenderOrder = renderOrder;
		if (mRenderOrder < 0)
		{
			logError("布局深度不能小于0,否则无法正确计算窗口深度");
			return;
		}
		if (mRoot == null)
		{
			return;
		}
		mRoot.setSortingOrder(mRenderOrder);
		// 刷新所有窗口注册的深度
		setUIDepth(mRoot, mRenderOrder);
	}
	public void getAllCollider(List<Collider> colliders, bool append = false)
	{
		if (!append)
		{
			colliders.Clear();
		}
		var mainList = mObjectList.getMainList();
		foreach (var obj in mainList)
		{
			Collider collider = obj.Value.getCollider();
			if(collider != null)
			{
				colliders.Add(collider);
			}
		}
	}
	public void setVisible(bool visible, bool immediately, string param)
	{
		if (mScript == null || !mScriptInited)
		{
			return;
		}
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
			// 通知所有会接收布局隐藏的窗口
			foreach (var item in mObjectList.startForeach())
			{
				if (item.Value.isReceiveLayoutHide())
				{
					item.Value.onLayoutHide();
				}
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
		// 直接设置布局显示或隐藏
		mRoot.setActive(visible);
		// 通知所有会接收布局隐藏的窗口
		foreach (var item in mObjectList.startForeach())
		{
			if (item.Value.isReceiveLayoutHide())
			{
				item.Value.onLayoutHide();
			}
		}
	}
	public void notifyUIObjectNeedUpdate(myUIObject uiObj, bool needUpdate)
	{
		if (needUpdate)
		{
			if(!mNeedUpdateList.containsKey(uiObj.getID()))
			{
				mNeedUpdateList.add(uiObj.getID(), uiObj);
			}
		}
		else
		{
			mNeedUpdateList.remove(uiObj.getID());
		}
	}
	public void registerUIObject(myUIObject uiObj)
	{
		mObjectList.add(uiObj.getID(), uiObj);
		mGameObjectSearchList.Add(uiObj.getObject(), uiObj);
		if(mDefaultUpdateWindow || uiObj.isEnable())
		{
			mNeedUpdateList.add(uiObj.getID(), uiObj);
		}
	}
	public void unregisterUIObject(myUIObject uiObj)
	{
		mObjectList.remove(uiObj.getID());
		mNeedUpdateList.remove(uiObj.getID());
		mGameObjectSearchList.Remove(uiObj.getObject());
	}
	
	// 有节点删除或者增加,或者节点在当前父节点中的位置有改变,parent表示有变动的节点的父节点
	public void refreshUIDepth(myUIObject parent, bool ignoreInactive = false)
	{
		setUIDepth(parent, 0, false, ignoreInactive);
	}
	public myUIObject getUIObject(GameObject go)
	{
		mGameObjectSearchList.TryGetValue(go, out myUIObject obj);
		return obj;
	}
	// get
	public myUIObject getRoot()								{ return mRoot; }
	public LayoutScript getScript()							{ return mScript; }
	public LAYOUT_ORDER getRenderOrderType()				{ return mRenderOrderType; }
	public string getName()									{ return mName; }
	public int getID()										{ return mID; }
	public int getRenderOrder()								{ return mRenderOrder; }
	public int getDefaultLayer()							{ return mDefaultLayer; }
	public bool isVisible()									{ return mRoot.isActive(); }
	public bool isCheckBoxAnchor()							{ return mCheckBoxAnchor; }
	public bool isIgnoreTimeScale()							{ return mIgnoreTimeScale; }
	public bool canUIObjectUpdate(myUIObject uiObj)			{ return mNeedUpdateList.containsKey(uiObj.getID()); }
	public bool isScriptControlHide()						{ return mScriptControlHide; }
	public bool isBlurBack()								{ return mBlurBack; }
	public bool isAnchorApplied()							{ return mAnchorApplied; }
	public bool isInResources()								{ return mInResources; }
	// set
	public void setPrefab(GameObject prefab)				{ mPrefab = prefab; }
	public void setOrderType(LAYOUT_ORDER orderType)		{ mRenderOrderType = orderType; }
	// 设置是否会立即隐藏,应该由布局脚本调用
	public void setScriptControlHide(bool control)			{ mScriptControlHide = control; }
	public void setCheckBoxAnchor(bool check)				{ mCheckBoxAnchor = check; }
	public void setIgnoreTimeScale(bool ignore)				{ mIgnoreTimeScale = ignore; }
	public void setDefaultUpdateWindow(bool defaultUpdate)	{ mDefaultUpdateWindow = defaultUpdate; }
	public void setInResources(bool inResources)			{ mInResources = inResources; }
	public void setLayer(string layer)						{ setGameObjectLayer(mRoot.getObject(), layer); }
	public void setBlurBack(bool blurBack)					{ mBlurBack = blurBack; }
	public void setParent(myUIObject parent)				{ mParent = parent; }
	public void setID(int id)								{ mID = id; }
	public void setName(string name)						{ mName = name; }
	//------------------------------------------------------------------------------------------------------------------------------
	// ignoreInactive表示是否忽略未启用的节点,当includeSelf为true时orderInParent才会生效
	protected void setUIDepth(myUIObject window, int orderInParent, bool includeSelf = true, bool ignoreInactive = false)
	{
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
		// 根据面板上的顺序获取子节点,不再依赖myUIObject中存储的mChildList
		Transform transform = window.getTransform();
		int childOrder = 0;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			myUIObject child = getUIObject(transform.GetChild(i).gameObject);
			if (child == null)
			{
				continue;
			}
			setUIDepth(child, childOrder + 1, true, ignoreInactive);
			++childOrder;
		}
	}
}