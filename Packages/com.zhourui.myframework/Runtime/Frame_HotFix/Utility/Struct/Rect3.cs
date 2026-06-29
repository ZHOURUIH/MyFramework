using UnityEngine;

// 3D空间中的平面矩形
public struct Rect3
{
	public Vector3 mCenter;		// 矩形中心
	public Vector3 mUp;			// 矩形的向上的方向
	public Vector3 mNormal;		// 矩形的法线
	public float mWidth;		// 宽度
	public float mHeight;		// 高度
	public Rect3(Vector3 center, Vector3 up, Vector3 normal, float width, float heigth)
	{
		mCenter = center;
		mUp = up;
		mNormal = normal;
		mWidth = width;
		mHeight = heigth;
	}
	public Rect toRect()
	{
		return new(new(mCenter.x - mWidth * 0.5f, mCenter.z - mHeight * 0.5f), new(mWidth, mHeight));
	}
}