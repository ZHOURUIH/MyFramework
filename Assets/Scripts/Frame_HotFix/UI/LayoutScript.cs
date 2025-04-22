using System.Collections.Generic;
#if USE_CSHARP_10
using System.Runtime.CompilerServices;
#endif
using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static FrameBaseUtility;

// 布局脚本基类,用于执行布局相关的逻辑
public abstract class LayoutScript : DelayCmdWatcher, ILocalizationCollection
{
	protected HashSet<myUGUIScrollRect> mScrollViewRegisteList = new();	// 用于检测ScrollView合法性的列表
	protected HashSet<IInputField> mInputFieldRegisteList = new();      // 用于检测InputField合法性的列表
	protected HashSet<WindowStructPoolBase> mPoolList;					// 布局中使用的窗口对象池列表,收集后方便统一销毁
	protected HashSet<WindowObjectBase> mWindowObjectList;				// 布局中使用的非对象池中的窗口对象,收集后方便统一销毁
	protected HashSet<IUGUIObject> mLocalizationObjectList;				// 注册的需要本地化的对象,因为每次修改文本显示都会往列表里加,所以使用HashSet
	protected GameLayout mLayout;										// 所属布局
	protected myUGUIObject mRoot;										// 布局中的根节点
	protected bool mRegisterChecked;								    // 是否已经检测过了合法性
	protected bool mNeedUpdate = true;									// 布局脚本是否需要指定update,为了提高效率,可以不执行当前脚本的update,虽然update可能是空的,但是不调用会效率更高
	protected bool mEscHide;											// 按Esc键时是否关闭此界面
	public override void destroy()
	{
		base.destroy();
		// 避免遗漏本地化的注销,此处再次确认注销一次
		clearLocalization();

		foreach (WindowStructPoolBase item in mPoolList.safe())
		{
			item.destroy();
		}
		mPoolList?.Clear();
		foreach (WindowObjectBase item in mWindowObjectList.safe())
		{
			item.destroy();
		}
		mWindowObjectList?.Clear();

		// 为了避免子类中遗漏注销监听,基类会再次注销监听一次
		mEventSystem?.unlistenEvent(this);
		interruptAllCommand();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mScrollViewRegisteList.Clear();
		mInputFieldRegisteList.Clear();
		mLayout = null;
		mRoot = null;
		mRegisterChecked = false;
		mNeedUpdate = true;
		mEscHide = false;
	}
	public virtual void setLayout(GameLayout layout) { mLayout = layout; }
	public virtual bool onESCDown()
	{
		if (mEscHide)
		{
			close();
		}
		return mEscHide;
	}
	public bool isNeedUpdate() { return mNeedUpdate; }
	public bool isVisible() { return mLayout.isVisible(); }
	public GameLayout getLayout() { return mLayout; }
	public void setRoot(myUGUIObject root) { mRoot = root; }
	public myUGUIObject getRoot() { return mRoot; }
	public void notifyUIObjectNeedUpdate(myUIObject uiObj, bool needUpdate)
	{
		mLayout.notifyUIObjectNeedUpdate(uiObj, needUpdate);
	}
	public void registeScrollRect(myUGUIScrollRect scrollRect, myUGUIObject viewport, myUGUIObject content, float verticalPivot = 1.0f, float horizontalPivot = 0.5f)
	{
		if (isEditor())
		{
			mScrollViewRegisteList.Add(scrollRect);
		}
		scrollRect.initScrollRect(viewport, content, verticalPivot, horizontalPivot);
		// 所有的可滑动列表都是不能穿透射线的
		scrollRect.registeCollider();
		bindPassOnlyParent(viewport);
	}
	public void registeInputField(IInputField inputField)
	{
		if (isEditor())
		{
			mInputFieldRegisteList.Add(inputField);
		}
		mInputSystem.registeInputField(inputField);
		// 所有的输入框都是不能穿透射线的
		(inputField as myUIObject).registeCollider();
	}
	public void unregisteInputField(IInputField inputField)
	{
		mInputSystem.unregisteInputField(inputField);
	}
	// parent的区域中才能允许parent的子节点接收射线检测
	public void bindPassOnlyParent(myUIObject parent)
	{
		// 设置当前窗口需要调整深度在所有子节点之上,并计算深度调整值
		parent.setDepthOverAllChild(true);
		parent.setDepth(parent.getParent().getDepth(), parent.getDepth().getOrderInParent());
		// 刷新深度
		parent.registeCollider();
		mGlobalTouchSystem.bindPassOnlyParent(parent);
	}
	// parent的区域中只有passOnlyArea的区域可以穿透
	public void bindPassOnlyArea(myUIObject parent, myUIObject passOnlyArea)
	{
		parent.registeCollider();
		passOnlyArea.registeCollider();
		mGlobalTouchSystem.bindPassOnlyArea(parent, passOnlyArea);
	}
	public void addLocalizationObject(IUGUIObject obj)
	{
		mLocalizationObjectList ??= new();
		mLocalizationObjectList.Add(obj);
	}
	public void addWindowStructPool(WindowStructPoolBase pool)
	{
		mPoolList ??= new();
		if (!mPoolList.Add(pool))
		{
			logError("不能重复注册对象池");
		}
	}
	public void addWindowObject(WindowObjectBase windowObj)
	{
		mWindowObjectList ??= new();
		if (!mWindowObjectList.Add(windowObj))
		{
			logError("不能重复注册UI对象");
		}
	}
	public abstract void assignWindow();
	public virtual void init() { }
	public virtual void update(float elapsedTime) { }
	public virtual void lateUpdate(float elapsedTime) { }
	// 在开始显示之前,需要将所有的状态都重置到加载时的状态
	public virtual void onReset()
	{
		if (isEditor() && !mRegisterChecked)
		{
			mRegisterChecked = true;
			// 检查是否注册了所有的ScrollRect
			using var a = new ListScope<ScrollRect>(out var scrollViewList);
			mRoot.getObject().GetComponentsInChildren(scrollViewList);
			foreach (ScrollRect item in scrollViewList)
			{
				if (!mScrollViewRegisteList.Contains(mLayout.getUIObject(item.gameObject) as myUGUIScrollRect))
				{
					logError("滑动列表未注册:" + item.gameObject.name + ", layout:" + mLayout.getName());
				}
			}

			using var b = new ListScope<InputField>(out var inputFieldList);
			mRoot.getObject().GetComponentsInChildren(inputFieldList);
			foreach (InputField item in inputFieldList)
			{
				if (!mInputFieldRegisteList.Contains(mLayout.getUIObject(item.gameObject) as IInputField))
				{
					logError("输入框未注册:" + item.gameObject.name + ", layout:" + mLayout.getName());
				}
			}
		}
	}
	// 重置布局状态后,再根据当前游戏状态设置布局显示前的状态
	public virtual void onGameState() { }
	public virtual void onDrawGizmos() { }
	public virtual void onHide()
	{
		clearLocalization();
		mEventSystem?.unlistenEvent(this);
	}
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
		parent ??= mRoot;
		return getGameObject(name, parent.getObject()) != null;
	}
	public T cloneObject<T>(myUIObject parent, myUIObject oriObj, string name) where T : myUIObject, new()
	{
		cloneObject(out T target, parent, oriObj, name, true);
		return target;
	}
	public T cloneObject<T>(myUIObject parent, myUIObject oriObj, string name, bool active) where T : myUIObject, new()
	{
		cloneObject(out T target, parent, oriObj, name, active);
		return target;
	}
	// 各种形式的创建窗口操作一律不会排序子节点,不会刷新布局中的窗口深度,因为一般都是在assignWindow中调用
	// 而assignWindow后会刷新当前布局的窗口深度,而子节点排序只有在部分情况下才会使用,大部分情况不会用到
	public void cloneObject<T>(out T target, myUIObject parent, myUIObject oriObj, string name) where T : myUIObject, new()
	{
		cloneObject(out target, parent, oriObj, name, true);
	}
	public void cloneObject<T>(out T target, myUIObject parent, myUIObject oriObj, string name, bool active) where T : myUIObject, new()
	{
		parent ??= mRoot;
		GameObject obj = UnityUtility.cloneObject(oriObj.getObject(), name);
		target = newUIObject<T>(parent, mLayout, obj);
		target.setActive(active);
		target.cloneFrom(oriObj);
	}
	// 创建myUIObject,并且新建GameObject,分配到myUIObject中
	// refreshUIDepth表示创建后是否需要刷新所属父节点下所有子节点的深度信息
	// sortChild表示创建后是否需要对myUIObject中的子节点列表进行排序,使列表的顺序与面板的顺序相同,对需要射线检测的窗口有影响
	public T createUGUIObject<T>(myUIObject parent, string name, bool active) where T : myUGUIObject, new()
	{
		GameObject go = createGameObject(name);
		parent ??= mRoot;
		// UGUI需要添加RectTransform
		getOrAddComponent<RectTransform>(go);
		go.layer = parent.getObject().layer;
		T obj = newUIObject<T>(parent, mLayout, go);
		obj.setActive(active);
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		return obj;
	}
	public T createUIObject<T>(myUIObject parent, string name, bool active) where T : myUIObject, new()
	{
		GameObject go = createGameObject(name);
		parent ??= mRoot;
		go.layer = parent.getObject().layer;
		T obj = newUIObject<T>(parent, mLayout, go);
		obj.setActive(active);
		go.transform.localScale = Vector3.one;
		go.transform.localEulerAngles = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		return obj;
	}
	public void createUIObject<T>(out T obj, myUIObject parent, string name) where T : myUIObject, new()
	{
		obj = createUIObject<T>(parent, name, true);
	}
	public T createUGUIObject<T>(myUIObject parent, string name) where T : myUGUIObject, new()
	{
		return createUGUIObject<T>(parent, name, true);
	}
	public void createUGUIObject<T>(out T obj, myUIObject parent, string name) where T : myUGUIObject, new()
	{
		obj = createUGUIObject<T>(parent, name, true);
	}
	public T createUGUIObject<T>(string name, bool active) where T : myUGUIObject, new()
	{
		return createUGUIObject<T>(null, name, active);
	}
	public T newObject<T>(out T obj, string name) where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name, -1, true);
	}
	// 仅支持C#10
#if USE_CSHARP_10
	public T newObject<T>(out T obj, [CallerArgumentExpression("obj")] string name = "") where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name.rangeToEnd(1), -1, true);
	}
	public T newObject<T>(out T obj, int active, [CallerArgumentExpression("obj")] string name = "") where T : myUIObject, new()
	{
		return newObject(out obj, mRoot, name.rangeToEnd(1), active, true);
	}
	public T newObject<T>(out T obj, myUIObject parent, [CallerArgumentExpression("obj")] string name = "") where T : myUIObject, new()
	{
		return newObject(out obj, parent, name.rangeToEnd(1), -1, true);
	}
	public T newObject<T>(out T obj, myUIObject parent, int active, [CallerArgumentExpression("obj")] string name = "") where T : myUIObject, new()
	{
		return newObject(out obj, parent, name.rangeToEnd(1), active, true);
	}
#endif
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
	public T newObject<T>(out T obj, myUIObject parent, string name, int active, bool showError) where T : myUIObject, new()
	{
		obj = null;
		GameObject parentObj = parent?.getObject();
		GameObject gameObject;
		if (parentObj == null)
		{
			gameObject = getRootGameObject(name, showError);
		}
		else
		{
			gameObject = getGameObject(name, parentObj, showError, false);
		}
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
	public static T newUIObject<T>(myUIObject parent, GameLayout layout, GameObject go) where T : myUIObject, new()
	{
		T obj = new();
		obj.setLayout(layout);
		obj.setObject(go);
		obj.setParent(parent, false);
		obj.init();
		// 如果在创建窗口对象时,布局已经完成了自适应,则通知窗口
		if (layout != null && layout.isAnchorApplied())
		{
			obj.notifyAnchorApply();
		}
		return obj;
	}
	public static GameObject instantiate(myUIObject parent, string prefabPath, string name, int tag = 0)
	{
		GameObject go = mPrefabPoolManager.createObject(prefabPath, tag, false, false, parent.getObject());
		if (go != null)
		{
			go.name = name;
		}
		return go;
	}
	public static CustomAsyncOperation instantiateAsync(string prefabPath, string name, int tag, GameObjectCallback callback)
	{
		return mPrefabPoolManager.createObjectAsync(prefabPath, tag, false, false, (GameObject go) =>
		{
			if (go != null)
			{
				go.name = name;
			}
			callback?.Invoke(go);
		});
	}
	public static void instantiate(myUIObject parent, string prefabName)
	{
		instantiate(parent, prefabName, getFileNameNoSuffixNoDir(prefabName));
	}
	public static void destroyInstantiate(myUIObject window, bool destroyReally)
	{
		if (window == null)
		{
			return;
		}
		GameObject go = window.getObject();
		myUIObject.destroyWindow(window, false);
		mPrefabPoolManager.destroyObject(ref go, destroyReally);
		// 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
	}
	// 虽然执行内容与类似,但是为了外部使用方便,所以添加了对于不同方式创建出来的窗口的销毁方法
	public static void destroyCloned(myUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
	}
	public static void destroyObject(ref myUGUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
		obj = null;
	}
	public static void destroyObject(ref myUIObject obj, bool immediately = false)
	{
		destroyObject(obj, immediately);
		obj = null;
	}
	public static void destroyObject(myUIObject obj, bool immediately = false)
	{
		obj.setDestroyImmediately(immediately);
		myUIObject.destroyWindow(obj, true);
		// 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
	}
	public void close()
	{
		CmdLayoutManagerVisible.execute(GetType(), false, false);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void clearLocalization()
	{
		foreach (myUGUIObject item in mLocalizationObjectList.safe())
		{
			mLocalizationManager.unregisteLocalization(item);
		}
		mLocalizationObjectList?.Clear();
	}
}