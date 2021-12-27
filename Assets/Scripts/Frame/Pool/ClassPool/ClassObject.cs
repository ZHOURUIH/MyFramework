using System;

// 可使用对象池进行创建和销毁的对象
// 继承FrameUtility是为了在调用工具函数时方便,把一些完全独立的工具函数类串起来继承
// 所有继承自ClassObject的类都可以直接访问工具类中的函数
public class ClassObject : FrameUtility
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
	public virtual void setDestroy(bool isDestroy) { mDestroy = isDestroy; }
	public virtual bool isDestroy() { return mDestroy; }
	public virtual void setAssignID(long assignID) { mAssignID = assignID; }
	public virtual long getAssignID() { return mAssignID; }
	public override int GetHashCode() { return (int)mObjectInstanceID; }
}