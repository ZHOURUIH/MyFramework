using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
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
	public new IClassObject newClass(Type type)
	{
		return newClass(type, out _);
	}
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public IClassObject newClass(Type type, out bool isNewObject)
	{
		isNewObject = false;
		if (type == null)
		{
			return null;
		}
		IClassObject obj = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.TryGetValue(type, out Stack<IClassObject> classList) && classList.Count > 0)
			{
				obj = classList.Pop();
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
		
		return obj;
	}
	// 仅用于主工程中的类,否则无法识别
	public T newClass<T>(out T obj) where T : class, IClassObject
	{
		IClassObject classObj = newClass(Typeof<T>());
		obj = classObj as T;
		if (obj == null)
		{
			logError("创建类实例失败,可能传入的type类型与目标类型不一致");
		}
		return obj;
	}
	public new void destroyClass(IClassObject classObject)
	{
		mListLock.waitForUnlock();
		try
		{
			classObject.resetProperty();
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
		if (!mInusedList.TryGetValue(type, out List<IClassObject> objList))
		{
			objList = new List<IClassObject>();
			mInusedList.Add(type, objList);
		}
		else
		{
			if (objList.Contains(classObject))
			{
				logError("object is in inused list");
				return;
			}
		}
		// 加入使用列表
		objList.Add(classObject);
	}
	protected void removeInuse(IClassObject classObject)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		Type type = Typeof(classObject);
		if (!mInusedList.TryGetValue(type, out List<IClassObject> classList))
		{
			logError("can not find class type in Inused List! type : " + type);
		}
		if (!classList.Remove(classObject))
		{
			logError("Inused List not contains class object!");
		}
	}
	protected void addUnuse(IClassObject classObject)
	{
		// 加入未使用列表
		Type type = Typeof(classObject);
		if (!mUnusedList.TryGetValue(type, out Stack<IClassObject> objList))
		{
			objList = new Stack<IClassObject>();
			mUnusedList.Add(type, objList);
		}
		else
		{
			if (objList.Contains(classObject))
			{
				logError("ClassObject is in Unused list! can not add again!");
				return;
			}
		}
		objList.Push(classObject);
	}
}