using UnityEngine;
#if USE_CSHARP_10
using System.Runtime.CompilerServices;
#endif
using static UnityUtility;
using static FrameBase;
using static StringUtility;

// 布局脚本基类,用于执行布局相关的逻辑
public abstract class LayoutScript : DelayCmdWatcher
{
	protected GameLayout mLayout;			// 所属布局
	protected myUGUIObject mRoot;			// 布局中的根节点
	public override void destroy()
	{
		base.destroy();
		interruptAllCommand();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLayout = null;
		mRoot = null;
	}
	public virtual void setLayout(GameLayout layout) { mLayout = layout; }
	public bool isVisible() { return mLayout.isVisible(); }
	public void setRoot(myUGUIObject root) { mRoot = root; }
	public abstract void assignWindow();
	public virtual void init() { }
	public virtual void update(float elapsedTime) { }
	public virtual void lateUpdate(float elapsedTime) { }
	// 在开始显示之前,需要将所有的状态都重置到加载时的状态
	public virtual void onReset() { }
	// 重置布局状态后,再根据当前游戏状态设置布局显示前的状态
	public virtual void onGameState() { }
	public virtual void onDrawGizmos() { }
	public virtual void onHide(){}
	// 通知脚本开始显示或隐藏,中断全部命令
	public void notifyStartShowOrHide()
	{
		interruptAllCommand();
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
	public static GameObject instantiate(myUIObject parent, string prefabPath, string name)
	{
		GameObject go = mPrefabPoolManager.createObject(prefabPath, 0, false, false, parent.getObject());
		if (go != null)
		{
			go.name = name;
		}
		return go;
	}
	public static CustomAsyncOperation instantiateAsync(string prefabPath, string name, GameObjectCallback callback)
	{
		return mPrefabPoolManager.createObjectAsync(prefabPath, 0, false, false, (GameObject go) =>
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
		LT.HIDE(GetType());
	}
}