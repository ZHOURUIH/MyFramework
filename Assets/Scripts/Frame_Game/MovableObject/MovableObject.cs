using UnityEngine;
using static FrameBase;
using static CSharpUtility;
using static StringUtility;

// 可移动物体,表示一个3D物体
public class MovableObject : Transformable, IMouseEventCollect
{
	protected COMMovableObjectInteractive mCOMInteractive;	// 交互组件
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
	// 可移动物体没有固定深度,只在实时检测时根据相交点来判断深度
	public virtual UIDepth getDepth()										{ return null; }
	public virtual bool isHandleInput()										{ return mCOMInteractive != null && mCOMInteractive.isHandleInput(); }
	public virtual bool isReceiveScreenMouse()								{ return mCOMInteractive != null && mCOMInteractive.isReceiveScreenMouse(); }
	public virtual bool isPassRay()											{ return mCOMInteractive == null || mCOMInteractive.isPassRay(); }
	public virtual bool isPassDragEvent()									{ return !isDragable() || (mCOMInteractive != null && mCOMInteractive.isPassDragEvent()); }
	public virtual bool isDragable()										{ return false; }
	public string getDescription()											{ return EMPTY; }
	// set
	//------------------------------------------------------------------------------------------------------------------------------
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