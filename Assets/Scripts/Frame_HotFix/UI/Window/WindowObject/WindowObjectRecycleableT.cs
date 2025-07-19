
// 所有可复用窗口对象的基类
// 可用于WindowStructPool和WindowStructPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
public abstract class WindowObjectRecycleableT<T> : WindowObjectT<T>, IRecycleable where T : myUIObject, new()
{
	protected long mAssignID = -1;					// 唯一的分配ID
	public WindowObjectRecycleableT(IWindowObjectOwner parent) : base(parent) {}
	// 在对象被回收时调用
	public virtual void recycle() { }
	public void setAssignID(long assignID) { mAssignID = assignID; }
	public long getAssignID() { return mAssignID; }
	public override void setActive(bool active)
	{
		checkRoot();
		base.setActive(active);
	}
}

// 根节点是myUIObject类型
public abstract class WindowRecycleableUI : WindowObjectRecycleableT<myUIObject>
{
	public WindowRecycleableUI(IWindowObjectOwner parent) : base(parent) { }
}

// 根节点是myUGUIObject类型
public abstract class WindowRecycleableUGUI : WindowObjectRecycleableT<myUGUIObject>
{
	public WindowRecycleableUGUI(IWindowObjectOwner parent) : base(parent) { }
}