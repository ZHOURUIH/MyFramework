using System;

[CommonControl]
public class TabItem : WindowObjectUGUI
{
	protected myUGUIObject mNormal;
	protected myUGUIObject mSelected;
	protected myUGUIObject mNormalText;
	protected myUGUIObject mSelectedText;
	protected LegendButton mButton;
	protected Action mCallback;
	public TabItem(IWindowObjectOwner parent) : base(parent)
	{
		mButton = new(this);
	}
	protected override void assignWindowInternal()
	{
		mButton.assignWindow(mRoot);
		newObject(out mNormal, "Normal");
		newObject(out mSelected, "Selected");
		newObject(out mNormalText, "NormalText", false);
		newObject(out mSelectedText, "SelectedText", false);
	}
	public override void init()
	{
		base.init();
		mButton.registeCollider(onClick);
		setSelected(false);
	}
	public void setCallback(Action callback) { mCallback = callback; }
	public void setSelected(bool selected) 
	{
		mNormal.setActive(!selected);
		mNormalText?.setActive(!selected);
		mSelected.setActive(selected);
		mSelectedText?.setActive(selected);
	}
	public bool isSelected() { return mSelected.isActive(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onClick()
	{
		mCallback?.Invoke();
	}
}