using System.Collections.Generic;
using System;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static FrameEditorUtility;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
// 多线程的对象池无法判断临时对象有没有正常回收,因为子线程的帧与主线程不同步
public class ClassPoolSingle : ClassObject
{
	protected HashSet<ClassObject> mInusedList = new();     // 已使用的列表,为了提高运行时效率,仅在编辑器下使用
	protected Queue<ClassObject> mUnusedList = new();		// 未使用的列表
	protected ThreadLock mListLock = new();					// 列表的线程锁
	protected Type mType;									// 对象类型
	protected static long mAssignIDSeed;					// 分配ID的种子
	public override void resetProperty()
	{
		base.resetProperty();
		mInusedList.Clear();
		mUnusedList.Clear();
		// mListLock.unlock();
		mType = null;
	}
	public void setType(Type type) { mType = type; }
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public ClassObject newClass()
	{
		ClassObject obj = null;
		using (new ThreadLockScope(mListLock))
		{
			try
			{
				// 先从未使用的列表中查找是否有可用的对象
				if (mUnusedList.Count > 0)
				{
					obj = mUnusedList.Dequeue();
				}
				// 未使用列表中没有,创建一个新的
				else
				{
					obj = createInstance<ClassObject>(mType);
					// 创建实例时重置是为了与后续复用的实例状态保持一致
					obj.resetProperty();
					if (isEditor() && mInusedList.Count > 0 && mInusedList.Count % 1000 == 0)
					{
						Debug.Log("创建的总数量已经达到:" + mInusedList.Count + "个,type:" + mType);
					}
				}
				obj.setAssignID(++mAssignIDSeed);
				obj.setDestroy(false);
				// 添加到已使用列表
				if (isEditor() && !mInusedList.Add(obj))
				{
					Debug.LogError("加入已使用列表失败, Type: " + mType);
				}
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
		// 由于传递的是引用,所以为了确保不受外部影响,定义一个临时变量
		T temp = classObject;
		classObject = null;
		if (temp == null)
		{
			return;
		}
		// 先重置属性
		temp.setPendingDestroy(true);
		temp.destroy();
		temp.setDestroy(true);
		temp.resetProperty();

		// 再加入未使用列表
		using (new ThreadLockScope(mListLock))
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			if (isEditor())
			{
				if (mUnusedList.Contains(temp))
				{
					Debug.LogError("对象已经在未使用列表中,无法再次销毁! Type: " + mType);
				}
				if (!mInusedList.Remove(temp))
				{
					Debug.LogError("已使用列表找不到指定对象, Type: " + mType);
				}
			}
			mUnusedList.Enqueue(temp);
		}
	}
}