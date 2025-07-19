using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static FrameBaseHotFix;
using static MathUtility;
using static FrameBaseUtility;

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
	protected bool mIsNewObject;								// 是否是从空的GameObject创建的,一般都是会确认已经存在了对应组件,而不是要动态添加组件
	public myUIObject()
	{
		mID = makeID();
		mNeedUpdate = false;    // 出于效率考虑,窗口默认不启用更新,只有部分窗口和使用组件进行变化时才自动启用更新
		mDestroy = false;		// 由于一般myUIObject不会使用对象池来管理,所以构造时就设置当前对象为有效
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
	public override void setActive(bool active)
	{
		if (active == isActive())
		{
			return;
		}
		base.setActive(active);
		mGlobalTouchSystem?.notifyWindowActiveChanged();
	}
	public static void collectChild<T>(myUIObject window, List<T> list) where T : myUIObject
	{
		list.addNotNull(window as T);
		foreach (myUIObject item in window.mChildList.safe())
		{
			collectChild(item, list);
		}
	}
	public static void destroyWindow(myUIObject window, bool destroyReally)
	{
		if (window == null)
		{
			return;
		}
		// 先销毁所有子节点,因为遍历中会修改子节点列表,所以需要复制一个列表
		if (!window.mChildList.isEmpty())
		{
			using var a = new ListScope<myUIObject>(out var childList, window.mChildList);
			foreach (myUIObject item in childList)
			{
				destroyWindow(item, destroyReally);
			}
		}
		window.getParent()?.removeChild(window);
		// 再销毁自己
		destroyWindowSingle(window, destroyReally);
	}
	public static void destroyWindowSingle(myUIObject window, bool destroyReally)
	{
		mGlobalTouchSystem?.unregisteCollider(window);
		if (window is IInputField inputField)
		{
			mInputSystem?.unregisteInputField(inputField);
		}
		window.mLayout?.unregisterUIObject(window);
		GameObject go = window.getObject();
		mAllowDestroyWindow = true;
		window.destroy();
		mAllowDestroyWindow = false;
		window.mLayout = null;
		if (destroyReally)
		{
			destroyUnityObject(go, window.mDestroyImmediately);
		}
	}
	public virtual void onLayoutHide() { }
	public virtual void init()
	{
		initComponents();
		mLayout?.registerUIObject(this);
		if (mObject.TryGetComponent<BoxCollider>(out var boxCollider))
		{
			getCOMCollider().setBoxCollider(boxCollider);
		}
	}
	public void removeChild(myUIObject child)
	{
		if (mChildSet != null && mChildSet.Remove(child))
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
	public override Collider getCollider(bool addIfNotExist = false)
	{
		var collider = tryGetUnityComponent<Collider>();
		// 由于Collider无法直接添加到GameObject上,所以只能默认添加BoxCollider
		if (addIfNotExist && collider == null)
		{
			collider = getOrAddUnityComponent<BoxCollider>();
			getCOMCollider().setBoxCollider(collider as BoxCollider);
			// 新加的碰撞盒需要设置大小
			ensureColliderSize();
		}
		return collider;
	}
	public void setAsLastSibling(bool refreshUIDepth = true)
	{
		mTransform.SetAsLastSibling();
		mChildOrderSorted = false;
		if (refreshUIDepth)
		{
			mLayout.refreshUIDepth(mParent, true);
		}
	}
	public void setAsFirstSibling(bool refreshUIDepth = true)
	{
		mTransform.SetAsFirstSibling();
		mChildOrderSorted = false;
		if (refreshUIDepth)
		{
			mLayout.refreshUIDepth(mParent, true);
		}
	}
	public int getSibling() { return mTransform.GetSiblingIndex(); }
	public bool setSibling(int index, bool refreshUIDepth = true)
	{
		if (mTransform.GetSiblingIndex() == index)
		{
			return false;
		}
		mTransform.SetSiblingIndex(index);
		mChildOrderSorted = false;
		if (refreshUIDepth)
		{
			mLayout.refreshUIDepth(mParent, true);
		}
		return true;
	}
	// 当自适应更新完以后调用
	public virtual void notifyAnchorApply() { }
	// 获取描述,UI则返回所处布局名
	public string getDescription() { return mLayout?.getName(); }
	// get
	//------------------------------------------------------------------------------------------------------------------------------
	public int getID()							{ return mID; }
	public GameLayout getLayout()				{ return mLayout; }
	public List<myUIObject> getChildList()		{ return mChildList; }
	public virtual bool isReceiveScreenMouse()	{ return mCOMWindowInteractive?.getOnScreenMouseUp() != null; }
	public myUIObject getParent()				{ return mParent; }
	public override float getAlpha()			{ return 1.0f; }
	public virtual Color getColor()				{ return Color.white; }
	public UIDepth getDepth()					{ return getCOMInteractive().getDepth(); }
	public virtual bool isHandleInput()			{ return mCOMWindowCollider != null && mCOMWindowCollider.isHandleInput(); }
	public virtual bool isPassRay()				{ return mCOMWindowInteractive == null || mCOMWindowInteractive.isPassRay(); }
	public virtual bool isPassDragEvent()		{ return !isDragable() || (mCOMWindowInteractive != null && mCOMWindowInteractive.isPassDragEvent()); }
	public virtual bool isDragable()			{ return getActiveComponent<COMWindowDrag>() != null; }
	public bool isMouseHovered()				{ return mCOMWindowInteractive != null && mCOMWindowInteractive.isMouseHovered(); }
	public int getClickSound()					{ return mCOMWindowInteractive?.getClickSound() ?? 0; }
	public bool isDepthOverAllChild()			{ return mCOMWindowInteractive != null && mCOMWindowInteractive.isDepthOverAllChild(); }
	public float getLongPressLengthThreshold()	{ return mCOMWindowInteractive?.getLongPressLengthThreshold() ?? -1.0f; }
	public bool isReceiveLayoutHide()			{ return mReceiveLayoutHide; }
	public bool isColliderForClick()			{ return mCOMWindowInteractive != null && mCOMWindowInteractive.isColliderForClick(); }
	public bool isAllowGenerateDepth()			{ return mCOMWindowInteractive == null || mCOMWindowInteractive.isAllowGenerateDepth(); }
	// 是否可以计算深度,与mAllowGenerateDepth类似,都是计算深度的其中一个条件,只不过这个可以由子类重写
	public virtual bool canGenerateDepth() { return true; }
	public virtual bool isCulled() { return false; }
	public override bool raycastSelf(ref Ray ray, out RaycastHit hit, float maxDistance)
	{
		if (mCOMWindowCollider == null)
		{
			hit = new();
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
		if (mChildList.isEmpty())
		{
			return null;
		}
		if (mChildList.Count > 1 && !mChildOrderSorted)
		{
			logError("子节点没有被排序,无法获得正确的最后一个子节点");
		}
		myUIObject lastChild = mChildList[^1];
		if (lastChild.mChildList.Count == 0)
		{
			return lastChild;
		}
		return lastChild.getLastChild();
	}
	// set
	//------------------------------------------------------------------------------------------------------------------------------
	public void setDepthOverAllChild(bool depthOver)					{ getCOMInteractive().setDepthOverAllChild(depthOver); }
	public void setDestroyImmediately(bool immediately)					{ mDestroyImmediately = immediately; }
	public void setAllowGenerateDepth(bool allowGenerate)				{ getCOMInteractive().setAllowGenerateDepth(allowGenerate); }
	public virtual void setAlpha(float alpha, bool fadeChild)			{ }
	public virtual void setColor(Color color)							{ }
	public virtual void setFillPercent(float percent)					{ logError("can not set window fill percent with myUIObject"); }
	public void setPassRay(bool passRay)								{ getCOMInteractive().setPassRay(passRay); }
	public void setPassDragEvent(bool pass)								{ getCOMInteractive().setPassDragEvent(pass); }
	public void setDepth(UIDepth parentDepth, int orderInParent)		{ getCOMInteractive().setDepth(parentDepth, orderInParent); }
	public virtual void setWindowSize(Vector2 size)						{ logError("can not set window size with myUIObject"); }
	public void setLongPressLengthThreshold(float threshold)			{ getCOMInteractive().setLongPressLengthThreshold(threshold); }
	// 自己调用的callback,仅在启用自定义输入系统时生效
	public void setPreClickCallback(Action callback)					{ getCOMInteractive().setPreClickCallback(callback); }
	public void setPreClickDetailCallback(ClickCallback callback)		{ getCOMInteractive().setPreClickDetailCallback(callback); }
	public void setClickCallback(Action callback)						{ getCOMInteractive().setClickCallback(callback); }
	public void setClickDetailCallback(ClickCallback callback)			{ getCOMInteractive().setClickDetailCallback(callback); }
	public void setHoverCallback(BoolCallback callback)					{ getCOMInteractive().setHoverCallback(callback); }
	public void setHoverDetailCallback(HoverCallback callback)			{ getCOMInteractive().setHoverDetailCallback(callback); }
	public void setPressCallback(BoolCallback callback)					{ getCOMInteractive().setPressCallback(callback); }
	public void setPressDetailCallback(PressCallback callback)			{ getCOMInteractive().setPressDetailCallback(callback); }
	public void setDoubleClickCallback(Action callback)					{ getCOMInteractive().setDoubleClickCallback(callback); }
	public void setDoubleClickDetailCallback(ClickCallback callback)	{ getCOMInteractive().setDoubleClickDetailCallback(callback); }
	public void setOnReceiveDrag(OnReceiveDrag callback)				{ getCOMInteractive().setOnReceiveDrag(callback); }
	public void setOnDragHover(OnDragHover callback)					{ getCOMInteractive().setOnDragHover(callback); }
	public void setOnMouseEnter(OnMouseEnter callback)					{ getCOMInteractive().setOnMouseEnter(callback); }
	public void setOnMouseLeave(OnMouseLeave callback)					{ getCOMInteractive().setOnMouseLeave(callback); }
	public void setOnMouseDown(Vector3IntCallback callback)				{ getCOMInteractive().setOnMouseDown(callback); }
	public void setOnMouseUp(Vector3IntCallback callback)				{ getCOMInteractive().setOnMouseUp(callback); }
	public void setOnMouseMove(OnMouseMove callback)					{ getCOMInteractive().setOnMouseMove(callback); }
	public void setOnMouseStay(Vector3IntCallback callback)				{ getCOMInteractive().setOnMouseStay(callback); }
	public void setOnScreenMouseUp(OnScreenMouseUp callback)			{ getCOMInteractive().setOnScreenMouseUp(callback); }
	public void setLayout(GameLayout layout)							{ mLayout = layout; }
	public void setReceiveLayoutHide(bool receive)						{ mReceiveLayoutHide = receive; }
	public void setColliderForClick(bool forClick)						{ getCOMInteractive().setColliderForClick(forClick); }
	public void setClickSound(int sound)								{ getCOMInteractive().setClickSound(sound); }
	public override void setObject(GameObject go)
	{
		setName(go.name);
		base.setObject(go);
		if (isEditor())
		{
			// 由于物体可能使克隆出来的,所以如果已经添加了调试组件,直接获取即可
			getOrAddUnityComponent<WindowDebug>().setWindow(this);
		}
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
	public void addLongPress(Action callback, float pressTime, FloatCallback pressingCallback = null)
	{
		getCOMInteractive().addLongPress(callback, pressTime, pressingCallback);
	}
	public void removeLongPress(Action callback) { mCOMWindowInteractive?.removeLongPress(callback); }
	public void clearLongPress() { mCOMWindowInteractive?.clearLongPress(); }
	// callback
	//------------------------------------------------------------------------------------------------------------------------------
	public virtual void onMouseEnter(Vector3 mousePos, int touchID) { mCOMWindowInteractive?.onMouseEnter(mousePos, touchID); }
	public virtual void onMouseLeave(Vector3 mousePos, int touchID) { mCOMWindowInteractive?.onMouseLeave(mousePos, touchID); }
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos, int touchID) { mCOMWindowInteractive?.onMouseDown(mousePos, touchID); }
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos, int touchID) { mCOMWindowInteractive?.onMouseUp(mousePos, touchID); }
	// 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mCOMWindowInteractive?.onMouseMove(mousePos, moveDelta, moveTime, touchID);
	}
	// 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public virtual void onMouseStay(Vector3 mousePos, int touchID) { mCOMWindowInteractive?.onMouseStay(mousePos, touchID); }
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos, int touchID) { mCOMWindowInteractive?.onScreenMouseUp(mousePos, touchID); }
	// 鼠标在屏幕上按下
	public virtual void onScreenMouseDown(Vector3 mousePos, int touchID)  { mCOMWindowInteractive?.onScreenMouseDown(mousePos, touchID); }
	// 有物体拖动到了当前窗口上
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent)
	{
		mCOMWindowInteractive?.onReceiveDrag(dragObj, mousePos, ref continueEvent);
	}
	// 有物体拖动到了当前窗口上
	public virtual void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover)
	{
		mCOMWindowInteractive?.onDragHoverd(dragObj, mousePos, hover);
	}
	public void sortChild()
	{
		if (mChildList.count() <= 1)
		{
			return;
		}
		mChildOrderSorted = true;
		quickSort(mChildList, mCompareSiblingIndex);
	}
	// registeEvent,这些函数只是用于简化注册碰撞体的操作
	public void registeCollider(Action clickCallback, Action preClick, bool passRay = false)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickCallback(clickCallback);
		setPreClickCallback(preClick);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	// 用于接收GlobalTouchSystem处理的输入事件
	public void registeCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(passRay);
		// 由碰撞体的窗口都需要启用更新,以便可以保证窗口大小与碰撞体大小一致
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeCollider(this, camera);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback)
	{
		mGlobalTouchSystem.registeCollider(this);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeCollider(ClickCallback clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickDetailCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeCollider(Action clickCallback, bool passRay, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeCollider(ClickCallback clickCallback, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickDetailCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeCollider(Action clickCallback)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeCollider(this, camera);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(ClickCallback clickCallback)
	{
		mGlobalTouchSystem.registeCollider(this);
		setClickDetailCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(ClickCallback clickCallback, GameCamera camera)
	{
		setClickDetailCallback(clickCallback);
		mGlobalTouchSystem.registeCollider(this, camera);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeCollider()
	{
		mGlobalTouchSystem.registeCollider(this);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void unregisteCollider()
	{
		mGlobalTouchSystem?.unregisteCollider(this);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void ensureColliderSize() { }
	protected virtual void onWorldScaleChanged(Vector2 lastWorldScale) { }
	protected void addChild(myUIObject child, bool refreshDepth)
	{
		mChildSet ??= new();
		if (!mChildSet.Add(child))
		{
			return;
		}
		mChildList ??= new();
		mChildList.Add(child);
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
		return mCOMWindowInteractive ??= addComponent<COMWindowInteractive>(true);
	}
	protected COMWindowCollider getCOMCollider()
	{
		return mCOMWindowCollider ??= addComponent<COMWindowCollider>(true);
	}
	protected void setObjectCallback(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback)
	{
		setClickCallback(clickCallback);
		setPressCallback(pressCallback);
		setHoverCallback(hoverCallback);
	}
}