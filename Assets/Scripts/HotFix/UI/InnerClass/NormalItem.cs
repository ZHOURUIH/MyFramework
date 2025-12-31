
// auto generate classname start
public class NormalItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject mIcon;
	protected myUGUITextTMP mName;
	// auto generate member end
	public NormalItem(IWindowObjectOwner parent) : base(parent){}
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
	public override void onShow()
	{
		base.onShow();
	}
	public void setData(string name)
	{
		mName.setText(name);
	}
}
