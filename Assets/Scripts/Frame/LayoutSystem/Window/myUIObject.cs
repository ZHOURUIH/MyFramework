using System;
using System.Collections.Generic;
using UnityEngine;

public class myUIObject : Transformable, IMouseEventCollect, IEquatable<myUIObject>
{
	protected static Comparison<myUIObject> mCompareSiblingIndex = compareSiblingIndex;
	protected HashSet<myUIObject> mChildSet;                    // 子节点列表,用于查询是否有子节点
	protected List<myUIObject> mChildList;                      // 子节点列表,与GameObject的子节点顺序保持一致
	protected ObjectDoubleClickCallback mDoubleClickCallback;	// 双击回调
	protected ObjectPreClickCallback mObjectPreClickCallback;	// 单击的预回调,单击时会首先调用此回调
	protected OnReceiveDrag mReceiveDragCallback;				// 接收到有物体拖到当前窗口时的回调
	protected OnDragHover mDragHoverCallback;					// 有物体拖拽悬停到当前窗口时的回调
	protected ObjectClickCallback mClickCallback;				// 单击回调,在预回调之后调用
	protected ObjectHoverCallback mHoverCallback;				// 悬停回调
	protected ObjectPressCallback mPressCallback;				// 按下时回调
	protected OnMultiTouchStart mOnMultiTouchStart;				// 窗口中多点触控开始的回调,只支持两个点的触控
	protected OnMultiTouchMove mOnMultiTouchMove;               // 窗口中多点触控移动的回调,只支持两个点的触控
	protected OnMultiTouchEnd mOnMultiTouchEnd;					// 窗口中多点触控结束的回调,只支持两个点的触控
	protected OnScreenMouseUp mOnScreenMouseUp;					// 屏幕上鼠标抬起的回调,无论鼠标在哪儿
	protected OnLongPressing mOnLongPressing;                   // 长按进度回调,可获取当前还有多久到达长按时间阈值
	protected OnMouseEnter mOnMouseEnter;						// 鼠标进入时的回调
	protected OnMouseLeave mOnMouseLeave;						// 鼠标离开时的回调
	protected OnLongPress mOnLongPress;                         // 达到长按时间阈值时的回调
	protected OnMouseMove mOnMouseMove;							// 鼠标移动的回调
	protected OnMouseStay mOnMouseStay;							// 鼠标静止在当前窗口内的回调
	protected OnMouseDown mOnMouseDown;							// 鼠标按下的回调
	protected OnMouseUp mOnMouseUp;								// 鼠标抬起的回调
	protected AudioSource mAudioSource;							// 音频组件
	protected BoxCollider mBoxCollider;							// 碰撞组件
	protected Transform mTransform;								// 变换组件
	protected GameLayout mLayout;								// 所属布局
	protected GameObject mObject;								// 物体节点
	protected myUIObject mParent;								// 父节点窗口
	protected UIDepth mDepth;									// UI深度,深度越大,渲染越靠前,仅UGUI使用
	protected Vector3 mMouseDownPosition;						// 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected Vector3 mLastWorldScale;							// 上一次设置的世界缩放值
	protected object mObjectPreClickUserData;					// 回调函数参数
	protected float mLongPressLengthThreshold;					// 小于0表示不判断鼠标移动对长按检测的影响
	protected float mLongPressTimeThreshold;					// 长按的时间阈值,超过阈值时检测为长按
	protected float mLastClickTime;								// 上一次点击距离当前的时间,小于0表示未计时,大于等于0表示正在计时
	protected float mPressedTime;								// 小于0表示未计时,大于等于0表示正在计时长按操作,防止长时间按下时总会每隔指定时间调用一次回调
	protected uint mID;											// 每个窗口的唯一ID
	protected bool mDestroyImmediately;							// 销毁窗口时是否立即销毁
	protected bool mDepthOverAllChild;							// 计算深度时是否将深度设置为所有子节点之上,实际调整的是mExtraDepth
	protected bool mMouseHovered;								// 当前鼠标是否悬停在窗口上
	protected bool mPressing;									// 鼠标当前是否在窗口中处于按下状态,鼠标离开窗口时认为鼠标不在按下状态
	protected bool mPassRay;									// 当存在且注册了碰撞体时是否允许射线穿透
	protected static bool mAllowDestroyWindow = false;
	public myUIObject()
	{
		mID = makeID();
		mDepth = new UIDepth();
		mChildList = new List<myUIObject>();
		mChildSet = new HashSet<myUIObject>();
		mLongPressTimeThreshold = 1.0f;
		mPressedTime = -1.0f;
		mLongPressLengthThreshold = -1.0f;
		mLastClickTime = -1.0f;
		mPassRay = true;
		mEnable = false;    // 出于效率考虑,窗口默认不启用更新,只有部分窗口和使用组件进行变化时才自动启用更新
		mDestroy = false;	// 由于一般myUIObject不会使用对象池来管理,所以构造时就设置当前对象为有效
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
	public static void destroyWindow(myUIObject window, bool destroyReally)
	{
		if (window == null)
		{
			return;
		}
		// 先销毁所有子节点,因为遍历中会修改子节点列表,所需需要复制一个列表
		if (window.mChildList.Count > 0)
		{
			LIST(out List<myUIObject> childList);
			childList.AddRange(window.mChildList);
			int childCount = childList.Count;
			for (int i = 0; i < childCount; ++i)
			{
				destroyWindow(childList[i], destroyReally);
			}
			UN_LIST(childList);
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
		mAllowDestroyWindow = true;
		window.destroy();
		mAllowDestroyWindow = false;
		if (destroyReally)
		{
			destroyGameObject(ref window.mObject, window.mDestroyImmediately);
		}
		window.mObject = null;
	}
	public void setLayout(GameLayout layout) { mLayout = layout; }
	public virtual void init()
	{
		initComponents();
		mLayout?.registerUIObject(this);
		mAudioSource = mObject.GetComponent<AudioSource>();
		mBoxCollider = mObject.GetComponent<BoxCollider>();
		if (mBoxCollider != null && mLayout != null && mLayout.isCheckBoxAnchor() && mLayoutManager.isUseAnchor())
		{
			string layoutName = mLayout.getName();
			// BoxCollider的中心必须为0,因为UIWidget会自动调整BoxCollider的大小和位置,而且调整后位置为0,所以在制作时BoxCollider的位置必须为0
			if (!isFloatZero(mBoxCollider.center.sqrMagnitude))
			{
				logWarning("BoxCollider's center must be zero! Otherwise can not adapt to the screen sometimes! name : " + mName + ", layout : " + layoutName);
			}
			if (mObject.GetComponent<ScaleAnchor>() == null && mObject.GetComponent<PaddingAnchor>() == null)
			{
				logWarning("Window with BoxCollider and Widget must has ScaleAnchor! Otherwise can not adapt to the screen sometimes! name : " + mName + ", layout : " + layoutName);
			}
		}
	}
	public void addChild(myUIObject child, bool needSortChild = true)
	{
		if (!mChildSet.Contains(child))
		{
			mChildList.Add(child);
			mChildSet.Add(child);
		}
		// 添加子节点后根据子节点顺序进行排序
		if (needSortChild)
		{
			sortChild();
		}
	}
	public void removeChild(myUIObject child)
	{
		mChildList.Remove(child);
		mChildSet.Remove(child);
	}
	public AudioSource createAudioSource()
	{
		return mAudioSource = mObject.AddComponent<AudioSource>();
	}
	public bool canUpdate() { return mObject.activeInHierarchy && mEnable; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 长按检测
		if ((mOnLongPress != null || mOnLongPressing != null) && mPressing && mPressedTime >= 0.0f)
		{
			if (mLongPressLengthThreshold < 0.0f || lengthLess(mMouseDownPosition - getMousePosition(), mLongPressLengthThreshold))
			{
				mPressedTime += elapsedTime;
				if (mOnLongPressing != null)
				{
					float progress = !isFloatZero(mLongPressTimeThreshold) ? mPressedTime / mLongPressTimeThreshold : 0.0f;
					clampMax(ref progress, 1.0f);
					mOnLongPressing(progress);
				}
				if (mPressedTime >= mLongPressTimeThreshold)
				{
					mOnLongPress?.Invoke();
					mPressedTime = -1.0f;
				}
			}
			else
			{
				mPressedTime = -1.0f;
				mOnLongPressing?.Invoke(0.0f);
			}
		}
		// 双击计时
		if (mLastClickTime >= 0.0f && mLastClickTime < FrameDefine.DOUBLE_CLICK_THRESHOLD)
		{
			mLastClickTime += elapsedTime;
		}
		// 超过一定时间后停止计时
		else if (mLastClickTime >= FrameDefine.DOUBLE_CLICK_THRESHOLD)
		{
			mLastClickTime = -1.0f;
		}
		// 检测世界缩放值是否有变化
		if (!isVectorEqual(mLastWorldScale, getWorldScale()))
		{
			onWorldScaleChanged(mLastWorldScale);
			mLastWorldScale = getWorldScale();
		}
	}
	public virtual void cloneFrom(myUIObject obj)
	{
		if (Typeof(obj) != Typeof(this))
		{
			logError("ui type is different, can not clone!");
			return;
		}
		setPosition(obj.getPosition());
		setRotation(obj.getRotation());
		setScale(obj.getScale());
	}
	public void setAsLastSibling(bool notifyLayout = true)
	{
		mTransform.SetAsLastSibling();
		if (notifyLayout)
		{
			mParent.sortChild();
			mLayout.notifyChildChanged(mParent);
		}
	}
	public void setAsFirstSibling(bool notifyLayout = true)
	{
		mTransform.SetAsFirstSibling();
		if (notifyLayout)
		{
			mParent.sortChild();
			mLayout.notifyChildChanged(mParent);
		}
	}
	public void setSibling(int index, bool notifyLayout = true)
	{
		if (mTransform.GetSiblingIndex() == index)
		{
			return;
		}
		mTransform.SetSiblingIndex(index);
		if (notifyLayout)
		{
			mParent.sortChild();
			mLayout.notifyChildChanged(mParent);
		}
	}
	public override Vector3 localToWorld(Vector3 point) { return localToWorld(mTransform, point); }
	public override Vector3 worldToLocal(Vector3 point) { return worldToLocal(mTransform, point); }
	public override Vector3 localToWorldDirection(Vector3 direction) { return localToWorldDirection(mTransform, direction); }
	public override Vector3 worldToLocalDirection(Vector3 direction) { return worldToLocalDirection(mTransform, direction); }
	// 当自适应更新完以后调用
	public virtual void notifyAnchorApply() { }
	//get
	//-------------------------------------------------------------------------------------------------------------------------------------
	public int getSiblingIndex() { return mTransform.GetSiblingIndex(); }
	public uint getID() { return mID; }
	public GameLayout getLayout() { return mLayout; }
	public virtual GameObject getObject() { return mObject; }
	public List<myUIObject> getChildList() { return mChildList; }
	public virtual bool isReceiveScreenMouse() { return mOnScreenMouseUp != null; }
	public myUIObject getParent() { return mParent; }
	public Transform getTransform() { return mTransform; }
	public AudioSource getAudioSource() { return mAudioSource; }
	public override bool isActive() { return mObject.activeSelf; }
	public virtual bool isActiveInHierarchy() { return mObject.activeInHierarchy; }
	public T getUnityComponent<T>(bool addIfNotExist = true) where T : Component
	{
		T com = mObject.GetComponent<T>();
		if (com == null && addIfNotExist)
		{
			com = mObject.AddComponent<T>();
		}
		return com;
	}
	public void getUnityComponent<T>(out T com, bool addIfNotExist = true) where T : Component
	{
		com = mObject.GetComponent<T>();
		if (com == null && addIfNotExist)
		{
			com = mObject.AddComponent<T>();
		}
	}
	public Collider getCollider() { return mBoxCollider; }
	public bool raycast(ref Ray ray, out RaycastHit hit, float maxDistance) 
	{
		if (!mBoxCollider)
		{
			hit = new RaycastHit();
			return false;
		}
		return mBoxCollider.Raycast(ray, out hit, maxDistance);
	}
	public override Vector3 getPosition() { return mTransform.localPosition; }
	public override Vector3 getScale() { return new Vector2(mTransform.localScale.x, mTransform.localScale.y); }
	public override Vector3 getRotation()
	{
		Vector3 vector3 = mTransform.localEulerAngles;
		adjustAngle180(ref vector3.z);
		return vector3;
	}
	public override Vector3 getWorldRotation() { return mTransform.eulerAngles; }
	public override Vector3 getWorldPosition() { return mTransform.position; }
	public override Vector3 getWorldScale() { return mTransform.lossyScale; }
	public Vector3 getRotationRadian()
	{
		Vector3 vector3 = toRadian(mTransform.localEulerAngles);
		adjustRadian180(ref vector3.z);
		return vector3;
	}
	public Quaternion getRotationQuat() { return mTransform.localRotation; }
	public Quaternion getWorldRotationQuat() { return mTransform.rotation; }
	public int getChildCount() { return mTransform.childCount; }
	public GameObject getChild(int index) { return mTransform.GetChild(index).gameObject; }
	public virtual float getAlpha() { return 1.0f; }
	public virtual Color getColor() { return Color.white; }
	public virtual float getFillPercent()
	{
		logError("can not get window fill percent with myUIObject");
		return 1.0f;
	}
	public UIDepth getDepth() { return mDepth; }
	public virtual bool isHandleInput() { return mBoxCollider != null && mBoxCollider.enabled; }
	public virtual bool isPassRay() { return mPassRay; }
	public virtual Vector2 getWindowSize(bool transformed = false)
	{
		logError("can not get window size with myUIObject");
		return Vector2.zero;
	}
	public virtual bool isDragable() { return getComponent<COMWindowDrag>(true, false) != null; }
	public bool isMouseHovered() { return mMouseHovered; }
	public virtual bool isChildOf(IMouseEventCollect parent)
	{
		var obj = parent as myUIObject;
		if (obj == null)
		{
			return false;
		}
		return mTransform.IsChildOf(obj.getTransform());
	}
	public bool isDepthOverAllChild() { return mDepthOverAllChild; }
	// 递归返回最后一个子节点,如果没有子节点,则返回空
	public myUIObject getLastChild()
	{
		if (mChildList.Count == 0)
		{
			return null;
		}
		myUIObject lastChild = mChildList[mChildList.Count - 1];
		if (lastChild.getChildList().Count == 0)
		{
			return mChildList[mChildList.Count - 1];
		}
		return lastChild.getLastChild();
	}
	public float getLongPressTimeThreshold() { return mLongPressTimeThreshold; }
	public float getLongPressLengthThreshold() { return mLongPressLengthThreshold; }
	//set
	//-------------------------------------------------------------------------------------------------------------------------------------
	public void setGameObject(GameObject go)
	{
		setName(go.name);
		mObject = go;
		mTransform = mObject.transform;
#if UNITY_EDITOR
		// 由于物体可能使克隆出来的,所以如果已经添加了调试组件,直接获取即可
		getUnityComponent<WindowDebug>().setWindow(this);
#endif
	}
	public void setParent(myUIObject parent, bool needSortChild = true)
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
			mParent.addChild(this, needSortChild);
		}
	}
	public override void setName(string name)
	{
		base.setName(name);
		if (mObject != null && mObject.name != name)
		{
			mObject.name = name;
		}
	}
	public void setDepthOverAllChild(bool depthOver) { mDepthOverAllChild = depthOver; }
	public virtual void setHandleInput(bool enable)
	{
		if (mBoxCollider != null)
		{
			mBoxCollider.enabled = enable;
		}
	}
	public void setDestroyImmediately(bool immediately) { mDestroyImmediately = immediately; }
	public override void setActive(bool active)
	{
		if(active == mObject.activeSelf)
		{
			return;
		}
		mObject.SetActive(active);
		base.setActive(active);
	}
	public override void setScale(Vector3 scale) { mTransform.localScale = scale; }
	public override void setPosition(Vector3 pos) { mTransform.localPosition = pos; }
	public override void setRotation(Vector3 rot) { mTransform.localEulerAngles = rot; }
	public override void setWorldRotation(Vector3 rot) { mTransform.eulerAngles = rot; }
	public override void setWorldPosition(Vector3 pos) { mTransform.position = pos; }
	public override void setWorldScale(Vector3 scale) { setScale(devideVector3(scale, mParent.getWorldScale())); }
	public virtual void setAlpha(float alpha, bool fadeChild) { }
	public virtual void setColor(Color color) { }
	public virtual void setFillPercent(float percent) { logError("can not set window fill percent with myUIObject"); }
	public void setPassRay(bool passRay) { mPassRay = passRay; }
	public virtual void setWindowSize(Vector2 size) { logError("can not set window size with myUIObject"); }
	public void setLongPressLengthThreshold(float threshold) { mLongPressLengthThreshold = threshold; }
	// 自己调用的callback,仅在启用自定义输入系统时生效
	public void setPreClickCallback(ObjectPreClickCallback callback, object userData) { mObjectPreClickCallback = callback; mObjectPreClickUserData = userData; }
	public void setClickCallback(ObjectClickCallback callback) { mClickCallback = callback; }
	public void setHoverCallback(ObjectHoverCallback callback) { mHoverCallback = callback; }
	public void setPressCallback(ObjectPressCallback callback) { mPressCallback = callback; }
	public void setDoubleClickCallback(ObjectDoubleClickCallback callback) { mDoubleClickCallback = callback; }
	public void setOnLongPress(OnLongPress callback, float pressTime)
	{
		mOnLongPress = callback;
		mLongPressTimeThreshold = pressTime;
	}
	public void setOnLongPressing(OnLongPressing callback) { mOnLongPressing = callback; }
	public void setReceiveDragCallback(OnReceiveDrag callback) { mReceiveDragCallback = callback; }
	public void setDragHoverCallback(OnDragHover callback) { mDragHoverCallback = callback; }
	public void setOnMouseEnter(OnMouseEnter callback) { mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback) { mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback) { mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback) { mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback) { mOnMouseMove = callback; }
	public void setOnMouseStay(OnMouseStay callback) { mOnMouseStay = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback) { mOnScreenMouseUp = callback; }
	public void setMultiTouchStart(OnMultiTouchStart callback) { mOnMultiTouchStart = callback; }
	public void setMultiTouchEnd(OnMultiTouchEnd callback) { mOnMultiTouchEnd = callback; }
	public void setMultiTouchMove(OnMultiTouchMove callback) { mOnMultiTouchMove = callback; }
	// callback
	//--------------------------------------------------------------------------------------------------------------------------------
	public virtual void onMultiTouchStart(Vector3 touch0, Vector3 touch1)
	{
		mOnMultiTouchStart?.Invoke(touch0, touch1);
	}
	public virtual void onMultiTouchEnd()
	{
		mOnMultiTouchEnd?.Invoke();
	}
	public virtual void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1)
	{
		mOnMultiTouchMove?.Invoke(touch0, lastTouch0, touch1, lastTouch1);
	}
	public virtual void onMouseEnter(int touchID)
	{
		mMouseHovered = true;
		mHoverCallback?.Invoke(this, true);
		mOnMouseEnter?.Invoke(this, touchID);
	}
	public virtual void onMouseLeave(int touchID)
	{
		mMouseHovered = false;
		mPressing = false;
		mPressedTime = -1.0f;
		mHoverCallback?.Invoke(this, false);
		mOnMouseLeave?.Invoke(this, touchID);
		mOnLongPressing?.Invoke(0.0f);
	}
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos, int touchID)
	{
		mPressing = true;
		mPressedTime = 0.0f;
		mMouseDownPosition = mousePos;
		mPressCallback?.Invoke(this, true);
		mOnMouseDown?.Invoke(mousePos, touchID);
		mOnLongPressing?.Invoke(0.0f);
	}
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos, int touchID)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mPressCallback?.Invoke(this, false);
		if (lengthLess(mMouseDownPosition - mousePos, FrameDefine.CLICK_THRESHOLD))
		{
			mObjectPreClickCallback?.Invoke(this, mObjectPreClickUserData);
			mClickCallback?.Invoke(this);
			if (mDoubleClickCallback != null && mLastClickTime > 0.0f && mLastClickTime < FrameDefine.DOUBLE_CLICK_THRESHOLD)
			{
				mDoubleClickCallback(this);
				// 已完成双击操作,结束计时
				mLastClickTime = -1.0f;
			}
			// 未开始双击计时则开始计时
			if (mLastClickTime < 0.0f)
			{
				mLastClickTime = 0.0f;
			}
		}
		mOnMouseUp?.Invoke(mousePos, touchID);
		mOnLongPressing?.Invoke(0.0f);
	}
	// 鼠标在窗口内,并且有移动
	public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mOnMouseMove?.Invoke(mousePos, moveDelta, moveTime, touchID);
	}
	// 鼠标在窗口内,但是不移动
	public virtual void onMouseStay(Vector3 mousePos, int touchID)
	{
		mOnMouseStay?.Invoke(mousePos, touchID);
	}
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mOnScreenMouseUp?.Invoke(this, mousePos, touchID);
	}
	// 鼠标在屏幕上按下
	public virtual void onScreenMouseDown(Vector3 mousePos, int touchID) { }
	// 有物体拖动到了当前窗口上
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, BOOL continueEvent)
	{
		if (mReceiveDragCallback != null)
		{
			continueEvent.set(false);
			mReceiveDragCallback(dragObj, continueEvent);
		}
	}
	// 有物体拖动到了当前窗口上
	public virtual void onDragHoverd(IMouseEventCollect dragObj, bool hover)
	{
		mDragHoverCallback?.Invoke(dragObj, hover);
	}
	public override int GetHashCode() { return (int)mID; }
	public bool Equals(myUIObject obj) { return mID == obj.mID; }
	//----------------------------------------------------------------------------------------------------------------------------------
	protected virtual void onWorldScaleChanged(Vector2 lastWorldScale) { }
	protected void sortChild()
	{
		if (mChildList.Count <= 1)
		{
			return;
		}
		quickSort(mChildList, mCompareSiblingIndex);
	}
	protected static int compareSiblingIndex(myUIObject child0, myUIObject child1)
	{
		return sign(child0.getTransform().GetSiblingIndex() - child1.getTransform().GetSiblingIndex());
	}
}