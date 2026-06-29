using UnityEngine;

// 球体
public struct Circle3
{
	public Vector3 mCenter;	// 球体中心点
	public float mRadius;	// 半径
	public Circle3(Vector3 center, float radius)
	{
		mCenter = center;
		mRadius = radius;
	}
}