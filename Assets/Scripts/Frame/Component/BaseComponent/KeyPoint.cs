using UnityEngine;

public struct KeyPoint
{
	public Vector3 mPosition;
	public float mDistanceFromStart;    // 当前点到起点的距离
	public float mDistanceFromLast;     // 当前点到上一个点的距离
	public KeyPoint(Vector3 pos, float distanceFromStart, float distanceFromLast)
	{
		mPosition = pos;
		mDistanceFromStart = distanceFromStart;
		mDistanceFromLast = distanceFromLast;
	}
}