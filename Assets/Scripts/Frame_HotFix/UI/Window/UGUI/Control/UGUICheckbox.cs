using static UnityUtility;

// 自定义的勾选框
public class UGUICheckbox : WindowObjectUGUI, ICommonUI
{
	protected myUGUIObject mMark;				// 勾选图片节点
	protected myUGUITextAuto mLabel;			// 文字节点
	protected CheckCallback mCheckCallback;		// 勾选状态改变的回调
	public UGUICheckbox(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal()
	{
		base.assignWindowInternal();
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
	public myUGUITextAuto getLabelObject() { return mLabel; }
	public void setLabel(string label) { mLabel?.setText(label); }
	public void setCheckCallback(CheckCallback callback) { mCheckCallback = callback; }
	public void setChecked(bool check) { mMark.setActive(check); }
	public bool isChecked() { return mMark.isActiveInHierarchy(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onCheckClick()
	{
		mMark.setActive(!mMark.isActiveInHierarchy());
		mCheckCallback?.Invoke(this);
	}
}