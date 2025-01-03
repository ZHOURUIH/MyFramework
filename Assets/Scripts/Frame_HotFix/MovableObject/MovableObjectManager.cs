using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using System;

// MovableObject的管理器,只用于管理MovableObject,其他的派生类则由其他的管理器管理
// 因为MovableObject的派生类比较多,一般都会派生出其他的子类用作不同的用途
public class MovableObjectManager : FrameSystem
{
	protected Dictionary<int, MovableObject> mMovableObjectList = new();	// 所有物体的列表,用于查询
	protected List<MovableObject> mMovableObjectOrderList = new();          // 保存物体顺序的列表,用于更新
	public MovableObjectManager()
	{
		mCreateObject = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (MovableObject item in mMovableObjectOrderList)
		{
			item.update(item.isIgnoreTimeScale() ? Time.unscaledDeltaTime : elapsedTime);
		}
	}
	public MovableObject createMovableObject(string name)
	{
		return createMovableObjectInternal(typeof(MovableObject), null, name);
	}
	public MovableObject createMovableObject(GameObject go)
	{
		if (go == null)
		{
			logError("物体为空,无法创建MovableObject");
			return null;
		}
		return createMovableObjectInternal(typeof(MovableObject), go, go.name);
	}
	public MovableObject createMovableObject(GameObject parent, string name)
	{
		return createMovableObjectInternal(typeof(MovableObject), getGameObject(name, parent), name);
	}
	public T createMovableObject<T>(GameObject parent, string name) where T : MovableObject
	{
		return createMovableObjectInternal(typeof(T), getGameObject(name, parent), name) as T;
	}
	public void destroyObject(MovableObject obj)
	{
		destroyObject(ref obj);
	}
	public void destroyObject(ref MovableObject obj)
	{
		if (obj == null)
		{
			return;
		}
		if (mMovableObjectList.Remove(obj.getObjectID()))
		{
			mMovableObjectOrderList.Remove(obj);
		}
		UN_CLASS(ref obj);
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
	protected MovableObject createMovableObjectInternal(Type type, GameObject go, string name)
	{
		var obj = CLASS(type) as MovableObject;
		obj.setName(name);
		obj.setObject(go);
		obj.init();
		mMovableObjectList.Add(obj.getObjectID(), obj);
		return mMovableObjectOrderList.add(obj);
	}
}