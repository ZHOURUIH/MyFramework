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
	public void setPointListBezier(IList<Vector3> pointList, int bezierDetail = 10)
	{
		setPointList(getBezierPoints(pointList, false, bezierDetail));
	}
	public void setPointListSmooth(IList<Vector3> pointList, int bezierDetail = 10)
	{
		List<Vector3> curveList = newList(out curveList);
		getCurvePoints(pointList, curveList, false, bezierDetail);
		setPointList(curveList);
		destroyList(curveList);
	}
}