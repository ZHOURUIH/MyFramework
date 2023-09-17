using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;

// MovableObject的管理器,只用于管理MovableObject,其他的派生类则由其他的管理器管理
// 因为MovableObject的派生类比较多,一般都会派生出其他的子类用作不同的用途
public class MovableObjectManager : FrameSystem
{
	protected Dictionary<int, MovableObject> mMovableObjectList;   // 所有物体的列表,用于查询
	protected List<MovableObject> mMovableObjectOrderList;          // 保存物体顺序的列表,用于更新
	public MovableObjectManager()
	{
		mMovableObjectList = new Dictionary<int, MovableObject>();
		mMovableObjectOrderList = new List<MovableObject>();
		mCreateObject = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		int count = mMovableObjectOrderList.Count;
		for (int i = 0; i < count; ++i)
		{
			mMovableObjectOrderList[i].update(elapsedTime);
		}
	}
	public MovableObject createMovableObject(string name)
	{
		return createMovableObject(null, name, true);
	}
	public MovableObject createMovableObject(GameObject go)
	{
		return createMovableObject(go, go.name, false);
	}
	public MovableObject createMovableObject(GameObject parent, string name)
	{
		return createMovableObject(getGameObject(name, parent), name, false);
	}
	public void destroyObject(ref MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		obj.destroy();
		if (mMovableObjectList.Remove(obj.getObjectID()))
		{
			mMovableObjectOrderList.Remove(obj);
		}
		obj = null;
	}
	// 将物体移动到列表的尾部,更改更新顺序
	public void moveObjectIndexToEnd(MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (mMovableObjectList.ContainsKey(obj.getObjectID()))
		{
			mMovableObjectOrderList.Remove(obj);
			mMovableObjectOrderList.Add(obj);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected MovableObject createMovableObject(GameObject go, string name, bool autoManageObject)
	{
		if (!autoManageObject && go == null)
		{
			logError("在创建MovableObject时如果不指定一个GameObject,则应该设置为自动创建GameObject");
		}
		var obj = new MovableObject();
		obj.setName(name);
		obj.setAutoManageObject(autoManageObject);
		// 如果不自动管理节点,则需要设置一个节点,所以需要go和autoCreateObject至少一个是有效的
		if (!autoManageObject)
		{
			obj.setObject(go);
		}
		obj.init();
		mMovableObjectList.Add(obj.getObjectID(), obj);
		mMovableObjectOrderList.Add(obj);
		return obj;
	}
}