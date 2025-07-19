using static UnityUtility;

// 自定义的勾选框
public class UGUICheckbox : WindowObjectUGUI
{
	protected myUGUIObject mMark;       // 勾选图片节点
#if USE_TMP
	protected myUGUITextTMP mLabel;     // 文字节点
#else
	protected myUGUIText mLabel;        // 文字节点
#endif
	protected OnCheck mCheckCallback;	// 勾选状态改变的回调
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
#if USE_TMP
	public myUGUITextTMP getLabelObject() { return mLabel; }
#else
	public myUGUIText getLabelObject() { return mLabel; }
#endif
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