using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class NGUITriangle : GameBase, INGUIShape
{
	public List<Vector2> mTrianglePoints;
	public List<Vector3> mVertices;
	public List<Color> mColors;
	public List<Vector2> mUVs;
	public Color mColor = Color.white;
	public bool mDirty = true;
	public NGUITriangle()
	{
		mTrianglePoints = new List<Vector2>();
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
	public List<Vector2> getPolygonPoints() { return mTrianglePoints; }
	public void setColor(Color color)
	{
		mColor = color;
		mDirty = true;
	}
	public void setPoints(Vector3 point0, Vector3 point1, Vector3 point2)
	{
		mTrianglePoints.Clear();
		mTrianglePoints.Add(point0);
		mTrianglePoints.Add(point1);
		mTrianglePoints.Add(point2);
		mDirty = true;
	}
	public void onPointsChanged()
	{
		// 计算顶点,纹理坐标,颜色
		mVertices.Clear();
		mColors.Clear();
		mUVs.Clear();
		int pointCount = mTrianglePoints.Count;
		for (int i = 0; i < pointCount; ++i)
		{
			mVertices.Add(mTrianglePoints[i]);
			mUVs.Add(Vector2.zero);
			mColors.Add(mColor);
		}
		// 由于NGUI为4个顶点一个片元,所以三角形也要凑齐4个顶点
		mVertices.Add(mVertices[pointCount - 1]);
		mUVs.Add(mUVs[pointCount - 1]);
		mColors.Add(mColors[pointCount - 1]);
	}
}