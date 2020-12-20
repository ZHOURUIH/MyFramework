using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class CurveInfo
{
	public int mID;
	public AnimationCurve mCurve;
	public CurveInfo(int id, AnimationCurve curve)
	{
		mID = id;
		mCurve = curve;
	}
}

public class GameKeyframe : MonoBehaviour
{
	[HideInInspector]
	public List<CurveInfo> mCurveList;
	public AnimationCurve CreateKeyframe()
	{
		if (mCurveList == null)
		{
			mCurveList = new List<CurveInfo>();
		}
		Dictionary<int, CurveInfo> searchList = new Dictionary<int, CurveInfo>();
		int key = 1;
		while(mCurveList.Find((CurveInfo info) => { return info.mID == key; }) != null)
		{
			++key;
		}
		AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
		mCurveList.Add(new CurveInfo(key, curve));
		mCurveList.Sort((CurveInfo x, CurveInfo y)=> { return MathUtility.sign(x.mID - y.mID); });
		return curve;
	}
	public void DestroyKeyframe(CurveInfo info)
	{
		mCurveList?.Remove(info);
	}
}