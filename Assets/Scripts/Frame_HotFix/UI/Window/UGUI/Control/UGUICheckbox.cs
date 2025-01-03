using static UnityUtility;

// 自定义的勾选框
public class UGUICheckbox : WindowObjectUGUI
{
	protected myUGUIObject mMark;       // 勾选图片节点
	protected myUGUIText mLabel;		// 文字节点
	protected OnCheck mCheckCallback;	// 勾选状态改变的回调
	public UGUICheckbox(LayoutScript script)
		:base(script){ }
	public void assignWindow(myUGUIObject parent, string rootName)
	{
		base.assignWindow(parent, rootName);
		newObject(out mMark, "Mark");
		newObject(out mLabel, "Label", false);
	}
	public override void init()
	{
		if (mMark == null)
		{
			logError("UGUICheckbox需要有一个名为Mark的节点");
		}
		mRoot.registeCollider(onCheckClick);
	}
	public myUGUIText getLabelObject() { return mLabel; }
	public void setLabel(string label) { mLabel?.setText(label); }
	public void setOnCheck(OnCheck callback) { mCheckCallback = callback; }
	public void setChecked(bool check) { mMark.setActive(check); }
	public bool isChecked() { return mMark.isActiveInHierarchy(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onCheckClick()
	{
		mMark.setActive(!mMark.isActiveInHierarchy());
		mCheckCallback?.Invoke(this, mMark.isActiveInHierarchy());
	}
}