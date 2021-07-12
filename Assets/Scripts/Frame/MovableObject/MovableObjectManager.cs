using UnityEngine;
using System.Collections.Generic;

public class MovableObjectManager : FrameSystem
{
	protected Dictionary<uint, MovableObject> mMovableObjectList;
	protected List<MovableObject> mMovableObjectOrderList;			// 保存物体顺序的列表,用于更新
	public MovableObjectManager()
	{
		mMovableObjectList = new Dictionary<uint, MovableObject>();
		mMovableObjectOrderList = new List<MovableObject>();
		mCreateObject = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		int count = mMovableObjectOrderList.Count;
		for(int i = 0; i < count; ++i)
		{
			mMovableObjectOrderList[i].update(elapsedTime);
		}
	}
	public MovableObject createMovableObject(GameObject go, bool autoDestroyObject)
	{
		if(go == null)
		{
			return null;
		}
		MovableObject obj = new MovableObject();
		obj.setName(go.name);
		obj.setObject(go, true);
		obj.setDestroyObject(autoDestroyObject);
		obj.init();
		mMovableObjectList.Add(obj.getObjectID(), obj);
		mMovableObjectOrderList.Add(obj);
		return obj;
	}
	public MovableObject createMovableObject(string name, bool autoDestroyObject)
	{
		GameObject go = createGameObject(name, mObject);
		return createMovableObject(go, autoDestroyObject);
	}
	public MovableObject createMovableObject(GameObject parent, string name, bool autoDestroyObject)
	{
		GameObject go = getGameObject(name, parent);
		return createMovableObject(go, autoDestroyObject);
	}
	public void destroyObject(ref MovableObject obj)
	{
		if(obj == null)
		{
			return;
		}
		obj.destroy();
		if (mMovableObjectList.ContainsKey(obj.getObjectID()))
		{
			mMovableObjectList.Remove(obj.getObjectID());
			mMovableObjectOrderList.Remove(obj);
		}
		obj = null;
	}
	// 将物体移动到列表的尾部,更改更新顺序
	public void moveObjectIndexToEnd(MovableObject obj)
	{
		if(obj == null)
		{
			return;
		}
		if (mMovableObjectList.ContainsKey(obj.getObjectID()))
		{
			mMovableObjectOrderList.Remove(obj);
			mMovableObjectOrderList.Add(obj);
		}
	}
}