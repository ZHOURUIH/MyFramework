using System;

// 可使用对象池进行创建和销毁的对象
public class ClassObject : IEquatable<ClassObject>
{
	protected static long mObjectInstanceIDSeed;	// 对象实例ID的种子
	protected long mObjectInstanceID;               // 对象实例ID
	protected long mAssignID;						// 重新分配时的ID,每次分配都会设置一个新的唯一执行ID
	protected bool mDestroy;						// 当前对象是否已经被回收
	public ClassObject()
	{
		mDestroy = true;
		mObjectInstanceID = ++mObjectInstanceIDSeed;
	}
	public virtual void resetProperty()
	{
		mAssignID = 0;
		mDestroy = true;
		// mObjectInstanceID不重置
		// mObjectInstanceID
	}
	public void setDestroy(bool isDestroy)	{ mDestroy = isDestroy; }
	public void setAssignID(long assignID)	{ mAssignID = assignID; }
	public bool isDestroy()					{ return mDestroy; }
	public long getAssignID()				{ return mAssignID; }
	public long getObjectInstanceID()		{ return mObjectInstanceID; }
	public bool Equals(ClassObject obj)		{ return mObjectInstanceID == obj.mObjectInstanceID; }
}