using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ClassPool : FrameSystem
{
	protected Dictionary<Type, List<IClassObject>> mInusedList;
	protected Dictionary<Type, Stack<IClassObject>> mUnusedList;
	protected ThreadLock mListLock;
	public ClassPool()
	{
		mInusedList = new Dictionary<Type, List<IClassObject>>();
		mUnusedList = new Dictionary<Type, Stack<IClassObject>>();
		mListLock = new ThreadLock();
	}
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public bool newClass(out IClassObject obj, Type type)
	{
		bool isNewObject = false;
		obj = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.ContainsKey(type) && mUnusedList[type].Count > 0)
			{
				obj = mUnusedList[type].Pop();
				isNewObject = false;
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				obj = createInstance<IClassObject>(type);
				isNewObject = true;
			}
			// 添加到已使用列表
			addInuse(obj);
		}
		catch(Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		// 重置实例
		obj?.resetProperty();
		return isNewObject;
	}
	public bool newClass<T>(out T obj, Type type) where T : class, IClassObject
	{
		IClassObject classObj;
		bool ret = newClass(out classObj, type);
		obj = classObj as T;
		if (obj == null)
		{
			logError("创建类实例失败,可能传入的type类型与目标类型不一致");
		}
		return ret;
	}
	public void destroyClass(IClassObject classObject)
	{
		mListLock.waitForUnlock();
		try
		{
			addUnuse(classObject);
			removeInuse(classObject);
		}
		catch(Exception e)
		{
			logError(e.Message);
		}	
		mListLock.unlock();
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(IClassObject classObject)
	{
		Type type = Typeof(classObject);
		if (!mInusedList.ContainsKey(type))
		{
			mInusedList.Add(type, new List<IClassObject>());
		}
		else
		{
			if (mInusedList[type].Contains(classObject))
			{
				logError("object is in inused list");
				return;
			}
		}
		// 加入使用列表
		mInusedList[type].Add(classObject);
	}
	protected void removeInuse(IClassObject classObject)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = Typeof(classObject);
		if (!mInusedList.ContainsKey(type))
		{
			logError("can not find class type in Inused List! type : " + type);
		}
		if (!mInusedList[type].Remove(classObject))
		{
			logError("Inused List not contains class object!");
		}
	}
	protected void addUnuse(IClassObject classObject)
	{
		// 加入未使用列表
		Type type = Typeof(classObject);
		if (!mUnusedList.ContainsKey(type))
		{
			mUnusedList.Add(type, new Stack<IClassObject>());
		}
		else
		{
			if (mUnusedList[type].Contains(classObject))
			{
				logError("ClassObject is in Unused list! can not add again!");
				return;
			}
		}
		mUnusedList[type].Push(classObject);
	}
}