using System;

// 可使用对象池进行创建和销毁的对象
public class ClassObject : IEquatable<ClassObject>, IEventListener, IResetProperty
{
	protected static long mObjectInstanceIDSeed;	// 对象实例ID的种子
	protected long mObjectInstanceID;               // 对象实例ID
	protected long mAssignID;						// 重新分配时的ID,每次分配都会设置一个新的唯一执行ID
	protected bool mHasDestroy;						// 当前对象是否已经被回收
	protected bool mPendingDestroy;					// 当前对象是否正在回收中
	public ClassObject()
	{
		mHasDestroy = true;
		mObjectInstanceID = ++mObjectInstanceIDSeed;
	}
	public virtual void resetProperty()
	{
		mAssignID = 0;
		mHasDestroy = true;
		mPendingDestroy = false;
		// mObjectInstanceID不重置
		// mObjectInstanceID = 0
	}
	// 每次被分配出去时调用,无论是第一次创建还是从池中获取都会调用,与destroy形成完成的生命周期
	public virtual void onCreate() { }
	public virtual void destroy() { }
	public void setDestroy(bool isDestroy)			{ mHasDestroy = isDestroy; }
	public void setAssignID(long assignID)			{ mAssignID = assignID; }
	public void setPendingDestroy(bool pending)		{ mPendingDestroy = pending; }
	public bool isDestroy()							{ return mHasDestroy; }
	public long getAssignID()						{ return mAssignID; }
	public long getObjectInstanceID()				{ return mObjectInstanceID; }
	public bool Equals(ClassObject obj)				{ return mObjectInstanceID == obj.mObjectInstanceID; }
	public bool isPendingDestroy()					{ return mPendingDestroy; }
}