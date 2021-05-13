using UnityEngine;

public class MovableObject : Transformable, IMouseEventCollect
{
	protected ObjectClickCallback mClickCallback;
	protected ObjectHoverCallback mHoverCallback;
	protected ObjectPressCallback mPressCallback;
	protected OnScreenMouseUp mOnScreenMouseUp;
	protected OnMouseEnter mOnMouseEnter;
	protected OnMouseLeave mOnMouseLeave;
	protected OnMouseDown mOnMouseDown;
	protected OnMouseMove mOnMouseMove;
	protected OnMouseUp mOnMouseUp;
	protected AudioSource mAudioSource;
	protected GameObject mObject;
	protected Transform mTransform;
	protected Vector3 mLastPhysicsSpeedVector;  // FixedUpdate中上一帧的移动速度
	protected Vector3 mLastPhysicsPosition;     // 上一帧FixedUpdate中的位置
	protected Vector3 mPhysicsAcceleration;     // FixedUpdate中的加速度
	protected Vector3 mPhysicsSpeedVector;      // FixedUpdate中的移动速度
	protected Vector3 mMouseDownPosition;       // 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected Vector3 mCurFramePosition;        // 当前位置
	protected Vector3 mMoveSpeedVector;         // 当前移动速度向量,根据上一帧的位置和当前位置以及时间计算出来的实时速度
	protected Vector3 mLastSpeedVector;         // 上一帧的移动速度向量
	protected Vector3 mLastPosition;            // 上一帧的位置
	protected float mRealtimeMoveSpeed;			// 当前实时移动速率
	protected uint mObjectID;                   // 物体的客户端ID
	protected bool mMovedDuringFrame;           // 角色在这一帧内是否移动过
	protected bool mHasLastPosition;            // mLastPosition是否有效
	protected bool mDestroyObject;              // 如果是外部管理的节点,则一定不要在MovableObject自动销毁
	protected bool mMouseHovered;               // 鼠标当前是否悬停在物体上
	protected bool mHandleInput;                // 是否接收鼠标输入事件
	protected bool mPassRay;                    // 是否允许射线穿透
	protected bool mEnableFixedUpdate;          // 是否启用FixedUpdate来计算Physics相关属性
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
		mGlobalTouchSystem.unregisteCollider(this);
		base.destroy();
		if (mDestroyObject)
		{
			destroyGameObject(ref mObject);
		}
		mTransform = null;
		mDestroy = true;
	}
	public virtual void setObject(GameObject obj, bool destroyOld = true)
	{
		if (destroyOld && mObject != null)
		{
			destroyGameObject(ref mObject);
		}
		mObject = obj;
		if (mObject != null)
		{
			mObject.name = mName;
			mTransform = mObject.transform;
			mAudioSource = mObject.GetComponent<AudioSource>();
		}
		else
		{
			mTransform = null;
			mAudioSource = null;
		}
	}
	public virtual void init()
	{
		mDestroy = false;
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
	public override Vector3 localToWorld(Vector3 point) { return localToWorld(mTransform, point); }
	public override Vector3 worldToLocal(Vector3 point) { return worldToLocal(mTransform, point); }
	public override Vector3 localToWorldDirection(Vector3 direction) { return localToWorldDirection(mTransform, direction); }
	public override Vector3 worldToLocalDirection(Vector3 direction) { return worldToLocalDirection(mTransform, direction); }
	public T addUnityComponent<T>() where T : Component
	{
		if (mObject == null)
		{
			return null;
		}
		if (mObject.GetComponent<T>() != null)
		{
			return mObject.GetComponent<T>();
		}
		return mObject.AddComponent<T>();
	}
	// get
	//-------------------------------------------------------------------------------------------------------------------------
	public AudioSource getAudioSource() { return mAudioSource; }
	public virtual GameObject getObject() { return mObject; }
	public bool isUnityComponentEnabled<T>() where T : Behaviour
	{
		T com = getUnityComponent<T>(false);
		return com != null && com.enabled;
	}
	public void enableUnityComponent<T>(bool enable) where T : Behaviour
	{
		T com = getUnityComponent<T>();
		if (com != null)
		{
			com.enabled = enable;
		}
	}
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
	public T[] getUnityComponentsInChild<T>(bool includeInactive = true) where T : Component
	{
		return mObject.GetComponentsInChildren<T>(includeInactive);
	}
	public T getUnityComponentInChild<T>(string childName) where T : Component
	{
		GameObject child = getGameObject(childName, mObject);
		return child.GetComponent<T>();
	}
	public override Vector3 getPosition() { return mTransform.localPosition; }
	public override Vector3 getRotation() { return mTransform.localEulerAngles; }
	public override Vector3 getScale() { return mTransform.localScale; }
	public override Vector3 getWorldPosition() { return mTransform.position; }
	public override Vector3 getWorldScale() { return mTransform.lossyScale; }
	public override Vector3 getWorldRotation() { return mTransform.rotation.eulerAngles; }
	public Vector3 getPhysicsSpeed() { return mPhysicsSpeedVector; }
	public Vector3 getPhysicsAcceleration() { return mPhysicsAcceleration; }
	public Quaternion getQuaternionRotation() { return mTransform.localRotation; }
	public Quaternion getWorldQuaternionRotation() { return mTransform.rotation; }
	public string getLayer() { return LayerMask.LayerToName(mObject.layer); }
	public int getLayerInt() { return mObject.layer; }
	public Transform getTransform() { return mTransform; }
	public override bool isActive() { return mObject.activeSelf; }
	public virtual bool isActiveInHierarchy() { return mObject.activeInHierarchy; }
	public uint getObjectID() { return mObjectID; }
	public bool hasMovedDuringFrame() { return mMovedDuringFrame; }
	public bool isEnableFixedUpdate() { return mEnableFixedUpdate; }
	public Vector3 getMoveSpeedVector() { return mMoveSpeedVector; }
	public Vector3 getLastSpeedVector() { return mLastSpeedVector; }
	public Vector3 getLastPosition() { return mLastPosition; }
	public Vector3 getLeft(bool ignoreY = false)
	{
		Vector3 left = localToWorldDirection(Vector3.left);
		return ignoreY ? normalize(resetY(left)) : left;
	}
	public Vector3 getRight(bool ignoreY = false)
	{
		Vector3 right = localToWorldDirection(Vector3.right);
		return ignoreY ? normalize(resetY(right)) : right;
	}
	public Vector3 getBack(bool ignoreY = false)
	{
		Vector3 back = localToWorldDirection(Vector3.back);
		return ignoreY ? normalize(resetY(back)) : back;
	}
	public Vector3 getForward(bool ignoreY = false)
	{
		Vector3 forward = localToWorldDirection(Vector3.forward);
		return ignoreY ? normalize(resetY(forward)) : forward;
	}
	public Collider[] getColliders() { return mObject.GetComponents<Collider>(); }
	public Collider2D[] getColliders2D() { return mObject.GetComponents<Collider2D>(); }
	public virtual bool isHandleInput() { return mHandleInput; }
	// 返回第一个碰撞体
	public virtual Collider getCollider()
	{
		var colliders = getColliders();
		if (colliders != null && colliders.Length > 0)
		{
			return colliders[0];
		}
		return null;
	}
	public bool raycast(ref Ray ray, out RaycastHit hit, float maxDistance)
	{
		Collider collider = getCollider();
		if (collider == null)
		{
			hit = new RaycastHit();
			return false;
		}
		return collider.Raycast(ray, out hit, maxDistance);
	}
	// 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
	public virtual UIDepth getDepth() { return null; }
	public virtual bool isReceiveScreenMouse() { return mOnScreenMouseUp != null; }
	public virtual bool isPassRay() { return mPassRay; }
	public virtual bool isDragable() { return getComponent<COMMovableObjectDrag>(true, false) != null; }
	public virtual bool isMouseHovered() { return mMouseHovered; }
	public virtual bool isChildOf(IMouseEventCollect parent)
	{
		var obj = parent as MovableObject;
		if (obj == null)
		{
			return false;
		}
		return mTransform.IsChildOf(obj.getTransform());
	}
	// set
	//-------------------------------------------------------------------------------------------------------------------------
	public virtual void setPassRay(bool passRay) { mPassRay = passRay; }
	public virtual void setHandleInput(bool handleInput) { mHandleInput = handleInput; }
	public override void setName(string name)
	{
		base.setName(name);
		if (mObject != null && mObject.name != name)
		{
			mObject.name = name;
		}
	}
	public void setDestroyObject(bool value) { mDestroyObject = value; }
	public override void setActive(bool active)
	{
		mObject.SetActive(active);
		base.setActive(active);
	}
	public void resetActive()
	{
		// 重新禁用再启用,可以重置状态
		mObject.SetActive(false);
		mObject.SetActive(true);
	}
	public override void setPosition(Vector3 pos) { mTransform.localPosition = pos; }
	public override void setScale(Vector3 scale) { mTransform.localScale = scale; }
	// 角度制的欧拉角,分别是绕xyz轴的旋转角度
	public override void setRotation(Vector3 rot) { mTransform.localEulerAngles = rot; }
	public override void setWorldPosition(Vector3 pos) { mTransform.position = pos; }
	public override void setWorldRotation(Vector3 rot) { mTransform.eulerAngles = rot; }
	public override void setWorldScale(Vector3 scale)
	{
		if (mTransform.parent != null)
		{
			mTransform.localScale = devideVector3(scale, mTransform.parent.lossyScale);
		}
		else
		{
			mTransform.localScale = scale;
		}
	}
	public void setPositionX(float x) { setPosition(replaceX(getPosition(), x)); }
	public void setPositionY(float y) { setPosition(replaceY(getPosition(), y)); }
	public void setPositionZ(float z) { setPosition(replaceZ(getPosition(), z)); }
	public void setRotationX(float rotX) { setRotation(replaceX(mTransform.localEulerAngles, rotX)); }
	public void setRotationY(float rotY) { setRotation(replaceY(mTransform.localEulerAngles, rotY)); }
	public void setRotationZ(float rotZ) { setRotation(replaceZ(mTransform.localEulerAngles, rotZ)); }
	public void setScaleX(float x) { setScale(replaceX(mTransform.localScale, x)); }
	public virtual void move(Vector3 moveDelta, Space space = Space.Self)
	{
		if (space == Space.Self)
		{
			moveDelta = rotateVector3(moveDelta, getQuaternionRotation());
		}
		setPosition(getPosition() + moveDelta);
	}
	public void rotate(Vector3 rotation) { mTransform.Rotate(rotation, Space.Self); }
	public void rotateWorld(Vector3 rotation) { mTransform.Rotate(rotation, Space.World); }
	// angle为角度制
	public void rotateAround(Vector3 axis, float angle) { mTransform.Rotate(axis, angle, Space.Self); }
	public void rotateAroundWorld(Vector3 axis, float angle) { mTransform.Rotate(axis, angle, Space.World); }
	public void lookAt(Vector3 direction) { setRotation(getLookAtRotation(direction)); }
	public void lookAtPoint(Vector3 point) { setRotation(getLookAtRotation(point - getPosition())); }
	public void yawPitch(float yaw, float pitch)
	{
		Vector3 curRot = getRotation();
		curRot.x += pitch;
		curRot.y += yaw;
		setRotation(curRot);
	}
	public void resetTransform()
	{
		mTransform.localPosition = Vector3.zero;
		mTransform.localEulerAngles = Vector3.zero;
		mTransform.localScale = Vector3.one;
	}
	public void setParent(GameObject parent, bool resetTrans = true)
	{
		if (parent == null)
		{
			mTransform.parent = null;
		}
		else
		{
			mTransform.parent = parent.transform;
		}
		if (resetTrans)
		{
			resetTransform();
		}
	}
	public void setOnMouseEnter(OnMouseEnter callback) { mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback) { mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback) { mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback) { mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback) { mOnMouseMove = callback; }
	public virtual void setClickCallback(ObjectClickCallback callback) { mClickCallback = callback; }
	public virtual void setHoverCallback(ObjectHoverCallback callback) { mHoverCallback = callback; }
	public virtual void setPressCallback(ObjectPressCallback callback) { mPressCallback = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback) { mOnScreenMouseUp = callback; }
	public void copyObjectTransform(GameObject obj)
	{
		Transform objTrans = obj.transform;
		FT.MOVE(this, objTrans.localPosition);
		FT.ROTATE(this, objTrans.localEulerAngles);
		FT.SCALE(this, objTrans.localScale);
	}
	public void enableAllColliders(bool enable)
	{
		var colliders = mObject.GetComponents<Collider>();
		int count = colliders.Length;
		for (int i = 0; i < count; ++i)
		{
			colliders[i].enabled = enable;
		}
	}
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
		mObject = null;
		mTransform = null;
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
	public virtual void setAlpha(float alpha)
	{
		var renderers = getUnityComponentsInChild<Renderer>();
		int count = renderers.Length;
		for (int i = 0; i < count; ++i)
		{
			Material material = renderers[i].material;
			Color color = material.color;
			color.a = alpha;
			material.color = color;
		}
	}
	public virtual float getAlpha()
	{
		return getUnityComponent<Renderer>().material.color.a;
	}
}