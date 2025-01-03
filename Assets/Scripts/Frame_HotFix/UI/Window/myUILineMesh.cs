using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

// 使用Mesh的方式进行画线的窗口
public class myUILineMesh : myUGUIObject
{
	public UGUILineMesh mUGUILine = new();		// 用于画线的对象
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
	public void setPointList(Span<Vector3> pointList)
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
		Span<Vector3> curveList = stackalloc Vector3[pointList.Count * bezierDetail];
		getCurvePoints(pointList, curveList, false, bezierDetail);
		setPointList(curveList);
	}
}