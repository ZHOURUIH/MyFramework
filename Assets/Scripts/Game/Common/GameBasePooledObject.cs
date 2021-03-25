using System;

public class GameBasePooledObject : GameBase, IClassObject
{
	protected ulong mAssignID;	// 重新分配时的ID,每次分配都会设置一个新的唯一执行ID
	protected bool mDestroy;	// 当前对象是否已经被回收
	public GameBasePooledObject()
	{
		mDestroy = true;
	}
	public virtual void resetProperty() 
	{
		mAssignID = 0;
		mDestroy = true;
	}
	public virtual void setDestroy(bool isDestroy) { mDestroy = isDestroy; }
	public virtual bool isDestroy() { return mDestroy; }
	public virtual void setAssignID(ulong assignID) { mAssignID = assignID; }
	public virtual ulong getAssignID() { return mAssignID; }
}
