using System.Collections.Generic;
using System;

// 不支持带参构造的类,因为在再次利用时参数无法正确传递
public class ClassPoolSingle : FrameBase
{
	protected Type mType;
	protected HashSet<IClassObject> mInusedList;
	protected HashSet<IClassObject> mUnusedList;
	protected ThreadLock mListLock;
	public ClassPoolSingle()
	{
		mInusedList = new HashSet<IClassObject>();
		mUnusedList = new HashSet<IClassObject>();
		mListLock = new ThreadLock();
	}
	public void setType(Type type) { mType = type; }
	public Type getType() { return mType; }
	// 返回值表示是否是new出来的对象,false则为从回收列表中重复使用的对象
	public IClassObject newClass(out bool isNewObject)
	{
		mListLock.waitForUnlock();
		isNewObject = false;
		IClassObject obj = null;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.Count > 0)
		{
			foreach(var item in mUnusedList)
			{
				obj = item;
				break;
			}
			mUnusedList.Remove(obj);
			isNewObject = false;
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			obj = createInstance<IClassObject>(mType);
			isNewObject = true;
		}
		// 添加到已使用列表
		mInusedList.Add(obj);
		mListLock.unlock();
		return obj;
	}
	public new void destroyClass(IClassObject classObject)
	{
		mListLock.waitForUnlock();
		classObject.resetProperty();
		// 加入未使用列表
		if (!mUnusedList.Add(classObject))
		{
			logError("对象已经在未使用列表中,无法再次销毁! Type: " + mType);
		}
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (!mInusedList.Remove(classObject))
		{
			logError("从未使用列表中移除失败! Type: " + mType);
		}
		mListLock.unlock();
	}
	public void destroyClassReally(IClassObject classObject)
	{
		bool inuse = isInuse(classObject);
		mListLock.waitForUnlock();
		classObject.resetProperty();
		// 从已使用列表中移除
		if (inuse)
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			if (!mInusedList.Remove(classObject))
			{
				logError("在未使用列表中找不到对象! Type: " + mType);
			}
		}
		// 从未使用列表中移除
		else
		{
			mUnusedList.Remove(classObject);
		}
		mListLock.unlock();
	}
	public bool isInuse(IClassObject classObject)
	{
		mListLock.waitForUnlock();
		bool inuse = mInusedList.Contains(classObject);
		mListLock.unlock();
		return inuse;
	}
}