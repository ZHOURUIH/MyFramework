
public abstract class DragViewItem<DataType> : WindowRecycleableUGUI where DataType : ClassObject
{
	protected int mIndex;
	public DragViewItem(IWindowObjectOwner parent) : base(parent) { }
	public abstract void setData(DataType data);
	public void setIndex(int index) { mIndex = index; }
	public int getIndex() { return mIndex; }
	public override void update() { base.update(); }
}