using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class ClassPoolSingle : ClassObject
{
	protected HashSet<ClassObject> mInusedList;     // 已使用的列表,为了提高运行时效率,仅在编辑器下使用
	protected List<ClassObject> mUnusedList;		// 未使用的列表
	protected ThreadLock mListLock;					// 列表的线程锁
	protected Type mType;							// 对象类型
	protected static long mAssignIDSeed;            // 分配ID的种子
	public ClassPoolSingle()
	{
		mInusedList = new HashSet<ClassObject>();
		mUnusedList = new List<ClassObject>();
		mListLock = new ThreadLock();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mInusedList.Clear();
		mUnusedList.Clear();
		//mListLock.unlock();
		mType = null;
	}
	public void setType(Type type) { mType = type; }
	public Type getType() { return mType; }
	public void clearUnused()
	{
		using (new ThreadLockScope(mListLock))
		{
			mUnusedList.Clear();
		}
	}
	public HashSet<ClassObject> getInusedList() { return mInusedList; }
	public List<ClassObject> getUnusedList() { return mUnusedList; }
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public ClassObject newClass(out bool isNewObject, bool newDirect)
	{
		isNewObject = false;
		ClassObject obj = null;
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.Count > 0)
				{
					obj = mUnusedList[mUnusedList.Count - 1];
					mUnusedList.RemoveAt(mUnusedList.Count - 1);
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					if (newDirect)
					{
						obj = createInstanceDirect<ClassObject>(mType);
					}
					else
					{
						obj = createInstance<ClassObject>(mType);
					}
					// 创建实例时重置是为了与后续复用的实例状态保持一致
					obj.resetProperty();
					isNewObject = true;
#if UNITY_EDITOR
					if (mInusedList.Count > 0 && mInusedList.Count % 1000 == 0)
					{
						Debug.Log("创建的总数量已经达到:" + mInusedList.Count + "个,type:" + mType);
					}
#endif
				}
				obj.setAssignID(++mAssignIDSeed);
				obj.setDestroy(false);
				// 添加到已使用列表
#if UNITY_EDITOR
				if (!mInusedList.Add(obj))
				{
					Debug.LogError("加入已使用列表失败, Type: " + mType);
				}
#endif
			}
			catch (Exception e)
			{
				logException(e);
			}
		}
		return obj;
	}
	public void destroyClass<T>(ref T classObject) where T : ClassObject
	{
		// 先重置属性
		classObject.resetProperty();
		classObject.setDestroy(true);

		// 再加入未使用列表
		using (new ThreadLockScope(mListLock))
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
#if UNITY_EDITOR
			if (mUnusedList.Contains(classObject))
			{
				Debug.LogError("对象已经在未使用列表中,无法再次销毁! Type: " + mType);
			}
			if (!mInusedList.Remove(classObject))
			{
				Debug.LogError("已使用列表找不到指定对象, Type: " + mType);
			}
#endif
			mUnusedList.Add(classObject);
			classObject = null;
		}
	}
	public void destroyClass(List<ClassObject> classObjectList)
	{
		// 先重置属性
		int allCount = classObjectList.Count;
		for (int i = 0; i < allCount; ++i)
		{
			ClassObject classObject = classObjectList[i];
			classObject.resetProperty();
			classObject.setDestroy(true);
		}
		// 再加入未使用列表
		using (new ThreadLockScope(mListLock))
		{
			for (int i = 0; i < allCount; ++i)
			{
				ClassObject classObject = classObjectList[i];
				// 从使用列表移除,要确保操作的都是从本类创建的实例
#if UNITY_EDITOR
				if (mUnusedList.Contains(classObject))
				{
					Debug.LogError("对象已经在未使用列表中,无法再次销毁! Type: " + mType);
				}
				if (!mInusedList.Remove(classObject))
				{
					Debug.LogError("已使用列表找不到指定对象, Type: " + mType);
				}
#endif
				mUnusedList.Add(classObject);
			}
		}
		classObjectList.Clear();
	}
	public void destroyClassReally(ref ClassObject classObject)
	{
#if UNITY_EDITOR
		using (new ThreadLockScope(mListLock))
		{
			// 从已使用列表中移除
			if (!mInusedList.Remove(classObject))
			{
				Debug.LogError("移除失败:" + mType);
			}
			if (mUnusedList.Contains(classObject))
			{
				Debug.LogError("要移除的对象仍然存在于UnusedList中:" + mType);
			}
		}
#endif
		classObject = null;
	}
	public void destroyClassReally(List<ClassObject> classObjectList)
	{
#if UNITY_EDITOR
		using (new ThreadLockScope(mListLock))
		{
			int count = classObjectList.Count;
			for (int i = 0; i < count; ++i)
			{
				ClassObject classObject = classObjectList[i];
				// 从已使用列表中移除
				if (!mInusedList.Remove(classObject))
				{
					Debug.LogError("移除失败:" + mType);
				}
				if (mUnusedList.Contains(classObject))
				{
					Debug.LogError("要移除的对象仍然存在于UnusedList中:" + mType);
				}
			}
		}
#endif
		classObjectList.Clear();
	}
}