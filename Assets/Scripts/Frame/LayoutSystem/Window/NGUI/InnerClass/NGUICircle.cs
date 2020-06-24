using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class NGUICircle : GameBase, INGUIShape
{
	public const int DEFAULT_DETAIL = 40;
	public float mRadius = 1.0f;
	public int mDetails = DEFAULT_DETAIL;
	public List<Vector2> mCirclePoints;
	public List<Vector3> mVertices;
	public List<Color> mColors;
	public List<Vector2> mUVs;
	public Color mColor = Color.white;
	public bool mDirty = true;
	public NGUICircle()
	{
		mCirclePoints = new List<Vector2>();
		mVertices = new List<Vector3>();
		mColors = new List<Color>();
		mUVs = new List<Vector2>();
	}
	public List<Vector3> getVertices() { return mVertices; }
	public List<Color> getColors() { return mColors; }
	public List<Vector2> getUVs() { return mUVs; }
	public Color getColor() { return mColor; }
	public bool isDirty() { return mDirty; }
	public void setDirty(bool dirty) { mDirty = dirty; }
	public List<Vector2> getPolygonPoints() { return mCirclePoints; }
	public void setColor(Color color)
	{
		mColor = color;
		mDirty = true;
	}
	public void setRadius(float radius, int detail = 0)
	{
		mRadius = radius;
		if (detail > 0)
		{
			mDetails = detail;
		}
		mDirty = true;
	}
	public void onPointsChanged()
	{
		// 计算顶点,纹理坐标,颜色
		mCirclePoints.Clear();
		mVertices.Clear();
		mColors.Clear();
		mUVs.Clear();
		float singleAngle = TWO_PI_RADIAN / mDetails;
		for (int i = 0; i < mDetails + 1; ++i)
		{
			mCirclePoints.Add(new Vector2(sin(singleAngle * i) * mRadius, cos(singleAngle * i) * mRadius));
		}
		for (int i = 0; i < mDetails; ++i)
		{
			mVertices.Add(mCirclePoints[i]);
			mVertices.Add(mCirclePoints[i + 1]);
			mVertices.Add(Vector3.zero);
			mVertices.Add(Vector3.zero);
			mUVs.Add(Vector2.zero);
			mUVs.Add(Vector2.zero);
			mUVs.Add(Vector2.zero);
			mUVs.Add(Vector2.zero);
			mColors.Add(mColor);
			mColors.Add(mColor);
			mColors.Add(mColor);
			mColors.Add(mColor);
		}
	}
}