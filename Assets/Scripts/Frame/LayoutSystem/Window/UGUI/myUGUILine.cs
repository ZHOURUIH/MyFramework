using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class myUGUILine : myUGUIObject
{
	public UGUILine mUGUILine;
	public myUGUILine()
	{
		mUGUILine = new UGUILine();
	}
	public override void init()
	{
		base.init();
		mUGUILine.init(mObject);
	}
	public override void destroy()
	{
		mUGUILine.destroy();
		base.destroy();
	}
	public void setPointList(List<Vector3> pointList)
	{
		mUGUILine.setPointList(pointList);
	}
	public void setPointList(Vector3[] pointList)
	{
		mUGUILine.setPointList(pointList);
	}
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
}