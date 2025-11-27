using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseHotFix;

public abstract class WindowObjectBase : ILocalizationCollection, IWindowObjectOwner
{
	protected WindowObjectBase mParent;                     // 因为会有一些WindowObject中嵌套WindowObject,所以需要存储一个父节点
	protected List<WindowObjectBase> mChildList;            // 当前WindowObject的所有子节点
	protected List<IDragViewLoop> mDragViewLoopList;        // 当前WindowObject拥有的DragViewLoop列表,用于调用列表的update
	protected List<WindowStructPoolBase> mPoolList;			// 此物体创建的对象池列表
	protected HashSet<IUGUIObject> mLocalizationObjectList; // 需要本地化的文本对象
	protected LayoutScript mScript;							// 所属的布局脚本
	protected bool mDestroied;                              // 是否已经销毁过了,用于检测重复销毁的
	protected bool mInited;                                 // 是否已经初始化过了,用于检测重复初始化
	protected bool mCalledOnHide;                           // 是否已经调用过了onHide
	protected bool mNeedUpdate;                             // 是否需要调用此对象的update,默认不调用update
	protected bool mUnuseAllWhenHide = true;				// 是否在隐藏时将引用的对象池中的对象全部回收
	public WindowObjectBase(IWindowObjectOwner parent)
	{
		if (parent is WindowObjectBase objBase)
		{
			mScript = objBase.mScript;
			mParent = objBase;
		}
		else if (parent is LayoutScript script)
		{
			mScript = script;
			mParent = null;
		}
		mParent?.addChild(this);
	}
	// 第一次创建时调用,如果是对象池中的物体,则由对象池调用,非对象池物体需要在使用的地方自己调用,不过一般会由所在的UI类自动调用
	public virtual void init() 
	{
		if (mInited)
		{
			logError("已经初始化过了,type:" + GetType());
			return;
		}
		mInited = true;
		mDestroied = false;
		foreach (WindowObjectBase item in mChildList.safe())
		{
			item.init();
		}
	}
	// 每次被分配使用时调用,如果是对象池中的物体,则由对象池调用,非对象池物体需要在使用的地方自己调用
	public virtual void reset() 
	{
		mCalledOnHide = false;
		foreach (WindowObjectBase item in mChildList.safe())
		{
			item.reset();
		}
	}
	public void updateDragViewLoop()
	{
		// 更新自己的滚动列表
		foreach (IDragViewLoop item in mDragViewLoopList.safe())
		{
			if (item.isActive())
			{
				item.updateDragView();
			}
		}
		// 更新所有子节点的滚动列表
		foreach (WindowObjectBase item in mChildList.safe())
		{
			if (item.isActive())
			{
				item.updateDragViewLoop();
			}
		}
	}
	// 这个update需要主动调用,界面管理器不会自动去调用这个update,因为对效率影响比较大
	public virtual void update(){}
	// 被隐藏时调用,界面被隐藏时也会调用所有子窗口的onHide
	public virtual void onHide() 
	{
		if (mCalledOnHide)
		{
			logError("已经调用过onHide了,type:" + GetType() + ", hash:" + GetHashCode());
			return;
		}
		mCalledOnHide = true;
		foreach (WindowObjectBase item in mChildList.safe())
		{
			if (item.isActive())
			{
				item.onHide();
			}
		}
		if (mUnuseAllWhenHide)
		{
			foreach (WindowStructPoolBase item in mPoolList.safe())
			{
				item.unuseAll();
			}
		}
	}
	public virtual void destroy()
	{
		if (mDestroied)
		{
			logWarning("WindowObject重复销毁对象:" + GetType() + ",hash:" + GetHashCode());
		}
		mDestroied = true;

		foreach (WindowObjectBase item in mChildList.safe())
		{
			item.destroy();
		}
		mLocalizationManager?.unregisteLocalization(mLocalizationObjectList);
		mLocalizationObjectList?.Clear();
	}
	public void addLocalizationObject(IUGUIObject obj)
	{
		mLocalizationObjectList ??= new();
		mLocalizationObjectList.Add(obj);
	}
	public virtual bool isActive() { return false; }
	public virtual void setActive(bool active) 
	{
		if (active)
		{
			mCalledOnHide = false;
			// 需要标记所有子节点也允许再调用onHide
			foreach (WindowObjectBase item in mChildList.safe())
			{
				item.mCalledOnHide = false;
			}
		}
		else if (isActive())
		{
			onHide();
		}
	}
	public virtual void setParent(myUGUIObject parent, bool refreshDepth = true) { }
	public virtual void setAsLastSibling(bool refreshDepth = true) { }
	public virtual void setAsFirstSibling(bool refreshDepth = true) { }
	public bool isRootWindowObject() { return mParent == null; }
	public void addWindowPool(WindowStructPoolBase pool) 
	{
		mPoolList ??= new();
		if (!mPoolList.addUnique(pool))
		{
			logError("重复加入对象池");
		}
	}
	public LayoutScript getScript() { return mScript; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected T0 newObject<T0>(out T0 obj, myUGUIObject parent, string name, bool showError) where T0 : myUGUIObject, new()
	{
		return mScript.newObject(out obj, parent, name, showError);
	}
	protected T0 newObject<T0>(out T0 obj, myUGUIObject parent, string name) where T0 : myUGUIObject, new()
	{
		return mScript.newObject(out obj, parent, name, true);
	}
	protected void addChild(WindowObjectBase child)
	{
		mChildList ??= new();
		if (!mChildList.addUnique(child))
		{
			logError("重复加入子节点");
		}
		if (child is IDragViewLoop dragViewLoop)
		{
			mDragViewLoopList ??= new();
			mDragViewLoopList.Add(dragViewLoop);
		}
	}
	// 由于如果让应用层子类都去重写多个assignWindow就会显得很繁琐,而且会有重复代码
	// 所以应用层子类只需要重写assignWindowInternal,在这里写逻辑即可,然后会在assignWindow中调用assignWindowInternal
	protected abstract void assignWindowInternal();
}