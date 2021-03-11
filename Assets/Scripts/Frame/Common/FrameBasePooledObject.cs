using System;

public class FrameBasePooledObject : FrameBase, IClassObject
{
	public ulong mAssignID;		// 重新分配时的ID,每次分配都会设置一个新的唯一执行ID
	public bool mDestroy;		// 当前对象是否已经被回收
	public FrameBasePooledObject()
	{
		mDestroy = true;
	}
	public virtual void resetProperty() 
	{
		mDestroy = true;
		mAssignID = 0;
	}
	public void setDestroy(bool isDestroy) { mDestroy = isDestroy; }
	public bool isDestroy() { return mDestroy; }
	public void setAssignID(ulong assignID) { mAssignID = assignID; }
	public ulong getAssignID() { return mAssignID; }
}
