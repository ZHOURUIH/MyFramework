using UnityEngine;
using System.Collections.Generic;

public abstract class ComponentCurve : ComponentKeyFrameNormal
{
	protected List<KeyPoint> mKeyPointList;   // 移动开始时的位置
	public ComponentCurve()
	{
		mKeyPointList = new List<KeyPoint>();
	}
	public void setKeyList(List<Vector3> posList)
	{
		if (posList == null || posList.Count == 0)
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		var uiObj = mComponentOwner as Transformable;
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curDisatnce = value * mKeyPointList[mKeyPointList.Count - 1].mDistanceFromStart;
		int index = findPointIndex(mKeyPointList, curDisatnce, 0, mKeyPointList.Count - 1);
		Vector3 pos;
		if (index != mKeyPointList.Count - 1)
		{
			float percent = 0.0f;
			if (!isFloatZero(mKeyPointList[index + 1].mDistanceFromLast))
			{
				percent = (curDisatnce - mKeyPointList[index].mDistanceFromStart) / mKeyPointList[index + 1].mDistanceFromLast;
			}
			pos = lerp(mKeyPointList[index].mPosition, mKeyPointList[index + 1].mPosition, percent);
		}
		else
		{
			pos = mKeyPointList[index].mPosition;
		}
		uiObj.setPosition(pos);
	}
	protected abstract void setValue(Vector3 value);
}