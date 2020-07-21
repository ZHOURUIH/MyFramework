using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ClassPool : FrameComponent
{
	protected Dictionary<Type, List<IClassObject>> mInusedList;
	protected Dictionary<Type, List<IClassObject>> mUnusedList;
	protected ThreadLock mListLock;
	public ClassPool(string name)
		: base(name)
	{
		mInusedList = new Dictionary<Type, List<IClassObject>>();
		mUnusedList = new Dictionary<Type, List<IClassObject>>();
		mListLock = new ThreadLock();
	}
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public bool newClass(out IClassObject obj, Type type)
	{
		bool isNewObject = false;
		string info = null;
		obj = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.ContainsKey(type) && mUnusedList[type].Count > 0)
			{
				obj = mUnusedList[type][0];
				isNewObject = false;
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				obj = createInstance<IClassObject>(type);
				isNewObject = true;
			}
			// 标记为已使用
			if (!markUsed(obj))
			{
				info = "ClassObject is in Inused list! can not add again!";
			}
		}
		catch(Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		if (!isEmpty(info))
		{
			logError(info);
		}
		// 重置实例
		obj?.resetProperty();
		return isNewObject;
	}
	public bool newClass<T>(out T obj) where T : IClassObject, new()
	{
		IClassObject classObj;
		bool ret = newClass(out classObj, typeof(T));
		obj = (T)classObj;
		return ret;
	}
	public void destroyClass(IClassObject classObject)
	{
		string info = "";
		bool ret = false;
		mListLock.waitForUnlock();
		try
		{
			ret = markUnused(classObject, ref info);
		}
		catch(Exception e)
		{
			logError(e.Message);
		}	
		mListLock.unlock();
		if(!ret)
		{
			logError(info);
		}
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected bool checkUsed(IClassObject classObject)
	{
		Type type = classObject.GetType();
		return mInusedList.ContainsKey(type) && mInusedList[type].Contains(classObject);
	}
	protected bool markUsed(IClassObject classObject)
	{
		Type type = classObject.GetType();
		if (!mInusedList.ContainsKey(type))
		{
			mInusedList.Add(type, new List<IClassObject>());
		}
		else
		{
			if (mInusedList[type].Contains(classObject))
			{
				return false;
			}
		}
		// 加入使用列表
		mInusedList[type].Add(classObject);
		// 从未使用列表移除
		if (mUnusedList.ContainsKey(type))
		{
			mUnusedList[type].Remove(classObject);
		}
		return true;
	}
	protected bool markUnused(IClassObject classObject, ref string info)
	{
		// 加入未使用列表
		Type type = classObject.GetType();
		if (!mUnusedList.ContainsKey(type))
		{
			mUnusedList.Add(type, new List<IClassObject>());
		}
		else
		{
			if (mUnusedList[type].Contains(classObject))
			{
				info = "ClassObject is in Unused list! can not add again!";
				return false;
			}
		}
		mUnusedList[type].Add(classObject);
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (!mInusedList.ContainsKey(type))
		{
			info = "can not find class type in Inused List! type : " + type;
			return false;
		}
		if (!mInusedList[type].Remove(classObject))
		{
			info = "Inused List not contains class object!";
			return false;
		}
		return true;
	}
}