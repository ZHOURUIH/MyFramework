
// 所有可复用窗口对象的基类
// 可用于WindowStructPool和WindowStructPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
public abstract class WindowObjectRecyclableT<T> : WindowObjectT<T>, IRecyclable where T : myUGUIObject, new()
{
	protected long mAssignID = -1;					// 唯一的分配ID
	public WindowObjectRecyclableT(IWindowObjectOwner parent) : base(parent) {}
	// 在对象被回收时调用
	public virtual void recycle() { mAssignID = -1; }
	public void setAssignID(long assignID) { mAssignID = assignID; }
	public long getAssignID() { return mAssignID; }
	public override void setActive(bool active)
	{
		checkRoot();
		base.setActive(active);
	}
}

// 根节点是myUGUIObject类型
public abstract class WindowRecyclableUGUI : WindowObjectRecyclableT<myUGUIObject>
{
	public WindowRecyclableUGUI(IWindowObjectOwner parent) : base(parent) { }
}