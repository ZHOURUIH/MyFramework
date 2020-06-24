using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUIShapeCustom : txNGUIShape
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
	public override void cloneFrom(txUIObject obj)
	{
		base.cloneFrom(obj);
		txNGUIShapeCustom ori = obj as txNGUIShapeCustom;
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