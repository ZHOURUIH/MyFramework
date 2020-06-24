using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TransformableComponentRotatePath : ComponentPathNormal, IComponentModifyRotation
{
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curTime = value * mMaxLength;
		int index = findPointIndex(mTimeList, curTime, 0, mTimeList.Count - 1);
		if (index != mTimeList.Count - 1)
		{
			Vector3 startValue = mValueKeyFrame[mTimeList[index]];
			Vector3 endValue = mValueKeyFrame[mTimeList[index + 1]];
			perfectRotationDelta(ref startValue, ref endValue);
			float timePercentInSection = inverseLerp(mTimeList[index], mTimeList[index + 1], curTime);
			setValue(lerp(startValue, endValue, timePercentInSection) + mValueOffset);
		}
		else
		{
			setValue(mValueKeyFrame[mTimeList[index]] + mValueOffset);
		}
	}
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setRotation(value);
	}
}