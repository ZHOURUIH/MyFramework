using System.Collections.Generic;
using System;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
// 只能在主线程使用的对象池
public class ClassPool : FrameSystem
{
	protected Dictionary<Type, HashSet<ClassObject>> mPersistentInuseList;
	protected Dictionary<Type, HashSet<ClassObject>> mInusedList;
	protected Dictionary<Type, HashSet<ClassObject>> mUnusedList;
	protected Dictionary<ClassObject, string> mObjectStack;
	protected static long mAssignIDSeed;
	public ClassPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<ClassObject>>();
		mInusedList = new Dictionary<Type, HashSet<ClassObject>>();
		mUnusedList = new Dictionary<Type, HashSet<ClassObject>>();
		mObjectStack = new Dictionary<ClassObject, string>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<ClassPoolDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		foreach (var item in mInusedList)
		{
			foreach (var itemList in item.Value)
			{
				string stack = mObjectStack[itemList];
				if(isEmpty(stack))
				{
					stack = "当前未开启对象池的堆栈追踪,可在对象分配前使用F4键开启堆栈追踪,然后就可以在此错误提示中看到对象分配时所在的堆栈\n";
				}
				else
				{
					stack = "create stack:\n" + stack + "\n";
				}
				logError("有临时对象正在使用中,是否在申请后忘记回收到池中! \n" + stack);
				break;
			}
		}
#endif
	}
	public Dictionary<Type, HashSet<ClassObject>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<ClassObject>> getInusedList() { return mInusedList; }
	public Dictionary<Type, HashSet<ClassObject>> getUnusedList() { return mUnusedList; }
	public ClassObject newClass(Type type, bool onlyOnce)
	{
		return newClass(type, out _, onlyOnce);
	}
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public ClassObject newClass(Type type, out bool isNewObject, bool onlyOnce)
	{
		isNewObject = false;
		if(!isMainThread())
		{
			logError("只能在主线程中使用此对象池,子线程中请使用ClassPoolThread代替");
			return null;
		}
		if (type == null)
		{
			return null;
		}
		ClassObject obj = null;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(type, out HashSet<ClassObject> classList) && classList.Count > 0)
		{
			foreach (var item in classList)
			{
				obj = item;
				break;
			}
			classList.Remove(obj);
			isNewObject = false;
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			obj = createInstance<ClassObject>(type);
			// 创建实例时重置是为了与后续复用的实例状态保持一致
			obj.resetProperty();
			isNewObject = true;
		}
		obj.setAssignID(++mAssignIDSeed);
		obj.setDestroy(false);
		// 添加到已使用列表
		addInuse(obj, onlyOnce);
#if UNITY_EDITOR
		if(mGameFramework.isEnablePoolStackTrace())
		{
			mObjectStack.Add(obj, getStackTrace());
		}
		else
		{
			mObjectStack.Add(obj, EMPTY);
		}
#endif
		return obj;
	}
	// 仅用于主工程中的类,否则无法识别
	public T newClass<T>(out T obj, bool onlyOnce) where T : ClassObject
	{
		ClassObject classObj = newClass(Typeof<T>(), onlyOnce);
		obj = classObj as T;
		if (obj == null)
		{
			logError("创建类实例失败,可能传入的type类型与目标类型不一致");
		}
		return obj;
	}
	public void destroyClass(ClassObject classObject)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(classObject);
#endif
		classObject.resetProperty();
		classObject.setDestroy(true);
		addUnuse(classObject);
		removeInuse(classObject);
	}
	public void destroyClassReally(ClassObject classObject)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
#if UNITY_EDITOR
		mObjectStack.Remove(classObject);
#endif
		bool inuse = isInuse(classObject);
		classObject.resetProperty();
		classObject.setDestroy(true);
		// 从已使用列表中移除
		if (inuse)
		{
			removeInuse(classObject);
		}
		// 从未使用列表中移除
		else
		{
			if (mUnusedList.TryGetValue(Typeof(classObject), out HashSet<ClassObject> list) && list.Count > 0)
			{
				list.Remove(classObject);
			}
		}
	}
	public bool isInuse(ClassObject classObject)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return false;
		}
		return mInusedList.TryGetValue(Typeof(classObject), out HashSet<ClassObject> list) && list.Contains(classObject);
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(ClassObject classObject, bool onlyOnce)
	{
		Type type = Typeof(classObject);
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<ClassObject> objList))
		{
			objList = new HashSet<ClassObject>();
			inuseList.Add(type, objList);
		}
		else if (objList.Contains(classObject))
		{
			logError("object is in inused list");
			return;
		}
		// 加入使用列表
		objList.Add(classObject);
	}
	protected void removeInuse(ClassObject classObject)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = Typeof(classObject);
		HashSet<ClassObject> classList;
		if (mInusedList.TryGetValue(type, out classList) && classList.Remove(classObject))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(type, out classList) && classList.Remove(classObject))
		{
			return;
		}
		logError("Inused List not contains class object! Type: " + type);
	}
	protected void addUnuse(ClassObject classObject)
	{
		// 加入未使用列表
		Type type = Typeof(classObject);
		if (!mUnusedList.TryGetValue(type, out HashSet<ClassObject> objList))
		{
			objList = new HashSet<ClassObject>();
			mUnusedList.Add(type, objList);
		}
		else if (objList.Contains(classObject))
		{
			logError("ClassObject is in Unused list! can not add again! Type: " + type);
			return;
		}
		objList.Add(classObject);
	}
}