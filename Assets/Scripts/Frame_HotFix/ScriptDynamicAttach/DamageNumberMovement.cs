using UnityEngine;
using static MathUtility;

public class DamageNumberMovement
{
	public static void updateDamage(DamageNumberData data)
	{
		// 根据当前的时间找出位于哪两个点之间
		float curTime = data.mCurTime;

		// 计算位置
		float[] posTimeList = data.mPositionTimeList;
		Vector3[] posList = data.mPositionList;
		int index0 = findValueIndex(posTimeList, curTime, data.mLastPositionKeyIndex);
		data.mLastPositionKeyIndex = index0;
		Vector3 startValue0 = posList[index0];
		if (index0 < data.mPositionKeyFrames.Count - 1)
		{
			startValue0 = lerpSimple(startValue0, posList[index0 + 1], inverseLerp(posTimeList[index0], posTimeList[index0 + 1], curTime));
		}
		data.mPosition = startValue0 + data.mPositionOffset;

		// 计算缩放
		float[] scaleTimeList = data.mScaleTimeList;
		Vector3[] scaleList = data.mScaleList;
		int index1 = findValueIndex(scaleTimeList, curTime, data.mLastScaleKeyIndex);
		data.mLastScaleKeyIndex = index1;
		Vector3 startValue1 = scaleList[index1];
		if (index1 < data.mScaleKeyFrames.Count - 1)
		{
			startValue1 = lerpSimple(startValue1, scaleList[index1 + 1], inverseLerp(scaleTimeList[index1], scaleTimeList[index1 + 1], curTime));
		}
		data.mScale = multiVector3(startValue1, data.mScaleOffset);
	}
	protected static int findValueIndex(float[] timeList, float curTime, int startIndex)
	{
		if (timeList[startIndex] > curTime)
		{
			return clampMin(startIndex - 1);
		}
		int count = timeList.Length;
		for (int i = startIndex + 1; i < count; ++i)
		{
			if (timeList[i] > curTime)
			{
				return i - 1;
			}
		}
		return count - 1;
	}
}