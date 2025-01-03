using UnityEngine;
using System;
using System.Collections.Generic;
using static MathUtility;

// 沿指定点变化的组件基类,变化速度恒定
public abstract class ComponentCurve : ComponentKeyFrame
{
	protected List<KeyPoint> mKeyPointList = new();   // 移动开始时的位置
	public void setKeyList(List<Vector3> posList)
	{
		if (posList.isEmpty())
		{
			setActive(false);
			return;
		}
		// 计算整个曲线的长度
		generateDistanceList(posList, mKeyPointList);
	}
	public void setKeyList(Span<Vector3> posList)
	{
		if (posList.isEmptySpan())
		{
			setActive(false);
			return;
		}
		// 计算整个曲线的长度
		generateDistanceList(posList, mKeyPointList);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mKeyPointList.Clear();
	}
	public List<KeyPoint> getKeyPointList() { return mKeyPointList; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var uiObj = mComponentOwner as Transformable;
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curDisatnce = value * mKeyPointList[^1].mDistanceFromStart;
		int index = findPointIndex(mKeyPointList, curDisatnce, 0, mKeyPointList.Count - 1);
		KeyPoint curPoint = mKeyPointList[index];
		Vector3 pos;
		if (index < mKeyPointList.Count - 1)
		{
			KeyPoint nextPoint = mKeyPointList[index + 1];
			float percent = divide(curDisatnce - curPoint.mDistanceFromStart, nextPoint.mDistanceFromLast);
			pos = lerp(curPoint.mPosition, nextPoint.mPosition, percent);
		}
		else
		{
			pos = curPoint.mPosition;
		}
		uiObj.setPosition(pos);
	}
	protected abstract void setValue(Vector3 value);
}