using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static CSharpUtility;
using static FrameBase;
using static MathUtility;

// UI对象基类,此基类中不区分具体的UI插件,比如UGUI,NGUI等等
public class myUIObject : Transformable, IMouseEventCollect
{
	private static Comparison<myUIObject> mCompareSiblingIndex = compareSiblingIndex;   // 用于避免GC的委托
	private static bool mAllowDestroyWindow = false;            // 是否允许销毁窗口,仅此类内部使用
	protected COMWindowInteractive mCOMWindowInteractive;		// 鼠标键盘响应逻辑的组件
	protected COMWindowCollider mCOMWindowCollider;				// 碰撞逻辑组件
	protected HashSet<myUIObject> mChildSet;                    // 子节点列表,用于查询是否有子节点
	protected List<myUIObject> mChildList;                      // 子节点列表,与GameObject的子节点顺序保持一致(已排序情况下),用于获取最后一个子节点
	protected GameLayout mLayout;                               // 所属布局
	protected myUIObject mParent;                               // 父节点窗口
	protected Vector3 mLastWorldScale;                          // 上一次设置的世界缩放值
	protected int mID;                                          // 每个窗口的唯一ID
	protected bool mDestroyImmediately;                         // 销毁窗口时是否立即销毁
	protected bool mReceiveLayoutHide;                          // 布局隐藏时是否会通知此窗口,默认不通知
	protected bool mChildOrderSorted;                           // 子节点在列表中的顺序是否已经按照面板上的顺序排序了
	public myUIObject()
	{
		mID = makeID();
		mEnable = false;    // 出于效率考虑,窗口默认不启用更新,只有部分窗口和使用组件进行变化时才自动启用更新
		mDestroy = false;   // 由于一般myUIObject不会使用对象池来管理,所以构造时就设置当前对象为有效
	}
	public override void destroy()
	{
		if (!mAllowDestroyWindow)
		{
			logError("can not call window's destroy()! use destroyWindow(myUIObject window, bool destroyReally) instead");
		}
		base.destroy();
		mDestroy = true;
	}
	public static void collectChild<T>(myUIObject window, List<T> list) where T : myUIObject
	{
		if (window is T)
		{
			list.Add(window as T);
		}
		if (window.mChildList != null)
		{
			int childCount = window.mChildList.Count;
			for (int i = 0; i < childCount; ++i)
			{
				collectChild(window.mChildList[i], list);
			}
		}
	}
	public static void destroyWindow(myUIObject window, bool destroyReally)
	{
		if (window == null)
		{
			return;
		}
		// 先销毁所有子节点,因为遍历中会修改子节点列表,所以需要复制一个列表
		if (window.mChildList != null && window.mChildList.Count > 0)
		{
			using (new ListScope<myUIObject>(out var childList))
			{
				childList.AddRange(window.mChildList);
				int childCount = childList.Count;
				for (int i = 0; i < childCount; ++i)
				{
					destroyWindow(childList[i], destroyReally);
				}
			}
		}
		window.getParent()?.removeChild(window);
		// 再销毁自己
		destroyWindowSingle(window, destroyReally);
	}
	public static void destroyWindowSingle(myUIObject window, bool destroyReally)
	{
		mGlobalTouchSystem.unregisteCollider(window);
		if (window is IInputField)
		{
			mInputSystem.unregisteInputField(window as IInputField);
		}
		window.mLayout?.unregisterUIObject(window);
		window.mLayout = null;
		GameObject go = window.getObject();
		mAllowDestroyWindow = true;
		window.destroy();
		mAllowDestroyWindow = false;
		if (destroyReally)
		{
			destroyGameObject(go, window.mDestroyImmediately);
		}
	}
	public virtual void onLayoutHide() { }
	public virtual void init()
	{
		initComponents();
		mLayout?.registerUIObject(this);
		var boxCollider = mObject.GetComponent<BoxCollider>();
		if (boxCollider != null)
		{
			getCOMCollider().setBoxCollider(boxCollider);
		}
	}
	public void removeChild(myUIObject child)
	{
		if (mChildSet == null)
		{
			return;
		}
		if (mChildSet.Remove(child))
		{
			mChildList?.Remove(child);
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);

		ensureColliderSize();

		// 检测世界缩放值是否有变化
		if (!isVectorEqual(mLastWorldScale, getWorldScale()))
		{
			onWorldScaleChanged(mLastWorldScale);
			mLastWorldScale = getWorldScale();
		}
	}
	public void setAsLastSibling(bool refreshUIDepth = true)
	{
		mTransform.SetAsLastSibling();
		mChildOrderSorted = false;
		if (refreshUIDepth)
		{
			mLayout.refreshUIDepth(mParent, false);
		}
	}
	public void setAsFirstSibling(bool refreshUIDepth = true)
	{
		mTransform.SetAsFirstSibling();
		mChildOrderSorted = false;
		if (refreshUIDepth)
		{
			mLayout.refreshUIDepth(mParent, false);
		}
	}
	public void setSibling(int index, bool refreshUIDepth = true)
	{
		if (mTransform.GetSiblingIndex() == index)
		{
			return;
		}
		mTransform.SetSiblingIndex(index);
		mChildOrderSorted = false;
		if (refreshUIDepth)
		{
			mLayout.refreshUIDepth(mParent, false);
		}
	}
	// 当自适应更新完以后调用
	public virtual void notifyAnchorApply() { }
	// 获取描述,UI则返回所处布局名
	public string getDescription() { return mLayout?.getName(); }
	// get
	//------------------------------------------------------------------------------------------------------------------------------
	public int getID() { return mID; }
	public GameLayout getLayout() { return mLayout; }
	public List<myUIObject> getChildList() { return mChildList; }
	public virtual bool isReceiveScreenMouse() { return getCOMInteractive().getOnScreenMouseUp() != null; }
	public myUIObject getParent() { return mParent; }
	public override float getAlpha() { return 1.0f; }
	public virtual Color getColor() { return Color.white; }
	public UIDepth getDepth() { return getCOMInteractive().getDepth(); }
	public void setDepth(UIDepth parentDepth, int orderInParent) { getCOMInteractive().setDepth(parentDepth, orderInParent); }
	public virtual bool isHandleInput() { return mCOMWindowCollider != null && mCOMWindowCollider.isHandleInput(); }
	public virtual bool isPassRay() { return getCOMInteractive().isPassRay(); }
	public virtual bool isDragable() { return getComponent<COMWindowDrag>(true, false) != null; }
	public bool isMouseHovered() { return getCOMInteractive().isMouseHovered(); }
	public bool isDepthOverAllChild() { return getCOMInteractive().isDepthOverAllChild(); }
	public float getLongPressLengthThreshold() { return getCOMInteractive().getLongPressLengthThreshold(); }
	public bool isReceiveLayoutHide() { return mReceiveLayoutHide; }
	public bool isColliderForClick() { return getCOMInteractive().isColliderForClick(); }
	public bool isAllowGenerateDepth() { return getCOMInteractive().isAllowGenerateDepth(); }
	// 是否可以计算深度,与mAllowGenerateDepth类似,都是计算深度的其中一个条件,只不过这个可以由子类重写
	public virtual bool canGenerateDepth() { return true; }
	public virtual bool isCulled() { return false; }
	public override bool raycast(ref Ray ray, out RaycastHit hit, float maxDistance)
	{
		if (mCOMWindowCollider == null)
		{
			hit = new RaycastHit();
			return false;
		}
		return mCOMWindowCollider.raycast(ref ray, out hit, maxDistance);
	}
	public virtual float getFillPercent()
	{
		logError("can not get window fill percent with myUIObject");
		return 1.0f;
	}
	public virtual Vector2 getWindowSize(bool transformed = false)
	{
		logError("can not get window size with myUIObject");
		return Vector2.zero;
	}
	// 递归返回最后一个子节点,如果没有子节点,则返回空
	public myUIObject getLastChild()
	{
		if (mChildList == null || mChildList.Count == 0)
		{
			return null;
		}
		if (mChildList.Count > 1 && !mChildOrderSorted)
		{
			logError("子节点没有被排序,无法获得正确的最后一个子节点");
		}
		myUIObject lastChild = mChildList[mChildList.Count - 1];
		if (lastChild.mChildList.Count == 0)
		{
			return lastChild;
		}
		return lastChild.getLastChild();
	}
	// set
	//------------------------------------------------------------------------------------------------------------------------------
	public void setDepthOverAllChild(bool depthOver) { getCOMInteractive().setDepthOverAllChild(depthOver); }
	public void setDestroyImmediately(bool immediately) { mDestroyImmediately = immediately; }
	public void setAllowGenerateDepth(bool allowGenerate) { getCOMInteractive().setAllowGenerateDepth(allowGenerate); }
	public virtual void setAlpha(float alpha, bool fadeChild) { }
	public virtual void setColor(Color color) { }
	public virtual void setFillPercent(float percent) { logError("can not set window fill percent with myUIObject"); }
	public void setPassRay(bool passRay) { getCOMInteractive().setPassRay(passRay); }
	public virtual void setWindowSize(Vector2 size) { logError("can not set window size with myUIObject"); }
	public void setLongPressLengthThreshold(float threshold) { getCOMInteractive().setLongPressLengthThreshold(threshold); }
	// 自己调用的callback,仅在启用自定义输入系统时生效
	public void setPreClickCallback(ObjectPreClickCallback callback, object userData) { getCOMInteractive().setPreClickCallback(callback, userData); }
	public void setClickCallback(ObjectClickCallback callback) { getCOMInteractive().setClickCallback(callback); }
	public void setHoverCallback(ObjectHoverCallback callback) { getCOMInteractive().setHoverCallback(callback); }
	public void setPressCallback(ObjectPressCallback callback) { getCOMInteractive().setPressCallback(callback); }
	public void setDoubleClickCallback(ObjectDoubleClickCallback callback) { getCOMInteractive().setDoubleClickCallback(callback); }
	public void setOnReceiveDrag(OnReceiveDrag callback) { getCOMInteractive().setOnReceiveDrag(callback); }
	public void setOnDragHover(OnDragHover callback) { getCOMInteractive().setOnDragHover(callback); }
	public void setOnMouseEnter(OnMouseEnter callback) { getCOMInteractive().setOnMouseEnter(callback); }
	public void setOnMouseLeave(OnMouseLeave callback) { getCOMInteractive().setOnMouseLeave(callback); }
	public void setOnMouseDown(OnMouseDown callback) { getCOMInteractive().setOnMouseDown(callback); }
	public void setOnMouseUp(OnMouseUp callback) { getCOMInteractive().setOnMouseUp(callback); }
	public void setOnMouseMove(OnMouseMove callback) { getCOMInteractive().setOnMouseMove(callback); }
	public void setOnMouseStay(OnMouseStay callback) { getCOMInteractive().setOnMouseStay(callback); }
	public void setOnScreenMouseUp(OnScreenMouseUp callback) { getCOMInteractive().setOnScreenMouseUp(callback); }
	public void setLayout(GameLayout layout) { mLayout = layout; }
	public void setReceiveLayoutHide(bool receive) { mReceiveLayoutHide = receive; }
	public void setColliderForClick(bool forClick) { getCOMInteractive().setColliderForClick(forClick); }
	public override void setObject(GameObject go)
	{
		setName(go.name);
		base.setObject(go);
#if UNITY_EDITOR
		// 由于物体可能使克隆出来的,所以如果已经添加了调试组件,直接获取即可
		getUnityComponent<WindowDebug>().setWindow(this);
#endif
	}
	public void setParent(myUIObject parent, bool refreshDepth)
	{
		if (mParent == parent)
		{
			return;
		}
		// 从原来的父节点上移除
		mParent?.removeChild(this);
		// 设置新的父节点
		mParent = parent;
		if (mParent != null)
		{
			// 先设置Transform父节点,因为在addChild中会用到Transform的GetSiblingIndex
			if (mTransform.parent != mParent.getTransform())
			{
				mTransform.SetParent(mParent.getTransform());
			}
			mParent.addChild(this, refreshDepth);
		}
	}
	public virtual void setHandleInput(bool enable) { mCOMWindowCollider?.enableCollider(enable); }
	public void addLongPress(OnLongPress callback, OnLongPressing pressingCallback, float pressTime)
	{
		getCOMInteractive().addLongPress(callback, pressingCallback, pressTime);
	}
	public void removeLongPress(OnLongPress callback)
	{
		getCOMInteractive().removeLongPress(callback);
	}
	public void clearLongPress()
	{
		getCOMInteractive().clearLongPress();
	}
	// callback
	//------------------------------------------------------------------------------------------------------------------------------
	public virtual void onMouseEnter(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onMouseEnter(mousePos, touchID);
	}
	public virtual void onMouseLeave(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onMouseLeave(mousePos, touchID);
	}
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onMouseDown(mousePos, touchID);
	}
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onMouseUp(mousePos, touchID);
	}
	// 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		getCOMInteractive().onMouseMove(mousePos, moveDelta, moveTime, touchID);
	}
	// 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public virtual void onMouseStay(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onMouseStay(mousePos, touchID);
	}
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onScreenMouseUp(mousePos, touchID);
	}
	// 鼠标在屏幕上按下
	public virtual void onScreenMouseDown(Vector3 mousePos, int touchID) 
	{
		getCOMInteractive().onScreenMouseDown(mousePos, touchID);
	}
	// 有物体拖动到了当前窗口上
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, BOOL continueEvent)
	{
		getCOMInteractive().onReceiveDrag(dragObj, mousePos, continueEvent);
	}
	// 有物体拖动到了当前窗口上
	public virtual void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
	{
		getCOMInteractive().onDragHoverd(dragObj, mousePos, hover);
	}
	public void sortChild()
	{
		if (mChildList == null || mChildList.Count <= 1)
		{
			return;
		}
		mChildOrderSorted = true;
		quickSort(mChildList, mCompareSiblingIndex);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void ensureColliderSize() { }
	protected virtual void onWorldScaleChanged(Vector2 lastWorldScale) { }
	protected void addChild(myUIObject child, bool refreshDepth)
	{
		if (mChildSet == null)
		{
			mChildSet = new HashSet<myUIObject>();
		}
		if (mChildSet.Contains(child))
		{
			return;
		}
		if (mChildList == null)
		{
			mChildList = new List<myUIObject>();
		}
		mChildList.Add(child);
		mChildSet.Add(child);
		mChildOrderSorted = false;
		if (refreshDepth)
		{
			mLayout.refreshUIDepth(this, false);
		}
	}
	protected static int compareSiblingIndex(myUIObject child0, myUIObject child1)
	{
		return sign(child0.getTransform().GetSiblingIndex() - child1.getTransform().GetSiblingIndex());
	}
	protected COMWindowInteractive getCOMInteractive()
	{
		if (mCOMWindowInteractive == null)
		{
			mCOMWindowInteractive = addComponent<COMWindowInteractive>(true);
		}
		return mCOMWindowInteractive;
	}
	protected COMWindowCollider getCOMCollider()
	{
		if (mCOMWindowCollider == null)
		{
			mCOMWindowCollider = addComponent<COMWindowCollider>(true);
		}
		return mCOMWindowCollider;
	}
}