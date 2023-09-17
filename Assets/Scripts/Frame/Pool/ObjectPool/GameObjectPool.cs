using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;

// GameObject的池,用于缓存在代码中动态创建的GameObject
public class GameObjectPool : FrameSystem
{
	protected HashSet<GameObject> mInusedList;  // 已使用的列表,为了提高运行时效率,仅在编辑器下使用
	protected List<GameObject> mUnusedList;		// 未使用的列表
	protected int mCreatedCount;
	public GameObjectPool()
	{
		mInusedList = new HashSet<GameObject>();
		mUnusedList = new List<GameObject>();
		mCreateObject = true;
	}
	public void clearUnused()
	{
		int count = mUnusedList.Count;
		for (int i = 0; i < count; ++i)
		{
			Object.DestroyImmediate(mUnusedList[i]);
		}
		mUnusedList.Clear();
	}
	public HashSet<GameObject> getInusedList() { return mInusedList; }
	public List<GameObject> getUnusedList() { return mUnusedList; }
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
			go = mUnusedList[mUnusedList.Count - 1];
			mUnusedList.RemoveAt(mUnusedList.Count - 1);
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			go = new GameObject();
#if UNITY_EDITOR
			++mCreatedCount;
			if (mCreatedCount % 1000 == 0)
			{
				logForceNoLock("GameObject数量已经达到了" + mCreatedCount + "个, name:" + name);
			}
#endif
		}
		setNormalProperty(go, parent, name);
#if UNITY_EDITOR
		// 添加到已使用列表
		addInuse(go);
#endif
		go.SetActive(true);
		return go;
	}
	// 销毁一个GameObject,但是调用方需要确保此GameObject上已经没有任何的除Transform以外的组件,否则可能会出现复用时错误
	public void destroyObject(GameObject go)
	{
		setNormalProperty(go, mObject);
		// 加入未使用列表
		mUnusedList.Add(go);

#if UNITY_EDITOR
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		removeInuse(go);
#endif
		go.SetActive(false);
	}
	public void destroyClassReally(GameObject go)
	{
#if UNITY_EDITOR
		// 从已使用列表中移除
		if (mInusedList.Contains(go))
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			removeInuse(go);
		}
#endif
		// 从未使用列表中移除
		mUnusedList.Remove(go);
		destroyGameObject(go);
	}
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