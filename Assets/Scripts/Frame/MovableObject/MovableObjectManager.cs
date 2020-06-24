using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovableObjectManager : FrameComponent
{
	protected Dictionary<int, MovableObject> mMovableObjectList;
	protected List<MovableObject> mMovableObjectOrderList;		// 保存物体顺序的列表,用于更新
	public MovableObjectManager(string name)
		:base(name)
	{
		mMovableObjectList = new Dictionary<int, MovableObject>();
		mMovableObjectOrderList = new List<MovableObject>();
		mCreateObject = true;
	}
	public override void destroy()
	{
		base.destroy();
	}
	public override void init()
	{
		base.init();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach(var item in mMovableObjectOrderList)
		{
			item.update(elapsedTime);
		}
	}
	public MovableObject createMovableObject(GameObject go, bool autoDestroyObject)
	{
		if(go == null)
		{
			return null;
		}
		string name = go.name;
		MovableObject obj = new MovableObject(name);
		obj.setObject(go);
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
		GameObject go = getGameObject(parent, name);
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