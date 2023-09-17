using UnityEngine;
using System.Collections.Generic;

public class DropItem : PooledWindowUGUI
{
	protected UGUIDropList mParent;
	protected myUGUIObject mHover;
	protected myUGUIText mLabel;
	public override void assignWindow(myUIObject parent, myUIObject template, string name)
	{
		mScript.cloneObject(out mRoot, parent, template, name);
		mScript.newObject(out mHover, mRoot, "Hover", false);
		mScript.newObject(out mLabel, mRoot, "Label");
	}
	public override void init()
	{
		base.init();
		mScript.registeCollider(mRoot, onClick);
		if (mHover != null)
		{
			mRoot.setHoverCallback(onHover);
		}
	}
	public override void reset()
	{
		base.reset();
		mHover.setActive(false);
	}
	public string getText() { return mLabel.getText(); }
	public void setText(string text) { mLabel.setText(text); }
	public void setParent(UGUIDropList parent) { mParent = parent; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onClick(IMouseEventCollect obj, Vector3 mousePos)
	{
		mParent.dropItemClick(this);
	}
	protected void onHover(IMouseEventCollect obj, Vector3 mousePos, bool hover)
	{
		mHover.setActive(hover);
	}
}