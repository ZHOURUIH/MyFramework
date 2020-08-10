using System;
using System.Collections.Generic;
using UnityEngine;

public class txUIObject : Transformable, IMouseEventCollect
{
	protected List<txUIObject> mChildList;
	protected AudioSource mAudioSource;
	protected Transform mTransform;
	protected RectTransform mRectTransform;
	protected BoxCollider mBoxCollider;
	protected GameLayout mLayout;
	protected GameObject mObject;
	protected txUIObject mParent;
	protected OnReceiveDragCallback mReceiveDragCallback;   // 接收到有物体拖到当前窗口时的回调
	protected OnDragHoverCallback mDragHoverCallback;   // 有物体拖拽悬停到当前窗口时的回调
	protected OnMouseEnter mOnMouseEnter;
	protected OnMouseLeave mOnMouseLeave;
	protected OnMouseDown mOnMouseDown;
	protected OnMouseUp mOnMouseUp;
	protected OnMouseMove mOnMouseMove;
	protected OnMouseStay mOnMouseStay;
	protected OnScreenMouseUp mOnScreenMouseUp;
	protected OnLongPress mOnLongPress;
	protected OnLongPressing mOnLongPressing;
	protected OnMultiTouchStart mOnMultiTouchStart;
	protected OnMultiTouchMove mOnMultiTouchMove;
	protected OnMultiTouchEnd mOnMultiTouchEnd;
	protected ObjectPreClickCallback mObjectPreClickCallback;
	protected ObjectClickCallback mClickCallback;
	protected ObjectHoverCallback mHoverCallback;
	protected ObjectPressCallback mPressCallback;
	protected ObjectDoubleClickCallback mDoubleClickCallback;
	protected Vector3 mMouseDownPosition;			// 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected Vector3 mLastWorldScale;				// 上一次设置的世界缩放值
	protected object mObjectPreClickCallbackUserData;// 回调函数参数
	protected float mLongPressTimeThreshold;		// 长按的时间阈值,超过阈值时检测为长按
	protected float mPressedTime;					// 小于0表示未计时,大于等于0表示正在计时长按操作,防止长时间按下时总会每隔指定时间调用一次回调
	protected float mLongPressLengthThreshold;		// 小于0表示不判断鼠标移动对长按检测的影响
	protected float mLastClickTime;					// 上一次点击距离当前的时间,小于0表示未计时,大于等于0表示正在计时
	protected bool mDestroyImmediately;				// 销毁窗口时是否立即销毁
	protected bool mPassRay;						// 当存在且注册了碰撞体时是否允许射线穿透
	protected bool mMouseHovered;					// 当前鼠标是否悬停在窗口上
	protected bool mPressing;						// 鼠标当前是否在窗口中处于按下状态,鼠标离开窗口时认为鼠标不在按下状态
	protected bool mEnable;							// 是否启用窗口,不启用则不更新,但是还是会显示
	protected int mDepth;							// UI深度,深度越大,渲染越靠前,仅UGUI使用
	protected int mID;								// 每个窗口的唯一ID
	protected static bool mAllowDestroyWindow = false;
	public txUIObject()
		: base(EMPTY_STRING)
	{
		mID = makeID();
		mChildList = new List<txUIObject>();
		mLongPressTimeThreshold = 1.0f;
		mPressedTime = -1.0f;
		mLongPressLengthThreshold = -1.0f;
		mLastClickTime = -1.0f;
		mPassRay = true;
		mEnable = true;
	}
	public override void destroy()
	{
		if (!mAllowDestroyWindow)
		{
			logError("can not call window's destroy()! use destroyWindow(txUIObject window, bool destroyReally) instead");
		}
		base.destroy();
	}
	public static void destroyWindow(txUIObject window, bool destroyReally)
	{
		if (window == null)
		{
			return;
		}
		// 先销毁所有子节点
		int childCount = window.mChildList.Count;
		for (int i = 0; i < childCount; ++i)
		{
			destroyWindow(window.mChildList[i], destroyReally);
		}
		// 再销毁自己
		destroyWindowSingle(window, destroyReally);
	}
	public static void destroyWindowSingle(txUIObject window, bool destroyReally)
	{
		window.destroyAllComponents();
		mGlobalTouchSystem.unregisteBoxCollider(window);
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
	public virtual void init(GameObject go, txUIObject parent)
	{
		setGameObject(go);
		setParent(parent);
		initComponents();
		mLayout?.registerUIObject(this);
		mAudioSource = mObject.GetComponent<AudioSource>();
		mBoxCollider = mObject.GetComponent<BoxCollider>();
		mRectTransform = mObject.GetComponent<RectTransform>();
		// 因为在使用UGUI时,原来的Transform会被替换为RectTransform,所以需要重新设置Transform组件
		if (mRectTransform != null)
		{
			mTransform = mRectTransform;
		}
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
		if (mBoxCollider != null && mRectTransform != null)
		{
			mBoxCollider.size = mRectTransform.rect.size;
		}
	}
	public void addChild(txUIObject child)
	{
		if (!mChildList.Contains(child))
		{
			mChildList.Add(child);
		}
	}
	public void removeChild(txUIObject child)
	{
		mChildList.Remove(child);
	}
	public AudioSource createAudioSource()
	{
		return mAudioSource = mObject.AddComponent<AudioSource>();
	}
	public bool canUpdate() { return mObject.activeSelf && mEnable; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 确保RectTransform和BoxCollider一样大
		if (mRectTransform != null && mBoxCollider != null)
		{
			if (mRectTransform.rect.width != mBoxCollider.size.x || mRectTransform.rect.height != mBoxCollider.size.y)
			{
				mBoxCollider.size = mRectTransform.rect.size;
				mBoxCollider.center = multiVector2(mRectTransform.rect.size, new Vector2(0.5f, 0.5f) - mRectTransform.pivot);
			}
		}
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
		if (mLastClickTime >= 0.0f && mLastClickTime < CommonDefine.DOUBLE_CLICK_THRESHOLD)
		{
			mLastClickTime += elapsedTime;
		}
		// 超过一定时间后停止计时
		else if (mLastClickTime >= CommonDefine.DOUBLE_CLICK_THRESHOLD)
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
	public virtual void cloneFrom(txUIObject obj)
	{
		if(obj.GetType() != GetType())
		{
			logError("ui type is different, can not clone!");
			return;
		}
		setPosition(obj.getPosition());
		setRotation(obj.getRotation());
		setScale(obj.getScale());
	}
	public void setAsLastSibling()
	{
		mTransform.SetAsLastSibling();
		mLayout.notifyObjectOrderChanged(mParent);
	}
	public void setAsFirstSibling()
	{
		mTransform.SetAsFirstSibling();
		mLayout.notifyObjectOrderChanged(mParent);
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
			mLayout.notifyObjectOrderChanged(mParent);
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
	public int getID() { return mID; }
	public GameLayout getLayout() { return mLayout; }
	public GameObject getObject() { return mObject; }
	public List<txUIObject> getChildList() { return mChildList; }
	public virtual bool isReceiveScreenMouse() { return mOnScreenMouseUp != null; }
	public txUIObject getParent() { return mParent; }
	public Transform getTransform() { return mTransform; }
	public RectTransform getRectTransform() { return mRectTransform; }
	public AudioSource getAudioSource() { return mAudioSource; }
	public UIDepth getUIDepth() { return new UIDepth(mLayout.getRenderOrder(), getDepth()); }
	public override bool isActive() { return mObject.activeSelf; }
	public bool isEnable() { return mEnable; }
	public T getUnityComponent<T>(bool addIfNotExist = true) where T : Component
	{
		T component = mObject.GetComponent<T>();
		if (component == null && addIfNotExist)
		{
			component = mObject.AddComponent<T>();
		}
		return component;
	}
	public void getUnityComponent<T>(out T component, bool addIfNotExist = true) where T : Component
	{
		component = mObject.GetComponent<T>();
		if (component == null && addIfNotExist)
		{
			component = mObject.AddComponent<T>();
		}
	}
	public Collider getCollider(bool addIfNull = false)
	{
		if (mBoxCollider == null && addIfNull)
		{
			mBoxCollider = mObject.AddComponent<BoxCollider>();
		}
		return mBoxCollider;
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
		logError("can not get window fill percent with txUIObject");
		return 1.0f;
	}
	public virtual int getDepth() { return mDepth; }
	public virtual bool isHandleInput() { return mBoxCollider != null && mBoxCollider.enabled; }
	public bool isPassRay() { return mPassRay; }
	public virtual Vector2 getWindowSize(bool transformed = false)
	{
		logError("can not get window size with txUIObject");
		return Vector2.zero;
	}
	// 是否支持递归改变子节点的透明度
	public virtual bool selfAlphaChild() { return false; }
	public bool isDragable() { return getComponent<WindowComponentDrag>(true, false) != null; }
	public bool isMouseHovered() { return mMouseHovered; }
	//set
	//-------------------------------------------------------------------------------------------------------------------------------------
	public void setParent(txUIObject parent)
	{
		if (mParent == parent)
		{
			return;
		}
		// 从原来的父节点上移除
		mParent?.removeChild(this);
		// 设置新的父节点
		mParent = parent;
		if (parent != null)
		{
			parent.addChild(this);
			if (mTransform.parent != parent.getTransform())
			{
				mTransform.SetParent(parent.getTransform());
			}
		}
	}
	protected void setGameObject(GameObject go)
	{
		setName(go.name);
		mObject = go;
		mTransform = mObject.transform;
	}
	public override void setName(string name)
	{
		base.setName(name);
		if (mObject != null && mObject.name != name)
		{
			mObject.name = name;
		}
	}
	public virtual void setDepth(int depth)
	{
		mDepth = depth;
		mGlobalTouchSystem.notifyWindowDepthChanged(this);
	}
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
	public void setEnable(bool enable) { mEnable = enable; }
	public override void setScale(Vector3 scale) { mTransform.localScale = scale; }
	public override void setPosition(Vector3 pos) { mTransform.localPosition = pos; }
	public override void setRotation(Vector3 rot) { mTransform.localEulerAngles = rot; }
	public override void setWorldRotation(Vector3 rot) { mTransform.eulerAngles = rot; }
	public override void setWorldPosition(Vector3 pos) { mTransform.position = pos; }
	public override void setWorldScale(Vector3 scale) { setScale(devideVector3(scale, mParent.getWorldScale())); }
	public virtual void setAlpha(float alpha, bool fadeChild) { }
	public virtual void setColor(Color color) { }
	public virtual void setFillPercent(float percent) { logError("can not set window fill percent with txUIObject"); }
	public void setPassRay(bool passRay) { mPassRay = passRay; }
	public virtual void setWindowSize(Vector2 size) { logError("can not set window size with txUIObject"); }
	public void setLongPressLengthThreshold(float threshold) { mLongPressLengthThreshold = threshold; }
	// 自己调用的callback,仅在启用自定义输入系统时生效
	public void setPreClickCallback(ObjectPreClickCallback callback, object userData) { mObjectPreClickCallback = callback; mObjectPreClickCallbackUserData = userData; }
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
	public void setReceiveDragCallback(OnReceiveDragCallback callback) { mReceiveDragCallback = callback; }
	public void setDragHoverCallback(OnDragHoverCallback callback) { mDragHoverCallback = callback; }
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
	public virtual void onMultiTouchStart(Vector2 touch0, Vector2 touch1)
	{
		mOnMultiTouchStart?.Invoke(touch0, touch1);
	}
	public virtual void onMultiTouchEnd()
	{
		mOnMultiTouchEnd?.Invoke();
	}
	public virtual void onMultiTouchMove(Vector2 touch0, Vector2 lastTouch0, Vector2 touch1, Vector2 lastTouch1)
	{
		mOnMultiTouchMove?.Invoke(touch0, lastTouch0, touch1, lastTouch1);
	}
	public virtual void onMouseEnter()
	{
		mMouseHovered = true;
		mHoverCallback?.Invoke(this, true);
		mOnMouseEnter?.Invoke(this);
	}
	public virtual void onMouseLeave()
	{
		mMouseHovered = false;
		mPressing = false;
		mPressedTime = -1.0f;
		mHoverCallback?.Invoke(this, false);
		mOnMouseLeave?.Invoke(this);
		mOnLongPressing?.Invoke(0.0f);
	}
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos)
	{
		mPressing = true;
		mPressedTime = 0.0f;
		mMouseDownPosition = mousePos;
		mPressCallback?.Invoke(this, true);
		mOnMouseDown?.Invoke(mousePos);
		mOnLongPressing?.Invoke(0.0f);
	}
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mPressCallback?.Invoke(this, false);
		if (lengthLess(mMouseDownPosition - mousePos, CommonDefine.CLICK_THRESHOLD))
		{
			mObjectPreClickCallback?.Invoke(this, mObjectPreClickCallbackUserData);
			mClickCallback?.Invoke(this);
			if (mDoubleClickCallback != null && mLastClickTime > 0.0f && mLastClickTime < CommonDefine.DOUBLE_CLICK_THRESHOLD)
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
		mOnMouseUp?.Invoke(mousePos);
		mOnLongPressing?.Invoke(0.0f);
	}
	// 鼠标在窗口内,并且有移动
	public virtual void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime)
	{
		mOnMouseMove?.Invoke(ref mousePos, ref moveDelta, moveTime);
	}
	// 鼠标在窗口内,但是不移动
	public virtual void onMouseStay(Vector3 mousePos)
	{
		mOnMouseStay?.Invoke(mousePos);
	}
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos)
	{
		mPressing = false;
		mPressedTime = -1.0f;
		mOnScreenMouseUp?.Invoke(this, mousePos);
	}
	// 鼠标在屏幕上按下
	public virtual void onScreenMouseDown(Vector3 mousePos) { }
	// 有物体拖动到了当前窗口上
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, ref bool continueEvent)
	{
		if (mReceiveDragCallback != null)
		{
			continueEvent = false;
			mReceiveDragCallback(dragObj, ref continueEvent);
		}
	}
	// 有物体拖动到了当前窗口上
	public virtual void onDragHoverd(IMouseEventCollect dragObj, bool hover)
	{
		mDragHoverCallback?.Invoke(dragObj, hover);
	}
	protected virtual void onWorldScaleChanged(Vector2 lastWorldScale) { }
}