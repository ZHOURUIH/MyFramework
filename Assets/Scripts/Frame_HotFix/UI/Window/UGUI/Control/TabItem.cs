using System;

public class TabItem : WindowObjectUGUI, ICommonUI
{
	protected myUGUIObject mNormal;
	protected myUGUIObject mSelected;
	protected LegendButton mButton;
	public TabItem(IWindowObjectOwner parent) : base(parent)
	{
		mButton = new(this);
	}
	protected override void assignWindowInternal()
	{
		mButton.assignWindow(mRoot);
		newObject(out mNormal, "Normal");
		newObject(out mSelected, "Selected");
	}
	public override void init()
	{
		base.init();
		setSelected(false);
	}
	public void registeCollider(Action callback)
	{
		mButton.registeCollider(callback);
	}
	public void setSelected(bool selected) 
	{
		mNormal.setActive(!selected);
		mSelected.setActive(selected);
	}
}