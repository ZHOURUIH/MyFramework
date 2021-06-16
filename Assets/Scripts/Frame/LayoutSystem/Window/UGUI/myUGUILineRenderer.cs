using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class myUGUILineRenderer : myUGUIObject
{
	public LineRenderer mLineRenderer;
	public override void init()
	{
		base.init();
		mLineRenderer = getUnityComponent<LineRenderer>();
	}
	public void setPointList(Vector3[] pointList)
	{
		if (pointList.Length > mLineRenderer.positionCount)
		{
			mLineRenderer.positionCount = pointList.Length;
		}
		mLineRenderer.SetPositions(pointList);
		if (pointList.Length < mLineRenderer.positionCount)
		{
			// 将未使用的点坐标设置为最后一个点
			int unuseCount = mLineRenderer.positionCount - pointList.Length;
			for (int i = 0; i < unuseCount; ++i)
			{
				mLineRenderer.SetPosition(i + pointList.Length, pointList[pointList.Length - 1]);
			}
		}
	}
	public void setPointListBezier(Vector3[] pointList, int bezierDetail = 10)
	{
		LIST(out List<Vector3> curveList);
		getBezierPoints(pointList, curveList, mLineRenderer.loop, bezierDetail);
		Vector3[] pointArray = new Vector3[curveList.Count];
		int count = curveList.Count;
		for (int i = 0; i < count; ++i)
		{
			pointArray[i] = curveList[i];
		}
		setPointList(pointArray);
		UN_LIST(curveList);
	}
	public void setPointListSmooth(Vector3[] pointList, int bezierDetail = 10)
	{
		LIST(out List<Vector3> curveList);
		getCurvePoints(pointList, curveList, mLineRenderer.loop, bezierDetail);
		Vector3[] pointArray = new Vector3[curveList.Count];
		int count = curveList.Count;
		for(int i = 0; i < count; ++i)
		{
			pointArray[i] = curveList[i];
		}
		setPointList(pointArray);
		UN_LIST(curveList);
	}
}