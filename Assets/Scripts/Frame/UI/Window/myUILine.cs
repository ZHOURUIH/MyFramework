using UnityEngine;
using System.Collections.Generic;
using System;
using static FrameUtility;
using static MathUtility;

// 使用Mesh的方式进行画线的窗口
public class myUGUILine : myUGUIObject
{
	public UGUILine mUGUILine;		// 用于画线的对象
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
		using (new ListScope<Vector3>(out var curveList))
		{
			getCurvePoints(pointList, curveList, false, bezierDetail);
			setPointList(curveList);
		}
	}
}