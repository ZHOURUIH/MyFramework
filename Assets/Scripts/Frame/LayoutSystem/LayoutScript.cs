using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class LayoutScript : GameBase
{
	protected List<int> mDelayCmdList;  // 布局显示和隐藏时的延迟命令列表,当命令执行时,会从列表中移除该命令
	protected CommandCallback mCmdCallback;
	protected GameLayout mLayout;
	protected myUIObject mRoot;
	protected Type mType;				// 因为有些布局可能是在ILRuntime中,所以类型获取可能不正确,需要将类型存储下来
	protected int mID;
	public LayoutScript()
	{
		mDelayCmdList = new List<int>();
		mCmdCallback = onCmdStarted;
	}
	public virtual void destroy()
	{
		interruptAllCommand();
	}
	public void setLayout(GameLayout layout) 
	{
		mLayout = layout;
		mID = mLayout.getID(); 
	}
	public bool isVisible() { return mLayout.isVisible(); }
	public int getID() { return mID; }
	public GameLayout getLayout() { return mLayout; }
	public void setRoot(myUIObject root) { mRoot = root; }
	public myUIObject getRoot() { return mRoot; }
	public void registeCollider(myUIObject obj, ObjectClickCallback clickCallback, ObjectPreClickCallback preClick, object preClickUserData, bool passRay = false)
	{
		setObjectCallback(obj, clickCallback, null, null);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPreClickCallback(preClick, preClickUserData);
		obj.setPassRay(passRay);
		obj.setEnable(true);
	}
	// 用于接收GlobalTouchSystem处理的输入事件
	public void registeCollider(myUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback, bool passRay)
	{
		setObjectCallback(obj, clickCallback, hoverCallback, pressCallback);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPassRay(passRay);
		// 由碰撞体的窗口都需要启用更新,以便可以保证窗口大小与碰撞体大小一致
		obj.setEnable(true);
	}
	public void registeCollider(myUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback, GameCamera camera)
	{
		setObjectCallback(obj, clickCallback, hoverCallback, pressCallback);
		mGlobalTouchSystem.registeCollider(obj, camera);
		obj.setPassRay(false);
		obj.setEnable(true);
	}
	public void registeCollider(myUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback)
	{
		setObjectCallback(obj, clickCallback, hoverCallback, pressCallback);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPassRay(false);
		obj.setEnable(true);
	}
	public void registeCollider(myUIObject obj, ObjectClickCallback clickCallback, bool passRay)
	{
		setObjectCallback(obj, clickCallback, null, null);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPassRay(passRay);
		obj.setEnable(true);
	}
	public void registeCollider(myUIObject obj, ObjectClickCallback clickCallback)
	{
		setObjectCallback(obj, clickCallback, null, null);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPassRay(false);
		obj.setEnable(true);
	}
	public void registeCollider(myUIObject obj, bool passRay)
	{
		setObjectCallback(obj, null, null, null);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPassRay(passRay);
		obj.setEnable(true);
	}
	public void registeCollider(myUIObject obj)
	{
		setObjectCallback(obj, null, null, null);
		mGlobalTouchSystem.registeCollider(obj);
		obj.setPassRay(false);
		obj.setEnable(true);
	}
	public void setObjectCallback(myUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback)
	{
		obj.setClickCallback(clickCallback);
		obj.setPressCallback(pressCallback);
		obj.setHoverCallback(hoverCallback);
	}
	// 用于接收NGUI处理的输入事件
#if USE_NGUI
	public void registeColliderNGUI(txNGUIObject obj, 
									UIEventListener.VoidDelegate clickCallback,
									UIEventListener.BoolDelegate pressCallback = null, 
									UIEventListener.BoolDelegate hoverCallback = null)
	{
		if (obj.getCollider() == null)
		{
			logInfo("window must has box collider that can registeBoxColliderNGUI!");
			return;
		}
		obj.setClickCallback(clickCallback);
		obj.setPressCallback(pressCallback);
		obj.setHoverCallback(hoverCallback);
	}
#endif
	public void unregisteCollider(myUIObject obj)
	{
		mGlobalTouchSystem.unregisteCollider(obj);
	}
	public void registeInputField(IInputField inputField)
	{
		mInputManager.registeInputField(inputField);
		// 所有的输入框都是不能穿透射线的
		registeCollider(inputField as myUIObject);
	}
	public void unregisteInputField(IInputField inputField)
	{
		mInputManager.unregisteInputField(inputField);
	}
	public void bindPassOnlyParent(myUIObject obj)
	{
		// 设置当前窗口需要调整深度在所有子节点之上,并计算深度调整值
		obj.setDepthOverAllChild(true);
		UIDepth depth = obj.getDepth();
		depth.setDepthValue(obj.getParent().getDepth(), depth.getOrderInParent(), obj.isDepthOverAllChild());
		// 刷新深度
		mGlobalTouchSystem.bindPassOnlyParent(obj);
	}
	public void bindPassOnlyArea(myUIObject background, myUIObject passOnlyArea)
	{
		mGlobalTouchSystem.bindPassOnlyArea(background, passOnlyArea);
	}
	public abstract void assignWindow();
	public virtual void init() { }
	public virtual void update(float elapsedTime) { }
	public virtual void lateUpdate(float elapsedTime) { }
	// 在开始显示之前,需要将所有的状态都重置到加载时的状态
	public virtual void onReset() { }
	// 重置布局状态后,再根据当前游戏状态设置布局显示前的状态
	public virtual void onGameState() { }
	public virtual void onDrawGizmos() { }
	public virtual void onShow(bool immediately, string param) { }
	// immediately表示是否需要立即隐藏,即便有隐藏的动画也需要立即执行完
	public virtual void onHide(bool immediately, string param) { }
	// 通知脚本开始显示或隐藏,中断全部命令
	public void notifyStartShowOrHide()
	{
		interruptAllCommand();
	}
	public void addDelayCmd(Command cmd)
	{
		mDelayCmdList.Add(cmd.mAssignID);
		cmd.addStartCommandCallback(mCmdCallback);
	}
	public bool hasObject(string name)
	{
		return hasObject(mRoot, name);
	}
	public bool hasObject(myUIObject parent, string name)
	{
		if (parent == null)
		{
			parent = mRoot;
		}
		GameObject gameObject = getGameObject(name, parent.getObject());
		return gameObject != null;
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T cloneObject<T>(myUIObject parent, T oriObj, string name, bool active = true, bool refreshUIDepth = true, bool needSortChild = true) where T : myUIObject, new()
	{
		if (parent == null)
		{
			parent = mRoot;
		}
		GameObject obj = cloneObject(oriObj.getObject(), name);
		T window = newUIObject<T>(parent, mLayout, obj, needSortChild);
		window.setActive(active);
		window.cloneFrom(oriObj);
		// 通知布局有窗口添加
		if (refreshUIDepth)
		{
			mLayout.notifyChildChanged(parent);
		}
		return window;
	}
	// 创建myUIObject,并且新建GameObject,分配到myUIObject中
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	// refreshUIDepth表示创建后是否需要刷新所属父节点下所有子节点的深度信息
	// needSortChild表示创建后是否需要对myUIObject中的子节点列表进行排序,使列表的顺序与面板的顺序相同,对需要射线检测的窗口有影响
	public T createObject<T>(myUIObject parent, string name, bool active = true, bool refreshUIDepth = true, bool needSortChild = true) where T : myUIObject, new()
	{
		GameObject go = createGameObject(name);
		if (parent == null)
		{
			parent = mRoot;
		}
		go.layer = parent.getObject().layer;
		T obj = newUIObject<T>(parent, mLayout, go, needSortChild);
		obj.setActive(active);
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		// 通知布局有窗口添加
		if (refreshUIDepth)
		{
			mLayout.notifyChildChanged(parent);
		}
		return obj;
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T createObject<T>(string name, bool active = true, bool refreshUIDepth = true, bool needSortChild = true) where T : myUIObject, new()
	{
		return createObject<T>(null, name, active, refreshUIDepth, needSortChild);
	}
	public T newObjectNoSort<T>(out T obj, myUIObject parent, string name, int active = -1, bool showError = true) where T : myUIObject, new()
	{
		return newObject(out obj, parent, name, active, showError, false);
	}
	public T newObjectNoSort<T>(out T obj, string name, int active = -1, bool showError = true) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, active, showError, false);
	}
	// 创建myUIObject,并且在布局中查找GameObject分配到myUIObject
	// active为-1则表示不设置active,0表示false,1表示true
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T newObject<T>(out T obj, myUIObject parent, string name, int active = -1, bool showError = true, bool needSortChild = true) where T : myUIObject, new()
	{
		obj = null;
		GameObject parentObj = parent != null ? parent.getObject() : null;
		GameObject gameObject = getGameObject(name, parentObj, showError, false);
		if (gameObject == null)
		{
			return obj;
		}
		obj = newUIObject<T>(parent, mLayout, gameObject, needSortChild);
		if(active >= 0)
		{
			obj.setActive(active != 0);
		}
		return obj;
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T newObject<T>(out T obj, string name, int active = -1, bool needSortChild = true) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, active);
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public static T newUIObject<T>(myUIObject parent, GameLayout layout, GameObject gameObj, bool needSortChild = true) where T : myUIObject, new()
	{
		T obj = new T();
		obj.setLayout(layout);
		obj.setGameObject(gameObj);
		obj.setParent(parent, needSortChild);
		obj.init();
		// 如果在创建窗口对象时,布局已经完成了自适应,则通知窗口
		if (layout != null && layout.isAnchorApplied())
		{
			obj.notifyAnchorApply();
		}
		return obj;
	}
	public GameObject instantiateObject(myUIObject parent, string prefabPath, string name, int tag = 0)
	{
		GameObject go = mObjectPool.createObject(prefabPath, tag);
		if(go != null)
		{
			go.name = name;
			go.SetActive(false);
			go.transform.SetParent(parent.getTransform());
		}
		return go;
	}
	public void instantiateObject(myUIObject parent, string prefabName)
	{
		instantiateObject(parent, prefabName, getFileNameNoSuffix(prefabName, true));
	}
	public void destroyInstantiateObject(myUIObject window, bool destroyReally)
	{
		GameObject go = window.getObject();
		myUIObject.destroyWindow(window, false);
		mObjectPool.destroyObject(ref go, destroyReally);
		// 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
	}
	// 虽然执行内容与类似,但是为了外部使用方便,所以添加了对于不同方式创建出来的窗口的销毁方法
	public void destroyClonedObject(myUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
	}
	public void destroyObject(ref myUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
		obj = null;
	}
	public void destroyObject(myUIObject obj, bool immediately = false)
	{
		obj.setDestroyImmediately(immediately);
		myUIObject.destroyWindow(obj, true);
		// 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
	}
	public void interruptCommand(int assignID, bool showError = true)
	{
		if (mDelayCmdList.Contains(assignID))
		{
			mDelayCmdList.Remove(assignID);
			mCommandSystem.interruptCommand(assignID, showError);
		}
	}
	public void setType(Type type) { mType = type; }
	public Type getType() { return mType; }
	//----------------------------------------------------------------------------------------------------
	protected void onCmdStarted(Command cmd)
	{
		if (!mDelayCmdList.Remove(cmd.mAssignID))
		{
			logError("命令执行后移除命令失败!");
		}
	}
	protected void interruptAllCommand()
	{
		int count = mDelayCmdList.Count;
		for (int i = 0; i < count; ++i)
		{
			mCommandSystem.interruptCommand(mDelayCmdList[i], false);
		}
		mDelayCmdList.Clear();
	}
}