using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 可用于在UI上画线条的窗口
public class myUGUICustomLine : myUGUIObject
{
	protected CustomLine mLine;     // 自定义的代替LineRenderer的组件
	protected bool mLoop;
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mLine))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个CustomLine组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mLine = mObject.AddComponent<CustomLine>();
			// 添加UGUI组件后需要重新获取RectTransform,这里由于是自定义组件,不一定需要
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public void setLineWidth(float width)
	{
		mLine.setWidth(width);
	}
	public void setPointList(List<Vector3> list)
	{
		mLine.setPointList(list);
	}
	public void setPointList(Span<Vector3> list)
	{
		mLine.setPointList(list);
	}
	public void setPointList(Vector3[] list)
	{
		mLine.setPointList(list);
	}
	public void setLoop(bool loop) { mLoop = loop; }
	public bool isLoop() { return mLoop; }
	public void setPointListBezier(Vector3[] pointList, int bezierDetail = 10)
	{
		Span<Vector3> curveList = stackalloc Vector3[bezierDetail];
		getBezierPoints(pointList, curveList, mLoop, bezierDetail);
		Vector3[] pointArray = new Vector3[curveList.Length];
		setPointList(pointArray.setRange(curveList));
	}
	public void setPointListSmooth(Vector3[] pointList, int bezierDetail = 10)
	{
		Span<Vector3> curveList = stackalloc Vector3[pointList.Length * bezierDetail];
		getCurvePoints(pointList, curveList, mLoop, bezierDetail);
		Vector3[] pointArray = new Vector3[curveList.Length];
		setPointList(pointArray.setRange(curveList));
	}
}