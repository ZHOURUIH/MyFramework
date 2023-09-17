using UnityEngine;
using System;
using System.Collections.Generic;
using static MathUtility;

// 用于实现可以在面板上预览以及编辑所有的关键帧曲线
public class GameKeyframe : MonoBehaviour
{
	[HideInInspector]
	public List<CurveInfo> mCurveList;		// 关键帧曲线列表
	public AnimationCurve createKeyframe()
	{
		if (mCurveList == null)
		{
			mCurveList = new List<CurveInfo>();
		}
		Dictionary<int, CurveInfo> searchList = new Dictionary<int, CurveInfo>();
		// ID从101开始,100以内是内置曲线
		int key = 101;
		while(mCurveList.Find((CurveInfo info) => { return info.mID == key; }) != null)
		{
			++key;
		}
		AnimationCurve curve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 0.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 0.0f));
		mCurveList.Add(new CurveInfo(key, curve));
		mCurveList.Sort((CurveInfo x, CurveInfo y) => { return sign(x.mID - y.mID); });
		return curve;
	}
	public void destroyKeyframe(CurveInfo info)
	{
		mCurveList?.Remove(info);
	}
}