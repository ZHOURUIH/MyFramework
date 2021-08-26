using UnityEngine;
using System.Collections.Generic;
using System;

// 布局脚本基类,用于执行布局相关的逻辑
public abstract class LayoutScript : DelayCmdWatcher
{
	protected GameLayout mLayout;		// 所属布局
	protected myUIObject mRoot;			// 布局中的根节点
	protected Type mType;				// 因为有些布局可能是在ILRuntime中,所以类型获取可能不正确,需要将类型存储下来
	protected int mID;					// 布局ID,与GameLayout中的ID一致
	protected bool mNeedUpdate;			// 布局脚本是否需要指定update,为了提高效率,可以不执行不必要的update
	public LayoutScript()
	{
		mNeedUpdate = true;
	}
	public virtual void destroy()
	{
		interruptAllCommand();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLayout = null;
		mRoot = null;
		mType = null;
		mID = 0;
		mNeedUpdate = true;
	}
	public virtual void setLayout(GameLayout layout)
	{
		mLayout = layout;
		mID = mLayout.getID();
	}
	public virtual bool onESCDown() { return false; }
	public bool isNeedUpdate() { return mNeedUpdate; }
	public bool isVisible() { return mLayout.isVisible(); }
	public int getID() { return mID; }
	public GameLayout getLayout() { return mLayout; }
	public void setRoot(myUIObject root) { mRoot = root; }
	public myUIObject getRoot() { return mRoot; }
	public void notifyUIObjectNeedUpdate(myUIObject uiObj, bool needUpdate)
	{
		mLayout.notifyUIObjectNeedUpdate(uiObj, needUpdate);
	}
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
	public void unregisteCollider(myUIObject obj)
	{
		mGlobalTouchSystem.unregisteCollider(obj);
	}
	public void registeScrollRect(myUGUIScrollRect scrollRect, myUGUIObject viewport, myUGUIObject content)
	{
		scrollRect.initScrollRect(viewport, content);
		registeCollider(viewport);
		bindPassOnlyParent(viewport);
	}
	public void registeInputField(IInputField inputField)
	{
		mInputSystem.registeInputField(inputField);
		// 所有的输入框都是不能穿透射线的
		registeCollider(inputField as myUIObject);
	}
	public void unregisteInputField(IInputField inputField)
	{
		mInputSystem.unregisteInputField(inputField);
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
	// 各种形式的创建窗口操作一律不会排序子节点,不会刷新布局中的窗口深度,因为一般都是在assignWindow中调用
	// 而assignWindow后会刷新当前布局的窗口深度,而子节点排序只有在部分情况下才会使用,大部分情况不会用到
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T cloneObject<T>(myUIObject parent, T oriObj, string name) where T : myUIObject, new()
	{
		return cloneObject(parent, oriObj, name, true);
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T cloneObject<T>(myUIObject parent, T oriObj, string name, bool active) where T : myUIObject, new()
	{
		if (parent == null)
		{
			parent = mRoot;
		}
		GameObject obj = cloneObject(oriObj.getObject(), name);
		T window = newUIObject<T>(parent, mLayout, obj);
		window.setActive(active);
		window.cloneFrom(oriObj);
		return window;
	}
	// 创建myUIObject,并且新建GameObject,分配到myUIObject中
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	// refreshUIDepth表示创建后是否需要刷新所属父节点下所有子节点的深度信息
	// sortChild表示创建后是否需要对myUIObject中的子节点列表进行排序,使列表的顺序与面板的顺序相同,对需要射线检测的窗口有影响
	public T createObject<T>(myUIObject parent, string name, bool active) where T : myUIObject, new()
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
		return obj;
	}
	public T createObject<T>(myUIObject parent, string name) where T : myUIObject, new()
	{
		return createObject<T>(parent, name, true);
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T createObject<T>(string name, bool active) where T : myUIObject, new()
	{
		return createObject<T>(null, name, active);
	}
	public T newObject<T>(out T obj, string name) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, -1, true);
	}
	public T newObject<T>(out T obj, string name, int active) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, active, true);
	}
	public T newObject<T>(out T obj, string name, bool showError) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, -1, showError);
	}
	public T newObject<T>(out T obj, string name, int active, bool showError) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, active, showError);
	}
	public T newObject<T>(out T obj, myUIObject parent, string name) where T : myUIObject, new()
	{
		return newObject(out obj, parent, name, -1, true);
	}
	public T newObject<T>(out T obj, myUIObject parent, string name, int active) where T : myUIObject, new()
	{
		return newObject(out obj, parent, name, active, true);
	}
	public T newObject<T>(out T obj, myUIObject parent, string name, bool showError) where T : myUIObject, new()
	{
		return newObject(out obj, parent, name, -1, showError);
	}
	// 创建myUIObject,并且在布局中查找GameObject分配到myUIObject
	// active为-1则表示不设置active,0表示false,1表示true
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public T newObject<T>(out T obj, myUIObject parent, string name, int active, bool showError) where T : myUIObject, new()
	{
		obj = null;
		GameObject parentObj = parent != null ? parent.getObject() : null;
		GameObject gameObject = getGameObject(name, parentObj, showError, false);
		if (gameObject == null)
		{
			return obj;
		}
		obj = newUIObject<T>(parent, mLayout, gameObject);
		if (active >= 0)
		{
			obj.setActive(active != 0);
		}
		return obj;
	}
	public T newObject<T>(out T obj, myUIObject parent, GameObject go) where T : myUIObject, new()
	{
		return newObject(out obj, parent, go, -1);
	}
	public T newObject<T>(out T obj, myUIObject parent, GameObject go, int active) where T : myUIObject, new()
	{
		obj = newUIObject<T>(parent, mLayout, go);
		if (active >= 0)
		{
			obj.setActive(active != 0);
		}
		return obj;
	}
	// 因为此处可以确定只有主工程的类,所以可以使用new T()
	public static T newUIObject<T>(myUIObject parent, GameLayout layout, GameObject go) where T : myUIObject, new()
	{
		T obj = new T();
		obj.setLayout(layout);
		obj.setObject(go);
		obj.setParent(parent, false, false);
		obj.init();
		// 如果在创建窗口对象时,布局已经完成了自适应,则通知窗口
		if (layout != null && layout.isAnchorApplied())
		{
			obj.notifyAnchorApply();
		}
		return obj;
	}
	public GameObject instantiate(myUIObject parent, string prefabPath, string name, int tag = 0)
	{
		GameObject go = mObjectPool.createObject(prefabPath, tag);
		if (go != null)
		{
			go.name = name;
			go.SetActive(false);
			go.transform.SetParent(parent.getTransform());
		}
		return go;
	}
	public void instantiate(myUIObject parent, string prefabName)
	{
		instantiate(parent, prefabName, getFileNameNoSuffix(prefabName, true));
	}
	public void destroyInstantiate(myUIObject window, bool destroyReally)
	{
		GameObject go = window.getObject();
		myUIObject.destroyWindow(window, false);
		mObjectPool.destroyObject(ref go, destroyReally);
		// 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
	}
	// 虽然执行内容与类似,但是为了外部使用方便,所以添加了对于不同方式创建出来的窗口的销毁方法
	public void destroyCloned(myUIObject obj, bool immediately = false)
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
	public void setType(Type type) { mType = type; }
	public Type getType() { return mType; }
	public void close()
	{
		LT.HIDE_LAYOUT(mID);
	}
}