using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameDefine;
using static FrameBaseUtility;

// GameObject的池,用于缓存在代码中动态创建的GameObject
public class GameObjectPool : FrameSystem
{
	protected HashSet<GameObject> mInusedList = new();  // 已使用的列表,为了提高运行时效率,仅在编辑器下使用
	protected Queue<GameObject> mUnusedList = new();	// 未使用的列表
	protected int mCreatedCount;						// 累计创建的对象数量
	public GameObjectPool()
	{
		mCreateObject = true;
	}
	public void clearUnused()
	{
		foreach (GameObject go in mUnusedList)
		{
			Object.DestroyImmediate(go);
		}
		mUnusedList.Clear();
	}
	public HashSet<GameObject> getInusedList() { return mInusedList; }
	public Queue<GameObject> getUnusedList() { return mUnusedList; }
	public GameObject newObject() { return newObject(null, mObject); }
	public GameObject newObject(string name){ return newObject(name, mObject); }
	public GameObject newObject(string name, GameObject parent)
	{
		bool isNew = false;
		GameObject go;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.Count > 0)
		{
			go = mUnusedList.Dequeue();
		}
		// 未使用列表中没有,创建一个新的
		else
		{
			go = new();
			isNew = true;
			++mCreatedCount;
		}
		setNormalProperty(go, parent, name);
		if (isEditor())
		{
			// 添加到已使用列表
			addInuse(go);
			if (isNew && mCreatedCount % 1000 == 0)
			{
				logNoLock("GameObject数量已经达到了" + mCreatedCount + "个, last name:" + name);
			}
		}
		if (!go.activeSelf)
		{
			go.SetActive(true);
		}
		return go;
	}
	// 销毁一个GameObject,但是调用方需要确保此GameObject上已经没有任何的除Transform以外的组件,否则可能会出现复用时错误
	public void destroyObject(GameObject go, bool moveToHide)
	{
		if (go == null)
		{
			return;
		}
		setNormalProperty(go, mObject);
		// 加入未使用列表
		mUnusedList.Enqueue(go);

		if (isEditor())
		{
			// 从使用列表移除,要确保操作的都是从本类创建的实例
			removeInuse(go);
		}
		if (moveToHide)
		{
			go.transform.localPosition = FAR_POSITION;
		}
		else
		{
			go.SetActive(false);
		}
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