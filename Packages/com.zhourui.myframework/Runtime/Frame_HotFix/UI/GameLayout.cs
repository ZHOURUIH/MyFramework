using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static WidgetUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 用于表示一个布局
// 对于布局prefab原本就引用的资源,可以不用关心在代码中的引用
public class GameLayout
{
	protected Dictionary<int, myUGUIObject> mGameObjectSearchList = new();	// 用于根据GameObject查找UI,key是GameObject的InstanceID
	protected SafeList<myUGUIObject> mNeedUpdateList = new();				// mObjectList中需要更新的窗口列表
	protected SafeDictionary<int, myUGUIObject> mObjectList = new();		// 布局中UI物体列表,用于保存所有已获取的UI
	protected myUGUICanvas mRoot;					// 布局根节点
	protected LayoutScript mScript;					// 布局脚本
	protected myUGUIObject mParent;					// 布局父节点,可能是UGUIRoot,也可能为空
	protected ResourceRef<GameObject> mPrefab;      // 布局预设,布局从该预设实例化
	protected Type mType;							// 布局的脚本类型
	protected string mName;							// 布局名称
	protected int mDefaultLayer;					// 布局加载时所处的层
	protected int mRenderOrder;						// 渲染顺序,越大则渲染优先级越高,不能小于0
	protected bool mDefaultUpdateWindow = true;		// 是否默认就将所有注册的窗口添加到更新列表中,默认是添加的,在某些需要重点优化的布局中可以选择将哪些窗口放入更新列表
	protected bool mScriptControlHide;				// 是否由脚本来控制隐藏
	protected bool mIgnoreTimeScale;				// 更新布局时是否忽略时间缩放
	protected bool mCheckBoxAnchor = true;			// 是否检查布局中所有带碰撞盒的窗口是否自适应分辨率
	protected bool mAnchorApplied;					// 是否已经完成了自适应的调整
	protected bool mScriptInited;					// 脚本是否已经初始化
	protected bool mBlurBack;						// 布局显示时是否需要使布局背后(比当前布局层级低)的所有布局模糊显示
	protected LAYOUT_ORDER mRenderOrderType;		// 布局渲染顺序的计算方式
	public void init()
	{
		mScript = mLayoutManager.createScript(this);
		if (mScript == null)
		{
			logError("can not create layout script!, type:" + mType);
			return;
		}
		mLayoutManager.notifyLayoutChanged(this);

		// 初始化布局脚本
		mScript.newObject(out mRoot, mParent, mName);
		
		// 去除自带的锚点
		// 在unity2020中,不知道为什么实例化以后的RectTransform的大小会自动变为视图窗口大小,为了适配计算正确,这里需要重置一次
		RectTransform rectTransform = mRoot.getRectTransform();
		rectTransform.anchorMin = Vector2.one * 0.5f;
		rectTransform.anchorMax = Vector2.one * 0.5f;
		setRectSize(rectTransform, FrameSettings.getUISize());

		mRoot.setDestroyImmediately(true);
		mDefaultLayer = mRoot.getGameObject().layer;
		mScript.setRoot(mRoot);
		mScript.assignWindow();
		// 布局实例化完成,初始化之前,需要调用自适应组件的更新
		if (mLayoutManager.isUseAnchor())
		{
			applyAnchor(mRoot.getGameObject(), true, this);
		}
		mAnchorApplied = true;
		mScript.init();
		mScript.postInit();
		// init后再次设置布局的渲染顺序,这样可以在此处刷新所有窗口的深度,因为是否刷新跟是否注册了碰撞体有关
		// 所以在assignWindow和init中不需要在创建窗口对象时刷新深度,这样会造成很大的性能浪费
		setRenderOrder(mRenderOrder);
		mScriptInited = true;
		// 加载完布局后强制隐藏
		setVisibleForce(false);
		if (isEditor())
		{
			mRoot.getOrAddUnityComponent<LayoutDebug>().setLayout(this);
		}
	}
	public void update(float elapsedTime)
	{
		if (!isVisible() || mScript == null || !mScriptInited)
		{
			return;
		}

		if (mIgnoreTimeScale)
		{
			elapsedTime = mGameFrameworkHotFix.getUnscaledTime();
		}

		// 更新所有的UI物体
		if (mNeedUpdateList.count() > 0)
		{
			using var a = new ProfilerScope("UpdateLayout");
			using var c = new SafeListReader<myUGUIObject>(mNeedUpdateList);
			foreach (myUGUIObject uiObj in c.mReadList)
			{
				if (uiObj.canUpdate())
				{
					uiObj.update(uiObj.isIgnoreTimeScale() ? mGameFrameworkHotFix.getUnscaledTime() : elapsedTime);
				}
			}
		}

		// 更新脚本逻辑
		using var b = new ProfilerScope("UpdateScript");
		mScript.updateAllDragView();
		if (mScript.isNeedUpdate())
		{
			mScript.update(elapsedTime);
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
		if (isVisible() && mScript != null && mScriptInited)
		{
			mScript.lateUpdate(elapsedTime);
		}
	}
	public void destroy()
	{
		if (mScript != null)
		{
			mScript.destroy();
			mScript = null;
			mLayoutManager.notifyLayoutChanged(this);
		}
		myUGUIObject.destroyWindow(mRoot, true);
		mRoot = null;
		mResourceManager.unload(ref mPrefab);
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
		mObjectList.forValue(obj => colliders.addNotNull(obj.getCollider()));
	}
	public void setVisible(bool visible)
	{
		if (mRoot == null || mScript == null || !mScriptInited || visible == mRoot.isActiveInHierarchy())
		{
			return;
		}
		// 显示布局时立即显示
		if (visible)
		{
			mRoot.setActive(visible);
			mScript.onGameState();
		}
		// 隐藏布局时需要判断
		else
		{
			// 通知所有会接收布局隐藏的窗口
			using var a = new SafeDictionaryReader<int, myUGUIObject>(mObjectList);
			foreach (var item in a.mReadList)
			{
				if (item.Value.isReceiveLayoutHide())
				{
					item.Value.onLayoutHide();
				}
			}
			mScript.onHide();
			if (!mScriptControlHide)
			{
				mRoot.setActive(visible);
			}
		}
	}
	public void setVisibleForce(bool visible)
	{
		if (mScript == null || !mScriptInited || visible == mRoot.isActiveInHierarchy())
		{
			return;
		}
		// 直接设置布局显示或隐藏
		mRoot.setActive(visible);
		// 通知所有会接收布局隐藏的窗口
		using var a = new SafeDictionaryReader<int, myUGUIObject>(mObjectList);
		foreach (var item in a.mReadList)
		{
			if (item.Value.isReceiveLayoutHide())
			{
				item.Value.onLayoutHide();
			}
		}
	}
	public void notifyUIObjectNeedUpdate(myUGUIObject uiObj, bool needUpdate)
	{
		mNeedUpdateList.addOrRemove(uiObj, needUpdate);
	}
	public void registerUIObject(myUGUIObject uiObj)
	{
		mObjectList.add(uiObj.getID(), uiObj);
		if (mGameObjectSearchList.TryGetValue(uiObj.getGameObjectInstanceID(), out myUGUIObject obj))
		{
			logError("两个UI窗口的GameObject实例ID一致,UI窗口对象相同:" + (uiObj != obj) + ",GameObject是否相同：" + (obj.getGameObject() == uiObj.getGameObject()) + ", obj name:" + obj.getName() + ", uiObj name:" + uiObj.getName());
		}
		mGameObjectSearchList.Add(uiObj.getGameObjectInstanceID(), uiObj);
		mNeedUpdateList.addIf(uiObj, mDefaultUpdateWindow || uiObj.isNeedUpdate());
	}
	public void unregisterUIObject(myUGUIObject uiObj)
	{
		mObjectList.remove(uiObj.getID());
		mNeedUpdateList.remove(uiObj);
		mGameObjectSearchList.Remove(uiObj.getGameObjectInstanceID());
	}
	// 有节点删除或者增加,或者节点在当前父节点中的位置有改变,parent表示有变动的节点的父节点
	public void refreshUIDepth(myUGUIObject parent, bool ignoreInactive)
	{
		if (ignoreInactive && !parent.isActiveInHierarchy())
		{
			logWarning("要刷新的节点没有被激活,且设置了忽略未激活节点,将不会刷新任何节点的深度");
		}
		setUIDepth(parent, 0, false, ignoreInactive);
	}
	// get
	public myUGUIObject getUIObject(GameObject go)			{ return mGameObjectSearchList.get(getGameObjectID(go)); }
	public Dictionary<int, myUGUIObject> getUIObjectList()	{ return mGameObjectSearchList; }
	public myUGUICanvas getRoot()							{ return mRoot; }
	public LayoutScript getScript()							{ return mScript; }
	public LAYOUT_ORDER getRenderOrderType()				{ return mRenderOrderType; }
	public string getName()									{ return mName; }
	public Type getType()									{ return mType; }
	public int getRenderOrder()								{ return mRenderOrder; }
	public int getDefaultLayer()							{ return mDefaultLayer; }
	public bool isVisible()									{ return mRoot != null && mRoot.isActiveInHierarchy(); }
	public bool isCheckBoxAnchor()							{ return mCheckBoxAnchor; }
	public bool isIgnoreTimeScale()							{ return mIgnoreTimeScale; }
	public bool canUIObjectUpdate(myUGUIObject uiObj)		{ return mNeedUpdateList.contains(uiObj); }
	public bool isScriptControlHide()						{ return mScriptControlHide; }
	public bool isBlurBack()								{ return mBlurBack; }
	public bool isAnchorApplied()							{ return mAnchorApplied; }
	// set
	public void setPrefab(ResourceRef<GameObject> prefab)	{ mPrefab = prefab; }
	public void setOrderType(LAYOUT_ORDER orderType)		{ mRenderOrderType = orderType; }
	// 设置是否会立即隐藏,应该由布局脚本调用
	public void setScriptControlHide(bool control)			{ mScriptControlHide = control; }
	public void setCheckBoxAnchor(bool check)				{ mCheckBoxAnchor = check; }
	public void setIgnoreTimeScale(bool ignore)				{ mIgnoreTimeScale = ignore; }
	public void setDefaultUpdateWindow(bool defaultUpdate)	{ mDefaultUpdateWindow = defaultUpdate; }
	public void setLayer(int layer)							{ setGameObjectLayer(mRoot.getGameObject(), layer); }
	public void setBlurBack(bool blurBack)					{ mBlurBack = blurBack; }
	public void setParent(myUGUIObject parent)				{ mParent = parent; }
	public void setType(Type type)							{ mType = type; }
	public void setName(string name)						{ mName = name; }
	//------------------------------------------------------------------------------------------------------------------------------
	// ignoreInactive表示是否忽略未启用的节点,当includeSelf为true时orderInParent才会生效
	protected void setUIDepth(myUGUIObject window, int orderInParent, bool includeSelf = true, bool ignoreInactive = false)
	{
		if ((ignoreInactive && !window.isActive()) || !window.isAllowGenerateDepth())
		{
			return;
		}
		// 编辑器下检查是否希望计算窗口深度,但是由于未注册碰撞体而无法计算,由于确实存在一些有碰撞体,但是不需要参与射线检测的窗口,比如各种template
		// 而一般情况下template都是未启用的状态,所以此处只检查启用的窗口
		// 此处无法完全确定窗口上的碰撞体的真实用途,所以需要由窗口自己指出其用途,此处也只检测用于鼠标点击的窗口碰撞体
		if (isEditor() &&
			includeSelf &&
			window.isActiveInHierarchy() &&
			window.getCollider() != null &&
			window.isColliderForClick() &&
			!mGlobalTouchSystem.isColliderRegisted(window))
		{
			logError("窗口拥有碰撞体,但是由于未注册碰撞体,所以无法为窗口计算深度,Layout:" + getName() + ", Window:" + getGameObjectPath(window.getGameObject()));
		}
		// 先设置当前窗口的深度
		// 只有当拥有子节点或者已经注册碰撞体时,或者拥有Canvas组件,才会计算窗口的深度
		if (includeSelf)
		{
			// 当有Canvas组件时,前面所有父节点的深度都会被忽略,从头开始计算深度
			Canvas canvas = window.getCanvas();
			if (canvas != null)
			{
				window.setDepth(null, canvas.sortingOrder);
			}
			else if (window.getChildCount() > 0 || mGlobalTouchSystem.isColliderRegisted(window))
			{
				if (window.getParent() == null)
				{
					logError("有窗口的父节点为空,name:" + window.getName());
				}
				window.setDepth(window != mRoot ? window.getParent().getDepth() : null, orderInParent);
			}
		}

		// 再设置子窗口的深度,子节点在父节点中的下标从1开始,如果从0开始,则会与默认值的0混淆
		// 需要排序保证子节点的顺序正确
		int childOrder = 0;
		window.sortChild();
		var childList = window.getChildList();
		if (childList == null)
		{
			return;
		}
		for (int i = 0; i < childList.Count; ++i)
		{
			setUIDepth(childList[i], ++childOrder, true, ignoreInactive);
		}
	}
}