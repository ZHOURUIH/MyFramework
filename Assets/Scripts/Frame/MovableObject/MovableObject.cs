using UnityEngine;

public class MovableObject : Transformable, IMouseEventCollect
{
	protected ObjectClickCallback mClickCallback;	// 鼠标点击物体的回调
	protected ObjectHoverCallback mHoverCallback;	// 鼠标悬停在物体上的持续回调
	protected ObjectPressCallback mPressCallback;	// 鼠标在物体上处于按下状态的持续回调
	protected OnScreenMouseUp mOnScreenMouseUp;		// 鼠标在任意位置抬起的回调
	protected OnMouseEnter mOnMouseEnter;			// 鼠标进入物体的回调
	protected OnMouseLeave mOnMouseLeave;			// 鼠标离开物体的回调
	protected OnMouseDown mOnMouseDown;				// 鼠标在物体上按下的回调
	protected OnMouseMove mOnMouseMove;				// 鼠标在物体上移动的回调
	protected OnMouseUp mOnMouseUp;					// 鼠标在物体上抬起的回调
	protected AudioSource mAudioSource;				// 音频组件
	protected Vector3 mLastPhysicsSpeedVector;		// FixedUpdate中上一帧的移动速度
	protected Vector3 mLastPhysicsPosition;			// 上一帧FixedUpdate中的位置
	protected Vector3 mPhysicsAcceleration;			// FixedUpdate中的加速度
	protected Vector3 mPhysicsSpeedVector;			// FixedUpdate中的移动速度
	protected Vector3 mMouseDownPosition;			// 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected Vector3 mCurFramePosition;			// 当前位置
	protected Vector3 mMoveSpeedVector;				// 当前移动速度向量,根据上一帧的位置和当前位置以及时间计算出来的实时速度
	protected Vector3 mLastSpeedVector;				// 上一帧的移动速度向量
	protected Vector3 mLastPosition;				// 上一帧的位置
	protected float mRealtimeMoveSpeed;				// 当前实时移动速率
	protected uint mObjectID;						// 物体的客户端ID
	protected bool mEnableFixedUpdate;				// 是否启用FixedUpdate来计算Physics相关属性
	protected bool mMovedDuringFrame;				// 角色在这一帧内是否移动过
	protected bool mHasLastPosition;				// mLastPosition是否有效
	protected bool mDestroyObject;					// 如果是外部管理的节点,则一定不要在MovableObject自动销毁
	protected bool mMouseHovered;					// 鼠标当前是否悬停在物体上
	protected bool mHandleInput;					// 是否接收鼠标输入事件
	protected bool mPassRay;						// 是否允许射线穿透
	public MovableObject()
	{
		mObjectID = makeID();
		mDestroyObject = true;
		mHandleInput = true;
		mPassRay = true;
		mEnableFixedUpdate = true;
	}
	public override void destroy()
	{
		// 因为在基类中会重置mObject,所以需要先保存一个
		GameObject obj = mObject;
		mGlobalTouchSystem.unregisteCollider(this);
		base.destroy();
		if (mDestroyObject)
		{
			destroyGameObject(ref obj);
		}
	}
	public virtual void setObject(GameObject obj, bool destroyOld)
	{
		if (destroyOld && mObject != null)
		{
			destroyGameObject(ref mObject);
		}
		setGameObject(obj);
		mAudioSource = mObject?.GetComponent<AudioSource>();
		if (mObject != null)
		{
			mObject.name = mName;
		}
	}
	public virtual void init()
	{
		initComponents();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isDestroy())
		{
			return;
		}
		if (elapsedTime > 0.0f)
		{
			mLastPosition = mCurFramePosition;
			mCurFramePosition = getPosition();
			mLastSpeedVector = mMoveSpeedVector;
			mMoveSpeedVector = mHasLastPosition ? (mCurFramePosition - mLastPosition) / elapsedTime : Vector3.zero;
			mRealtimeMoveSpeed = getLength(mMoveSpeedVector);
			mMovedDuringFrame = !isVectorEqual(mLastPosition, mCurFramePosition) && mHasLastPosition;
			mHasLastPosition = true;
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		if (!mEnableFixedUpdate)
		{
			return;
		}
		base.fixedUpdate(elapsedTime);
		Vector3 curPos = mTransform.localPosition;
		mPhysicsSpeedVector = (curPos - mLastPhysicsPosition) / elapsedTime;
		mLastPhysicsPosition = curPos;
		mPhysicsAcceleration = (mPhysicsSpeedVector - mLastPhysicsSpeedVector) / elapsedTime;
		mLastPhysicsSpeedVector = mPhysicsSpeedVector;
	}
	public AudioSource createAudioSource()
	{
		return mAudioSource = mObject.AddComponent<AudioSource>();
	}
	// get
	//-------------------------------------------------------------------------------------------------------------------------
	public AudioSource getAudioSource()									{ return mAudioSource; }
	public Vector3 getPhysicsSpeed()									{ return mPhysicsSpeedVector; }
	public Vector3 getPhysicsAcceleration()								{ return mPhysicsAcceleration; }
	public uint getObjectID()											{ return mObjectID; }
	public bool hasMovedDuringFrame()									{ return mMovedDuringFrame; }
	public bool isEnableFixedUpdate()									{ return mEnableFixedUpdate; }
	public Vector3 getMoveSpeedVector()									{ return mMoveSpeedVector; }
	public Vector3 getLastSpeedVector()									{ return mLastSpeedVector; }
	public Vector3 getLastPosition()									{ return mLastPosition; }
	public virtual bool isHandleInput()									{ return mHandleInput; }
	// 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
	public virtual UIDepth getDepth()									{ return null; }
	public virtual bool isReceiveScreenMouse()							{ return mOnScreenMouseUp != null; }
	public virtual bool isPassRay()										{ return mPassRay; }
	public virtual bool isDragable()									{ return getComponent<COMMovableObjectDrag>(true, false) != null; }
	public virtual bool isMouseHovered()								{ return mMouseHovered; }
	// set
	//-------------------------------------------------------------------------------------------------------------------------
	public virtual void setPassRay(bool passRay)						{ mPassRay = passRay; }
	public virtual void setHandleInput(bool handleInput)				{ mHandleInput = handleInput; }
	public void setDestroyObject(bool value)							{ mDestroyObject = value; }
	public void setOnMouseEnter(OnMouseEnter callback)					{ mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback)					{ mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback)					{ mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback)						{ mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback)					{ mOnMouseMove = callback; }
	public virtual void setClickCallback(ObjectClickCallback callback)	{ mClickCallback = callback; }
	public virtual void setHoverCallback(ObjectHoverCallback callback)	{ mHoverCallback = callback; }
	public virtual void setPressCallback(ObjectPressCallback callback)	{ mPressCallback = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback)			{ mOnScreenMouseUp = callback; }
	public override void resetProperty()
	{
		base.resetProperty();
		// 重置所有成员变量
		mPhysicsAcceleration = Vector3.zero;
		mLastPhysicsSpeedVector = Vector3.zero;
		mPhysicsSpeedVector = Vector3.zero;
		mLastPhysicsPosition = Vector3.zero;
		mCurFramePosition = Vector3.zero;
		mLastPosition = Vector3.zero;
		mMouseDownPosition = Vector3.zero;
		mLastSpeedVector = Vector3.zero;
		mMoveSpeedVector = Vector3.zero;
		mAudioSource = null;
		mOnMouseEnter = null;
		mOnMouseLeave = null;
		mOnMouseDown = null;
		mOnMouseUp = null;
		mOnMouseMove = null;
		mClickCallback = null;
		mHoverCallback = null;
		mPressCallback = null;
		mOnScreenMouseUp = null;
		mMovedDuringFrame = false;
		mHasLastPosition = false;
		mDestroyObject = true;
		mMouseHovered = false;
		mHandleInput = true;
		mPassRay = true;
		mEnableFixedUpdate = true;
		mRealtimeMoveSpeed = 0.0f;
		// mObjectID不重置
		//mObjectID = 0;
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
		mHoverCallback?.Invoke(this, false);
		mOnMouseLeave?.Invoke(this, touchID);
	}
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos, int touchID)
	{
		mMouseDownPosition = mousePos;
		mPressCallback?.Invoke(this, true);
		mOnMouseDown?.Invoke(mousePos, touchID);
	}
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos, int touchID)
	{
		mPressCallback?.Invoke(this, false);
		if (lengthLess(mMouseDownPosition - mousePos, FrameDefine.CLICK_THRESHOLD))
		{
			mClickCallback?.Invoke(this);
		}
		mOnMouseUp?.Invoke(mousePos, touchID);
	}
	// 鼠标在窗口内,并且有移动
	public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		mOnMouseMove?.Invoke(mousePos, moveDelta, moveTime, touchID);
	}
	public virtual void onMouseStay(Vector3 mousePos, int touchID) { }
	public virtual void onScreenMouseDown(Vector3 mousePos, int touchID) { }
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		mOnScreenMouseUp?.Invoke(this, mousePos, touchID);
	}
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, BOOL continueEvent) { }
	public virtual void onDragHoverd(IMouseEventCollect dragObj, bool hover) { }
	public virtual void onMultiTouchStart(Vector3 touch0, Vector3 touch1) { }
	public virtual void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) { }
	public virtual void onMultiTouchEnd() { }
}