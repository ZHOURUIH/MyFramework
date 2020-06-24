using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUILine : txNGUIShape
{
	protected NGUILine mNGUILine;
	public void setPointList(List<Vector3> pointList)
	{
		mNGUILine.setPointList(pointList);
		markChanged();
	}
	public void setPointList(Vector3[] pointList)
	{
		mNGUILine.setPointList(pointList);
		markChanged();
	}
	public NGUILine getNGUILine() { return mNGUILine; }
	public void setWidth(float width) { mNGUILine.setWidth(width); }
	public void setPointListBezier(List<Vector3> pointList, int bezierDetail = 10)
	{
		setPointList(getBezierPoints(pointList, false, bezierDetail));
	}
	public void setPointListBezier(Vector3[] pointList, int bezierDetail = 10)
	{
		setPointList(getBezierPoints(pointList, false, bezierDetail));
	}
	public void setPointListSmooth(List<Vector3> pointList, int bezierDetail = 10)
	{
		setPointList(getCurvePoints(pointList, false, bezierDetail));
	}
	public void setPointListSmooth(Vector3[] pointList, int bezierDetail = 10)
	{
		setPointList(getCurvePoints(pointList, false, bezierDetail));
	}
	//-----------------------------------------------------------------------------------------------------------------------
	protected override INGUIShape createShape()
	{
		return mNGUILine = new NGUILine();
	}
}

#endif