using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;
using static FrameBase;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
// 只能在主线程使用的对象池
public class ClassPool : FrameSystem
{
	protected Dictionary<Type, HashSet<ClassObject>> mPersistentInuseList;  // 持久使用的对象列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, HashSet<ClassObject>> mInusedList;           // 仅这一帧使用的对象列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, List<ClassObject>> mUnusedList;              // 未使用的对象列表
	protected Dictionary<ClassObject, string> mObjectStack;                 // 存储堆栈信息的列表
	protected static long mAssignIDSeed;                                    // 分配ID,用于标识每一个分配出去的对象
	public ClassPool()
	{
		mPersistentInuseList = new Dictionary<Type, HashSet<ClassObject>>();
		mInusedList = new Dictionary<Type, HashSet<ClassObject>>();
		mUnusedList = new Dictionary<Type, List<ClassObject>>();
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
				if (isEmpty(stack))
				{
					stack = "当前未开启对象池的堆栈追踪,可在对象分配前使用F4键开启堆栈追踪,然后就可以在此错误提示中看到对象分配时所在的堆栈\n";
				}
				else
				{
					stack = "create stack:\n" + stack + "\n";
				}
				logError("有临时对象正在使用中,是否在申请后忘记回收到池中! \n" + stack + ", type:" + itemList.GetType());
				break;
			}
		}
#endif
	}
	public void clearUnused() 
	{
		foreach (var item in mUnusedList)
		{
			item.Value.Clear();
		}
	}
	public Dictionary<Type, HashSet<ClassObject>> getPersistentInusedList() { return mPersistentInuseList; }
	public Dictionary<Type, HashSet<ClassObject>> getInusedList() { return mInusedList; }
	public Dictionary<Type, List<ClassObject>> getUnusedList() { return mUnusedList; }
	// isNewObject表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public ClassObject newClass(Type type, out bool isNewObject, bool onlyOnce, bool newDirect)
	{
		isNewObject = false;
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用此对象池,子线程中请使用ClassPoolThread代替");
			return null;
		}
#endif
		if (type == null)
		{
			return null;
		}
		int createSource = 0;
		ClassObject obj;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(type, out List<ClassObject> classList) && classList.Count > 0)
		{
			obj = classList[classList.Count - 1];
			classList.RemoveAt(classList.Count - 1);
			createSource = 1;
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			if (newDirect)
			{
				obj = createInstanceDirect<ClassObject>(type);
				createSource = 2;
			}
			else
			{
				obj = createInstance<ClassObject>(type);
				createSource = 3;
			}
			// 创建实例时重置是为了与后续复用的实例状态保持一致
			obj.resetProperty();
			isNewObject = true;
#if UNITY_EDITOR
			mInusedList.TryGetValue(type, out HashSet<ClassObject> temp0);
			mPersistentInuseList.TryGetValue(type, out HashSet<ClassObject> temp1);
			int totalCount = 1;
			if (temp0 != null)
			{
				totalCount += temp0.Count;
			}
			if (temp1 != null)
			{
				totalCount += temp1.Count;
			}
			if (totalCount % 1000 == 0)
			{
				Debug.Log("创建的总数量已经达到:" + totalCount + "个,type:" + type);
			}
#endif
		}
		obj.setAssignID(++mAssignIDSeed);
		obj.setDestroy(false);

#if UNITY_EDITOR
		// 添加到已使用列表
		var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
		if (!inuseList.TryGetValue(type, out HashSet<ClassObject> objList))
		{
			objList = new HashSet<ClassObject>();
			inuseList.Add(type, objList);
		}
		if (!objList.Add(obj))
		{
			Debug.LogError("对象已经在已使用列表中了,不能再添加,是否为持久使用:" + onlyOnce + ", 创建来源:" + IToS(createSource) + ", type:" + type);
		}
		if (mGameFramework.mEnablePoolStackTrace)
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
	public T newClass<T>(out T obj, bool onlyOnce, bool newDirect) where T : ClassObject
	{
		ClassObject classObj = newClass(typeof(T), out _, onlyOnce, newDirect);
		obj = classObj as T;
		if (obj == null)
		{
			Debug.LogError("创建类实例失败,可能传入的type类型与目标类型不一致");
		}
		return obj;
	}
	public void destroyClass<T>(ref T classObject) where T : ClassObject
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
#endif
		Type type = Typeof(classObject);

		// 加入未使用列表
		if (!mUnusedList.TryGetValue(type, out List<ClassObject> objList))
		{
			objList = new List<ClassObject>();
			mUnusedList.Add(type, objList);
		}
#if UNITY_EDITOR
		mObjectStack.Remove(classObject);
		if (objList.Contains(classObject))
		{
			Debug.LogError("ClassObject is in Unused list! can not add again! Type: " + type);
			return;
		}
		removeInuse(classObject, type);
#endif
		objList.Add(classObject);
		classObject.resetProperty();
		classObject.setDestroy(true);
		classObject = null;
	}
	public void destroyClass<T>(List<T> classObjectList) where T : ClassObject
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
#endif

		int count = classObjectList.Count;
		if (count == 0)
		{
			return;
		}
		for (int i = 0; i < count; ++i)
		{
			ClassObject classObject = classObjectList[i];
			if (classObject == null)
			{
				continue;
			}
			Type type = Typeof(classObject);
			if (!mUnusedList.TryGetValue(type, out List<ClassObject> objList))
			{
				objList = new List<ClassObject>();
				mUnusedList.Add(type, objList);
			}
#if UNITY_EDITOR
			mObjectStack.Remove(classObject);
			if (objList.Contains(classObject))
			{
				Debug.LogError("ClassObject is in Unused list! can not add again! Type: " + type);
				continue;
			}
			removeInuse(classObject, type);
#endif
			classObject.resetProperty();
			classObject.setDestroy(true);
			// 加入未使用列表
			objList.Add(classObject);
		}
		classObjectList.Clear();
	}
	public void destroyClass<K, T>(Dictionary<K, T> classObjectList) where T : ClassObject
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
#endif

		int count = classObjectList.Count;
		if (count == 0)
		{
			return;
		}
		
		foreach (var item in classObjectList)
		{
			ClassObject classObject = item.Value;
			if (classObject == null)
			{
				continue;
			}
			Type type = Typeof(classObject);
			if (!mUnusedList.TryGetValue(type, out List<ClassObject> objList))
			{
				objList = new List<ClassObject>();
				mUnusedList.Add(type, objList);
			}
#if UNITY_EDITOR
			mObjectStack.Remove(classObject);
			if (objList.Contains(classObject))
			{
				Debug.LogError("ClassObject is in Unused list! can not add again! Type: " + type);
				continue;
			}
			removeInuse(classObject, type);
#endif
			classObject.resetProperty();
			classObject.setDestroy(true);
			// 加入未使用列表
			objList.Add(classObject);
		}
		classObjectList.Clear();
	}
	public void destroyClassReally(ref ClassObject classObject)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
		mObjectStack.Remove(classObject);
		Type type = Typeof(classObject);
		// 从已使用列表中移除
		if (mInusedList.TryGetValue(type, out HashSet<ClassObject> list0) && list0.Contains(classObject))
		{
			removeInuse(classObject, type);
		}
		// 从未使用列表中移除
		else
		{
			if (mUnusedList.TryGetValue(type, out List<ClassObject> list) && list.Count > 0)
			{
				list.Remove(classObject);
			}
		}
#endif
		classObject = null;
	}
	public void destroyClassReally(List<ClassObject> classObjectList)
	{
#if UNITY_EDITOR
		if (!isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
		int count = classObjectList.Count;
		if (count == 0)
		{
			return;
		}
		Type type = null;
		for (int i = 0; i < count; ++i)
		{
			ClassObject classObject = classObjectList[i];
			if (classObject != null)
			{
				type = Typeof(classObject);
				break;
			}
		}
		if (type == null)
		{
			return;
		}
		mInusedList.TryGetValue(type, out HashSet<ClassObject> list0);
		mUnusedList.TryGetValue(type, out List<ClassObject> list1);
		for (int i = 0; i < count; ++i)
		{
			ClassObject classObject = classObjectList[i];
			mObjectStack.Remove(classObject);
			// 从已使用列表中移除
			if (list0 != null && list0.Contains(classObject))
			{
				removeInuse(classObject, type);
			}
			// 从未使用列表中移除
			if (list1 != null && list1.Count > 0)
			{
				if (list1.Remove(classObject))
				{
					Debug.LogError("要移除的对象仍然存在于UnusedList中:" + type);
				}
			}
		}
#endif
		classObjectList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 为了提升效率,所以传一个type进来
	protected void removeInuse(ClassObject classObject, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(type, out HashSet<ClassObject> classList) && classList.Remove(classObject))
		{
			return;
		}
		if (mPersistentInuseList.TryGetValue(type, out classList) && classList.Remove(classObject))
		{
			return;
		}
		Debug.LogError("Inused List not contains class object! Type: " + type);
	}
}