using UnityEngine;
using static UnityUtility;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 可移动物体,表示一个3D物体
public class MovableObject : Transformable, IMouseEventCollect
{
	protected COMMovableObjectInteractive mCOMInteractive;
	protected COMMovableObjectMoveInfo mCOMMoveInfo;
	protected int mObjectID;                       // 物体的客户端ID
	protected bool mAutoManageObject;				// 是否自动管理GameObject,如果自动管理,则外部不能对其进行重新赋值或者销毁,否则会引起错误
	public MovableObject()
	{
		mObjectID = makeID();
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
	// get
	//------------------------------------------------------------------------------------------------------------------------------
	public Vector3 getPhysicsSpeed()									
	{
		if (mCOMMoveInfo == null)
		{
			logError("未启用移动信息组件,无法获取速度");
			return Vector3.zero;
		}
		return mCOMMoveInfo.getPhysicsSpeed(); 
	}
	public Vector3 getPhysicsAcceleration()								
	{
		if (mCOMMoveInfo == null)
		{
			logError("未启用移动信息组件,无法获取加速度");
			return Vector3.zero;
		}
		return mCOMMoveInfo.getPhysicsAcceleration(); 
	}
	public bool hasMovedDuringFrame()									
	{
		if (mCOMMoveInfo == null)
		{
			logError("未启用移动信息组件,无法获取");
			return false;
		}
		return mCOMMoveInfo.hasMovedDuringFrame(); 
	}
	public bool isEnableFixedUpdate()									
	{
		return mCOMMoveInfo != null && mCOMMoveInfo.isEnableFixedUpdate(); 
	}
	public Vector3 getMoveSpeedVector()									
	{
		if (mCOMMoveInfo == null)
		{
			logError("未启用移动信息组件,无法获取");
			return Vector3.zero;
		}
		return mCOMMoveInfo.getMoveSpeedVector(); 
	}
	public Vector3 getLastSpeedVector()									
	{
		if (mCOMMoveInfo == null)
		{
			logError("未启用移动信息组件,无法获取");
			return Vector3.zero;
		}
		return mCOMMoveInfo.getLastSpeedVector(); 
	}
	public Vector3 getLastPosition()									
	{
		if (mCOMMoveInfo == null)
		{
			logError("未启用移动信息组件,无法获取");
			return Vector3.zero;
		}
		return mCOMMoveInfo.getLastPosition(); 
	}
	public int getObjectID()											{ return mObjectID; }
	// 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
	public virtual UIDepth getDepth()									{ return null; }
	public virtual bool isHandleInput()									{ return mCOMInteractive != null && mCOMInteractive.isHandleInput(); }
	public virtual bool isReceiveScreenMouse()							{ return mCOMInteractive != null && mCOMInteractive.isReceiveScreenMouse(); }
	public virtual bool isPassRay()										{ return mCOMInteractive == null || mCOMInteractive.isPassRay(); }
	public virtual bool isMouseHovered()								{ return mCOMInteractive != null && mCOMInteractive.isMouseHovered(); }
	public virtual bool isDragable()									{ return getComponent<COMMovableObjectDrag>(true, false) != null; }
	public string getDescription()										{ return EMPTY; }
	// set
	//------------------------------------------------------------------------------------------------------------------------------
	public virtual void setPassRay(bool passRay)						{ getCOMInteractive().setPassRay(passRay); }
	public virtual void setHandleInput(bool handleInput)				{ getCOMInteractive().setHandleInput(handleInput); }
	public void setOnMouseEnter(OnMouseEnter callback)					{ getCOMInteractive().setOnMouseEnter(callback); }
	public void setOnMouseLeave(OnMouseLeave callback)					{ getCOMInteractive().setOnMouseLeave(callback); }
	public void setOnMouseDown(OnMouseDown callback)					{ getCOMInteractive().setOnMouseDown(callback); }
	public void setOnMouseUp(OnMouseUp callback)						{ getCOMInteractive().setOnMouseUp(callback); }
	public void setOnMouseMove(OnMouseMove callback)					{ getCOMInteractive().setOnMouseMove(callback); }
	public virtual void setClickCallback(ObjectClickCallback callback)	{ getCOMInteractive().setClickCallback(callback); }
	public virtual void setHoverCallback(ObjectHoverCallback callback)	{ getCOMInteractive().setHoverCallback(callback); }
	public virtual void setPressCallback(ObjectPressCallback callback)	{ getCOMInteractive().setPressCallback(callback); }
	public void setOnScreenMouseUp(OnScreenMouseUp callback)			{ getCOMInteractive().setOnScreenMouseUp(callback); }
	public void setAutoManageObject(bool autoManage)					{ mAutoManageObject = autoManage; }
	public override void resetProperty()
	{
		base.resetProperty();
		mAutoManageObject = false;
		// mObjectID不重置
		// mObjectID = 0;
	}
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
	// 鼠标在窗口内,并且有移动
	public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		getCOMInteractive().onMouseMove(mousePos, moveDelta, moveTime, touchID);
	}
	public virtual void onMouseStay(Vector3 mousePos, int touchID) 
	{
		getCOMInteractive().onMouseStay(mousePos, touchID);
	}
	public virtual void onScreenMouseDown(Vector3 mousePos, int touchID) 
	{
		getCOMInteractive().onScreenMouseDown(mousePos, touchID);
	}
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onScreenMouseUp(mousePos, touchID);
	}
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, BOOL continueEvent) 
	{
		getCOMInteractive().onReceiveDrag(dragObj, mousePos, continueEvent);
	}
	public virtual void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover) 
	{
		getCOMInteractive().onDragHoverd(dragObj, mousePos, hover);
	}
	public virtual void onMultiTouchStart(Vector3 touch0, Vector3 touch1) 
	{
		getCOMInteractive().onMultiTouchStart(touch0, touch1);
	}
	public virtual void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) 
	{
		getCOMInteractive().onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
	}
	public virtual void onMultiTouchEnd() 
	{
		getCOMInteractive().onMultiTouchEnd();
	}
	public void enableMoveInfo()
	{
		if (mCOMMoveInfo != null)
		{
			return;
		}
		mCOMMoveInfo = addComponent<COMMovableObjectMoveInfo>();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected COMMovableObjectInteractive getCOMInteractive()
	{
		if (mCOMInteractive == null)
		{
			mCOMInteractive = addComponent<COMMovableObjectInteractive>();
		}
		return mCOMInteractive;
	}
}