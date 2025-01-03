using System;
using UnityEngine;
using static MathUtility;

// 使用LineRenderer的方式进行画线,是对LineRenderer的封装,用于在3D场景中画线,不局限于界面中
public class myLineRenderer
{
	protected LineRenderer mLineRenderer;		// Unity的LineRenderer组件
	public void setLineRenderer(LineRenderer renderer)
	{
		mLineRenderer = renderer;
	}
	public void setPointList(Span<Vector3> pointList)
	{
		int count = pointList.Length;
		Vector3[] list = new Vector3[count];
		for (int i = 0; i < count; ++i)
		{
			list[i] = pointList[i];
		}
		setPointList(list);
	}
	public void setPointList(Vector3[] pointList)
	{
		if (pointList == null)
		{
			mLineRenderer.SetPositions(pointList);
			return;
		}
		if (pointList.Length > mLineRenderer.positionCount)
		{
			mLineRenderer.positionCount = pointList.Length;
		}
		mLineRenderer.SetPositions(pointList);
		if (pointList.Length > 0 && pointList.Length < mLineRenderer.positionCount)
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
		if (pointList == null)
		{
			return;
		}
		Span<Vector3> curveList = stackalloc Vector3[bezierDetail];
		getBezierPoints(pointList, curveList, mLineRenderer.loop, bezierDetail);
		setPointList(curveList);
	}
	public void setPointListSmooth(Vector3[] pointList, int bezierDetail = 10)
	{
		if (pointList == null)
		{
			return;
		}
		Span<Vector3> curveList = stackalloc Vector3[bezierDetail];
		getCurvePoints(pointList, curveList, mLineRenderer.loop, bezierDetail);
		setPointList(curveList);
	}
	public LineRenderer getRenderer() { return mLineRenderer; }
	public void setActive(bool active) { mLineRenderer.gameObject.SetActive(active); }
	public GameObject getGameObject() { return mLineRenderer.gameObject; }
}