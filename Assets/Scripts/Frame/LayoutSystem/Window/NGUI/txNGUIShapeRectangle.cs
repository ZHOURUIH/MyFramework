using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUIShapeRectangle : txNGUIShape
{
	protected NGUIRectangle mRectangle;
	public void setSize(Vector2 size)
	{
		mRectangle.setSize(size);
		markChanged();
	}
	public override void cloneFrom(txUIObject obj)
	{
		base.cloneFrom(obj);
		txNGUIShapeRectangle ori = obj as txNGUIShapeRectangle;
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