using System;
using UnityEngine;
using static UnityUtility;

// 自定义的勾选框
public class UGUICheckbox : WindowObjectUGUI
{
	protected myUGUIObject mMark;       // 勾选图片节点
	protected myUGUIText mLabel;		// 文字节点
	protected OnCheck mCheckCallback;	// 勾选状态改变的回调
	public void assignWindow(myUGUIObject parent, string rootName)
	{
		base.assignWindow(parent, rootName);
		mScript.newObject(out mMark, mRoot, "Mark");
		mScript.newObject(out mLabel, mRoot, "Label", false);
	}
	public override void init()
	{
		if (mMark == null)
		{
			logError("UGUICheckbox需要有一个名为Mark的节点");
		}
		mScript.registeCollider(mRoot, onCheckClick);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mScript = null;
		mRoot = null;
		mMark = null;
		mCheckCallback = null;
	}
	public myUGUIText getLabelObject() { return mLabel; }
	public void setLabel(string label) { mLabel?.setText(label); }
	public void setOnCheck(OnCheck callback) { mCheckCallback = callback; }
	public void setChecked(bool check) { mMark.setActive(check); }
	public bool isChecked() { return mMark.isActive(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onCheckClick(IMouseEventCollect obj, Vector3 mousePos)
	{
		mMark.setActive(!mMark.isActive());
		mCheckCallback?.Invoke(this, mMark.isActive());
	}
}