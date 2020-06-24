using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUIShapeTriangle : txNGUIShape
{
	protected NGUITriangle mTriangle;
	public void setPoints(Vector3 point0, Vector3 point1, Vector3 point2)
	{
		mTriangle.setPoints(point0, point1, point2);
		markChanged();
	}
	public override void cloneFrom(txUIObject obj)
	{
		base.cloneFrom(obj);
		txNGUIShapeTriangle ori = obj as txNGUIShapeTriangle;
		if(ori != null)
		{
			setPoints(ori.mTriangle.mTrianglePoints[0], ori.mTriangle.mTrianglePoints[1], ori.mTriangle.mTrianglePoints[2]);
		}
	}
	public override Vector3 getShapeCenter()
	{
		Vector2 pointCenter = (mTriangle.mTrianglePoints[0] + mTriangle.mTrianglePoints[1] + mTriangle.mTrianglePoints[2]) * 0.3333f;
		return getPosition() + new Vector3(pointCenter.x, pointCenter.y, 0.0f);
	}
	//----------------------------------------------------------------------------------------------------------------
	protected override INGUIShape createShape()
	{
		return mTriangle = new NGUITriangle();
	}
}

#endif