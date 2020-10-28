using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class myNGUIShapeRectangle : myNGUIShape
{
	protected NGUIRectangle mRectangle;
	public void setSize(Vector2 size)
	{
		mRectangle.setSize(size);
		markChanged();
	}
	public override void cloneFrom(myUIObject obj)
	{
		base.cloneFrom(obj);
		myNGUIShapeRectangle ori = obj as myNGUIShapeRectangle;
		if(ori != null)
		{
			setSize(ori.mRectangle.mSize);
		}
	}
	//------------------------------------------------------------------------------------------------------
	protected override INGUIShape createShape()
	{
		return mRectangle = new NGUIRectangle();
	}
}

#endif