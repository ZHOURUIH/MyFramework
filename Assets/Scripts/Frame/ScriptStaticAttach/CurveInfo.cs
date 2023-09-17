using UnityEngine;
using System;
using System.Collections.Generic;

// 关键帧曲线的信息
[Serializable]
public class CurveInfo
{
	public int mID;					// 关键帧ID
	public AnimationCurve mCurve;	// 关键帧曲线
	public CurveInfo(int id, AnimationCurve curve)
	{
		mID = id;
		mCurve = curve;
	}
}