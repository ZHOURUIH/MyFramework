using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MathUtility;

// 自定义的线条组件,可以用于代替LineRenderer在UI上显示
public class CustomLine : MaskableGraphic
{
	protected List<Vector3> mPointList = new();
	protected Vector3[] mVertices;
	protected Color32[] mColors;
	protected Vector2[] mUVs;
	protected Vector3Int[] mTriangles;
	protected float mWidth;
	public void setPointList(List<Vector3> list)
	{
		mPointList.Clear();
		foreach (Vector3 pos in list.safe())
		{
			// 去除连续的重复的点
			mPointList.addIf(pos, mPointList.Count <= 0 || !isVectorEqual(pos, mPointList[^1]));
		}
		refreshPoints();
	}
	public void setPointList(Span<Vector3> list)
	{
		mPointList.Clear();
		foreach (Vector3 pos in list)
		{
			mPointList.addIf(pos, mPointList.Count <= 0 || !isVectorEqual(pos, mPointList[^1]));
		}
		refreshPoints();
	}
	public void setPointList(Vector3[] list)
	{
		mPointList.Clear();
		foreach (Vector3 pos in list.safe())
		{
			mPointList.addIf(pos, mPointList.Count <= 0 || !isVectorEqual(pos, mPointList[^1]));
		}
		refreshPoints();
	}
	public float getWidth() { return mWidth; }
	public void setWidth(float width) { mWidth = width; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void refreshPoints()
	{
		mVertices = null;
		mColors = null;
		mUVs = null;
		mTriangles = null;
		// 先将模型数据清空
		int pointCount = mPointList.Count;
		if (pointCount < 2)
		{
			return;
		}
		// 计算顶点,纹理坐标,颜色
		Span<Vector3> originVertices = stackalloc Vector3[pointCount * 2];
		mVertices = new Vector3[(pointCount - 1) * 4];
		mColors = new Color32[(pointCount - 1) * 4];
		mUVs = new Vector2[(pointCount - 1) * 4];
		mTriangles = new Vector3Int[(pointCount - 1) * 2];

		float halfWidth = mWidth * 0.5f;
		for (int i = 0; i < pointCount; ++i)
		{
			// 如果当前点跟上一个点相同,则取上一点计算出的结果
			if (i > 0 && i < pointCount - 1 && isVectorEqual(mPointList[i - 1], mPointList[i]))
			{
				originVertices[2 * i + 0] = originVertices[2 * (i - 1) + 0];
				originVertices[2 * i + 1] = originVertices[2 * (i - 1) + 1];
			}
			else
			{
				if (i == 0)
				{
					Vector3 dir = (mPointList[i + 1] - mPointList[i]).normalized;
					float halfAngle = HALF_PI_RADIAN;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					float length = divide(halfWidth, sin(halfAngle));
					originVertices[2 * i + 0] = rotateVector3(dir, q0) * length + mPointList[i];
					originVertices[2 * i + 1] = rotateVector3(dir, q1) * length + mPointList[i];
				}
				else if (i > 0 && i < pointCount - 1)
				{
					Vector3 dir = (mPointList[i + 1] - mPointList[i]).normalized;
					Vector3 dir1 = (mPointList[i - 1] - mPointList[i]).normalized;
					float halfAngle = getAngleVectorToVector(dir, dir1, false) * 0.5f;
					float extendLength = divide(halfWidth, sin(halfAngle));
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					originVertices[2 * i + 0] = rotateVector3(dir, q0) * extendLength + mPointList[i];
					originVertices[2 * i + 1] = rotateVector3(dir, q1) * extendLength + mPointList[i];
				}
				else if (i == pointCount - 1)
				{
					Vector3 dir = (mPointList[i] - mPointList[i - 1]).normalized;
					float halfAngle = HALF_PI_RADIAN;
					Quaternion q0 = Quaternion.AngleAxis(toDegree(halfAngle), Vector3.back);
					Quaternion q1 = Quaternion.AngleAxis(toDegree(halfAngle - PI_RADIAN), Vector3.back);
					float length = divide(halfWidth, sin(halfAngle));
					originVertices[2 * i + 0] = rotateVector3(dir, q0) * length + mPointList[i];
					originVertices[2 * i + 1] = rotateVector3(dir, q1) * length + mPointList[i];
				}
			}
		}
		float inverseWidth = divide(1.0f, mWidth);
		for (int i = 0; i < pointCount - 1; ++i)
		{
			for (int j = 0; j < 4; ++j)
			{
				mColors[i * 4 + j] = color;
			}
			mVertices[4 * i + 0] = originVertices[2 * i + 0];
			mVertices[4 * i + 1] = originVertices[2 * i + 1];
			mVertices[4 * i + 2] = originVertices[2 * i + 2];
			mVertices[4 * i + 3] = originVertices[2 * i + 3];
			mUVs[4 * i + 0] = new(0.0f, 0.0f);
			mUVs[4 * i + 1] = new(0.0f, 1.0f);
			mUVs[4 * i + 2] = new(getLength(mVertices[4 * i + 2] - mVertices[4 * i + 0]) * inverseWidth, 0.0f);
			mUVs[4 * i + 3] = new(getLength(mVertices[4 * i + 2] - mVertices[4 * i + 0]) * inverseWidth, 1.0f);
		}
		// 计算顶点索引,每两个点之间两个三角面，每个三角面三个顶点
		for (int i = 0; i < pointCount - 1; ++i)
		{
			int indexValue = i * 4;
			mTriangles[i * 2 + 0] = new(indexValue + 0, indexValue + 1, indexValue + 2);
			mTriangles[i * 2 + 1] = new(indexValue + 1, indexValue + 3, indexValue + 2);
		}
	}
	// 此函数由UGUI自动调用
	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		base.OnPopulateMesh(toFill);
		toFill.Clear();
		if (mVertices == null || mColors == null || mUVs == null || mTriangles == null)
		{
			return;
		}
		int vertCount = mVertices.Length;
		for (int i = 0; i < vertCount; ++i)
		{
			toFill.AddVert(mVertices[i], mColors[i], mUVs[i]);
		}
		int triangleCount = mTriangles.Length;
		for (int i = 0; i < triangleCount; ++i)
		{
			toFill.AddTriangle(mTriangles[i].x, mTriangles[i].y, mTriangles[i].z);
		}
	}
}