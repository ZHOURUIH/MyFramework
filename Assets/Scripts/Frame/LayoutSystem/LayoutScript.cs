using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class LayoutScript : GameBase
{
	protected List<int> mDelayCmdList;  // 布局显示和隐藏时的延迟命令列表,当命令执行时,会从列表中移除该命令
	protected GameLayout mLayout;
	protected txUIObject mRoot;
	protected LAYOUT mType;
	public LayoutScript()
	{
		mDelayCmdList = new List<int>();
	}
	public virtual void destroy()
	{
		interruptAllCommand();
	}
	public void setLayout(GameLayout layout) { mLayout = layout; mType = mLayout.getType(); }
	public bool isVisible() { return mLayout.isVisible(); }
	public LAYOUT getType() { return mType; }
	public GameLayout getLayout() { return mLayout; }
	public void setRoot(txUIObject root) { mRoot = root; }
	public txUIObject getRoot() { return mRoot; }
	public void registeBoxCollider(txUIObject obj, ObjectClickCallback clickCallback, ObjectPreClickCallback preClick, object preClickUserData, bool passRay = false)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, clickCallback, null, null);
		obj.setPreClickCallback(preClick, preClickUserData);
		obj.setPassRay(passRay);
	}
	// 用于接收GlobalTouchSystem处理的输入事件
	public void registeBoxCollider(txUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback, bool passRay)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, clickCallback, pressCallback, hoverCallback);
		obj.setPassRay(passRay);
	}
	public void registeBoxCollider(txUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback, GameCamera camera)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, clickCallback, pressCallback, hoverCallback, camera);
		obj.setPassRay(false);
	}
	public void registeBoxCollider(txUIObject obj, ObjectClickCallback clickCallback, ObjectHoverCallback hoverCallback, ObjectPressCallback pressCallback)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, clickCallback, pressCallback, hoverCallback);
		obj.setPassRay(false);
	}
	public void registeBoxCollider(txUIObject obj, ObjectClickCallback clickCallback, bool passRay)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, clickCallback, null, null);
		obj.setPassRay(passRay);
	}
	public void registeBoxCollider(txUIObject obj, ObjectClickCallback clickCallback)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, clickCallback, null, null);
		obj.setPassRay(false);
	}
	public void registeBoxCollider(txUIObject obj, bool passRay)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, null, null, null);
		obj.setPassRay(passRay);
	}
	public void registeBoxCollider(txUIObject obj)
	{
		mGlobalTouchSystem.registeBoxCollider(obj, null, null, null);
		obj.setPassRay(false);
	}
	// 用于接收NGUI处理的输入事件
#if USE_NGUI
	public void registeBoxColliderNGUI(txNGUIObject obj, UIEventListener.VoidDelegate clickCallback,
		UIEventListener.BoolDelegate pressCallback = null, UIEventListener.BoolDelegate hoverCallback = null)
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
	public void unregisteBoxCollider(txUIObject obj)
	{
		mGlobalTouchSystem.unregisteBoxCollider(obj);
	}
	public void registeInputField(IInputField inputField)
	{
		mInputManager.registeInputField(inputField);
	}
	public void unregisteInputField(IInputField inputField)
	{
		mInputManager.unregisteInputField(inputField);
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
		cmd.addStartCommandCallback(onCmdStarted);
	}
	public bool hasObject(string name)
	{
		return hasObject(mRoot, name);
	}
	public bool hasObject(txUIObject parent, string name)
	{
		if (parent == null)
		{
			parent = mRoot;
		}
		GameObject gameObject = getGameObject(parent.getObject(), name);
		return gameObject != null;
	}
	public T cloneObject<T>(txUIObject parent, T oriObj, string name, bool active = true, bool refreshUIDepth = true) where T : txUIObject, new()
	{
		if (parent == null)
		{
			parent = mRoot;
		}
		GameObject obj = cloneObject(oriObj.getObject(), name);
		T window = newUIObject<T>(parent, mLayout, obj);
		window.setActive(active);
		window.cloneFrom(oriObj);
		// 通知布局有窗口添加
		if(refreshUIDepth)
		{
			mLayout.notifyObjectChanged();
		}
		return window;
	}
	// 创建txUIObject,并且新建GameObject,分配到txUIObject中
	public T createObject<T>(txUIObject parent, string name, bool active = true) where T : txUIObject, new()
	{
		GameObject go = createGameObject(name);
		if (parent == null)
		{
			parent = mRoot;
		}
		go.layer = parent.getObject().layer;
		T obj = newUIObject<T>(parent, mLayout, go);
		obj.setActive(active);
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		// 通知布局有窗口添加
		mLayout.notifyObjectChanged();
		return obj;
	}
	public T createObject<T>(string name, bool active = true) where T : txUIObject, new()
	{
		return createObject<T>(null, name, active);
	}
	// 创建txUIObject,并且在布局中查找GameObject分配到txUIObject
	// active为-1则表示不设置active,0表示false,1表示true
	public T newObject<T>(out T obj, txUIObject parent, string name, int active = -1, bool showError = true) where T : txUIObject, new()
	{
		obj = null;
		GameObject parentObj = parent != null ? parent.getObject() : null;
		GameObject gameObject = getGameObject(parentObj, name, showError, false);
		if (gameObject == null)
		{
			return obj;
		}
		obj = newUIObject<T>(parent, mLayout, gameObject);
		if(active >= 0)
		{
			obj.setActive(active != 0);
		}
		return obj;
	}
	public T newObject<T>(out T obj, string name, int active = -1) where T : txUIObject, new()
	{
		return newObject(out obj, mRoot, name, active);
	}
	public static T newUIObject<T>(txUIObject parent, GameLayout layout, GameObject gameObj) where T : txUIObject, new()
	{
		T obj = new T();
		obj.setLayout(layout);
		obj.init(gameObj, parent);
		// 如果在创建窗口对象时,布局已经完成了自适应,则通知窗口
		if(layout != null && layout.isAnchorApplied())
		{
			obj.notifyAnchorApply();
		}
		return obj;
	}
	public GameObject instantiateObject(txUIObject parent, string prefabPath, string name, string tag = null)
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
	public void instantiateObject(txUIObject parent, string prefabName)
	{
		instantiateObject(parent, prefabName, getFileNameNoSuffix(prefabName, true));
	}
	public void destroyInstantiateObject(txUIObject window, bool destroyReally)
	{
		GameObject go = window.getObject();
		txUIObject.destroyWindow(window, false);
		mObjectPool.destroyObject(ref go, destroyReally);
		// 通知布局有窗口添加
		mLayout.notifyObjectChanged();
	}
	// 虽然执行内容与类似,但是为了外部使用方便,所以添加了对于不同方式创建出来的窗口的销毁方法
	public void destroyClonedObject(txUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
	}
	public void destroyObject(ref txUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
		obj = null;
	}
	public void destroyObject(txUIObject obj, bool immediately = false)
	{
		obj.setDestroyImmediately(immediately);
		txUIObject.destroyWindow(obj, true);
		// 通知布局有窗口添加
		mLayout.notifyObjectChanged();
	}
	public void interruptCommand(int assignID, bool showError = true)
	{
		if (mDelayCmdList.Contains(assignID))
		{
			mDelayCmdList.Remove(assignID);
			mCommandSystem.interruptCommand(assignID, showError);
		}
	}
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