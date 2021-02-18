using UnityEngine;
using System;

public class FrameSystem : ComponentOwner
{
	protected GameObject mObject;
	protected bool mDestroy;       // 是否已经销毁
	protected bool mCreateObject;
	protected int mInitOrder;
	protected int mUpdateOrder;
	protected int mDestroyOrder;
	public virtual void init()
	{
		if (mCreateObject)
		{
			mObject = createGameObject(Typeof(this).ToString(), mGameFramework.getGameFrameObject());
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
	public void setInitOrder(int order) { mInitOrder = order; }
	public void setUpdateOrder(int order) { mUpdateOrder = order; }
	public void setDestroyOrder(int order) { mDestroyOrder = order; }
	public void setCreateObject(bool create) { mCreateObject = create; }
	public GameObject getObject() { return mObject; }
	public bool isDestroy() { return mDestroy; }
	public virtual void onDrawGizmos() { }
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareInit(FrameSystem a, FrameSystem b)
	{
		return sign(a.mInitOrder - b.mInitOrder);
	}
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareUpdate(FrameSystem a, FrameSystem b)
	{
		return sign(a.mUpdateOrder - b.mUpdateOrder);
	}
	// a小于b返回-1, a等于b返回0, a大于b返回1,升序排序
	static public int compareDestroy(FrameSystem a, FrameSystem b)
	{
		return sign(a.mDestroyOrder - b.mDestroyOrder);
	}
}