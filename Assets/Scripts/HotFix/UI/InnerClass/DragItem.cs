// auto generate classname start
public class DragItem : DragViewItem<DragItem.Data>
// auto generate classname end
{
	public class Data : ClassObject
	{
		public string mText;
		public override void resetProperty()
		{
			base.resetProperty();
			mText = null;
		}
	}
	// auto generate member start
	protected myUGUIObject mIcon;
	protected myUGUITextTMP mName;
	// auto generate member end
	public DragItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out mIcon, "Icon");
		newObject(out mName, "Name");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
	}
	public override void setData(Data data)
	{
		mName.setText(data.mText);
	}
}
