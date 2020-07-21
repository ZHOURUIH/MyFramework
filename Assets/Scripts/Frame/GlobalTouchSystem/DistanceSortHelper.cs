using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct DistanceSortHelper
{
	public float mDistance;
	public IMouseEventCollect mObject;
	public DistanceSortHelper(float dis, IMouseEventCollect obj)
	{
		mDistance = dis;
		mObject = obj;
	}
	public static int distanceAscend(DistanceSortHelper a, DistanceSortHelper b)
	{
		return (int)MathUtility.sign(a.mDistance - b.mDistance);
	}
}