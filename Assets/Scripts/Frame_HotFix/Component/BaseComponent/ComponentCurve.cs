using UnityEngine;
using System;
using System.Collections.Generic;
using static MathUtility;

// 沿指定点变化的组件基类,变化速度恒定
public abstract class ComponentCurve : ComponentKeyFrame
{
	protected List<KeyPoint> mKeyPointList = new();		// 移动开始时的位置
	protected int mLastKeyIndex;						// 缓存的上一次查找的结果
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
		mLastKeyIndex = 0;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mKeyPointList.Clear();
		mLastKeyIndex = 0;
	}
	public List<KeyPoint> getKeyPointList() { return mKeyPointList; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var uiObj = mComponentOwner as ITransformable;
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curDistance = value * mKeyPointList[^1].mDistanceFromStart;
		mLastKeyIndex = findPointIndex(mKeyPointList, curDistance, mLastKeyIndex);
		KeyPoint curPoint = mKeyPointList[mLastKeyIndex];
		Vector3 pos;
		if (mLastKeyIndex < mKeyPointList.Count - 1)
		{
			KeyPoint nextPoint = mKeyPointList[mLastKeyIndex + 1];
			float percent = divide(curDistance - curPoint.mDistanceFromStart, nextPoint.mDistanceFromLast);
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