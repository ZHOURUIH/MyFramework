using System;
using UnityEngine;
using static UnityUtility;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 可移动物体,表示一个3D物体
public class MovableObject : Transformable, IMouseEventCollect
{
	protected COMMovableObjectInteractive mCOMInteractive;	// 交互组件
	protected COMMovableObjectMoveInfo mCOMMoveInfo;		// 移动信息组件
	protected int mObjectID;								// 物体的客户端ID
	protected bool mSelfCreatedObject;						// 是否已经由MovableObject自己创建一个GameObject作为节点
	public MovableObject()
	{
		mObjectID = makeID();
	}
	public override void destroy()
	{
		// 自动创建的GameObject需要在此处自动销毁
		destroySelfCreateObject();
		mGlobalTouchSystem?.unregisteCollider(this);
		base.destroy();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCOMInteractive = null;
		mCOMMoveInfo = null;
		mSelfCreatedObject = false;
		// mObjectID不重置
		// mObjectID = 0;
	}
	// mObject需要外部自己创建以及销毁,内部只是引用,不会管理其生命周期
	// 且此函数需要在init之前调用,这样的话就能检测到setObject是否与mAutoCreateObject冲突,而且初始化也能够正常执行
	public override void setObject(GameObject obj)
	{
		// 如果是当前类自动创建的GameObject设置为空了,而且设置了一个不同的节点(无论是否为空),则取消此标记
		if (mObject != obj)
		{
			destroySelfCreateObject();
		}
		base.setObject(obj);
	}
	public virtual void init()
	{
		// 自动创建GameObject
		if (mObject == null)
		{
			selfCreateObject();
		}
		initComponents();
	}
	// 让MovableObject自己创建一个GameObject作为自己的节点,同时销毁对象时会将此GameObject也销毁
	public void selfCreateObject(string name = null, GameObject parent = null)
	{
		if (parent == null)
		{
			parent = mGameObjectPool.getObject();
		}
		setObject(mGameObjectPool.newObject(name, parent));
		mSelfCreatedObject = true;
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
	public int getObjectID()												{ return mObjectID; }
	// 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
	public virtual UIDepth getDepth()										{ return null; }
	public virtual bool isHandleInput()										{ return mCOMInteractive != null && mCOMInteractive.isHandleInput(); }
	public virtual bool isReceiveScreenMouse()								{ return mCOMInteractive != null && mCOMInteractive.isReceiveScreenMouse(); }
	public virtual bool isPassRay()											{ return mCOMInteractive == null || mCOMInteractive.isPassRay(); }
	public virtual bool isPassDragEvent()									{ return !isDragable() || (mCOMInteractive != null && mCOMInteractive.isPassDragEvent()); }
	public virtual bool isMouseHovered()									{ return mCOMInteractive != null && mCOMInteractive.isMouseHovered(); }
	public virtual bool isDragable()										{ return getActiveComponent<COMMovableObjectDrag>() != null; }
	public int getClickSound()												{ return mCOMInteractive?.getClickSound() ?? 0; }
	public string getDescription()											{ return EMPTY; }
	public bool hasLastPosition()											{ return mCOMMoveInfo != null && mCOMMoveInfo.hasLastPosition(); }
	public COMMovableObjectMoveInfo getCOMMoveInfo()						{ return mCOMMoveInfo; }
	// set
	//------------------------------------------------------------------------------------------------------------------------------
	public virtual void setPassRay(bool passRay)							{ getCOMInteractive().setPassRay(passRay); }
	public virtual void setHandleInput(bool handleInput)					{ getCOMInteractive().setHandleInput(handleInput); }
	public void setOnMouseEnter(OnMouseEnter callback)						{ getCOMInteractive().setOnMouseEnter(callback); }
	public void setOnMouseLeave(OnMouseLeave callback)						{ getCOMInteractive().setOnMouseLeave(callback); }
	public void setOnMouseDown(Vector3IntCallback callback)					{ getCOMInteractive().setOnMouseDown(callback); }
	public void setOnMouseUp(Vector3IntCallback callback)					{ getCOMInteractive().setOnMouseUp(callback); }
	public void setOnMouseMove(OnMouseMove callback)						{ getCOMInteractive().setOnMouseMove(callback); }
	public virtual void setClickCallback(Action callback)					{ getCOMInteractive().setClickCallback(callback); }
	public virtual void setClickDetailCallback(ClickCallback callback)		{ getCOMInteractive().setClickDetailCallback(callback); }
	public virtual void setHoverCallback(BoolCallback callback)				{ getCOMInteractive().setHoverCallback(callback); }
	public virtual void setHoverDetailCallback(HoverCallback callback)		{ getCOMInteractive().setHoverDetailCallback(callback); }
	public virtual void setPressCallback(BoolCallback callback)				{ getCOMInteractive().setPressCallback(callback); }
	public virtual void setPressDetailCallback(PressCallback callback)		{ getCOMInteractive().setPressDetailCallback(callback); }
	public void setOnScreenMouseUp(OnScreenMouseUp callback)				{ getCOMInteractive().setOnScreenMouseUp(callback); }
	public void setDoubleClickCallback(Action callback)						{ getCOMInteractive().setDoubleClickCallback(callback); }
	public void setDoubleClickDetailCallback(ClickCallback callback)		{ getCOMInteractive().setDoubleClickDetailCallback(callback); }
	public void setPreClickCallback(Action callback)						{ getCOMInteractive().setPreClickCallback(callback); }
	public void setPreClickDetailCallback(ClickCallback callback)			{ getCOMInteractive().setPreClickDetailCallback(callback); }
	public void setClickSound(int sound)									{ getCOMInteractive().setClickSound(sound); }
	public virtual void onMouseEnter(Vector3 mousePos, int touchID)			{ getCOMInteractive().onMouseEnter(mousePos, touchID); }
	public virtual void onMouseLeave(Vector3 mousePos, int touchID)			{ getCOMInteractive().onMouseLeave(mousePos, touchID); }
	// 鼠标左键在窗口内按下
	public virtual void onMouseDown(Vector3 mousePos, int touchID)			{ getCOMInteractive().onMouseDown(mousePos, touchID); }
	// 鼠标左键在窗口内放开
	public virtual void onMouseUp(Vector3 mousePos, int touchID)			{ getCOMInteractive().onMouseUp(mousePos, touchID); }
	// 鼠标在窗口内,并且有移动
	public virtual void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
	{
		getCOMInteractive().onMouseMove(mousePos, moveDelta, moveTime, touchID);
	}
	public virtual void onMouseStay(Vector3 mousePos, int touchID)			{ getCOMInteractive().onMouseStay(mousePos, touchID); }
	public virtual void onScreenMouseDown(Vector3 mousePos, int touchID)	{ getCOMInteractive().onScreenMouseDown(mousePos, touchID); }
	// 鼠标在屏幕上抬起
	public virtual void onScreenMouseUp(Vector3 mousePos, int touchID)
	{
		getCOMInteractive().onScreenMouseUp(mousePos, touchID);
	}
	public virtual void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent) 
	{
		getCOMInteractive().onReceiveDrag(dragObj, mousePos, ref continueEvent);
	}
	public virtual void onDragHoverd(IMouseEventCollect dragObj, Vector3 mousePos, bool hover) 
	{
		getCOMInteractive().onDragHoverd(dragObj, mousePos, hover);
	}
	public virtual void onMultiTouchStart(Vector3 touch0, Vector3 touch1) { getCOMInteractive().onMultiTouchStart(touch0, touch1); }
	public virtual void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1) 
	{
		getCOMInteractive().onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
	}
	public virtual void onMultiTouchEnd()  { getCOMInteractive().onMultiTouchEnd(); }
	public void enableMoveInfo()
	{
		mCOMMoveInfo ??= addComponent<COMMovableObjectMoveInfo>(true);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected COMMovableObjectInteractive getCOMInteractive()
	{
		return mCOMInteractive ??= addComponent<COMMovableObjectInteractive>(false);
	}
	protected void destroySelfCreateObject()
	{
		if (mObject != null &&  mSelfCreatedObject)
		{
			mSelfCreatedObject = false;
			mGameObjectPool?.destroyObject(mObject, true);
			mObject = null;
		}
	}
}