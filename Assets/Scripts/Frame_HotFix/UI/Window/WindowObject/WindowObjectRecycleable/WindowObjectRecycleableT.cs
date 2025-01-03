using static FrameDefine;

// 所有可复用窗口对象的基类
public abstract class WindowObjectRecycleableT<T> : WindowObjectT<T>, IRecycleable where T : myUIObject, new()
{
	protected long mAssignID = -1;					// 唯一的分配ID
	protected bool mChangePositionAsInvisible;      // 是否使用移动位置来代替隐藏
	public WindowObjectRecycleableT(LayoutScript script) : base(script) {}
	// 在对象被回收时调用
	public virtual void recycle() { mAssignID = -1; }
	public void setAssignID(long assignID) { mAssignID = assignID; }
	public long getAssignID() { return mAssignID; }
	public override void setActive(bool active)
	{
		checkRoot();
		if (mChangePositionAsInvisible)
		{
			if (!active)
			{
				mRoot.setPosition(FAR_POSITION);
			}
		}
		else
		{
			mRoot.setActive(active);
		}
	}
}