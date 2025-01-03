using UnityEngine;
using System;
using static MathUtility;
using static BinaryUtility;

// 使用LineRenderer的方式进行画线的窗口,用于在界面中画线
public class myUILineRenderer : myUGUIObject
{
	public LineRenderer mLineRenderer;		// Unity的LineRenderer组件
	public override void init()
	{
		base.init();
		mLineRenderer = getOrAddUnityComponent<LineRenderer>();
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
				mLineRenderer.SetPosition(i + pointList.Length, pointList[^1]);
			}
		}
	}
	public void setPointListBezier(Vector3[] pointList, int bezierDetail = 10)
	{
		Span<Vector3> curveList = stackalloc Vector3[bezierDetail];
		getBezierPoints(pointList, curveList, mLineRenderer.loop, bezierDetail);
		Vector3[] pointArray = new Vector3[curveList.Length];
		memcpy(pointArray, curveList, 0, 0, curveList.Length);
		setPointList(pointArray);
	}
	public void setPointListSmooth(Vector3[] pointList, int bezierDetail = 10)
	{
		Span<Vector3> curveList = stackalloc Vector3[pointList.Length * bezierDetail];
		getCurvePoints(pointList, curveList, mLineRenderer.loop, bezierDetail);
		Vector3[] pointArray = new Vector3[curveList.Length];
		memcpy(pointArray, curveList, 0, 0, curveList.Length);
		setPointList(pointArray);
	}
}