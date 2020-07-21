using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FrameComponent : ComponentOwner
{
	public GameObject mObject;
	public bool mDestroy;       // 是否已经销毁
	public bool mCreateObject;
	public int mInitOrder;
	public int mUpdateOrder;
	public int mDestroyOrder;
	public FrameComponent(string name)
		: base(name) { }
	public virtual void init()
	{
		if(mCreateObject)
		{
			mObject = createGameObject(GetType().ToString(), mGameFramework.getGameFrameObject());
		}
		mDestroy = false;
		initComponents();
	}
	public override void destroy()
	{
		destroyGameObject(mObject);
		mObject = null;
		mDestroy = true;
		base.destroy();
	}
	public GameObject getObject() { return mObject; }
	public virtual void onDrawGizmos() { }
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareInit(FrameComponent a, FrameComponent b)
	{
		return sign(a.mInitOrder - b.mInitOrder);
	}
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareUpdate(FrameComponent a, FrameComponent b)
	{
		return sign(a.mUpdateOrder - b.mUpdateOrder);
	}
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareDestroy(FrameComponent a, FrameComponent b)
	{
		return sign(a.mDestroyOrder - b.mDestroyOrder);
	}
}