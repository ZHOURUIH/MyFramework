using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentMoveCurve : ComponentKeyFrameNormal, IComponentModifyPosition
{
	protected List<Vector3> mKeyPosList;   // 移动开始时的位置
	protected List<float> mPointToStartLengthList;
	protected float mTotalLength;
	public TransformableComponentMoveCurve()
	{
		mPointToStartLengthList = new List<float>();
		mKeyPosList = new List<Vector3>();
	}
	public void setKeyPosList(List<Vector3> posList)
	{
		mKeyPosList.Clear();
		mKeyPosList.AddRange(posList);
		if (mKeyPosList == null || mKeyPosList.Count == 0)
		{
			setActive(false);
			return;
		}
		mPointToStartLengthList.Clear();
		// 计算整个曲线的长度
		float totalLength = 0.0f;
		int count = mKeyPosList.Count;
		for (int i = 0; i < count; ++i)
		{
			if (i > 0)
			{
				float segmentLength = getLength(mKeyPosList[i] - mKeyPosList[i - 1]);
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
		if(index != mKeyPosList.Count - 1)
		{
			uiObj.setPosition(mKeyPosList[index] + normalize(mKeyPosList[index + 1] - mKeyPosList[index]) * (curDisatnce - mPointToStartLengthList[index]));
		}
		else
		{
			uiObj.setPosition(mKeyPosList[index]);
		}
	}
}