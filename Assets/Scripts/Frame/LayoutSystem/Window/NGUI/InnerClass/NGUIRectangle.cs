using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class NGUIRectangle : GameBase, INGUIShape
{
	public List<Vector2> mRectanglePoints;
	public List<Vector3> mVertices;
	public List<Color> mColors;
	public List<Vector2> mUVs;
	public Color mColor = Color.white;
	public Vector2 mSize;
	public bool mDirty = true;
	public NGUIRectangle()
	{
		mRectanglePoints = new List<Vector2>();
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
	public List<Vector2> getPolygonPoints() { return mRectanglePoints; }
	public void setColor(Color color)
	{
		mColor = color;
		mDirty = true;
	}
	public void setSize(Vector2 size)
	{
		mSize = size;
		mRectanglePoints.Clear();
		mRectanglePoints.Add(new Vector2(-mSize.x * 0.5f, -mSize.y * 0.5f));
		mRectanglePoints.Add(new Vector2(-mSize.x * 0.5f, mSize.y * 0.5f));
		mRectanglePoints.Add(new Vector2(mSize.x * 0.5f, mSize.y * 0.5f));
		mRectanglePoints.Add(new Vector2(mSize.x * 0.5f, -mSize.y * 0.5f));
		mDirty = true;
	}
	public void onPointsChanged()
	{
		// 计算顶点,纹理坐标,颜色
		mVertices.Clear();
		mColors.Clear();
		mUVs.Clear();
		int pointCount = mRectanglePoints.Count;
		for (int i = 0; i < pointCount; ++i)
		{
			mVertices.Add(mRectanglePoints[i]);
			mUVs.Add(Vector2.zero);
			mColors.Add(mColor);
		}
	}
}