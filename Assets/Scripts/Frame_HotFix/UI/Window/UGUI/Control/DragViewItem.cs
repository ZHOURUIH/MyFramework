
public abstract class DragViewItem<DataType> : WindowRecyclableUGUI where DataType : ClassObject
{
	protected int mIndex;
	public DragViewItem(IWindowObjectOwner parent) : base(parent) { }
	// 在设置数据之前会执行一次清空数据,对之前的设置进行一次清除,避免数据遗留带来的错误
	public virtual void clearData() { }
	public abstract void setData(DataType data);
	public void setIndex(int index) { mIndex = index; }
	public int getIndex() { return mIndex; }
	public override void update() { base.update(); }
}