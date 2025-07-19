﻿
// 自定义的下拉列表的项
public class DropItem : WindowRecycleableUGUI
{
	protected UGUIDropList mParentDropList;		// 自定义的下拉列表
	protected myUGUIObject mHover;				// 悬停窗口
#if USE_TMP
	protected myUGUITextTMP mLabel;				// 名字窗口
#else
	protected myUGUIText mLabel;				// 名字窗口
#endif
	protected int mCustomValue;					// 附带的自定义数据,一般都是枚举之类的
	public DropItem(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal()
	{
		base.assignWindowInternal();
		newObject(out mHover, "Hover", false);
		newObject(out mLabel, "Label");
	}
	public override void init()
	{
		base.init();
		mRoot.registeCollider(onClick);
		if (mHover != null)
		{
			mRoot.setHoverCallback(onHover);
		}
	}
	public override void reset()
	{
		base.reset();
		mHover?.setActive(false);
	}
	public string getText() { return mLabel.getText(); }
	public int getCustomValue() { return mCustomValue; }
	public void setText(string text) { mLabel.setText(text); }
	public void setCustomValue(int value) { mCustomValue = value; }
	public void setParent(UGUIDropList parent) { mParent = parent; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onClick()
	{
		mParentDropList.dropItemClick(this);
	}
	protected void onHover(bool hover)
	{
		mHover?.setActive(hover);
	}
}