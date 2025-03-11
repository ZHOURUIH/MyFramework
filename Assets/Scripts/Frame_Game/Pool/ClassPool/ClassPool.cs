using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static StringUtility;
using static CSharpUtility;
using static FrameBase;
using static FrameEditorUtility;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
// 只能在主线程使用的对象池
public class ClassPool : FrameSystem
{
	protected Dictionary<Type, HashSet<ClassObject>> mPersistentInuseList = new();  // 持久使用的对象列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, HashSet<ClassObject>> mInusedList = new();           // 仅这一帧使用的对象列表,为了提高运行时效率,仅在编辑器下使用
	protected Dictionary<Type, Queue<ClassObject>> mUnusedList = new();             // 未使用的对象列表
	protected Dictionary<ClassObject, string> mObjectStack = new();                 // 存储堆栈信息的列表
	protected static long mAssignIDSeed;									        // 分配ID,用于标识每一个分配出去的对象
	public ClassPool()
	{
		mCreateObject = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (isEditor())
		{
			foreach (var item in mInusedList.Values)
			{
				foreach (ClassObject itemList in item)
				{
					string stack = mObjectStack.get(itemList);
					if (stack.isEmpty())
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
		}
	}
	public T newClass<T>(bool onlyOnce) where T : ClassObject
	{
		return newClass(typeof(T), onlyOnce) as T;
	}
	// isNewObject表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public ClassObject newClass(Type type, bool onlyOnce)
	{
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程中使用此对象池,子线程中请使用ClassPoolThread代替");
			return null;
		}
		if (type == null)
		{
			return null;
		}
		int createSource = 0;
		ClassObject obj;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.TryGetValue(type, out var classList) && classList.Count > 0)
		{
			obj = classList.Dequeue();
			createSource = 1;
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			obj = createInstance<ClassObject>(type);
			createSource = 3;
			// 创建实例时重置是为了与后续复用的实例状态保持一致
			obj.resetProperty();
			if (isEditor())
			{
				int totalCount = 1;
				totalCount += mInusedList.get(type)?.Count ?? 0;
				totalCount += mPersistentInuseList.get(type)?.Count ?? 0;
				if (totalCount % 1000 == 0)
				{
					Debug.Log("创建的总数量已经达到:" + totalCount + "个,type:" + type);
				}
			}
		}
		obj.setAssignID(++mAssignIDSeed);
		obj.setDestroy(false);

		if (isEditor())
		{
			// 添加到已使用列表
			var inuseList = onlyOnce ? mInusedList : mPersistentInuseList;
			if (!inuseList.getOrAddNew(type).Add(obj))
			{
				Debug.LogError("对象已经在已使用列表中了,不能再添加,是否为持久使用:" + onlyOnce + ", 创建来源:" + IToS(createSource) + ", type:" + type);
			}
			mObjectStack.Add(obj, mGameFramework.mParam.mEnablePoolStackTrace ? getStackTrace() : EMPTY);
		}
		return obj;
	}
	public void destroyClass<T>(ref T classObject) where T : ClassObject
	{
		// 由于传递的是引用,所以为了确保不受外部影响,定义一个临时变量
		T temp = classObject;
		classObject = null;
		if (temp == null)
		{
			return;
		}
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
		temp.setPendingDestroy(true);
		temp.destroy();

		// 加入未使用列表
		Type type = temp.GetType();
		var objList = mUnusedList.getOrAddNew(type);
		if (isEditor())
		{
			mObjectStack.Remove(temp);
			if (objList.Contains(temp))
			{
				Debug.LogError("ClassObject is in Unused list! can not add again! Type: " + type);
				return;
			}
			removeInuse(temp, type);
		}
		objList.Enqueue(temp);
		temp.setDestroy(true);
		temp.resetProperty();
	}
	public void destroyClassList<T>(ICollection<T> classObjectList) where T : ClassObject
	{
		if (isEditor() && !isMainThread())
		{
			Debug.LogError("只能在主线程中使用ClassPool,子线程中请使用ClassPoolThread代替");
			return;
		}
		if (classObjectList == null)
		{
			return;
		}
		int count = classObjectList.Count;
		if (count == 0)
		{
			return;
		}
		foreach (T classObject in classObjectList)
		{
			if (classObject == null)
			{
				continue;
			}
			classObject.setPendingDestroy(true);
			classObject.destroy();
			Type type = classObject.GetType();
			var objList = mUnusedList.getOrAddNew(type);
			if (isEditor())
			{
				mObjectStack.Remove(classObject);
				if (objList.Contains(classObject))
				{
					Debug.LogError("ClassObject is in Unused list! can not add again! Type: " + type);
					continue;
				}
				removeInuse(classObject, type);
			}
			classObject.setDestroy(true);
			classObject.resetProperty();
			// 加入未使用列表
			objList.Enqueue(classObject);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 为了提升效率,所以传一个type进来
	protected void removeInuse(ClassObject classObject, Type type)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (mInusedList.TryGetValue(type, out var classList) && classList.Remove(classObject))
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