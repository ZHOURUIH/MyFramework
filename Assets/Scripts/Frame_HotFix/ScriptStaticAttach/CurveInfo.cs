using UnityEngine;
using System;

// 关键帧曲线的信息
[Serializable]
public class CurveInfo
{
	public int mID;					// 关键帧ID
	public string mName;			// 关键帧名字
	public AnimationCurve mCurve;	// 关键帧曲线
	public CurveInfo(int id, string name, AnimationCurve curve)
	{
		mID = id;
		mName = name;
		mCurve = curve;
	}
}