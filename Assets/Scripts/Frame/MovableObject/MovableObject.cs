using UnityEngine;

// 可移动物体,表示一个3D物体
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
	protected uint mObjectID;                       // 物体的客户端ID
	protected bool mEnableFixedUpdate;				// 是否启用FixedUpdate来计算Physics相关属性
	protected bool mAutoManageObject;				// 是否自动管理GameObject,如果自动管理,则外部不能对其进行重新赋值或者销毁,否则会引起错误
	protected bool mMovedDuringFrame;				// 角色在这一帧内是否移动过
	protected bool mHasLastPosition;				// mLastPosition是否有效
	protected bool mMouseHovered;					// 鼠标当前是否悬停在物体上
	protected bool mHandleInput;					// 是否接收鼠标输入事件
	protected bool mPassRay;						// 是否允许射线穿透
	public MovableObject()
	{
		mObjectID = makeID();
		mHandleInput = true;
		mPassRay = true;
		mEnableFixedUpdate = true;
	}
	public override void destroy()
	{
		// 自动创建的GameObject需要在此处自动销毁
		if (mAutoManageObject)
		{
			mGameObjectPool.destroyObject(mObject);
			mObject = null;
		}
		mGlobalTouchSystem.unregisteCollider(this);
		base.destroy();
	}
	// mObject需要外部自己创建以及销毁,内部只是引用,不会管理其生命周期
	// 且此函数需要在init之前调用,这样的话就能检测到setObject是否与mAutoCreateObject冲突,而且初始化也能够正常执行
	public override void setObject(GameObject obj)
	{
		if (obj != null && mObject != null)
		{
			logError("当前GameObject不为空,无法设置新的GameObject");
		}
		base.setObject(obj);
		mAudioSource = mObject?.GetComponent<AudioSource>();
	}
	public virtual void init()
	{
		// 自动创建GameObject
		if (mAutoManageObject)
		{
			setObject(mGameObjectPool.newObject());
		}
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
	//------------------------------------------------------------------------------------------------------------------------------
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
	public string getDescription()										{ return EMPTY; }
	// set
	//------------------------------------------------------------------------------------------------------------------------------
	public virtual void setPassRay(bool passRay)						{ mPassRay = passRay; }
	public virtual void setHandleInput(bool handleInput)				{ mHandleInput = handleInput; }
	public void setOnMouseEnter(OnMouseEnter callback)					{ mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback)					{ mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback)					{ mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback)						{ mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback)					{ mOnMouseMove = callback; }
	public virtual void setClickCallback(ObjectClickCallback callback)	{ mClickCallback = callback; }
	public virtual void setHoverCallback(ObjectHoverCallback callback)	{ mHoverCallback = callback; }
	public virtual void setPressCallback(ObjectPressCallback callback)	{ mPressCallback = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback)			{ mOnScreenMouseUp = callback; }
	public void setAutoManageObject(bool autoManage)					{ mAutoManageObject = autoManage; }
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
		mEnableFixedUpdate = true;
		mAutoManageObject = false;
		mMovedDuringFrame = false;
		mHasLastPosition = false;
		mMouseHovered = false;
		mHandleInput = true;
		mPassRay = true;
		mRealtimeMoveSpeed = 0.0f;
		// mObjectID不重置
		// mObjectID = 0;
	}
	public virtual void onMouseEnter(Vector3 mousePos, int touchID)
	{
		if (!mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(this, mousePos, true);
		}
		mOnMouseEnter?.Invoke(this, mousePos, touchID);
		mOnMouseMove?.Invoke(mousePos, Vector3.zero, 0.0f, touchID);
	}
	public virtual void onMouseLeave(Vector3 mousePos, int touchID)
	{
		if (mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(this, mousePos, false);
		}
		mOnMouseLeave?.Invoke(this, mousePos, touchID);
	}
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos, int touchID)
	{
		mMouseDownPosition = mousePos;
		mPressCallback?.Invoke(this, mousePos, true);
		mOnMouseDown?.Invoke(mousePos, touchID);

		// 如果是触屏的触点,则触点在当前物体内按下时,认为时开始悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && !mMouseHovered)
		{
			mMouseHovered = true;
			mHoverCallback?.Invoke(this, mousePos, true);
		}
	}
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos, int touchID)
	{
		mPressCallback?.Invoke(this, mousePos, false);
		if (lengthLess(mMouseDownPosition - mousePos, FrameDefine.CLICK_LENGTH))
		{
			mClickCallback?.Invoke(this, mousePos);
		}
		mOnMouseUp?.Invoke(mousePos, touchID);

		// 如果是触屏的触点,则触点在当前物体内抬起时,认为已经取消悬停
		TouchPoint touch = mInputSystem.getTouchPoint(touchID);
		if ((touch == null || !touch.isMouse()) && mMouseHovered)
		{
			mMouseHovered = false;
			mHoverCallback?.Invoke(this, mousePos, false);
		}
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
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, BOOL continueEvent) { }
	public virtual void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover) { }
	public virtual void onMultiTouchStart(Vector3 touch0, Vector3 touch1) { }
	public virtual void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) { }
	public virtual void onMultiTouchEnd() { }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addComponent<COMMovableObjectAudio>();
		addComponent<COMMovableObjectVolume>();
	}
}