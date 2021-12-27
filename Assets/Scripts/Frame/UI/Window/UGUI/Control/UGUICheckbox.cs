using System;
using System.Collections.Generic;
using UnityEngine;

// 自定义的勾选框
public class UGUICheckbox : WindowObjectUGUI
{
	protected myUGUIObject mMark;		// 勾选图片节点
	protected OnCheck mCheckCallback;	// 勾选状态改变的回调
	public void assignWindow(myUGUIObject parent, string rootName)
	{
		base.assignWindow(parent, rootName);
		mScript.newObject(out mMark, mRoot, "Mark");
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
	public void setOnCheck(OnCheck callback) { mCheckCallback = callback; }
	public void setChecked(bool check) { LT.ACTIVE(mMark, check); }
	public bool isChecked() { return mMark.isActive(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onCheckClick(IMouseEventCollect obj, Vector3 mousePos)
	{
		LT.ACTIVE(mMark, !mMark.isActive());
		mCheckCallback?.Invoke(mMark.isActive());
	}
}