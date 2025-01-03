using System;
using static MathUtility;

// 用于给物体按照距离排序用的
public class DistanceSortHelper : IEquatable<DistanceSortHelper>
{
	public static Comparison<DistanceSortHelper> mCompareAscend = distanceAscend;	// 避免GC的委托
	public IMouseEventCollect mObject;			// 物体
	public float mDistance;						// 距离
	public DistanceSortHelper(float dis, IMouseEventCollect obj)
	{
		mDistance = dis;
		mObject = obj;
	}
	public bool Equals(DistanceSortHelper value) { return value.mDistance == mDistance && value.mObject == mObject; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected static int distanceAscend(DistanceSortHelper a, DistanceSortHelper b)
	{
		return (int)sign(a.mDistance - b.mDistance);
	}
}