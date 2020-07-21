using UnityEngine;
using System.Collections;

public class MovableObject : Transformable, IMouseEventCollect
{
	protected GameObject mObject;
	protected Transform mTransform;
	protected AudioSource mAudioSource;
	protected ObjectClickCallback mClickCallback;
	protected ObjectHoverCallback mHoverCallback;
	protected ObjectPressCallback mPressCallback;
	protected OnScreenMouseUp mOnScreenMouseUp;
	protected OnMouseEnter mOnMouseEnter;
	protected OnMouseLeave mOnMouseLeave;
	protected OnMouseDown mOnMouseDown;
	protected OnMouseUp mOnMouseUp;
	protected OnMouseMove mOnMouseMove;
	protected Vector3 mPhysicsAcceleration; // FixedUpdate中的加速度
	protected Vector3 mLastPhysicsSpeed;    // FixedUpdate中上一帧的移动速度
	protected Vector3 mPhysicsSpeed;		// FixedUpdate中的移动速度
	protected Vector3 mLastPhysicsPosition;	// 上一帧FixedUpdate中的位置
	protected Vector3 mCurFramePosition;	// 当前位置
	protected Vector3 mLastPosition;		// 上一帧的位置
	protected Vector3 mMoveSpeed;			// 当前移动速度
	protected Vector3 mLastSpeed;			// 上一帧的移动速度
	protected Vector3 mMouseDownPosition;   // 鼠标按下时在窗口中的位置,鼠标在窗口中移动时该值不改变
	protected bool mMovedDuringFrame;		// 角色在这一帧内是否移动过
	protected bool mHasLastPosition;		// mLastPosition是否有效
	protected bool mDestroyObject;			// 如果是外部管理的节点,则一定不要在MovableObject自动销毁
	protected bool mDestroied;				// 物体是否已经销毁
	protected bool mMouseHovered;			// 鼠标当前是否悬停在物体上
	protected bool mHandleInput;			// 是否接收鼠标输入事件
	protected bool mPassRay;                // 是否允许射线穿透
	protected bool mEnableFixedUpdate;		// 是否启用FixedUpdate来计算Physics相关属性
	protected int mObjectID;				// 物体的客户端ID
	public MovableObject(string name)
		: base(name)
	{
		mObjectID = makeID();
		mDestroyObject = true;
		mHandleInput = true;
		mPassRay = true;
		mEnableFixedUpdate = true;
	}
	public override void destroy()
	{
		base.destroy();
		mGlobalTouchSystem.unregisteBoxCollider(this);
		destroyAllComponents();
		if(mDestroyObject)
		{
			destroyGameObject(ref mObject);
		}
		mTransform = null;
		mDestroied = true;
	}
	public virtual void setObject(GameObject obj, bool destroyOld = true)
	{
		if(destroyOld && mObject != null)
		{
			destroyGameObject(ref mObject);
		}
		mObject = obj;
		if(mObject != null)
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
		mDestroied = false;
		if(mObjectID == -1)
		{
			mObjectID = makeID();
		}
		initComponents();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if(elapsedTime > 0.0f)
		{
			mLastPosition = mCurFramePosition;
			mCurFramePosition = getPosition();
			mLastSpeed = mMoveSpeed;
			mMoveSpeed = mHasLastPosition ? (mCurFramePosition - mLastPosition) / elapsedTime : Vector3.zero;
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
		mPhysicsSpeed = (curPos - mLastPhysicsPosition) / elapsedTime;
		mLastPhysicsPosition = curPos;
		mPhysicsAcceleration = (mPhysicsSpeed - mLastPhysicsSpeed) / elapsedTime;
		mLastPhysicsSpeed = mPhysicsSpeed;
	}
	public bool isDestroied() { return mDestroied; }
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
		if(mObject == null)
		{
			return null;
		}
		if(mObject.GetComponent<T>() != null)
		{
			return mObject.GetComponent<T>();
		}
		return mObject.AddComponent<T>();
	}
	// get
	//-------------------------------------------------------------------------------------------------------------------------
	public AudioSource getAudioSource() { return mAudioSource; }
	public GameObject getObject() { return mObject; }
	public bool isUnityComponentEnabled<T>() where T : Behaviour
	{
		T component = getUnityComponent<T>(false);
		return component != null && component.enabled;
	}
	public void enableUnityComponent<T>(bool enable) where T : Behaviour
	{
		T component = getUnityComponent<T>();
		if(component != null)
		{
			component.enabled = enable;
		}
	}
	public T getUnityComponent<T>(bool addIfNotExist = true) where T : Component
	{
		T component = mObject.GetComponent<T>();
		if(component == null && addIfNotExist)
		{
			component = mObject.AddComponent<T>();
		}
		return component;
	}
	public void getUnityComponent<T>(out T component, bool addIfNotExist = true) where T : Component
	{
		component = mObject.GetComponent<T>();
		if(component == null && addIfNotExist)
		{
			component = mObject.AddComponent<T>();
		}
	}
	public T[] getUnityComponentsInChild<T>(bool includeInactive = true) where T : Component
	{
		return mObject.GetComponentsInChildren<T>(includeInactive);
	}
	public T getUnityComponentInChild<T>(string childName) where T : Component
	{
		GameObject child = getGameObject(mObject, childName);
		return child.GetComponent<T>();
	}
	public override Vector3 getPosition() { return mTransform.localPosition; }
	public override Vector3 getRotation() { return mTransform.localEulerAngles; }
	public override Vector3 getScale() { return mTransform.localScale; }
	public override Vector3 getWorldPosition() { return mTransform.position; }
	public override Vector3 getWorldScale() { return mTransform.lossyScale; }
	public override Vector3 getWorldRotation() { return mTransform.rotation.eulerAngles; }
	public float getYaw() { return getRotation().y; }
	public float getPitch() { return getRotation().x; }
	public float getRoll() { return getRotation().z; }
	public Vector3 getPhysicsSpeed() { return mPhysicsSpeed; }
	public Vector3 getPhysicsAcceleration() { return mPhysicsAcceleration; }
	public Quaternion getQuaternionRotation() { return mTransform.localRotation; }
	public Quaternion getWorldQuaternionRotation() { return mTransform.rotation; }
	public string getLayer() { return LayerMask.LayerToName(mObject.layer); }
	public int getLayerInt() { return mObject.layer; }
	public Transform getTransform() { return mTransform; }
	public override bool isActive() { return mObject.activeSelf; }
	public int getObjectID() { return mObjectID; }
	public bool hasMovedDuringFrame() { return mMovedDuringFrame; }
	public bool isEnableFixedUpdate() { return mEnableFixedUpdate; }
	public Vector3 getMoveSpeed() { return mMoveSpeed; }
	public Vector3 getLastSpeed() { return mLastSpeed; }
	public Vector3 getLastPosition() { return mLastPosition; }
	public Vector3 getLeft(bool ignoreY = false)
	{
		Vector3 left = localToWorldDirection(Vector3.left);
		if(ignoreY)
		{
			left = normalize(replaceY(left, 0.0f));
		}
		return left;
	}
	public Vector3 getRight(bool ignoreY = false)
	{
		Vector3 right = localToWorldDirection(Vector3.right);
		if(ignoreY)
		{
			right = normalize(replaceY(right, 0.0f));
		}
		return right;
	}
	public Vector3 getBack(bool ignoreY = false)
	{
		Vector3 back = localToWorldDirection(Vector3.back);
		if(ignoreY)
		{
			back = normalize(replaceY(back, 0.0f));
		}
		return back;
	}
	public Vector3 getForward(bool ignoreY = false)
	{
		Vector3 forward = localToWorldDirection(Vector3.forward);
		if(ignoreY)
		{
			forward = normalize(replaceY(forward, 0.0f));
		}
		return forward;
	}
	public Collider[] getColliders() { return mObject.GetComponents<Collider>(); }
	public Collider2D[] getColliders2D() { return mObject.GetComponents<Collider2D>(); }
	public virtual bool isHandleInput() { return mHandleInput; }
	// 返回第一个碰撞体
	public Collider getCollider(bool addIfNull = false)
	{
		var colliders = getColliders();
		if(colliders != null && colliders.Length > 0)
		{
			return colliders[0];
		}
		return null;
	}
	// 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
	public UIDepth getUIDepth() { return new UIDepth(); }
	public bool isReceiveScreenMouse() { return mOnScreenMouseUp != null; }
	public bool isPassRay() { return mPassRay; }
	public bool isDragable() { return getComponent<MovableObjectComponentDrag>(true, false) != null; }
	public bool isMouseHovered() { return mMouseHovered; }
	// set
	//-------------------------------------------------------------------------------------------------------------------------
	public void setPassRay(bool passRay) { mPassRay = passRay; }
	public virtual void setHandleInput(bool handleInput) { mHandleInput = handleInput; }
	public override void setName(string name)
	{
		base.setName(name);
		if(mObject != null && mObject.name != name)
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
		if(mTransform.parent != null)
		{
			mTransform.localScale = devideVector3(scale, mTransform.parent.lossyScale);
		}
		else
		{
			mTransform.localScale = scale;
		}
	}
	public void setPositionY(float y)
	{
		setPosition(replaceY(getPosition(), y));
	}
	public void setRotationY(float rotY)
	{
		mTransform.localEulerAngles = replaceY(mTransform.localEulerAngles, rotY);
	}
	public void setRotationX(float rotX)
	{
		mTransform.localEulerAngles = replaceX(mTransform.localEulerAngles, rotX);
	}
	public void setPitch(float pitch) { setRotation(replaceX(getRotation(), pitch)); }
	public void setYaw(float yaw) { setRotation(replaceY(getRotation(), yaw)); }
	public void setScaleX(float x) { mTransform.localScale = replaceX(mTransform.localScale, x); }
	public virtual void move(Vector3 moveDelta, Space space = Space.Self)
	{
		if(space == Space.Self)
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
	public void lookAt(Vector3 lookat)
	{
		setRotation(getLookAtRotation(lookat));
	}
	public void yawpitch(float fYaw, float fPitch)
	{
		Vector3 curRot = getRotation();
		curRot.x += fPitch;
		curRot.y += fYaw;
		setRotation(curRot);
	}
	public void resetLocalTransform()
	{
		mTransform.localPosition = Vector3.zero;
		mTransform.localEulerAngles = Vector3.zero;
		mTransform.localScale = Vector3.one;
	}
	public void setParent(GameObject parent, bool resetTrans = true)
	{
		if(parent == null)
		{
			mTransform.parent = null;
		}
		else
		{
			mTransform.parent = parent.transform;
		}
		if(resetTrans)
		{
			resetLocalTransform();
		}
	}
	public void setOnMouseEnter(OnMouseEnter callback) { mOnMouseEnter = callback; }
	public void setOnMouseLeave(OnMouseLeave callback) { mOnMouseLeave = callback; }
	public void setOnMouseDown(OnMouseDown callback) { mOnMouseDown = callback; }
	public void setOnMouseUp(OnMouseUp callback) { mOnMouseUp = callback; }
	public void setOnMouseMove(OnMouseMove callback) { mOnMouseMove = callback; }
	public void setClickCallback(ObjectClickCallback callback) { mClickCallback = callback; }
	public void setHoverCallback(ObjectHoverCallback callback) { mHoverCallback = callback; }
	public void setPressCallback(ObjectPressCallback callback) { mPressCallback = callback; }
	public void setOnScreenMouseUp(OnScreenMouseUp callback) { mOnScreenMouseUp = callback; }
	public void copyObjectTransform(GameObject obj)
	{
		Transform objTrans = obj.transform;
		OT.MOVE(this, objTrans.localPosition);
		OT.ROTATE(this, objTrans.localEulerAngles);
		OT.SCALE(this, objTrans.localScale);
	}
	public void enableAllColliders(bool enable)
	{
		var colliders = mObject.GetComponents<Collider>();
		foreach(var item in colliders)
		{
			item.enabled = enable;
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		// 重置所有成员变量
		mObjectID = -1;
		mPhysicsAcceleration = Vector3.zero;
		mLastPhysicsSpeed = Vector3.zero;
		mPhysicsSpeed = Vector3.zero;
		mLastPhysicsPosition = Vector3.zero;
		mCurFramePosition = Vector3.zero;
		mLastPosition = Vector3.zero;
		mMouseDownPosition = Vector3.zero;
		mLastSpeed = Vector3.zero;
		mMoveSpeed = Vector3.zero;
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
		mMovedDuringFrame = false;
		mHasLastPosition = false;
		mDestroyObject = true;
		mDestroied = false;
		mMouseHovered = false;
		mHandleInput = true;
		mPassRay = false;
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
		mHoverCallback?.Invoke(this, false);
		mOnMouseLeave?.Invoke(this);
	}
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos)
	{
		mMouseDownPosition = mousePos;
		mPressCallback?.Invoke(this, true);
		mOnMouseDown?.Invoke(mousePos);
	}
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos)
	{
		mPressCallback?.Invoke(this, false);
		if(lengthLess(mMouseDownPosition - mousePos, CommonDefine.CLICK_THRESHOLD))
		{
			mClickCallback?.Invoke(this);
		}
		mOnMouseUp?.Invoke(mousePos);
	}
	// 鼠标在窗口内,并且有移动
	public virtual void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime)
	{
		mOnMouseMove?.Invoke(ref mousePos, ref moveDelta, moveTime);
	}
	public virtual void onMouseStay(Vector3 mousePos) { }
	public virtual void onScreenMouseDown(Vector3 mousePos) { }
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos)
	{
		mOnScreenMouseUp?.Invoke(this, mousePos);
	}
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, ref bool continueEvent) { }
	public virtual void onDragHoverd(IMouseEventCollect dragObj, bool hover) { }
	public virtual void onMultiTouchStart(Vector2 touch0, Vector2 touch1) { }
	public virtual void onMultiTouchMove(Vector2 touch0, Vector2 lastTouch0, Vector2 touch1, Vector2 lastTouch1) { }
	public virtual void onMultiTouchEnd() { }
	public virtual void setAlpha(float alpha)
	{
		Renderer[] renderers = getUnityComponentsInChild<Renderer>();
		foreach(var item in renderers)
		{
			Color color = item.material.color;
			color.a = alpha;
			item.material.color = color;
		}
	}
	public virtual float getAlpha()
	{
		return getUnityComponent<Renderer>().material.color.a;
	}
}