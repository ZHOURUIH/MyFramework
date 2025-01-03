
// 自定义的下拉列表的项
public class DropItem : WindowRecycleableUGUI
{
	protected UGUIDropList mParent;		// 自定义的下拉列表
	protected myUGUIObject mHover;		// 悬停窗口
	protected myUGUIText mLabel;        // 名字窗口
	protected int mCustomValue;         // 附带的自定义数据,一般都是枚举之类的
	public DropItem(LayoutScript script)
		: base(script) { }
	public override void assignWindow(myUIObject parent, myUIObject template, string name)
	{
		base.assignWindow(parent, template, name);
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
		mParent.dropItemClick(this);
	}
	protected void onHover(bool hover)
	{
		mHover?.setActive(hover);
	}
}