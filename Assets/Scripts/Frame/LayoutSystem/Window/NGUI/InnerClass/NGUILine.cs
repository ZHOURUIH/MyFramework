using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class NGUILine : GameBase, INGUIShape
{
	// 为避免GC而创建的变量
	private List<Vector3> mTempVertices;
	private List<Color> mTempColors;
	private List<Vector2> mTempUVs;
	public List<Vector3> mVertices;
	public List<Color> mColors;
	public List<Vector2> mUVs;
	public OnNGUILineGenerated mOnNGUILineGenerated;
	public Color mColor = Color.green;
	protected List<Vector3> mPointList;
	protected List<Vector2> mPointList2;	// 存储Vector2类型的点列表
	protected float mWidth = 10.0f;     // 宽度的一半
	public bool mDirty = true;
	public NGUILine()
	{
		mPointList = new List<Vector3>();
		mPointList2 = new List<Vector2>();
		mVertices = new List<Vector3>();
		mColors = new List<Color>();
		mUVs = new List<Vector2>();
		mTempVertices = new List<Vector3>();
		mTempColors = new List<Color>();
		mTempUVs = new List<Vector2>();
	}
	public List<Vector3> getVertices() { return mVertices; }
	public List<Color> getColors() { return mColors; }
	public List<Vector2> getUVs() { return mUVs; }
	public void setColor(Color color) { mColor = color; }
	public Color getColor() { return mColor; }
	public void setWidth(float width) { mWidth = width; }
	public bool isDirty() { return mDirty; }
	public void setDirty(bool dirty) { mDirty = dirty; }
	public List<Vector2> getPolygonPoints() { return mPointList2; }
	public void setPointList(List<Vector3> list)
	{
		mPointList.Clear();
		mPointList2.Clear();
		int count = list.Count;
		for (int i = 0; i < count; ++i)
		{
			if (mPointList.Count > 0 && isVectorEqual(list[i], mPointList[mPointList.Count - 1]))
			{
				continue;
			}
			mPointList.Add(list[i]);
			mPointList2.Add(list[i]);
		}
		mDirty = true;
	}
	public void setPointList(Vector3[] list)
	{
		mPointList.Clear();
		mPointList2.Clear();
		int count = list.Length;
		for (int i = 0; i < count; ++i)
		{
			if (mPointList.Count > 0 && isVectorEqual(list[i], mPointList[mPointList.Count - 1]))
			{
				continue;
			}
			mPointList.Add(list[i]);
			mPointList2.Add(list[i]);
		}
		mDirty = true;
	}
	public void onPointsChanged()
	{
		mVertices.Clear();
		mColors.Clear();
		mUVs.Clear();
		int pointCount = mPointList.Count;
		if (pointCount < 2)
		{
			return;
		}
		// 计算顶点,纹理坐标,颜色
		for (int i = 0; i < pointCount; ++i)
		{
			// 如果当前点跟上一个点相同,则取上一点计算出的结果
			if (i > 0 && i < pointCount - 1 && isVectorEqual(mPointList[i - 1], mPointList[i]))
			{
				mVertices.Add(mVertices[2 * (i - 1) + 0]);
				mVertices.Add(mVertices[2 * (i - 1) + 1]);
				mColors.Add(mColors[2 * (i - 1) + 0]);
				mColors.Add(mColors[2 * (i - 1) + 1]);
				mUVs.Add(mUVs[2 * (i - 1) + 0]);
				mUVs.Add(mUVs[2 * (i - 1) + 1]);
			}
			else
			{
				if (i == 0)
				{
					Vector3 dir = (mPointList[i + 1] - mPointList[i]).normalized;
					float halfAngle = HALF_PI_RADIAN;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					mVertices.Add(rotateVector3(dir, q0) * mWidth / sin(halfAngle));
					mVertices.Add(rotateVector3(dir, q1) * mWidth / sin(halfAngle));
				}
				else if (i > 0 && i < pointCount - 1)
				{
					Vector3 dir = (mPointList[i + 1] - mPointList[i]).normalized;
					Vector3 dir1 = (mPointList[i - 1] - mPointList[i]).normalized;
					float halfAngle = getAngleFromVector3ToVector3(dir, dir1, false) * 0.5f;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					if (halfAngle >= 0.0f)
					{
						mVertices.Add(rotateVector3(dir, q0) * mWidth);
						mVertices.Add(rotateVector3(dir, q1) * mWidth);
					}
					else
					{
						mVertices.Add(rotateVector3(dir, q1) * mWidth);
						mVertices.Add(rotateVector3(dir, q0) * mWidth);
					}
				}
				else if (i == pointCount - 1)
				{
					Vector3 dir = (mPointList[i] - mPointList[i - 1]).normalized;
					float halfAngle = HALF_PI_RADIAN;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					mVertices.Add(rotateVector3(dir, q0) * mWidth / sin(halfAngle));
					mVertices.Add(rotateVector3(dir, q1) * mWidth / sin(halfAngle));
				}
				mVertices[2 * i + 0] += mPointList[i];
				mVertices[2 * i + 1] += mPointList[i];
				mColors.Add(mColor);
				mColors.Add(mColor);
				mUVs.Add(Vector2.zero);
				mUVs.Add(Vector2.zero);
			}
		}
		// 每4个点为一个面
		mTempVertices.Clear();
		int segmentCount = pointCount - 1;
		for (int i = 0; i < segmentCount; ++i)
		{
			mTempVertices.Add(mVertices[i * 2 + 0]);
			mTempVertices.Add(mVertices[i * 2 + 1]);
			mTempVertices.Add(mVertices[i * 2 + 3]);
			mTempVertices.Add(mVertices[i * 2 + 2]);
		}
		mVertices.Clear();
		mVertices.AddRange(mTempVertices);
		mTempColors.Clear();
		for (int i = 0; i < segmentCount; ++i)
		{
			mTempColors.Add(mColors[i * 2 + 0]);
			mTempColors.Add(mColors[i * 2 + 1]);
			mTempColors.Add(mColors[i * 2 + 3]);
			mTempColors.Add(mColors[i * 2 + 2]);
		}
		mColors.Clear();
		mColors.AddRange(mTempColors);
		mTempUVs.Clear();
		for (int i = 0; i < segmentCount; ++i)
		{
			mTempUVs.Add(mUVs[i * 2 + 0]);
			mTempUVs.Add(mUVs[i * 2 + 1]);
			mTempUVs.Add(mUVs[i * 2 + 3]);
			mTempUVs.Add(mUVs[i * 2 + 2]);
		}
		mUVs.Clear();
		mUVs.AddRange(mTempUVs);
		if(mVertices != null)
		{
			mOnNGUILineGenerated?.Invoke(this);
		}
	}
	public void setOnLineGenerated(OnNGUILineGenerated callback) { mOnNGUILineGenerated = callback; }
}