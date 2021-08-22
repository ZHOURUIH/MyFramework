using System;
using System.Collections.Generic;
using UnityEngine;

public class UGUICheck : ComponentOwner
{
	protected LayoutScript mScript;
	protected myUGUIObject mRoot;
	protected myUGUIObject mCheck;
	protected OnCheck mCheckCallback;
	public void setScript(LayoutScript script)
	{
		mScript = script;
	}
	public void assignWindow(myUGUIObject parent, string rootName, string markName)
	{
		mScript.newObject(out mRoot, parent, rootName);
		mScript.newObject(out mCheck, mRoot, markName);
	}
	public void init(OnCheck callback)
	{
		mName = "UGUICheck";
		setOnCheck(callback);
		mScript.registeCollider(mRoot, onCheckClick);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mScript = null;
		mRoot = null;
		mCheck = null;
		mCheckCallback = null;
	}
	public void setOnCheck(OnCheck callback) { mCheckCallback = callback; }
	public void setChecked(bool check) { LT.ACTIVE(mCheck, check); }
	public bool isChecked() { return mCheck.isActive(); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onCheckClick(IMouseEventCollect obj)
	{
		LT.ACTIVE(mCheck, !mCheck.isActive());
		mCheckCallback?.Invoke(mCheck.isActive());
	}
}