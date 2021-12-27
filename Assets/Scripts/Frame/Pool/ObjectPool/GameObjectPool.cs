using UnityEngine;
using System.Collections.Generic;

// GameObject的池,用于缓存在代码中动态创建的GameObject
public class GameObjectPool : FrameSystem
{
	protected HashSet<GameObject> mInusedList;	// 已使用的列表
	protected HashSet<GameObject> mUnusedList;	// 未使用的列表
	public GameObjectPool()
	{
		mInusedList = new HashSet<GameObject>();
		mUnusedList = new HashSet<GameObject>();
		mCreateObject = true;
	}
	public HashSet<GameObject> getInusedList() { return mInusedList; }
	public HashSet<GameObject> getUnusedList() { return mUnusedList; }
	public GameObject newObject()
	{
		return newObject(null, mObject);
	}
	public GameObject newObject(string name, GameObject parent)
	{
		GameObject go;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.Count > 0)
		{
			go = popFirstElement(mUnusedList);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			go = new GameObject();
		}
		setNormalProperty(go, parent, name);
		// 添加到已使用列表
		addInuse(go);
		go.SetActive(true);
		return go;
	}
	// 销毁一个GameObject,但是调用方需要确保此GameObject上已经没有任何的除Transform以外的组件,否则可能会出现复用时错误
	public void destroyObject(GameObject go)
	{
		setNormalProperty(go, mObject);
		// 加入未使用列表
		if (!mUnusedList.Add(go))
		{
			logError("GameObject已经在未使用列表中,无法再次销毁! Name: " + go.name);
		}
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		removeInuse(go);
		go.SetActive(false);
	}
	public void destroyClassReally(GameObject go)
	{
		// 从已使用列表中移除
		if (isInuse(go))
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			removeInuse(go);
		}
		// 从未使用列表中移除
		else
		{
			mUnusedList.Remove(go);
		}
		destroyGameObject(go);
	}
	public bool isInuse(GameObject go) { return mInusedList.Contains(go); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(GameObject go)
	{
		if (!mInusedList.Add(go))
		{
			logError("加入已使用列表失败, Name: " + go.name);
		}
	}
	protected void removeInuse(GameObject go)
	{
		if (!mInusedList.Remove(go))
		{
			logError("已使用列表找不到指定对象, Name: " + go.name);
		}
	}
}