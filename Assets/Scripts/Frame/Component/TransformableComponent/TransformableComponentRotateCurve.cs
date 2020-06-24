using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentRotateCurve : ComponentKeyFrameNormal, IComponentModifyRotation
{
	protected List<Vector3> mKeyRotList;   // 移动开始时的位置
	protected float mTotalLength;
	protected List<float> mPointToStartLengthList;
	public TransformableComponentRotateCurve()
	{
		mPointToStartLengthList = new List<float>();
		mKeyRotList = new List<Vector3>();
	}
	public void setKeyRotList(List<Vector3> rotList)
	{
		mKeyRotList.Clear();
		mKeyRotList.AddRange(rotList);
		if (mKeyRotList == null || mKeyRotList.Count == 0)
		{
			setActive(false);
			return;
		}
		mPointToStartLengthList.Clear();
		// 计算整个曲线的长度
		float totalLength = 0.0f;
		int count = mKeyRotList.Count;
		for (int i = 0; i < count; ++i)
		{
			if (i > 0)
			{
				float segmentLength = getLength(mKeyRotList[i] - mKeyRotList[i - 1]);
				totalLength += segmentLength;
				mPointToStartLengthList.Add(totalLength);
			}
			else
			{
				mPointToStartLengthList.Add(0.0f);
			}
		}
		mTotalLength = totalLength;
	}
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		Transformable uiObj = mComponentOwner as Transformable;
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curDisatnce = value * mTotalLength;
		int index = findPointIndex(mPointToStartLengthList, curDisatnce, 0, mPointToStartLengthList.Count - 1);
		if(index != mKeyRotList.Count - 1)
		{
			uiObj.setRotation(mKeyRotList[index] + normalize(mKeyRotList[index + 1] - mKeyRotList[index]) * (curDisatnce - mPointToStartLengthList[index]));
		}
		else
		{
			uiObj.setRotation(mKeyRotList[index]);
		}
	}
}