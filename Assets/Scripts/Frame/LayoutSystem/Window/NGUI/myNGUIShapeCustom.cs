using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class myNGUIShapeCustom : myNGUIShape
{
	protected NGUICustomShape mCustomShape;
	public void setPolygonPoints(List<Vector2> polygonPoints, bool forceUpdate = false)
	{
		mCustomShape.setPolygonPoints(polygonPoints);
		markChanged(forceUpdate);
	}
	public void setPolygonPoints(List<Vector3> polygonPoints, bool forceUpdate = false)
	{
		mCustomShape.setPolygonPoints(polygonPoints);
		markChanged(forceUpdate);
	}
	public override void cloneFrom(myUIObject obj)
	{
		base.cloneFrom(obj);
		myNGUIShapeCustom ori = obj as myNGUIShapeCustom;
		if(ori != null)
		{
			setPolygonPoints(ori.mCustomShape.getPolygonPoints());
		}
	}
	public void setGenerateTriangle(onGenerateTriangle callback)
	{
		mCustomShape.mOnGenerateTriangle = callback;
	}
	//------------------------------------------------------------------------------------------------------
	protected override INGUIShape createShape()
	{
		return mCustomShape = new NGUICustomShape();
	}
}

#endif