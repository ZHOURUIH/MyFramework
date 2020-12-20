using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DistanceSortHelper : IEquatable<DistanceSortHelper>
{
	public float mDistance;
	public IMouseEventCollect mObject;
	public static Comparison<DistanceSortHelper> mCompareAscend = distanceAscend;
	public DistanceSortHelper(float dis, IMouseEventCollect obj)
	{
		mDistance = dis;
		mObject = obj;
	}
	public bool Equals(DistanceSortHelper value) { return value.mDistance == mDistance && value.mObject == mObject; }
	protected static int distanceAscend(DistanceSortHelper a, DistanceSortHelper b)
	{
		return (int)MathUtility.sign(a.mDistance - b.mDistance);
	}
}