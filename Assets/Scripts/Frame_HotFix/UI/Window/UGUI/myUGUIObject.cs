using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityUtility;
using static MathUtility;
using static WidgetUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// UGUI窗口的基类
public class myUGUIObject : Transformable, IMouseEventCollect
{
	protected static Comparison<Transform> mCompareDescend = compareZDecending;				// 避免GC的回调
	private static Comparison<myUGUIObject> mCompareSiblingIndex = compareSiblingIndex;		// 用于避免GC的委托
	private static bool mAllowDestroyWindow = false;				// 是否允许销毁窗口,仅此类内部使用
	protected ComponentInteractive mCOMWindowInteractive;			// 鼠标键盘响应逻辑的组件
	protected COMWindowCollider mCOMWindowCollider;					// 碰撞逻辑组件
	protected HashSet<myUGUIObject> mChildSet;						// 子节点列表,用于查询是否有子节点
	protected List<myUGUIObject> mChildList;						// 子节点列表,与GameObject的子节点顺序保持一致(已排序情况下),用于获取最后一个子节点
	protected GameLayout mLayout;									// 所属布局
	protected myUGUIObject mParent;									// 父节点窗口
	protected Vector3 mLastWorldScale;								// 上一次设置的世界缩放值
	protected int mID;												// 每个窗口的唯一ID
	protected bool mDestroyImmediately;								// 销毁窗口时是否立即销毁
	protected bool mReceiveLayoutHide;								// 布局隐藏时是否会通知此窗口,默认不通知
	protected bool mChildOrderSorted;								// 子节点在列表中的顺序是否已经按照面板上的顺序排序了
	protected bool mIsNewObject;									// 是否是从空的GameObject创建的,一般都是会确认已经存在了对应组件,而不是要动态添加组件
	protected COMWindowUGUIInteractive mCOMWindowUGUIInteractive;	// UGUI的鼠标键盘响应逻辑的组件
	protected RectTransform mRectTransform;                         // UGUI的Transform
	public myUGUIObject()
	{
		mID = makeID();
		mNeedUpdate = false;    // 出于效率考虑,窗口默认不启用更新,只有部分窗口和使用组件进行变化时才自动启用更新
		mDestroy = false;       // 由于一般myUGUIObject不会使用对象池来管理,所以构造时就设置当前对象为有效
	}
	public virtual void init()
	{
		initComponents();
		mLayout?.registerUIObject(this);
		if (mObject.TryGetComponent<BoxCollider>(out var boxCollider))
		{
			getCOMCollider().setBoxCollider(boxCollider);
		}
		// 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
		if (!mObject.TryGetComponent(out mRectTransform))
		{
			mRectTransform = mObject.AddComponent<RectTransform>();
		}
		mTransform = mRectTransform;
		if (mRectTransform == null)
		{
			if (mTransform != null)
			{
				logError("Transform不是RectTransform,name:" + mTransform.name);
			}
			else
			{
				logError("RectTransform为空");
			}
		}
		mCOMWindowCollider?.setColliderSize(mRectTransform);
	}
	public void onLayoutHide() 
	{
		// 布局隐藏时需要将触点清除
		mCOMWindowUGUIInteractive?.clearMousePointer();
	}
	// 将当前窗口的顶部对齐父节点的顶部,只改Y坐标
	public void setTopToParentTop()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowTopInParent(uiObj.getWindowTopInSelf());
	}
	// 将当前窗口的顶部中心对齐父节点的顶部中心,X和Y坐标都改
	public void setTopCenterToParentTopCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowTopInParent(uiObj.getWindowTopInSelf());
		setWindowInParentCenterX();
	}
	// 将当前窗口的底部对齐父节点的底部,只改变Y坐标
	public void setBottomToParentBottom()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowBottomInParent(uiObj.getWindowBottomInSelf());
	}
	// 将当前窗口的底部中心对齐父节点的底部中心,X和Y坐标都改
	public void setBottomCenterToParentBottomCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowBottomInParent(uiObj.getWindowBottomInSelf());
		setWindowInParentCenterX();
	}
	// 将当前窗口的左边界对齐父节点的左边界.只改X坐标
	public void setLeftToParentLeft()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowLeftInParent(uiObj.getWindowLeftInSelf());
	}
	// 将当前窗口的左边界中心对齐父节点的左边界中心,X和Y坐标都改
	public void setLeftCenterToParentLeftCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowLeftInParent(uiObj.getWindowLeftInSelf());
		setWindowInParentCenterY();
	}
	// 将当前窗口的右边界对齐父节点的右边界,只改X坐标
	public void setRightToParentRight()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowRightInParent(uiObj.getWindowRightInSelf());
	}
	// 将当前窗口的右边界中心对齐父节点的右边界中心,X和Y坐标都改
	public void setRightCenterToParentRightCenter()
	{
		if (mParent is not myUGUIObject uiObj)
		{
			logError("父节点的类型不是myUGUIObject,无法获取其窗口大小");
			return;
		}
		setWindowRightInParent(uiObj.getWindowRightInSelf());
		setWindowInParentCenterY();
	}
	// 设置窗口在父节点中横向居中
	public void setWindowInParentCenterX() { setPositionX(0.0f); }
	// 设置窗口在父节点中纵向居中
	public void setWindowInParentCenterY() { setPositionY(0.0f); }
	// 设置窗口左边界在父节点中的X坐标
	public void setWindowLeftInParent(float leftInParent) { setPositionX(leftInParent - getWindowLeftInSelf()); }
	// 设置窗口右边界在父节点中的X坐标
	public void setWindowRightInParent(float rightInParent) { setPositionX(rightInParent - getWindowRightInSelf()); }
	// 设置窗口顶部在父节点中的Y坐标
	public void setWindowTopInParent(float topInParent) { setPositionY(topInParent - getWindowTopInSelf()); }
	// 设置窗口底部在父节点中的Y坐标
	public void setWindowBottomInParent(float bottomInParent) { setPositionY(bottomInParent - getWindowBottomInSelf()); }
	// 获得窗口左边界在父窗口中的X坐标
	public float getWindowLeftInParent() { return getPosition().x + getWindowLeftInSelf(); }
	// 获得窗口右边界在父窗口中的X坐标
	public float getWindowRightInParent() { return getPosition().x + getWindowRightInSelf(); }
	// 获得窗口顶部在父窗口中的Y坐标
	public float getWindowTopInParent() { return getPosition().y + getWindowTopInSelf(); }
	// 获得窗口底部在父窗口中的Y坐标
	public float getWindowBottomInParent() { return getPosition().y + getWindowBottomInSelf(); }
	// 获得窗口顶部在窗口中的相对于窗口pivot的Y坐标
	public float getWindowTopInSelf() { return getWindowSize().y * (1.0f - getPivot().y); }
	// 获得窗口底部在窗口中的相对于窗口pivot的Y坐标
	public float getWindowBottomInSelf() { return -getWindowSize().y * getPivot().y; }
	// 获得窗口左边界在窗口中的相对于窗口pivot的X坐标
	public float getWindowLeftInSelf() { return -getWindowSize().x * getPivot().x; }
	// 获得窗口右边界在窗口中的相对于窗口pivot的X坐标
	public float getWindowRightInSelf() { return getWindowSize().x * (1.0f - getPivot().x); }
	// 获取不考虑中心点偏移的坐标,也就是固定获取窗口中心的坐标
	// 由于pivot的影响,Transform.localPosition获得的坐标并不一定等于窗口中心的坐标
	public Vector3 getPositionNoPivot() { return WidgetUtility.getPositionNoPivot(mRectTransform); }
	// 使当前窗口右边界对齐另外一个窗口的左边界,只修改x轴,仅限同一父节点下
	public void setRightToOtherLeft(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionX(other.getPosition().x - other.getWindowSize().x * 0.5f - getWindowSize().x * 0.5f - interval);
	}
	// 使当前窗口左边界对齐另外一个窗口的右边界,只修改x轴,仅限同一父节点下
	public void setLeftToOtherRight(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionX(other.getPosition().x + other.getWindowSize().x * 0.5f + getWindowSize().x * 0.5f + interval);
	}
	// 使当前窗口下边界对齐另外一个窗口的上边界,只修改y轴,仅限同一父节点下
	public void setBottomToOtherTop(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionY(other.getPosition().y + other.getWindowSize().y * 0.5f + getWindowSize().y * 0.5f + interval);
	}
	// 使当前窗口上边界对齐另外一个窗口的下边界,只修改y轴,仅限同一父节点下
	public void setTopToOtherBottom(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionY(other.getPosition().y - other.getWindowSize().y * 0.5f - getWindowSize().y * 0.5f - interval);
	}
	// 使当前窗口左边界对齐另外一个窗口的左边界,只修改x轴,仅限同一父节点下
	public void setLeftToOtherLeft(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionX(other.getPosition().x - other.getWindowSize().x * 0.5f + getWindowSize().x * 0.5f + interval);
	}
	// 使当前窗口左边界对齐另外一个窗口的右边界,只修改x轴,仅限同一父节点下
	public void setRightToOtherRight(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionX(other.getPosition().x + other.getWindowSize().x * 0.5f - getWindowSize().x * 0.5f - interval);
	}
	// 使当前窗口下边界对齐另外一个窗口的上边界,只修改y轴,仅限同一父节点下
	public void setTopToOtherTop(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionY(other.getPosition().y + other.getWindowSize().y * 0.5f - getWindowSize().y * 0.5f - interval);
	}
	// 使当前窗口上边界对齐另外一个窗口的下边界,只修改y轴,仅限同一父节点下
	public void setBottomToOtherBottom(myUGUIObject other, float interval = 0.0f)
	{
		if (other.getParent() != getParent())
		{
			logError("只有同一父节点下的节点才能对齐");
			return;
		}
		setPositionY(other.getPosition().y - other.getWindowSize().y * 0.5f + getWindowSize().y * 0.5f + interval);
	}
	public Vector2 getPivot() { return mRectTransform.pivot; }
	public void setPivot(Vector2 pivot) { mRectTransform.pivot = pivot; }
	public RectTransform getRectTransform() { return mRectTransform; }
	public void setWindowWidth(float width)
	{
		if (isFloatEqual(mRectTransform.rect.size.x, width))
		{
			return;
		}
		// 还是需要调用setWindowSize,需要触发一些虚函数的调用
		setWindowSize(replaceX(getWindowSize(), width));
	}
	public void setWindowHeight(float height)
	{
		if (isFloatEqual(mRectTransform.rect.size.y, height))
		{
			return;
		}
		// 还是需要调用setWindowSize,需要触发一些虚函数的调用
		setWindowSize(replaceY(getWindowSize(), height));
	}
	public virtual void setWindowSize(Vector2 size)
	{
		if (isVectorEqual(mRectTransform.rect.size, size))
		{
			return;
		}
		setRectSize(mRectTransform, size);
		ensureColliderSize();
	}
	public virtual Vector2 getWindowSize(bool transformed = false)
	{
		Vector2 windowSize = mRectTransform.rect.size;
		if (transformed)
		{
			windowSize = multiVector2(windowSize, getWorldScale());
		}
		return windowSize;
	}
	public virtual void setAlpha(float alpha, bool fadeChild)
	{
		if (fadeChild)
		{
			setUGUIChildAlpha(mObject, alpha);
		}
	}
	public virtual void cloneFrom(myUGUIObject obj)
	{
		if (obj.GetType() != GetType())
		{
			logError("type is different, can not clone!, this:" + GetType() + ", source:" + obj.GetType());
			return;
		}
		setPosition(obj.getPosition());
		setRotation(obj.getRotationQuaternion());
		setScale(obj.getScale());
	}
	public void refreshChildDepthByPositionZ()
	{
		// z值越大的子节点越靠后
		using var a = new ListScope<Transform>(out var tempList);
		int childCount = getChildCount();
		for (int i = 0; i < childCount; ++i)
		{
			tempList.Add(mTransform.GetChild(i));
		}
		quickSort(tempList, mCompareDescend);
		int count = tempList.Count;
		for (int i = 0; i < count; ++i)
		{
			tempList[i].SetSiblingIndex(i);
		}
	}
	public void setUGUIClick(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIClick(callback);
	}
	public void setUGUIMouseDown(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseDown(callback);
		// 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
		mReceiveLayoutHide = true;
	}
	public void setUGUIMouseUp(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseUp(callback);
		// 因为点击事件会使用触点,为了确保触点的正确状态,所以需要在布局隐藏时清除触点
		mReceiveLayoutHide = true;
	}
	public void setUGUIMouseEnter(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseEnter(callback);
	}
	public void setUGUIMouseExit(Action<PointerEventData, GameObject> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseExit(callback);
	}
	public void setUGUIMouseMove(Action<Vector2, Vector3> callback) 
	{
		getCOMUGUIInteractive().setUGUIMouseMove(callback);
		// 如果设置了要监听鼠标移动,则需要激活当前窗口
		mNeedUpdate = true;
	}
	public void setUGUIMouseStay(Action<Vector3> callback)
	{
		getCOMUGUIInteractive().setUGUIMouseStay(callback);
		mNeedUpdate = true;
	}
	public override void destroy()
	{
		if (!mAllowDestroyWindow)
		{
			logError("can not call window's destroy()! use destroyWindow(myUGUIObject window, bool destroyReally) instead");
		}
		base.destroy();
		// 同样的,由于一般myUGUIObject不会使用对象池来管理,所以销毁时需要手动去标记为已销毁的状态
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
	public static void collectChild<T>(myUGUIObject window, List<T> list) where T : myUGUIObject
	{
		list.addNotNull(window as T);
		foreach (myUGUIObject item in window.mChildList.safe())
		{
			collectChild(item, list);
		}
	}
	public static void destroyWindow(myUGUIObject window, bool destroyReally)
	{
		if (window == null)
		{
			return;
		}
		// 先销毁所有子节点,因为遍历中会修改子节点列表,所以需要复制一个列表
		if (!window.mChildList.isEmpty())
		{
			using var a = new ListScope<myUGUIObject>(out var childList, window.mChildList);
			foreach (myUGUIObject item in childList)
			{
				destroyWindow(item, destroyReally);
			}
		}
		window.getParent()?.removeChild(window);
		// 再销毁自己
		destroyWindowSingle(window, destroyReally);
	}
	public static void destroyWindowSingle(myUGUIObject window, bool destroyReally)
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
	public void removeChild(myUGUIObject child)
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
	public int getID() { return mID; }
	public GameLayout getLayout() { return mLayout; }
	public List<myUGUIObject> getChildList() { return mChildList; }
	public virtual bool isReceiveScreenTouch() { return mCOMWindowInteractive?.getOnScreenTouchUp() != null; }
	public myUGUIObject getParent() { return mParent; }
	public override float getAlpha() { return 1.0f; }
	public virtual Color getColor() { return Color.white; }
	public UIDepth getDepth() { return getCOMInteractive().getDepth(); }
	public virtual bool isHandleInput() { return mCOMWindowCollider != null && mCOMWindowCollider.isHandleInput(); }
	public virtual bool isPassRay() { return mCOMWindowInteractive == null || mCOMWindowInteractive.isPassRay(); }
	public virtual bool isPassDragEvent() { return !isDraggable() || (mCOMWindowInteractive != null && mCOMWindowInteractive.isPassDragEvent()); }
	public virtual bool isDraggable() { return getActiveComponent<COMWindowDrag>() != null; }
	public bool isMouseHovered() { return mCOMWindowInteractive != null && mCOMWindowInteractive.isMouseHovered(); }
	public int getClickSound() { return mCOMWindowInteractive?.getClickSound() ?? 0; }
	public bool isDepthOverAllChild() { return mCOMWindowInteractive != null && mCOMWindowInteractive.isDepthOverAllChild(); }
	public float getLongPressLengthThreshold() { return mCOMWindowInteractive?.getLongPressLengthThreshold() ?? -1.0f; }
	public bool isReceiveLayoutHide() { return mReceiveLayoutHide; }
	public bool isColliderForClick() { return mCOMWindowInteractive != null && mCOMWindowInteractive.isColliderForClick(); }
	public bool isAllowGenerateDepth() { return mCOMWindowInteractive == null || mCOMWindowInteractive.isAllowGenerateDepth(); }
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
		logError("can not get window fill percent with myUGUIObject");
		return 1.0f;
	}
	// 递归返回最后一个子节点,如果没有子节点,则返回空
	public myUGUIObject getLastChild()
	{
		if (mChildList.isEmpty())
		{
			return null;
		}
		if (mChildList.Count > 1 && !mChildOrderSorted)
		{
			logError("子节点没有被排序,无法获得正确的最后一个子节点");
		}
		myUGUIObject lastChild = mChildList[^1];
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
	public virtual void setColor(Color color) { }
	public virtual void setFillPercent(float percent) { logError("can not set window fill percent with myUGUIObject"); }
	public void setPassRay(bool passRay)								{ getCOMInteractive().setPassRay(passRay); }
	public void setPassDragEvent(bool pass)								{ getCOMInteractive().setPassDragEvent(pass); }
	public void setDepth(UIDepth parentDepth, int orderInParent)		{ getCOMInteractive().setDepth(parentDepth, orderInParent); }
	public void setLongPressLengthThreshold(float threshold)			{ getCOMInteractive().setLongPressLengthThreshold(threshold); }
	// 自己调用的callback,仅在启用自定义输入系统时生效
	public void setPreClickCallback(Action callback)					{ getCOMInteractive().setPreClickCallback(callback); }
	public void setPreClickDetailCallback(Vector3Callback callback)		{ getCOMInteractive().setPreClickDetailCallback(callback); }
	public void setClickCallback(Action callback)						{ getCOMInteractive().setClickCallback(callback); }
	public void setClickDetailCallback(Vector3Callback callback)		{ getCOMInteractive().setClickDetailCallback(callback); }
	public void setHoverCallback(BoolCallback callback)					{ getCOMInteractive().setHoverCallback(callback); }
	public void setHoverDetailCallback(Vector3BoolCallback callback)	{ getCOMInteractive().setHoverDetailCallback(callback); }
	public void setPressCallback(BoolCallback callback)					{ getCOMInteractive().setPressCallback(callback); }
	public void setPressDetailCallback(Vector3BoolCallback callback)	{ getCOMInteractive().setPressDetailCallback(callback); }
	public void setDoubleClickCallback(Action callback)					{ getCOMInteractive().setDoubleClickCallback(callback); }
	public void setDoubleClickDetailCallback(Vector3Callback callback)	{ getCOMInteractive().setDoubleClickDetailCallback(callback); }
	public void setOnReceiveDrag(ReceiveDragCallback callback)			{ getCOMInteractive().setOnReceiveDrag(callback); }
	public void setOnDragHover(DragHoverCallback callback)				{ getCOMInteractive().setOnDragHover(callback); }
	public void setOnTouchEnter(Vector3IntCallback callback)			{ getCOMInteractive().setOnTouchEnter(callback); }
	public void setOnTouchLeave(Vector3IntCallback callback)			{ getCOMInteractive().setOnTouchLeave(callback); }
	public void setOnTouchDown(Vector3IntCallback callback)				{ getCOMInteractive().setOnTouchDown(callback); }
	public void setOnTouchUp(Vector3IntCallback callback)				{ getCOMInteractive().setOnTouchUp(callback); }
	public void setOnTouchMove(TouchMoveCallback callback)					{ getCOMInteractive().setOnTouchMove(callback); }
	public void setOnTouchStay(Vector3IntCallback callback)				{ getCOMInteractive().setOnTouchStay(callback); }
	public void setOnScreenTouchUp(Vector3IntCallback callback)			{ getCOMInteractive().setOnScreenTouchUp(callback); }
	public void setColliderForClick(bool forClick)						{ getCOMInteractive().setColliderForClick(forClick); }
	public void setClickSound(int sound)								{ getCOMInteractive().setClickSound(sound); }
	public void setLayout(GameLayout layout)							{ mLayout = layout; }
	public void setReceiveLayoutHide(bool receive)						{ mReceiveLayoutHide = receive; }
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
	public void setParent(myUGUIObject parent, bool refreshDepth)
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
	public virtual void setHandleInput(bool enable)							{ mCOMWindowCollider?.enableCollider(enable); }
	public void addLongPress(Action callback, float pressTime, FloatCallback pressingCallback = null)
	{
		getCOMInteractive().addLongPress(callback, pressTime, pressingCallback);
	}
	public void removeLongPress(Action callback)							{ mCOMWindowInteractive?.removeLongPress(callback); }
	public void clearLongPress()											{ mCOMWindowInteractive?.clearLongPress(); }
	// callback
	//------------------------------------------------------------------------------------------------------------------------------
	public virtual void onTouchEnter(Vector3 touchPos, int touchID)			{ mCOMWindowInteractive?.onTouchEnter(touchPos, touchID); }
	public virtual void onTouchLeave(Vector3 touchPos, int touchID)			{ mCOMWindowInteractive?.onTouchLeave(touchPos, touchID); }
	// 鼠标左键在窗口内按下
	public virtual void onTouchDown(Vector3 touchPos, int touchID)			{ mCOMWindowInteractive?.onTouchDown(touchPos, touchID); }
	// 鼠标左键在窗口内放开
	public virtual void onTouchUp(Vector3 touchPos, int touchID)			{ mCOMWindowInteractive?.onTouchUp(touchPos, touchID); }
	// 触点在移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public virtual void onTouchMove(Vector3 touchPos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mCOMWindowInteractive?.onTouchMove(touchPos, moveDelta, moveTime, touchID);
	}
	// 触点没有移动,此时触点是按下状态,且按下瞬间在窗口范围内
	public virtual void onTouchStay(Vector3 touchPos, int touchID)			{ mCOMWindowInteractive?.onTouchStay(touchPos, touchID); }
	// 鼠标在屏幕上抬起
	public virtual void onScreenTouchUp(Vector3 touchPos, int touchID)		{ mCOMWindowInteractive?.onScreenTouchUp(touchPos, touchID); }
	// 鼠标在屏幕上按下
	public virtual void onScreenTouchDown(Vector3 touchPos, int touchID)	{ mCOMWindowInteractive?.onScreenTouchDown(touchPos, touchID); }
	// 有物体拖动到了当前窗口上
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 touchPos, ref bool continueEvent)
	{
		mCOMWindowInteractive?.onReceiveDrag(dragObj, touchPos, ref continueEvent);
	}
	// 有物体拖动到了当前窗口上
	public virtual void onDragHovered(IMouseEventCollect dragObj, Vector3 touchPos, bool hover)
	{
		mCOMWindowInteractive?.onDragHovered(dragObj, touchPos, hover);
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
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickCallback(clickCallback);
		setPreClickCallback(preClick);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback, Action preClick, bool passRay = false)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickCallback(clickCallback);
		setPreClickCallback(preClick);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	// 用于接收GlobalTouchSystem处理的输入事件
	public void registeCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(passRay);
		// 由碰撞体的窗口都需要启用更新,以便可以保证窗口大小与碰撞体大小一致
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(passRay);
		// 由碰撞体的窗口都需要启用更新,以便可以保证窗口大小与碰撞体大小一致
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeCollider(this, camera, true);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeCollider(this, camera, false);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback, BoolCallback hoverCallback, BoolCallback pressCallback)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setObjectCallback(clickCallback, hoverCallback, pressCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeCollider(Vector3Callback clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickDetailCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Vector3Callback clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickDetailCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeColliderNoError(Action clickCallback, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeCollider(Action clickCallback, bool passRay, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeColliderNoError(Action clickCallback, bool passRay, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickCallback(clickCallback);
		setPassRay(passRay);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeCollider(Vector3Callback clickCallback, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickDetailCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeColliderNoError(Vector3Callback clickCallback, int clickSound)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickDetailCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
		setClickSound(clickSound);
	}
	public void registeCollider(Action clickCallback)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Action clickCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeCollider(this, camera, true);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Action clickCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeCollider(this, camera, false);
		setClickCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Vector3Callback clickCallback)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setClickDetailCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Vector3Callback clickCallback)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setClickDetailCallback(clickCallback);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(Vector3Callback clickCallback, GameCamera camera)
	{
		setClickDetailCallback(clickCallback);
		mGlobalTouchSystem.registeCollider(this, camera, true);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(Vector3Callback clickCallback, GameCamera camera)
	{
		setClickDetailCallback(clickCallback);
		mGlobalTouchSystem.registeCollider(this, camera, false);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeCollider(bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeColliderNoError(bool passRay)
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setPassRay(passRay);
		setNeedUpdate(true);
	}
	public void registeCollider()
	{
		mGlobalTouchSystem.registeCollider(this, null, true);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void registeColliderNoError()
	{
		mGlobalTouchSystem.registeCollider(this, null, false);
		setPassRay(false);
		setNeedUpdate(true);
	}
	public void unregisteCollider()
	{
		mGlobalTouchSystem?.unregisteCollider(this);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static int compareZDecending(Transform a, Transform b) { return (int)sign(b.localPosition.z - a.localPosition.z); }
	protected virtual void ensureColliderSize()
	{
		// 确保RectTransform和BoxCollider一样大
		mCOMWindowCollider?.setColliderSize(mRectTransform);
	}
	protected COMWindowUGUIInteractive getCOMUGUIInteractive()
	{
		return mCOMWindowUGUIInteractive ??= addComponent<COMWindowUGUIInteractive>(true);
	}
	protected virtual void onWorldScaleChanged(Vector2 lastWorldScale) { }
	protected void addChild(myUGUIObject child, bool refreshDepth)
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
	protected static int compareSiblingIndex(myUGUIObject child0, myUGUIObject child1)
	{
		return sign(child0.getTransform().GetSiblingIndex() - child1.getTransform().GetSiblingIndex());
	}
	protected ComponentInteractive getCOMInteractive()
	{
		return mCOMWindowInteractive ??= addComponent<ComponentInteractive>(true);
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