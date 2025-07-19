using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 可用于在UI上画线条的窗口
public class myUGUICustomLine : myUGUIObject
{
	protected CustomLine mLine;		// 自定义的代替LineRenderer的组件
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
	public void setWidth(float width)
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
}